using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Text;
using System.Web;

// Web Application project (CodeBehind + designer file, no namespace).
// Shows how ONE student answered ONE quiz: each question is marked
// green when they answered correctly and red when they got it wrong.
public partial class InstructorViewStudentAttempt : System.Web.UI.Page
{
    private string ConnStr
    {
        get { return ConfigurationManager.ConnectionStrings["SEAConnection"].ConnectionString; }
    }
    private int InstructorId { get { return Convert.ToInt32(Session["InstructorID"]); } }
    private int QuizId { get { int i; return int.TryParse(Request.QueryString["quiz"], out i) ? i : 0; } }
    private int StudentId { get { int i; return int.TryParse(Request.QueryString["student"], out i) ? i : 0; } }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["InstructorID"] == null) { Response.Redirect("Login.aspx"); return; }
        if (IsPostBack) return;

        litNavAvatar.Text = NavAvatar();

        string wallBg = InstructorTheme.GetWallpaper(ConnStr, InstructorId);
        if (!string.IsNullOrEmpty(wallBg)) pageBody.Style["background"] = wallBg;

        // back to the quiz that was being viewed
        lnkBack.HRef = "InstructorViewQuiz.aspx?id=" + QuizId;

        // quiz must belong to this instructor
        string title = null;
        using (SqlConnection cn = new SqlConnection(ConnStr))
        using (SqlCommand cmd = new SqlCommand(
            "SELECT QuizTitle FROM Quiz WHERE QuizID=@id AND InstructorID=@i", cn))
        {
            cmd.Parameters.AddWithValue("@id", QuizId);
            cmd.Parameters.AddWithValue("@i", InstructorId);
            cn.Open();
            object o = cmd.ExecuteScalar();
            if (o != null && o != DBNull.Value) title = Convert.ToString(o);
        }
        if (title == null) { Response.Redirect("InstructorDashboard.aspx"); return; }

        string student = Convert.ToString(ScalarId("SELECT StudentName FROM Student WHERE StudentID=@id", StudentId));

        lblQuizTitle.Text = HttpUtility.HtmlEncode(title);
        litQuestions.Text = RenderAttempt(QuizId, StudentId, student);
    }

    private string RenderAttempt(int quizId, int studentId, string studentName)
    {
        DataTable qs = new DataTable();
        using (SqlConnection cn = new SqlConnection(ConnStr))
        using (SqlCommand cmd = new SqlCommand(
            "SELECT q.QuestionID, q.QuestionText, q.QuestionType, q.Score, " +
            "  ISNULL(sa.QuestionStatus,'') AS Status, sa.AnsweredDate " +
            "FROM Question q " +
            "LEFT JOIN StudentsAnswer sa ON sa.QuestionID = q.QuestionID AND sa.StudentID = @s " +
            "WHERE q.QuizID = @id ORDER BY q.QuestionID", cn))
        {
            cmd.Parameters.AddWithValue("@id", quizId);
            cmd.Parameters.AddWithValue("@s", studentId);
            using (SqlDataAdapter da = new SqlDataAdapter(cmd)) da.Fill(qs);
        }

        int correct = 0, answered = 0, score = 0;
        foreach (DataRow r in qs.Rows)
        {
            string st = Convert.ToString(r["Status"]);
            if (st != "") answered++;
            if (st == "Correct") { correct++; score += Convert.ToInt32(r["Score"]); }
        }

        lblSub.Text = HttpUtility.HtmlEncode(studentName) + " &mdash; " + correct + " / " + qs.Rows.Count +
                      " correct &middot; " + score + " pts";

        if (qs.Rows.Count == 0) return "<div class=\"adm-empty\">This quiz has no questions.</div>";

        StringBuilder sb = new StringBuilder();
        int n = 1;
        foreach (DataRow q in qs.Rows)
        {
            int qid = Convert.ToInt32(q["QuestionID"]);
            string type = Convert.ToString(q["QuestionType"]);
            string status = Convert.ToString(q["Status"]);

            string chip = (status == "Correct")
                ? "<span class=\"ins-result correct\"><i class=\"ti ti-check\"></i> Correct</span>"
                : (status == "Wrong")
                    ? "<span class=\"ins-result wrong\"><i class=\"ti ti-x\"></i> Wrong</span>"
                    : "<span class=\"ins-result none\">Not answered</span>";

            sb.Append("<div class=\"q-card\">")
              .Append("<div class=\"q-head\"><span class=\"q-num\">Q").Append(n++).Append("</span>")
              .Append("<span class=\"q-tag\">").Append(HttpUtility.HtmlEncode(type)).Append("</span>")
              .Append("<span style=\"margin-left:auto;\">").Append(chip).Append("</span></div>")
              .Append("<p class=\"q-text\">").Append(HttpUtility.HtmlEncode(Convert.ToString(q["QuestionText"]))).Append("</p>")
              .Append(RenderAnswer(qid, type, status))
              .Append("</div>");
        }
        return sb.ToString();
    }

    // The database records whether the student was right or wrong (not which option
    // they picked), so the answer is shown green when they got it right and red when
    // they got it wrong.
    private string RenderAnswer(int qid, string type, string status)
    {
        string cls = (status == "Correct") ? "correct" : (status == "Wrong") ? "wrong" : "";
        string icon = (status == "Correct") ? "ti-circle-check" : (status == "Wrong") ? "ti-circle-x" : "ti-circle";

        StringBuilder sb = new StringBuilder();

        if (type == "MCQ")
        {
            foreach (DataRow o in QueryTable("SELECT OptionText, IsCorrect FROM MCQOption WHERE QuestionID=@id ORDER BY McqID", qid).Rows)
            {
                bool isAnswer = Convert.ToBoolean(o["IsCorrect"]);
                string txt = HttpUtility.HtmlEncode(Convert.ToString(o["OptionText"]));
                if (isAnswer)
                    sb.Append("<div class=\"ins-ans ").Append(cls).Append("\"><i class=\"ti ").Append(icon).Append("\"></i>")
                      .Append(txt).Append("</div>");
                else
                    sb.Append("<div class=\"ins-ans\" style=\"color:var(--text-2);\"><i class=\"ti ti-circle\"></i>")
                      .Append(txt).Append("</div>");
            }
        }
        else if (type == "Matching")
        {
            foreach (DataRow r in QueryTable("SELECT LeftItem, RightItem FROM MatchOption WHERE QuestionID=@id ORDER BY MatchID", qid).Rows)
                sb.Append("<div class=\"ins-ans ").Append(cls).Append("\"><i class=\"ti ").Append(icon).Append("\"></i>")
                  .Append(HttpUtility.HtmlEncode(Convert.ToString(r["LeftItem"]))).Append(" &rarr; ")
                  .Append(HttpUtility.HtmlEncode(Convert.ToString(r["RightItem"]))).Append("</div>");
        }
        else if (type == "Puzzle")
        {
            foreach (DataRow r in QueryTable("SELECT TokenText, PositionOrder FROM PuzzleToken WHERE QuestionID=@id ORDER BY PositionOrder", qid).Rows)
                sb.Append("<div class=\"ins-ans ").Append(cls).Append("\"><i class=\"ti ").Append(icon).Append("\"></i>")
                  .Append(Convert.ToString(r["PositionOrder"])).Append(". ")
                  .Append(HttpUtility.HtmlEncode(Convert.ToString(r["TokenText"]))).Append("</div>");
        }
        else // Essay - free text; only the result is recorded
        {
            string msg = (status == "Correct") ? "Answered correctly"
                       : (status == "Wrong") ? "Answered incorrectly"
                       : "Not answered yet";
            sb.Append("<div class=\"ins-ans ").Append(cls).Append("\"><i class=\"ti ").Append(icon).Append("\"></i>")
              .Append(msg).Append("</div>");
        }
        return sb.ToString();
    }

    // ---------- helpers ----------
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
}
