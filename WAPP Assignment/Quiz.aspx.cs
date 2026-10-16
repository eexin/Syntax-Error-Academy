using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;

// "Web Site" model (CodeFile + no namespace).
public partial class Quiz : System.Web.UI.Page
{
    private string ConnStr
    {
        get { return ConfigurationManager.ConnectionStrings["SEAConnection"].ConnectionString; }
    }

    // Collects per-essay AI feedback to show on the result panel.
    private List<string> _essayFeedback;

    // Used to shuffle the right-hand answers in matching questions.
    private static readonly Random _rng = new Random();

    protected void Page_Load(object sender, EventArgs e)
    {
        // Only logged-in students may take a quiz.
        if (Session["StudentID"] == null)
        {
            Response.Redirect("Login.aspx");
            return;
        }

        ApplyWallpaper();

        if (!IsPostBack)
        {
            int quizId;
            if (!int.TryParse(Request.QueryString["id"], out quizId))
            {
                Response.Redirect("StudentDashboard.aspx");
                return;
            }
            hfQuizId.Value = quizId.ToString();

            LoadHeader(Convert.ToInt32(Session["StudentID"]));
            LoadQuiz(quizId);
            BindQuestions(quizId);
        }
    }

    // ---------- Top bar ----------
    private void LoadHeader(int studentId)
    {
        using (SqlConnection cn = new SqlConnection(ConnStr))
        using (SqlCommand cmd = new SqlCommand(
            "SELECT StudentName, TotalPoint, Streak FROM Student WHERE StudentID = @sid", cn))
        {
            cmd.Parameters.AddWithValue("@sid", studentId);
            cn.Open();
            using (SqlDataReader r = cmd.ExecuteReader())
            {
                if (r.Read())
                {
                    litInitials.Text = Avatar.Render(Avatar.GetEquippedKeys(ConnStr, studentId), true);
                    lblCoins.Text = Convert.ToInt32(r["TotalPoint"]).ToString("N0");
                    lblStreak.Text = Convert.ToInt32(r["Streak"]).ToString();
                }
            }
        }
    }

    private void LoadQuiz(int quizId)
    {
        using (SqlConnection cn = new SqlConnection(ConnStr))
        using (SqlCommand cmd = new SqlCommand(
            "SELECT QuizTitle, QuizDescription FROM Quiz WHERE QuizID = @id", cn))
        {
            cmd.Parameters.AddWithValue("@id", quizId);
            cn.Open();
            using (SqlDataReader r = cmd.ExecuteReader())
            {
                if (r.Read())
                {
                    lblQuizTitle.Text = Server.HtmlEncode(r["QuizTitle"].ToString());
                    lblQuizDesc.Text = Server.HtmlEncode(r["QuizDescription"] == DBNull.Value ? "" : r["QuizDescription"].ToString());
                }
            }
        }
    }

    // ---------- Build the question list ----------
    private void BindQuestions(int quizId)
    {
        DataTable dt = new DataTable();
        using (SqlConnection cn = new SqlConnection(ConnStr))
        using (SqlCommand cmd = new SqlCommand(
            "SELECT QuestionID, QuestionText, QuestionType, Score FROM Question WHERE QuizID = @id ORDER BY QuestionID", cn))
        {
            cmd.Parameters.AddWithValue("@id", quizId);
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                da.Fill(dt);
        }
        rptQuestions.DataSource = dt;
        rptQuestions.DataBind();
    }

