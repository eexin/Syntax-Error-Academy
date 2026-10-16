using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Text;
using System.Web;

// Web Application project (CodeBehind + designer file, no namespace).
// Quiz Report: pick an approved quiz, see who answered it (today / last 7 days),
// overall correct-vs-wrong, and a per-question breakdown.
public partial class AdminQuizReport : System.Web.UI.Page
{
    private string ConnStr
    {
        get { return ConfigurationManager.ConnectionStrings["SEAConnection"].ConnectionString; }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["AdminID"] == null) { Response.Redirect("Login.aspx"); return; }
        if (IsPostBack) return;

        EnsureAnsweredDate();
        litInitials.Text = GetInitials(Convert.ToString(Session["AdminName"]));

        int quizId, level, day;
        bool hasQuiz = int.TryParse(Request.QueryString["quiz"], out quizId);
        bool hasLevel = int.TryParse(Request.QueryString["level"], out level);
        if (!int.TryParse(Request.QueryString["day"], out day)) day = 0;
        if (day < 0) day = 0; if (day > 6) day = 6;
        if (!hasLevel) level = FirstLevel();

        int activeLevel = hasQuiz ? LevelOfQuiz(quizId) : level;
        BuildRail(activeLevel);

        litMain.Text = hasQuiz ? RenderReport(quizId, day) : RenderPicker(level);
    }

    // ---------- left rail (levels) ----------
    private void BuildRail(int activeLevel)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("<div class=\"sea-side-label\">LEVELS</div>");
        foreach (DataRow r in QueryTable("SELECT DISTINCT Level FROM Quiz WHERE QuizStatus NOT IN ('Pending','Rejected') ORDER BY Level").Rows)
        {
            int lv = Convert.ToInt32(r["Level"]);
            string cls = (lv == activeLevel) ? "lvl active" : "lvl";
            sb.Append("<a href=\"AdminQuizReport.aspx?level=").Append(lv).Append("\" class=\"").Append(cls).Append("\">")
              .Append("<i class=\"ti ti-stack-2\" aria-hidden=\"true\"></i><span>Level ").Append(lv).Append("</span></a>");
        }
        litRail.Text = sb.ToString();
    }

    // ---------- quiz picker (cards, like View Question) ----------
    private string RenderPicker(int level)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("<h1 class=\"sea-h\">Quiz Report</h1>");
        DataTable dt = QueryTable("SELECT QuizID, QuizTitle FROM Quiz WHERE Level=@lvl AND QuizStatus NOT IN ('Pending','Rejected') ORDER BY QuizID", "@lvl", level);
        sb.Append("<p class=\"sea-sub\">Pick a quiz in Level ").Append(level).Append(" to see its report.</p>");
        if (dt.Rows.Count == 0)
            return sb.Append("<div class=\"adm-empty\">No published quizzes in this level.</div>").ToString();

        sb.Append("<section class=\"sea-grid\">");
        foreach (DataRow r in dt.Rows)
        {
            int qid = Convert.ToInt32(r["QuizID"]);
            string title = HttpUtility.HtmlEncode(Convert.ToString(r["QuizTitle"]));
            sb.Append("<a class=\"sea-card\" href=\"AdminQuizReport.aspx?quiz=").Append(qid).Append("\"><article>")
              .Append("<div class=\"sea-card-row\" style=\"justify-content:flex-end;\"><i class=\"ti ti-chart-bar\" style=\"color:var(--coral);font-size:19px;\"></i></div>")
              .Append("<h3 class=\"sea-card-title\">").Append(title).Append("</h3>")
              .Append("<div class=\"sea-meta\"><b>#").Append(qid).Append("</b> quiz id</div>")
              .Append("</article></a>");
        }
        return sb.Append("</section>").ToString();
    }

    // ---------- the report for one quiz ----------
    private string RenderReport(int quizId, int dayOffset)
    {
        string title = ""; int level = FirstLevel();
        using (SqlConnection cn = new SqlConnection(ConnStr))
        using (SqlCommand cmd = new SqlCommand("SELECT QuizTitle, Level FROM Quiz WHERE QuizID=@id", cn))
        {
            cmd.Parameters.AddWithValue("@id", quizId);
            cn.Open();
            using (SqlDataReader r = cmd.ExecuteReader())
                if (r.Read()) { title = Convert.ToString(r["QuizTitle"]); level = Convert.ToInt32(r["Level"]); }
        }

        StringBuilder sb = new StringBuilder();
        sb.Append("<a class=\"quiz-back\" href=\"AdminQuizReport.aspx?level=").Append(level).Append("\">")
          .Append("<i class=\"ti ti-arrow-left\"></i> Back to Level ").Append(level).Append("</a>");
        sb.Append("<h1 class=\"quiz-title\">").Append(HttpUtility.HtmlEncode(title))
          .Append(" <span style=\"color:var(--text-2);font-size:15px;\">#").Append(quizId).Append("</span></h1>");
        sb.Append("<p class=\"quiz-desc\">Quiz performance report.</p>");

        int qCount = ScalarInt("SELECT COUNT(*) FROM Question WHERE QuizID=@id", quizId);

        // ---- all-time totals ----
        int totStudents, totAnswers, totCorrect;
        DayStats(quizId, null, out totStudents, out totAnswers, out totCorrect);
        int totWrong = totAnswers - totCorrect;
        int okPct = totAnswers > 0 ? (int)Math.Round(100.0 * totCorrect / totAnswers) : 0;
        int badPct = totAnswers > 0 ? 100 - okPct : 0;

        // ---- selected-day totals ----
        DateTime selDate = DateTime.Today.AddDays(-dayOffset);
        int dStudents, dAnswers, dCorrect;
        DayStats(quizId, selDate, out dStudents, out dAnswers, out dCorrect);
        int dOkPct = dAnswers > 0 ? (int)Math.Round(100.0 * dCorrect / dAnswers) : 0;

        // ===== top stat cards =====
        sb.Append("<div class=\"rep-stats\">");
        sb.Append(StatCard(dStudents.ToString(), (dayOffset == 0 ? "Answered today" : "Answered that day"), "coral"));
        sb.Append(StatCard(totStudents.ToString(), "Total students (all time)", ""));
        sb.Append(StatCard(okPct + "%", "Correct rate", "ok"));
        sb.Append(StatCard(badPct + "%", "Wrong rate", "bad"));
        sb.Append(StatCard(qCount.ToString(), "Questions", ""));
        sb.Append(StatCard(totAnswers.ToString(), "Total answers", ""));
        sb.Append("</div>");

        // ===== correct vs wrong split =====
        sb.Append("<div class=\"rep-section-h\">Correct vs Wrong (all answers)</div>");
        sb.Append("<div class=\"rep-split\">")
          .Append("<div class=\"seg-ok\" style=\"width:").Append(okPct).Append("%\"></div>")
          .Append("<div class=\"seg-bad\" style=\"width:").Append(badPct).Append("%\"></div></div>");
        sb.Append("<div class=\"rep-split-legend\">")
          .Append("<span><span class=\"dot ok\"></span>Correct <b>").Append(totCorrect).Append("</b> (").Append(okPct).Append("%)</span>")
          .Append("<span><span class=\"dot bad\"></span>Wrong <b>").Append(totWrong).Append("</b> (").Append(badPct).Append("%)</span>")
          .Append("</div>");

        // ===== day selector (last 7 days) =====
        sb.Append("<div class=\"rep-section-h\">View a day (last 7 days)</div>");
        sb.Append("<div class=\"rep-days\">");
        for (int off = 0; off <= 6; off++)
        {
            DateTime d = DateTime.Today.AddDays(-off);
            string label = off == 0 ? "Today" : (off == 1 ? "Yesterday" : d.ToString("ddd"));
            string cls = off == dayOffset ? "rep-day active" : "rep-day";
            sb.Append("<a class=\"").Append(cls).Append("\" href=\"AdminQuizReport.aspx?quiz=").Append(quizId).Append("&day=").Append(off).Append("\">")
              .Append(label).Append("<small>").Append(d.ToString("dd MMM")).Append("</small></a>");
        }
        sb.Append("</div>");

        // selected-day summary line
        sb.Append("<p class=\"sea-sub\" style=\"margin-top:-6px;\">On <b>").Append(selDate.ToString("dd MMM yyyy"))
          .Append("</b>: ").Append(dStudents).Append(" student(s) answered, ").Append(dAnswers)
          .Append(" answer(s), ").Append(dOkPct).Append("% correct.</p>");

        // ===== 7-day chart (distinct students per day) =====
        sb.Append("<div class=\"rep-section-h\">Students answered per day</div>");
        sb.Append(RenderChart(quizId, dayOffset));

        // ===== per-question breakdown =====
        sb.Append("<div class=\"rep-section-h\">Question breakdown</div>");
        sb.Append(RenderQuestionTable(quizId));

        return sb.ToString();
    }

    // students / answers / correct for a quiz, optionally on a single day (null = all time)
    private void DayStats(int quizId, DateTime? day, out int students, out int answers, out int correct)
    {
        string where = "q.QuizID=@id";
        if (day.HasValue) where += " AND CAST(sa.AnsweredDate AS DATE)=@d";
        string sql =
            "SELECT ISNULL(COUNT(DISTINCT sa.StudentID),0) AS S, ISNULL(COUNT(*),0) AS A, " +
            "ISNULL(SUM(CASE WHEN sa.QuestionStatus='Correct' THEN 1 ELSE 0 END),0) AS C " +
            "FROM StudentsAnswer sa JOIN Question q ON q.QuestionID=sa.QuestionID WHERE " + where;
        students = answers = correct = 0;
        using (SqlConnection cn = new SqlConnection(ConnStr))
        using (SqlCommand cmd = new SqlCommand(sql, cn))
        {
            cmd.Parameters.AddWithValue("@id", quizId);
            if (day.HasValue) cmd.Parameters.AddWithValue("@d", day.Value.Date);
            cn.Open();
            using (SqlDataReader r = cmd.ExecuteReader())
                if (r.Read()) { students = Convert.ToInt32(r["S"]); answers = Convert.ToInt32(r["A"]); correct = Convert.ToInt32(r["C"]); }
        }
    }

    private string RenderChart(int quizId, int activeOffset)
    {
        // distinct students per calendar day, last 7 days
        int[] counts = new int[7];
        DataTable dt = QueryTable(
            "SELECT CAST(sa.AnsweredDate AS DATE) AS D, COUNT(DISTINCT sa.StudentID) AS S " +
            "FROM StudentsAnswer sa JOIN Question q ON q.QuestionID=sa.QuestionID " +
            "WHERE q.QuizID=@id AND sa.AnsweredDate >= CAST(DATEADD(DAY,-6,CAST(GETDATE() AS DATE)) AS DATETIME) " +
            "GROUP BY CAST(sa.AnsweredDate AS DATE)", "@id", quizId);
        foreach (DataRow r in dt.Rows)
        {
            if (r["D"] == DBNull.Value) continue;
            int off = (DateTime.Today - Convert.ToDateTime(r["D"]).Date).Days;
            if (off >= 0 && off <= 6) counts[off] = Convert.ToInt32(r["S"]);
        }
        int max = 1; foreach (int c in counts) if (c > max) max = c;

        StringBuilder sb = new StringBuilder("<div class=\"rep-chart\">");
        // oldest (off=6) on the left, today (off=0) on the right
        for (int off = 6; off >= 0; off--)
        {
            DateTime d = DateTime.Today.AddDays(-off);
            int h = (int)Math.Round(100.0 * counts[off] / max);
            if (counts[off] > 0 && h < 6) h = 6;
            string todayCls = off == 0 ? " today" : "";
            sb.Append("<div class=\"rep-col").Append(todayCls).Append("\">")
              .Append("<div class=\"rep-bar-num\">").Append(counts[off]).Append("</div>")
              .Append("<div class=\"rep-bar-fill\" style=\"height:").Append(h).Append("%\"></div>")
              .Append("<div class=\"rep-bar-day\">").Append(off == 0 ? "Today" : d.ToString("ddd")).Append("</div>")
              .Append("</div>");
        }
        return sb.Append("</div>").ToString();
    }

    private string RenderQuestionTable(int quizId)
    {
        DataTable dt = QueryTable(
            "SELECT q.QuestionID, q.QuestionText, " +
            "  COUNT(sa.StudentID) AS Attempts, " +
            "  SUM(CASE WHEN sa.QuestionStatus='Correct' THEN 1 ELSE 0 END) AS Correct " +
            "FROM Question q LEFT JOIN StudentsAnswer sa ON sa.QuestionID=q.QuestionID " +
            "WHERE q.QuizID=@id GROUP BY q.QuestionID, q.QuestionText ORDER BY q.QuestionID", "@id", quizId);

        if (dt.Rows.Count == 0) return "<div class=\"adm-empty\">This quiz has no questions.</div>";

        StringBuilder sb = new StringBuilder("<div class=\"adm-table-wrap\"><table class=\"adm-table\"><thead><tr>");
        sb.Append("<th>#</th><th>Question</th><th>Attempts</th><th>Correct</th><th>Correct %</th><th>Wrong %</th></tr></thead><tbody>");
        int n = 1;
        foreach (DataRow r in dt.Rows)
        {
            int attempts = Convert.ToInt32(r["Attempts"]);
            int correct = r["Correct"] == DBNull.Value ? 0 : Convert.ToInt32(r["Correct"]);
            int okPct = attempts > 0 ? (int)Math.Round(100.0 * correct / attempts) : 0;
            int badPct = attempts > 0 ? 100 - okPct : 0;
            string text = HttpUtility.HtmlEncode(Convert.ToString(r["QuestionText"]));
            if (text.Length > 70) text = text.Substring(0, 70) + "…";
            sb.Append("<tr><td>Q").Append(n++).Append("</td><td>").Append(text).Append("</td>")
              .Append("<td>").Append(attempts).Append("</td>")
              .Append("<td>").Append(correct).Append("</td>")
              .Append("<td style=\"color:var(--olive);font-weight:600;\">").Append(attempts > 0 ? okPct + "%" : "—").Append("</td>")
              .Append("<td style=\"color:#e0573f;font-weight:600;\">").Append(attempts > 0 ? badPct + "%" : "—").Append("</td></tr>");
        }
        return sb.Append("</tbody></table></div>").ToString();
    }

    private string StatCard(string val, string label, string cls)
    {
        return "<div class=\"rep-card\"><div class=\"rep-val " + cls + "\">" +
               HttpUtility.HtmlEncode(val) + "</div><div class=\"rep-label\">" +
               HttpUtility.HtmlEncode(label) + "</div></div>";
    }

    // ---------- helpers ----------

    // The report needs a date on each answer. Add the column if the DB lacks it.
    private void EnsureAnsweredDate()
    {
        try
        {
            using (SqlConnection cn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(
                "IF COL_LENGTH('dbo.StudentsAnswer','AnsweredDate') IS NULL " +
                "ALTER TABLE dbo.StudentsAnswer ADD AnsweredDate DATETIME NULL", cn))
            { cn.Open(); cmd.ExecuteNonQuery(); }
        }
        catch { }
    }

    private int FirstLevel()
    {
        object o = ScalarObj("SELECT MIN(Level) FROM Quiz WHERE QuizStatus NOT IN ('Pending','Rejected')");
        return (o == null || o == DBNull.Value) ? 1 : Convert.ToInt32(o);
    }
    private int LevelOfQuiz(int quizId)
    {
        object o = ScalarObj("SELECT Level FROM Quiz WHERE QuizID=" + quizId);
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
    private int ScalarInt(string sql, int id)
    {
        using (SqlConnection cn = new SqlConnection(ConnStr))
        using (SqlCommand cmd = new SqlCommand(sql, cn))
        {
            cmd.Parameters.AddWithValue("@id", id);
            cn.Open();
            object o = cmd.ExecuteScalar();
            return (o == null || o == DBNull.Value) ? 0 : Convert.ToInt32(o);
        }
    }
    private object ScalarObj(string sql)
    {
        using (SqlConnection cn = new SqlConnection(ConnStr))
        using (SqlCommand cmd = new SqlCommand(sql, cn))
        {
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
