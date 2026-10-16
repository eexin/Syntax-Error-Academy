using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Text;
using System.Web;

// Web Application project (CodeBehind + designer file, no namespace).
// Read-only view of every APPROVED quiz, grouped by level, with answers revealed.
public partial class AdminViewQuestion : System.Web.UI.Page
{
    private string ConnStr
    {
        get { return ConfigurationManager.ConnectionStrings["SEAConnection"].ConnectionString; }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["AdminID"] == null) { Response.Redirect("Login.aspx"); return; }
        if (IsPostBack) return;

        litInitials.Text = GetInitials(Convert.ToString(Session["AdminName"]));

        bool daily = Request.QueryString["daily"] == "1";
        int quizId, level;
        bool hasQuiz = int.TryParse(Request.QueryString["quiz"], out quizId);
        bool hasLevel = int.TryParse(Request.QueryString["level"], out level);
        if (!hasLevel) level = FirstLevel();

        BuildRail(daily, hasQuiz ? LevelOfQuiz(quizId) : level, daily ? 0 : (hasQuiz ? LevelOfQuiz(quizId) : level), daily);

        if (daily) litMain.Text = RenderDaily();
        else if (hasQuiz) litMain.Text = RenderQuiz(quizId);
        else litMain.Text = RenderLevel(level);
    }

    // ---------- left rail ----------
    private void BuildRail(bool dailyActive, int levelForBack, int activeLevel, bool daily)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("<div class=\"sea-side-label\">LEVELS</div>");

        string dCls = daily ? "lvl active" : "lvl";
        sb.Append("<a href=\"AdminViewQuestion.aspx?daily=1\" class=\"").Append(dCls).Append("\">")
          .Append("<i class=\"ti ti-bolt\" aria-hidden=\"true\"></i><span>Daily Challenge</span></a>");

        DataTable dt = QueryTable("SELECT DISTINCT Level FROM Quiz WHERE QuizStatus NOT IN ('Pending','Rejected') ORDER BY Level");
        foreach (DataRow r in dt.Rows)
        {
            int lv = Convert.ToInt32(r["Level"]);
            string cls = (!daily && lv == activeLevel) ? "lvl active" : "lvl";
            sb.Append("<a href=\"AdminViewQuestion.aspx?level=").Append(lv).Append("\" class=\"").Append(cls).Append("\">")
              .Append("<i class=\"ti ti-stack-2\" aria-hidden=\"true\"></i><span>Level ").Append(lv).Append("</span></a>");
        }
        litRail.Text = sb.ToString();
    }

    // ---------- level view: cards ----------
    private string RenderLevel(int level)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("<h1 class=\"sea-h\">Level ").Append(level).Append("</h1>");

        DataTable dt = QueryTable(
            "SELECT QuizID, QuizTitle FROM Quiz WHERE Level=@lvl AND QuizStatus NOT IN ('Pending','Rejected') ORDER BY QuizID", "@lvl", level);

        sb.Append("<p class=\"sea-sub\">").Append(dt.Rows.Count).Append(" published quiz")
          .Append(dt.Rows.Count == 1 ? "" : "zes").Append(" in this level</p>");

        if (dt.Rows.Count == 0)
            return sb.Append("<div class=\"adm-empty\">No published quizzes in this level yet.</div>").ToString();

        sb.Append("<section class=\"sea-grid\">");
        foreach (DataRow r in dt.Rows)
        {
            int qid = Convert.ToInt32(r["QuizID"]);
            string title = HttpUtility.HtmlEncode(Convert.ToString(r["QuizTitle"]));
            sb.Append("<a class=\"sea-card\" href=\"AdminViewQuestion.aspx?quiz=").Append(qid).Append("\"><article>")
              .Append("<div class=\"sea-card-row\" style=\"justify-content:flex-end;\"><i class=\"ti ti-eye\" style=\"color:var(--coral);font-size:19px;\"></i></div>")
              .Append("<h3 class=\"sea-card-title\">").Append(title).Append("</h3>")
              .Append("<div class=\"sea-meta\"><b>#").Append(qid).Append("</b> quiz id</div>")
              .Append("</article></a>");
        }
        sb.Append("</section>");
        return sb.ToString();
    }

    // ---------- quiz view: questions + answers ----------
    private string RenderQuiz(int quizId)
    {
        string title = "", desc = ""; int level = FirstLevel();
        using (SqlConnection cn = new SqlConnection(ConnStr))
        using (SqlCommand cmd = new SqlCommand("SELECT QuizTitle, QuizDescription, Level FROM Quiz WHERE QuizID=@id", cn))
        {
            cmd.Parameters.AddWithValue("@id", quizId);
            cn.Open();
            using (SqlDataReader r = cmd.ExecuteReader())
                if (r.Read())
                {
                    title = Convert.ToString(r["QuizTitle"]);
                    desc = r["QuizDescription"] == DBNull.Value ? "" : Convert.ToString(r["QuizDescription"]);
                    level = Convert.ToInt32(r["Level"]);
                }
        }

        StringBuilder sb = new StringBuilder();
        sb.Append("<a class=\"quiz-back\" href=\"AdminViewQuestion.aspx?level=").Append(level).Append("\">")
          .Append("<i class=\"ti ti-arrow-left\"></i> Back to Level ").Append(level).Append("</a>");
        sb.Append("<h1 class=\"quiz-title\">").Append(HttpUtility.HtmlEncode(title))
          .Append(" <span style=\"color:var(--text-2);font-size:15px;\">#").Append(quizId).Append("</span></h1>");
        sb.Append("<p class=\"quiz-desc\">").Append(HttpUtility.HtmlEncode(desc)).Append("</p>");

        DataTable qs = QueryTable(
            "SELECT QuestionID, QuestionText, QuestionType FROM Question WHERE QuizID=@id ORDER BY QuestionID", "@id", quizId);

        if (qs.Rows.Count == 0)
            return sb.Append("<div class=\"adm-empty\">This quiz has no questions.</div>").ToString();

        int n = 1;
        foreach (DataRow q in qs.Rows)
        {
            int qid = Convert.ToInt32(q["QuestionID"]);
            string type = Convert.ToString(q["QuestionType"]);
            sb.Append("<div class=\"q-card\">")
              .Append("<div class=\"q-head\"><span class=\"q-num\">Q").Append(n++).Append("</span>")
              .Append("<span class=\"q-tag\">").Append(HttpUtility.HtmlEncode(type)).Append("</span></div>")
              .Append("<p class=\"q-text\">").Append(HttpUtility.HtmlEncode(Convert.ToString(q["QuestionText"]))).Append("</p>");
            sb.Append(RenderAnswer(qid, type));
            sb.Append("</div>");
        }

        // admin can remove any quiz at any time
        sb.Append("<div style=\"display:flex;justify-content:center;margin:26px 0 6px;\">")
          .Append("<button type=\"button\" class=\"adm-btn\" ")
          .Append("style=\"background:#e0573f;color:#fff;\" ")
          .Append("onclick=\"adminDelQuiz(").Append(quizId).Append(")\">")
          .Append("<i class=\"ti ti-trash\"></i> Delete Quiz</button></div>");

        return sb.ToString();
    }

    // ---------- daily challenge (date-seeded, same all day) ----------
    private string RenderDaily()
    {
        string seed = DateTime.Today.ToString("yyyyMMdd");
        int qid = 0; string type = "", text = "";
        using (SqlConnection cn = new SqlConnection(ConnStr))
        using (SqlCommand cmd = new SqlCommand(
            "SELECT TOP 1 q.QuestionID, q.QuestionText, q.QuestionType FROM Question q " +
            "JOIN Quiz z ON z.QuizID = q.QuizID AND z.QuizStatus NOT IN ('Pending','Rejected') " +
            "ORDER BY ABS(CHECKSUM(CONVERT(VARCHAR(20), q.QuestionID) + @seed))", cn))
        {
            cmd.Parameters.AddWithValue("@seed", seed);
            cn.Open();
            using (SqlDataReader r = cmd.ExecuteReader())
                if (r.Read()) { qid = Convert.ToInt32(r["QuestionID"]); text = Convert.ToString(r["QuestionText"]); type = Convert.ToString(r["QuestionType"]); }
        }

        StringBuilder sb = new StringBuilder();
        sb.Append("<h1 class=\"quiz-title\">Daily Challenge</h1>");
        sb.Append("<p class=\"quiz-desc\">Today's featured question, shown with its answer.</p>");
        if (qid == 0)
            return sb.Append("<div class=\"adm-empty\">No questions available yet.</div>").ToString();

        sb.Append("<div class=\"q-card\">")
          .Append("<div class=\"q-head\"><span class=\"q-num\"><i class=\"ti ti-bolt\"></i></span>")
          .Append("<span class=\"q-tag\">").Append(HttpUtility.HtmlEncode(type)).Append("</span></div>")
          .Append("<p class=\"q-text\">").Append(HttpUtility.HtmlEncode(text)).Append("</p>")
          .Append(RenderAnswer(qid, type))
          .Append("</div>");
        return sb.ToString();
    }

    // ---------- answer rendering per type ----------
    private string RenderAnswer(int qid, string type)
    {
        StringBuilder sb = new StringBuilder();

        if (type == "MCQ")
        {
            DataTable opts = QueryTable(
                "SELECT OptionText, IsCorrect FROM MCQOption WHERE QuestionID=@q ORDER BY McqID", "@q", qid);
            foreach (DataRow o in opts.Rows)
            {
                bool correct = Convert.ToBoolean(o["IsCorrect"]);
                string txt = HttpUtility.HtmlEncode(Convert.ToString(o["OptionText"]));
                if (correct)
                    sb.Append("<div class=\"adm-answer-opt correct\"><i class=\"ti ti-circle-check\"></i>")
                      .Append(txt).Append("</div>");
                else
                    sb.Append("<div class=\"adm-answer-opt\"><i class=\"ti ti-circle\" style=\"color:var(--text-2);\"></i>")
                      .Append(txt).Append("</div>");
            }
        }
        else if (type == "Essay")
        {
            object exp = Scalar("SELECT TOP 1 EssayTitle FROM EssayOption WHERE QuestionID=@q", qid);
            string model = (exp == null || exp == DBNull.Value) ? "(no model answer stored)" : Convert.ToString(exp);
            sb.Append("<div class=\"adm-answer-opt correct\"><i class=\"ti ti-writing\"></i>Model answer: ")
              .Append(HttpUtility.HtmlEncode(model)).Append("</div>");
        }
        else if (type == "Matching")
        {
            DataTable m = QueryTable("SELECT LeftItem, RightItem FROM MatchOption WHERE QuestionID=@q ORDER BY MatchID", "@q", qid);
            foreach (DataRow r in m.Rows)
                sb.Append("<div class=\"adm-answer-opt correct\"><i class=\"ti ti-arrow-right\"></i>")
                  .Append(HttpUtility.HtmlEncode(Convert.ToString(r["LeftItem"]))).Append(" &rarr; ")
                  .Append(HttpUtility.HtmlEncode(Convert.ToString(r["RightItem"]))).Append("</div>");
        }
        else if (type == "Puzzle")
        {
            DataTable p = QueryTable("SELECT TokenText, PositionOrder FROM PuzzleToken WHERE QuestionID=@q ORDER BY PositionOrder", "@q", qid);
            foreach (DataRow r in p.Rows)
                sb.Append("<div class=\"adm-answer-opt correct\"><i class=\"ti ti-number\"></i>")
                  .Append(Convert.ToString(r["PositionOrder"])).Append(". ")
                  .Append(HttpUtility.HtmlEncode(Convert.ToString(r["TokenText"]))).Append("</div>");
        }
        return sb.ToString();
    }

    // Removes the quiz and everything under it (questions, options, answers).
    protected void btnDeleteQuiz_Click(object sender, EventArgs e)
    {
        int quizId;
        if (!int.TryParse(hfDelQuiz.Value, out quizId)) return;

        int level = 1;
        using (SqlConnection cn0 = new SqlConnection(ConnStr))
        using (SqlCommand cmd0 = new SqlCommand("SELECT Level FROM Quiz WHERE QuizID=@id", cn0))
        {
            cmd0.Parameters.AddWithValue("@id", quizId);
            cn0.Open();
            object o = cmd0.ExecuteScalar();
            if (o != null && o != DBNull.Value) level = Convert.ToInt32(o);
        }

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
        Response.Redirect("AdminViewQuestion.aspx?level=" + level);
    }

    private void Exec(SqlConnection cn, SqlTransaction tx, string sql, int id)
    {
        using (SqlCommand cmd = new SqlCommand(sql, cn, tx)) { cmd.Parameters.AddWithValue("@id", id); cmd.ExecuteNonQuery(); }
    }
    private void TryExec(SqlConnection cn, SqlTransaction tx, string sql, int id)
    {
        try { Exec(cn, tx, sql, id); } catch { }
    }

    // ---------- helpers ----------
    private int FirstLevel()
    {
        object o = Scalar("SELECT MIN(Level) FROM Quiz WHERE QuizStatus NOT IN ('Pending','Rejected')", 0);
        return (o == null || o == DBNull.Value) ? 1 : Convert.ToInt32(o);
    }
    private int LevelOfQuiz(int quizId)
    {
        object o = Scalar("SELECT Level FROM Quiz WHERE QuizID=@q", quizId);
        return (o == null || o == DBNull.Value) ? FirstLevel() : Convert.ToInt32(o);
    }

    private DataTable QueryTable(string sql) { return QueryTable(sql, null, 0); }
    private DataTable QueryTable(string sql, string p, int v)
    {
        DataTable dt = new DataTable();
        using (SqlConnection cn = new SqlConnection(ConnStr))
        using (SqlCommand cmd = new SqlCommand(sql, cn))
        {
            if (p != null) cmd.Parameters.AddWithValue(p, v);
            using (SqlDataAdapter da = new SqlDataAdapter(cmd)) da.Fill(dt);
        }
        return dt;
    }
    private object Scalar(string sql, int qid)
    {
        using (SqlConnection cn = new SqlConnection(ConnStr))
        using (SqlCommand cmd = new SqlCommand(sql, cn))
        {
            if (sql.Contains("@q")) cmd.Parameters.AddWithValue("@q", qid);
            cn.Open();
            return cmd.ExecuteScalar();
        }
    }

    private string GetInitials(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "A";
        string[] parts = name.Trim().Split(' ');
        if (parts.Length == 1) return parts[0].Substring(0, 1).ToUpper();
        return (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpper();
    }
}