    // Show the right input panel for each question and load its data.
    protected void rptQuestions_ItemDataBound(object sender, RepeaterItemEventArgs e)
    {
        if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
            return;

        DataRowView drv = (DataRowView)e.Item.DataItem;
        int qid = Convert.ToInt32(drv["QuestionID"]);
        string type = drv["QuestionType"].ToString();

        ((HiddenField)e.Item.FindControl("hfQid")).Value = qid.ToString();
        ((HiddenField)e.Item.FindControl("hfType")).Value = type;

        Panel pnlMCQ = (Panel)e.Item.FindControl("pnlMCQ");
        Panel pnlEssay = (Panel)e.Item.FindControl("pnlEssay");
        Panel pnlMatch = (Panel)e.Item.FindControl("pnlMatch");
        Panel pnlPuzzle = (Panel)e.Item.FindControl("pnlPuzzle");

        // Only the active question type's required-field validators should fire.
        ((RequiredFieldValidator)e.Item.FindControl("rfvMcq")).Enabled = (type == "MCQ");
        ((RequiredFieldValidator)e.Item.FindControl("rfvEssay")).Enabled = (type == "Essay");

        pnlMCQ.Visible = (type == "MCQ");
        pnlEssay.Visible = (type == "Essay");
        pnlMatch.Visible = (type == "Matching");
        pnlPuzzle.Visible = (type == "Puzzle");

        if (type == "MCQ")
        {
            RadioButtonList rbl = (RadioButtonList)e.Item.FindControl("rblOptions");
            rbl.DataSource = QueryTable("SELECT OptionText FROM MCQOption WHERE QuestionID = @q ORDER BY McqID", qid);
            rbl.DataTextField = "OptionText";
            rbl.DataValueField = "OptionText";
            rbl.DataBind();
        }
        else if (type == "Matching")
        {
            DataTable pairs = QueryTable(
                "SELECT LeftItem, RightItem FROM MatchOption WHERE QuestionID = @q ORDER BY MatchID", qid);

            // Right-hand answers shuffled so the columns don't line up.
            List<string> rights = new List<string>();
            foreach (DataRow row in pairs.Rows) rights.Add(row["RightItem"].ToString());
            for (int i = rights.Count - 1; i > 0; i--)            // Fisher-Yates shuffle
            {
                int j = _rng.Next(i + 1);
                string tmp = rights[i]; rights[i] = rights[j]; rights[j] = tmp;
            }

            HiddenField hfRes = (HiddenField)e.Item.FindControl("hfMatchResult");
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("<div class=\"match\" data-target=\"").Append(hfRes.ClientID).Append("\">");

            // Left column: each chip carries its correct answer in data-answer.
            sb.Append("<div class=\"match-col\">");
            foreach (DataRow row in pairs.Rows)
            {
                string l = row["LeftItem"].ToString();
                string r = row["RightItem"].ToString();
                sb.Append("<button type=\"button\" class=\"chip\" data-side=\"L\" data-answer=\"")
                  .Append(System.Web.HttpUtility.HtmlAttributeEncode(r)).Append("\" data-left=\"")
                  .Append(System.Web.HttpUtility.HtmlAttributeEncode(l)).Append("\">")
                  .Append(Server.HtmlEncode(l)).Append("</button>");
            }
            sb.Append("</div>");

            // Right column: shuffled answers.
            sb.Append("<div class=\"match-col\">");
            foreach (string r in rights)
            {
                sb.Append("<button type=\"button\" class=\"chip\" data-side=\"R\" data-right=\"")
                  .Append(System.Web.HttpUtility.HtmlAttributeEncode(r)).Append("\">")
                  .Append(Server.HtmlEncode(r)).Append("</button>");
            }
            sb.Append("</div></div>");

            ((Literal)e.Item.FindControl("litMatchUI")).Text = sb.ToString();
        }
        else if (type == "Puzzle")
        {
            Repeater rptPuzzle = (Repeater)e.Item.FindControl("rptPuzzle");
            rptPuzzle.DataSource = QueryTable("SELECT QuestionID, TokenText FROM PuzzleToken WHERE QuestionID = @q ORDER BY TokenID", qid);
            rptPuzzle.DataBind();
        }
    }

    // Each matching row: show the left item, fill the dropdown with all right items.
    // Each puzzle row: show the token, fill the dropdown with positions 1..N.
    protected void rptPuzzle_ItemDataBound(object sender, RepeaterItemEventArgs e)
    {
        if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
            return;

        DataRowView drv = (DataRowView)e.Item.DataItem;
        int qid = Convert.ToInt32(drv["QuestionID"]);
        string token = drv["TokenText"].ToString();

        ((Literal)e.Item.FindControl("lblToken")).Text = Server.HtmlEncode(token);
        ((HiddenField)e.Item.FindControl("hfToken")).Value = token;

        int n = Convert.ToInt32(Scalar("SELECT COUNT(*) FROM PuzzleToken WHERE QuestionID = @q", qid));
        DropDownList ddl = (DropDownList)e.Item.FindControl("ddlPos");
        ddl.Items.Add(new ListItem("-- pos --", ""));
        for (int i = 1; i <= n; i++)
            ddl.Items.Add(new ListItem(i.ToString(), i.ToString()));
    }

