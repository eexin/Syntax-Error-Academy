using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI.WebControls;

// Web Application project (CodeBehind + designer file).
public partial class DailyChallenge : System.Web.UI.Page
{
    private string ConnStr
    {
        get { return ConfigurationManager.ConnectionStrings["SEAConnection"].ConnectionString; }
    }

    private int Sid { get { return Convert.ToInt32(Session["StudentID"]); } }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["StudentID"] == null)
        {
            Response.Redirect("Login.aspx");
            return;
        }

        ApplyWallpaper();

        if (!IsPostBack)
        {
            Streak.EnsureCurrent(ConnStr, Sid);
            LoadHeader();

            // One challenge per day: if it's already done, show the "done" panel instead.
            if (DoneToday())
            {
                pnlForm.Visible = false;
                pnlDone.Visible = true;
            }
            else
            {
                LoadChallenge();
            }
        }
    }

    private void LoadHeader()
    {
        using (SqlConnection cn = new SqlConnection(ConnStr))
        using (SqlCommand cmd = new SqlCommand(
            "SELECT StudentName, TotalPoint, Streak FROM Student WHERE StudentID = @sid", cn))
        {
            cmd.Parameters.AddWithValue("@sid", Sid);
            cn.Open();
            using (SqlDataReader r = cmd.ExecuteReader())
            {
                if (r.Read())
                {
                    litInitials.Text = Avatar.Render(Avatar.GetEquippedKeys(ConnStr, Sid), true);
                    lblCoins.Text = Convert.ToInt32(r["TotalPoint"]).ToString("N0");
                    lblStreak.Text = Convert.ToInt32(r["Streak"]).ToString();
                }
            }
        }
    }

    // Pick one question the student has already answered. The date seed keeps it the
    // same question for the whole day.
    private int PickQuestionId()
    {
        string seed = DateTime.Today.ToString("yyyyMMdd");
        const string sql =
            "SELECT TOP 1 q.QuestionID FROM Question q " +
            "JOIN StudentsAnswer sa ON sa.QuestionID = q.QuestionID AND sa.StudentID = @sid " +
            "ORDER BY ABS(CHECKSUM(CONVERT(VARCHAR(20), q.QuestionID) + @seed))";
        using (SqlConnection cn = new SqlConnection(ConnStr))
        using (SqlCommand cmd = new SqlCommand(sql, cn))
        {
            cmd.Parameters.AddWithValue("@sid", Sid);
            cmd.Parameters.AddWithValue("@seed", seed);
            cn.Open();
            object o = cmd.ExecuteScalar();
            return o == null ? 0 : Convert.ToInt32(o);
        }
    }

    private void LoadChallenge()
    {
        int qid = PickQuestionId();
        if (qid == 0)
        {
            pnlForm.Visible = false;
            pnlLocked.Visible = true;
            return;
        }

        hfQid.Value = qid.ToString();
        string type = "";
        using (SqlConnection cn = new SqlConnection(ConnStr))
        using (SqlCommand cmd = new SqlCommand("SELECT QuestionText, QuestionType FROM Question WHERE QuestionID = @q", cn))
        {
            cmd.Parameters.AddWithValue("@q", qid);
            cn.Open();
            using (SqlDataReader r = cmd.ExecuteReader())
            {
                if (r.Read())
                {
                    lblQ.Text = Server.HtmlEncode(r["QuestionText"].ToString());
                    type = r["QuestionType"].ToString();
                }
            }
        }

        rfvMcq.Enabled = (type == "MCQ");
        rfvEssay.Enabled = (type == "Essay");
        pnlMCQ.Visible = (type == "MCQ");
        pnlEssay.Visible = (type == "Essay");
        pnlMatch.Visible = (type == "Matching");
        pnlPuzzle.Visible = (type == "Puzzle");

        if (type == "MCQ")
        {
            rblOptions.DataSource = QueryTable("SELECT OptionText FROM MCQOption WHERE QuestionID = @q ORDER BY McqID", qid);
            rblOptions.DataTextField = "OptionText";
            rblOptions.DataValueField = "OptionText";
            rblOptions.DataBind();
        }
        else if (type == "Matching")
        {
            rptMatch.DataSource = QueryTable("SELECT QuestionID, LeftItem FROM MatchOption WHERE QuestionID = @q ORDER BY MatchID", qid);
            rptMatch.DataBind();
        }
        else if (type == "Puzzle")
        {
            rptPuzzle.DataSource = QueryTable("SELECT QuestionID, TokenText FROM PuzzleToken WHERE QuestionID = @q ORDER BY TokenID", qid);
            rptPuzzle.DataBind();
        }
    }

    protected void rptMatch_ItemDataBound(object sender, RepeaterItemEventArgs e)
    {
        if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem) return;
        DataRowView drv = (DataRowView)e.Item.DataItem;
        int qid = Convert.ToInt32(drv["QuestionID"]);
        string left = drv["LeftItem"].ToString();
        ((Literal)e.Item.FindControl("lblLeft")).Text = Server.HtmlEncode(left);
        ((HiddenField)e.Item.FindControl("hfLeft")).Value = left;
        DropDownList ddl = (DropDownList)e.Item.FindControl("ddlMatch");
        ddl.Items.Add(new ListItem("-- choose --", ""));
        foreach (DataRow row in QueryTable("SELECT RightItem FROM MatchOption WHERE QuestionID = @q ORDER BY RightItem", qid).Rows)
            ddl.Items.Add(new ListItem(row["RightItem"].ToString(), row["RightItem"].ToString()));
    }

    protected void rptPuzzle_ItemDataBound(object sender, RepeaterItemEventArgs e)
    {
        if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem) return;
        DataRowView drv = (DataRowView)e.Item.DataItem;
        int qid = Convert.ToInt32(drv["QuestionID"]);
        string token = drv["TokenText"].ToString();
        ((Literal)e.Item.FindControl("lblToken")).Text = Server.HtmlEncode(token);
        ((HiddenField)e.Item.FindControl("hfToken")).Value = token;
        int n = Convert.ToInt32(Scalar("SELECT COUNT(*) FROM PuzzleToken WHERE QuestionID = @q", qid));
        DropDownList ddl = (DropDownList)e.Item.FindControl("ddlPos");
        ddl.Items.Add(new ListItem("-- pos --", ""));
        for (int i = 1; i <= n; i++) ddl.Items.Add(new ListItem(i.ToString(), i.ToString()));
    }

    // Has this student already completed today's challenge?
    private bool DoneToday()
    {
        using (SqlConnection cn = new SqlConnection(ConnStr))
        using (SqlCommand cmd = new SqlCommand(
            "SELECT LastDailyDate FROM Student WHERE StudentID = @sid", cn))
        {
            cmd.Parameters.AddWithValue("@sid", Sid);
            cn.Open();
            object o = cmd.ExecuteScalar();
            return o != null && o != DBNull.Value && Convert.ToDateTime(o).Date == DateTime.Today;
        }
    }

    // Stamp today's date so the challenge locks until tomorrow.
    private void MarkDoneToday()
    {
        using (SqlConnection cn = new SqlConnection(ConnStr))
        using (SqlCommand cmd = new SqlCommand(
            "UPDATE Student SET LastDailyDate = @d WHERE StudentID = @sid", cn))
        {
            cmd.Parameters.AddWithValue("@d", DateTime.Today);
            cmd.Parameters.AddWithValue("@sid", Sid);
            cn.Open();
            cmd.ExecuteNonQuery();
        }
    }

    protected void btnSubmit_Click(object sender, EventArgs e)
    {
        Page.Validate();
        if (!Page.IsValid) return;

        // Safety net: block a second submit on the same day (e.g. via back button).
        if (DoneToday())
        {
            pnlForm.Visible = false;
            pnlDone.Visible = true;
            return;
        }

        int qid = Convert.ToInt32(hfQid.Value);
        string type = Convert.ToString(Scalar("SELECT QuestionType FROM Question WHERE QuestionID = @q", qid));

        bool correct = Grade(qid, type);

        // Completing the daily challenge counts as today's activity (keeps the streak)
        // and locks the challenge until tomorrow.
        Streak.RecordActivity(ConnStr, Sid);
        MarkDoneToday();
        LoadHeader();

        pnlForm.Visible = false;
        pnlResult.Visible = true;
        lblResult.Text = correct ? "Correct!" : "Not quite";
        if (string.IsNullOrEmpty(lblFeedback.Text))
            lblFeedback.Text = correct ? "Nice — streak kept alive." : "Good effort — your streak still counts for today.";
    }

    private bool Grade(int qid, string type)
    {
        if (type == "MCQ")
        {
            object correct = Scalar("SELECT TOP 1 OptionText FROM MCQOption WHERE QuestionID = @q AND IsCorrect = 1", qid);
            return correct != null && rblOptions.SelectedValue == correct.ToString();
        }
        if (type == "Essay")
        {
            string answer = txtEssay.Text;
            string question = Convert.ToString(Scalar("SELECT QuestionText FROM Question WHERE QuestionID = @q", qid));
            object exp = Scalar("SELECT TOP 1 EssayTitle FROM EssayOption WHERE QuestionID = @q", qid);
            string expected = (exp == null || exp == DBNull.Value) ? "" : exp.ToString();

            EssayGrader.GradeResult gr = EssayGrader.Grade(question, expected, answer);
            if (!gr.Available) { lblFeedback.Text = "AI grading unavailable — answer accepted."; return !string.IsNullOrWhiteSpace(answer); }
            lblFeedback.Text = gr.Feedback;
            return gr.IsCorrect;
        }
        if (type == "Matching")
        {
            Dictionary<string, string> ans = new Dictionary<string, string>();
            foreach (DataRow row in QueryTable("SELECT LeftItem, RightItem FROM MatchOption WHERE QuestionID = @q", qid).Rows)
                ans[row["LeftItem"].ToString()] = row["RightItem"].ToString();
            foreach (RepeaterItem mi in rptMatch.Items)
            {
                if (mi.ItemType != ListItemType.Item && mi.ItemType != ListItemType.AlternatingItem) continue;
                string left = ((HiddenField)mi.FindControl("hfLeft")).Value;
                string chosen = ((DropDownList)mi.FindControl("ddlMatch")).SelectedValue;
                if (!ans.ContainsKey(left) || ans[left] != chosen) return false;
            }
            return true;
        }
        if (type == "Puzzle")
        {
            Dictionary<string, int> ans = new Dictionary<string, int>();
            foreach (DataRow row in QueryTable("SELECT TokenText, PositionOrder FROM PuzzleToken WHERE QuestionID = @q", qid).Rows)
                ans[row["TokenText"].ToString()] = Convert.ToInt32(row["PositionOrder"]);
            foreach (RepeaterItem pi in rptPuzzle.Items)
            {
                if (pi.ItemType != ListItemType.Item && pi.ItemType != ListItemType.AlternatingItem) continue;
                string token = ((HiddenField)pi.FindControl("hfToken")).Value;
                int pos;
                if (!int.TryParse(((DropDownList)pi.FindControl("ddlPos")).SelectedValue, out pos)) return false;
                if (!ans.ContainsKey(token) || ans[token] != pos) return false;
            }
            return true;
        }
        return false;
    }

    private DataTable QueryTable(string sql, int qid)
    {
        DataTable dt = new DataTable();
        using (SqlConnection cn = new SqlConnection(ConnStr))
        using (SqlCommand cmd = new SqlCommand(sql, cn))
        {
            cmd.Parameters.AddWithValue("@q", qid);
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                da.Fill(dt);
        }
        return dt;
    }

    private object Scalar(string sql, int qid)
    {
        using (SqlConnection cn = new SqlConnection(ConnStr))
        using (SqlCommand cmd = new SqlCommand(sql, cn))
        {
            cmd.Parameters.AddWithValue("@q", qid);
            cn.Open();
            return cmd.ExecuteScalar();
        }
    }

    private string GetInitials(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "U";
        string[] parts = name.Trim().Split(' ');
        if (parts.Length == 1) return parts[0].Substring(0, 1).ToUpper();
        return (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpper();
    }

    // Apply the wallpaper the student bought + equipped in the Item Shop (if any).
    // Runs on every load (including postbacks) so the background is always current.
    private void ApplyWallpaper()
    {
        string wall = Wallpaper.GetEquipped(ConnStr, Sid);
        if (!string.IsNullOrEmpty(wall))
            pageBody.Style["background"] = wall;
    }

}