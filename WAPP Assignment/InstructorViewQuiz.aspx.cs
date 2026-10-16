using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Text;
using System.Web;

// Web Application project (CodeBehind + designer file, no namespace).
public partial class InstructorViewQuiz : System.Web.UI.Page
{
    private string ConnStr
    {
        get { return ConfigurationManager.ConnectionStrings["SEAConnection"].ConnectionString; }
    }
    private int InstructorId { get { return Convert.ToInt32(Session["InstructorID"]); } }
    private int QuizId
    {
        get { int id; return int.TryParse(Request.QueryString["id"], out id) ? id : 0; }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["InstructorID"] == null) { Response.Redirect("Login.aspx"); return; }

        litNavAvatar.Text = NavAvatar();
        if (IsPostBack) return;

        // load quiz (must belong to this instructor)
        string title = null, desc = "", status = "", reason = "";
        using (SqlConnection cn = new SqlConnection(ConnStr))
        using (SqlCommand cmd = new SqlCommand(
            "SELECT QuizTitle, ISNULL(QuizDescription,'') AS D, QuizStatus, " +
            "ISNULL(RejectReason,'') AS RR FROM Quiz WHERE QuizID=@id AND InstructorID=@i", cn))
        {
            cmd.Parameters.AddWithValue("@id", QuizId);
            cmd.Parameters.AddWithValue("@i", InstructorId);
            cn.Open();
            using (SqlDataReader r = cmd.ExecuteReader())
                if (r.Read())
                {
                    title = Convert.ToString(r["QuizTitle"]); desc = Convert.ToString(r["D"]);
                    status = Convert.ToString(r["QuizStatus"]); reason = Convert.ToString(r["RR"]);
                }
        }
        if (title == null) { Response.Redirect("InstructorDashboard.aspx"); return; }

        // a rejected quiz shows the admin's reason above the title
        if (status.Trim().Equals("Rejected", StringComparison.OrdinalIgnoreCase))
        {
            pnlReject.Visible = true;
            litRejectReason.Text = (reason == "")
                ? "<i>No reason was provided.</i>"
                : HttpUtility.HtmlEncode(reason);
        }

        // equipped wallpaper carries across instructor pages
        string wall = InstructorTheme.GetWallpaper(ConnStr, InstructorId);
        if (!string.IsNullOrEmpty(wall)) pageBody.Style["background"] = wall;