    // ---------- Submit: grade, write answers, award points ----------
    protected void btnSubmit_Click(object sender, EventArgs e)
    {
        Page.Validate();
        if (!Page.IsValid) return;

        int sid = Convert.ToInt32(Session["StudentID"]);
        int correct = 0, total = 0, newPoints = 0;
        _essayFeedback = new List<string>();

        foreach (RepeaterItem item in rptQuestions.Items)
        {
            if (item.ItemType != ListItemType.Item && item.ItemType != ListItemType.AlternatingItem)
                continue;

            int qid = Convert.ToInt32(((HiddenField)item.FindControl("hfQid")).Value);
            string type = ((HiddenField)item.FindControl("hfType")).Value;

            bool isCorrect = GradeQuestion(item, qid, type);
            total++;
            if (isCorrect) correct++;

            // Award points only the first time a question is answered correctly.
            bool wasCorrect = WasAnsweredCorrect(qid, sid);

            // INSERT a new answer, or UPDATE the existing one (CRUD write side).
            UpsertAnswer(qid, sid, isCorrect ? "Correct" : "Wrong");

            if (isCorrect && !wasCorrect)
                newPoints += GetScore(qid);
        }

        if (newPoints > 0)
            AddPoints(sid, newPoints);

        // Doing a quiz counts as today's activity -> keep the streak going.
        Streak.RecordActivity(ConnStr, sid);

        // Refresh the top bar so the coin total reflects the new points.
        LoadHeader(sid);

        // Show the result, hide the form.
        pnlForm.Visible = false;
        pnlResult.Visible = true;
        lblScore.Text = correct + " / " + total;
        lblEarned.Text = newPoints > 0
            ? ("You earned " + newPoints + " points!")
            : "No new points this time — try the ones you missed.";

        // Offer a retry only when at least one answer was wrong.
        lnkRetry.Visible = correct < total;
        lnkRetry.NavigateUrl = "Quiz.aspx?id=" + hfQuizId.Value;

        if (_essayFeedback.Count > 0)
        {
            System.Text.StringBuilder fb = new System.Text.StringBuilder();
            foreach (string f in _essayFeedback)
                fb.Append("<div class=\"q-feedback\">").Append(Server.HtmlEncode(f)).Append("</div>");
            lblFeedback.Text = fb.ToString();
        }
    }

    // ---------- Grading per type (always checked against the database) ----------
    private bool GradeQuestion(RepeaterItem item, int qid, string type)
    {
        if (type == "MCQ")
        {
            RadioButtonList rbl = (RadioButtonList)item.FindControl("rblOptions");
            string chosen = rbl.SelectedValue;
            object correct = Scalar("SELECT TOP 1 OptionText FROM MCQOption WHERE QuestionID = @q AND IsCorrect = 1", qid);
            return correct != null && chosen == correct.ToString();
        }
        if (type == "Essay")
        {
            // Essays are graded by the AI through the n8n webhook (EssayGrader).
            TextBox txt = (TextBox)item.FindControl("txtEssay");
            string answer = txt.Text;

            string question = Convert.ToString(Scalar("SELECT QuestionText FROM Question WHERE QuestionID = @q", qid));
            object exp = Scalar("SELECT TOP 1 EssayTitle FROM EssayOption WHERE QuestionID = @q", qid);
            string expected = (exp == null || exp == DBNull.Value) ? "" : exp.ToString();

            EssayGrader.GradeResult gr = EssayGrader.Grade(question, expected, answer);

            if (!gr.Available)
            {
                // If the AI can't be reached, fall back to accepting a non-empty answer
                // so the quiz still works (e.g. no key configured during a demo).
                if (_essayFeedback != null)
                    _essayFeedback.Add("AI grading unavailable — your answer was accepted as submitted.");
                return !string.IsNullOrWhiteSpace(answer);
            }

            if (_essayFeedback != null)
                _essayFeedback.Add((gr.IsCorrect ? "Correct: " : "Needs work: ") + gr.Feedback);
            return gr.IsCorrect;
        }
        if (type == "Matching")
        {
            // Correct mapping from the database.
            Dictionary<string, string> answer = new Dictionary<string, string>();
            DataTable t = QueryTable("SELECT LeftItem, RightItem FROM MatchOption WHERE QuestionID = @q", qid);
            foreach (DataRow row in t.Rows)
                answer[row["LeftItem"].ToString()] = row["RightItem"].ToString();

            // What the student matched (written by the tap-to-match UI, URL-encoded JSON).
            string raw = ((HiddenField)item.FindControl("hfMatchResult")).Value;
            if (string.IsNullOrEmpty(raw)) return false;

            Dictionary<string, string> chosen;
            try
            {
                string json = Server.UrlDecode(raw);
                chosen = new System.Web.Script.Serialization.JavaScriptSerializer()
                             .Deserialize<Dictionary<string, string>>(json);
            }
            catch { return false; }

            if (chosen == null || chosen.Count != answer.Count) return false;
            foreach (KeyValuePair<string, string> kv in answer)
                if (!chosen.ContainsKey(kv.Key) || chosen[kv.Key] != kv.Value) return false;
            return true;
        }
        if (type == "Puzzle")
        {
            Dictionary<string, int> answer = new Dictionary<string, int>();
            DataTable t = QueryTable("SELECT TokenText, PositionOrder FROM PuzzleToken WHERE QuestionID = @q", qid);
            foreach (DataRow row in t.Rows)
                answer[row["TokenText"].ToString()] = Convert.ToInt32(row["PositionOrder"]);

            Repeater rpt = (Repeater)item.FindControl("rptPuzzle");
            foreach (RepeaterItem pi in rpt.Items)
            {
                if (pi.ItemType != ListItemType.Item && pi.ItemType != ListItemType.AlternatingItem) continue;
                string token = ((HiddenField)pi.FindControl("hfToken")).Value;
                string posStr = ((DropDownList)pi.FindControl("ddlPos")).SelectedValue;
                int pos;
                if (!int.TryParse(posStr, out pos)) return false;
                if (!answer.ContainsKey(token) || answer[token] != pos) return false;
            }
            return true;
        }
        return false;
    }

    // ---------- Small data helpers ----------
    private void UpsertAnswer(int qid, int sid, string status)
    {
        const string sql =
            "IF EXISTS (SELECT 1 FROM StudentsAnswer WHERE QuestionID=@q AND StudentID=@s) " +
            "  UPDATE StudentsAnswer SET QuestionStatus=@st, AnsweredDate=GETDATE() WHERE QuestionID=@q AND StudentID=@s " +
            "ELSE " +
            "  INSERT INTO StudentsAnswer (QuestionID, StudentID, QuestionStatus, AnsweredDate) VALUES (@q,@s,@st,GETDATE())";
        using (SqlConnection cn = new SqlConnection(ConnStr))
        {
            cn.Open();
            // make sure the AnsweredDate column exists (used by the admin reports)
            using (SqlCommand ec = new SqlCommand(
                "IF COL_LENGTH('dbo.StudentsAnswer','AnsweredDate') IS NULL ALTER TABLE dbo.StudentsAnswer ADD AnsweredDate DATETIME NULL", cn))
                ec.ExecuteNonQuery();
            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                cmd.Parameters.AddWithValue("@q", qid);
                cmd.Parameters.AddWithValue("@s", sid);
                cmd.Parameters.AddWithValue("@st", status);
                cmd.ExecuteNonQuery();
            }
        }
    }

    private void AddPoints(int sid, int points)
    {
        using (SqlConnection cn = new SqlConnection(ConnStr))
        using (SqlCommand cmd = new SqlCommand(
            "UPDATE Student SET TotalPoint = TotalPoint + @p, TotalScore = TotalScore + @p WHERE StudentID = @s", cn))
        {
            cmd.Parameters.AddWithValue("@p", points);
            cmd.Parameters.AddWithValue("@s", sid);
            cn.Open();
            cmd.ExecuteNonQuery();
        }
    }

    private bool WasAnsweredCorrect(int qid, int sid)
    {
        object o = Scalar2("SELECT COUNT(*) FROM StudentsAnswer WHERE QuestionID=@q AND StudentID=@s AND QuestionStatus='Correct'", qid, sid);
        return Convert.ToInt32(o) > 0;
    }

    private int GetScore(int qid)
    {
        return Convert.ToInt32(Scalar("SELECT Score FROM Question WHERE QuestionID = @q", qid));
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

    private object Scalar2(string sql, int qid, int sid)
    {
        using (SqlConnection cn = new SqlConnection(ConnStr))
        using (SqlCommand cmd = new SqlCommand(sql, cn))
        {
            cmd.Parameters.AddWithValue("@q", qid);
            cmd.Parameters.AddWithValue("@s", sid);
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
        string wall = Wallpaper.GetEquipped(ConnStr, Convert.ToInt32(Session["StudentID"]));
        if (!string.IsNullOrEmpty(wall))
            pageBody.Style["background"] = wall;
    }

}