        lblQuizTitle.Text = HttpUtility.HtmlEncode(title);
        lblQuizDesc.Text = HttpUtility.HtmlEncode(desc);
        litQuestions.Text = RenderQuestions(QuizId);
    }

    // ---------- tabs ----------
    protected void ShowQuestions(object sender, EventArgs e)
    {
        lnkQuestions.CssClass = "ins-tab active";
        lnkAnswers.CssClass = "ins-tab";
        pnlQuestions.Visible = true;
        pnlAnswers.Visible = false;
        litQuestions.Text = RenderQuestions(QuizId);
    }

    protected void ShowAnswers(object sender, EventArgs e)
    {
        lnkQuestions.CssClass = "ins-tab";
        lnkAnswers.CssClass = "ins-tab active";
        pnlQuestions.Visible = false;
        pnlAnswers.Visible = true;
        litAnswers.Text = RenderAttempts(QuizId);
    }

    // ---------- student attempts table ----------
    private string RenderAttempts(int quizId)
    {
        DataTable dt = new DataTable();
        using (SqlConnection cn = new SqlConnection(ConnStr))
        using (SqlCommand cmd = new SqlCommand(
            "SELECT s.StudentID, s.StudentName, " +
            "  MAX(sa.AnsweredDate) AS Attempted, " +
            "  SUM(CASE WHEN sa.QuestionStatus = 'Correct' THEN q.Score ELSE 0 END) AS Score " +
            "FROM StudentsAnswer sa " +
            "JOIN Question q ON q.QuestionID = sa.QuestionID " +
            "JOIN Student s ON s.StudentID = sa.StudentID " +
            "WHERE q.QuizID = @id " +
            "GROUP BY s.StudentID, s.StudentName ORDER BY s.StudentName", cn))
        {
            cmd.Parameters.AddWithValue("@id", quizId);
            using (SqlDataAdapter da = new SqlDataAdapter(cmd)) da.Fill(dt);
        }

        if (dt.Rows.Count == 0)
            return "<div class=\"adm-empty\" style=\"padding:30px;text-align:center;color:var(--text-2);\">No student has attempted this quiz yet.</div>";

        StringBuilder sb = new StringBuilder("<div class=\"ins-table-wrap\"><table class=\"ins-table\"><thead><tr>");
        sb.Append("<th>Student Name</th><th>Date Attempted</th><th>Score</th></tr></thead><tbody>");
        foreach (DataRow r in dt.Rows)
        {
            int sid = Convert.ToInt32(r["StudentID"]);
            string date = (r["Attempted"] == DBNull.Value)
                ? "&mdash;" : Convert.ToDateTime(r["Attempted"]).ToString("dd MMM yyyy");
            string score = (r["Score"] == DBNull.Value) ? "0" : Convert.ToString(r["Score"]);
            string url = "InstructorViewStudentAttempt.aspx?quiz=" + quizId + "&student=" + sid;
            sb.Append("<tr onclick=\"location.href='").Append(url).Append("'\">")
              .Append("<td>").Append(HttpUtility.HtmlEncode(Convert.ToString(r["StudentName"]))).Append("</td>")
              .Append("<td>").Append(date).Append("</td>")
              .Append("<td>").Append(score).Append(" pts</td></tr>");
        }
        return sb.Append("</tbody></table></div>").ToString();
    }

    private string RenderQuestions(int quizId)
    {
        DataTable qs = QueryTable(
            "SELECT QuestionID, QuestionText, QuestionType FROM Question WHERE QuizID=@id ORDER BY QuestionID", quizId);
        if (qs.Rows.Count == 0) return "<div class=\"adm-empty\">This quiz has no questions.</div>";

        StringBuilder sb = new StringBuilder();
        int n = 1;
        foreach (DataRow q in qs.Rows)
        {
            int qid = Convert.ToInt32(q["QuestionID"]);
            string type = Convert.ToString(q["QuestionType"]);
            sb.Append("<div class=\"q-card\"><div class=\"q-head\"><span class=\"q-num\">Q").Append(n++).Append("</span>")
              .Append("<span class=\"q-tag\">").Append(HttpUtility.HtmlEncode(type)).Append("</span></div>")
              .Append("<p class=\"q-text\">").Append(HttpUtility.HtmlEncode(Convert.ToString(q["QuestionText"]))).Append("</p>")
              .Append(RenderAnswer(qid, type)).Append("</div>");
        }
        return sb.ToString();
    }

    private string RenderAnswer(int qid, string type)
    {
        StringBuilder sb = new StringBuilder();
        if (type == "MCQ")
        {
            foreach (DataRow o in QueryTable("SELECT OptionText, IsCorrect FROM MCQOption WHERE QuestionID=@id ORDER BY McqID", qid).Rows)
            {
                bool correct = Convert.ToBoolean(o["IsCorrect"]);
                string txt = HttpUtility.HtmlEncode(Convert.ToString(o["OptionText"]));
                sb.Append(correct
                    ? "<div class=\"adm-answer-opt correct\"><i class=\"ti ti-circle-check\"></i>" + txt + "</div>"
                    : "<div class=\"adm-answer-opt\"><i class=\"ti ti-circle\" style=\"color:var(--text-2);\"></i>" + txt + "</div>");
            }
        }
        else if (type == "Essay")
        {
            object exp = ScalarId("SELECT TOP 1 EssayTitle FROM EssayOption WHERE QuestionID=@id", qid);
            string model = (exp == null || exp == DBNull.Value) ? "(no model answer)" : Convert.ToString(exp);
            sb.Append("<div class=\"adm-answer-opt correct\"><i class=\"ti ti-writing\"></i>Model answer: ").Append(HttpUtility.HtmlEncode(model)).Append("</div>");
        }
        else if (type == "Matching")
        {
            foreach (DataRow r in QueryTable("SELECT LeftItem, RightItem FROM MatchOption WHERE QuestionID=@id ORDER BY MatchID", qid).Rows)
                sb.Append("<div class=\"adm-answer-opt correct\"><i class=\"ti ti-arrow-right\"></i>")
                  .Append(HttpUtility.HtmlEncode(Convert.ToString(r["LeftItem"]))).Append(" &rarr; ")
                  .Append(HttpUtility.HtmlEncode(Convert.ToString(r["RightItem"]))).Append("</div>");
        }
        else if (type == "Puzzle")
        {
            foreach (DataRow r in QueryTable("SELECT TokenText, PositionOrder FROM PuzzleToken WHERE QuestionID=@id ORDER BY PositionOrder", qid).Rows)
                sb.Append("<div class=\"adm-answer-opt correct\"><i class=\"ti ti-number\"></i>")
                  .Append(Convert.ToString(r["PositionOrder"])).Append(". ")
                  .Append(HttpUtility.HtmlEncode(Convert.ToString(r["TokenText"]))).Append("</div>");
        }
        return sb.ToString();
    }

    // ---------- delete quiz + everything under it ----------
    protected void btnDelete_Click(object sender, EventArgs e)
    {
        if (Session["InstructorID"] == null) { Response.Redirect("Login.aspx"); return; }
        int quizId = QuizId;

        // make sure it belongs to this instructor
        object owner = ScalarId("SELECT InstructorID FROM Quiz WHERE QuizID=@id", quizId);
        if (owner == null || Convert.ToInt32(owner) != InstructorId) { Response.Redirect("InstructorDashboard.aspx"); return; }

        using (SqlConnection cn = new SqlConnection(ConnStr))
        {
            cn.Open();
            using (SqlTransaction tx = cn.BeginTransaction())
            {
                string questions = "(SELECT QuestionID FROM Question WHERE QuizID=@id)";
                Exec(cn, tx, "DELETE FROM StudentsAnswer WHERE QuestionID IN " + questions, quizId);
                Exec(cn, tx, "DELETE FROM MCQOption WHERE QuestionID IN " + questions, quizId);
                TryExec(cn, tx, "DELETE FROM MatchOption WHERE QuestionID IN " + questions, quizId);
                TryExec(cn, tx, "DELETE FROM PuzzleToken WHERE QuestionID IN " + questions, quizId);
                TryExec(cn, tx, "DELETE FROM EssayOption WHERE QuestionID IN " + questions, quizId);
                Exec(cn, tx, "DELETE FROM Question WHERE QuizID=@id", quizId);
                Exec(cn, tx, "DELETE FROM Quiz WHERE QuizID=@id", quizId);
                tx.Commit();
            }
        }
        Response.Redirect("InstructorDashboard.aspx");
    }

    // ---------- helpers ----------
    private void Exec(SqlConnection cn, SqlTransaction tx, string sql, int id)
    {
        using (SqlCommand cmd = new SqlCommand(sql, cn, tx)) { cmd.Parameters.AddWithValue("@id", id); cmd.ExecuteNonQuery(); }
    }
    private void TryExec(SqlConnection cn, SqlTransaction tx, string sql, int id)
    {
        try { Exec(cn, tx, sql, id); } catch { }
    }
    private DataTable QueryTable(string sql, int id)
    {
        DataTable dt = new DataTable();
        using (SqlConnection cn = new SqlConnection(ConnStr))
        using (SqlCommand cmd = new SqlCommand(sql, cn))
        {
            cmd.Parameters.AddWithValue("@id", id);
            using (SqlDataAdapter da = new SqlDataAdapter(cmd)) da.Fill(dt);
        }
        return dt;
    }
    private object ScalarId(string sql, int id)
    {
        using (SqlConnection cn = new SqlConnection(ConnStr))
        using (SqlCommand cmd = new SqlCommand(sql, cn))
        {
            cmd.Parameters.AddWithValue("@id", id);
            cn.Open();
            return cmd.ExecuteScalar();
        }
    }

    // instructor's equipped cosmetics -> avatar in the navbar
    private System.Collections.Generic.List<string> GetInsEquippedKeys()
    {
        System.Collections.Generic.List<string> keys = new System.Collections.Generic.List<string>();
        try
        {
            using (SqlConnection cn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT i.ItemPath FROM InstructorInventory inv JOIN Item i ON i.ItemID = inv.ItemID " +
                "WHERE inv.InstructorID=@i AND inv.EquipStatus='Equipped' AND i.ItemPath LIKE 'avatar:%'", cn))
            {
                cmd.Parameters.AddWithValue("@i", InstructorId);
                cn.Open();
                using (SqlDataReader r = cmd.ExecuteReader())
                    while (r.Read()) keys.Add(Convert.ToString(r[0]));
            }
        }
        catch { }
        return keys;
    }

    private string NavAvatar()
    {
        return Avatar.Render(GetInsEquippedKeys(), true);
    }

    private string GetInitials(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "I";
        string[] parts = name.Trim().Split(' ');
        if (parts.Length == 1) return parts[0].Substring(0, 1).ToUpper();
        return (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpper();
    }
}
