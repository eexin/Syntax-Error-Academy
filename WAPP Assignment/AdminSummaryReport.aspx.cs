using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Globalization;
using System.Text;
using System.Web;

// Web Application project (CodeBehind + designer file, no namespace).
// Summary Report: aggregates EVERY quiz's answers over a chosen week / month / year.
public partial class AdminSummaryReport : System.Web.UI.Page
{
    private string ConnStr
    {
        get { return ConfigurationManager.ConnectionStrings["SEAConnection"].ConnectionString; }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["AdminID"] == null) { Response.Redirect("Login.aspx"); return; }
        EnsureAnsweredDate();
        if (!IsPostBack)
            litInitials.Text = GetInitials(Convert.ToString(Session["AdminName"]));
    }

    protected void btnGenerate_Click(object sender, EventArgs e)
    {
        DateTime start, end;
        if (!DateTime.TryParse(hfStart.Value, out start) || !DateTime.TryParse(hfEnd.Value, out end))
        {
            litReport.Text = "<div class=\"adm-empty\">Please complete all selections, then click Generate.</div>";
            return;
        }
        litReport.Text = BuildReport(hfType.Value, hfLabel.Value, start.Date, end.Date);
    }

    // period is inclusive [start, end]; queries use  >= start  AND  < end+1day
    private string BuildReport(string type, string label, DateTime start, DateTime end)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("<div class=\"rep-section-h\" style=\"margin-top:6px;\">")
          .Append(HttpUtility.HtmlEncode(string.IsNullOrEmpty(type) ? "Summary" : type)).Append(" Summary &mdash; ")
          .Append(HttpUtility.HtmlEncode(label)).Append("</div>");
        sb.Append("<p class=\"sea-sub\" style=\"margin-top:-4px;\">")
          .Append(start.ToString("dd MMM yyyy")).Append(" to ").Append(end.ToString("dd MMM yyyy")).Append("</p>");

        // ---- overall totals across ALL quizzes in the period ----
        int students = 0, answers = 0, correct = 0, quizzes = 0;
        using (SqlConnection cn = new SqlConnection(ConnStr))
        {
            cn.Open();
            using (SqlCommand cmd = new SqlCommand(
                "SELECT ISNULL(COUNT(DISTINCT sa.StudentID),0) S, ISNULL(COUNT(*),0) A, " +
                "ISNULL(SUM(CASE WHEN sa.QuestionStatus='Correct' THEN 1 ELSE 0 END),0) C, " +
                "ISNULL(COUNT(DISTINCT q.QuizID),0) Q " +
                "FROM StudentsAnswer sa JOIN Question q ON q.QuestionID=sa.QuestionID " +
                "WHERE sa.AnsweredDate >= @s AND sa.AnsweredDate < @e", cn))
            {
                cmd.Parameters.AddWithValue("@s", start);
                cmd.Parameters.AddWithValue("@e", end.AddDays(1));
                using (SqlDataReader r = cmd.ExecuteReader())
                    if (r.Read())
                    {
                        students = Convert.ToInt32(r["S"]); answers = Convert.ToInt32(r["A"]);
                        correct = Convert.ToInt32(r["C"]); quizzes = Convert.ToInt32(r["Q"]);
                    }
            }
        }
        int wrong = answers - correct;
        int okPct = answers > 0 ? (int)Math.Round(100.0 * correct / answers) : 0;
        int badPct = answers > 0 ? 100 - okPct : 0;

        if (answers == 0)
        {
            sb.Append("<div class=\"adm-empty\">No quiz activity was recorded in this period. ")
              .Append("(Daily figures only count answers made after answer-dates were enabled.)</div>");
            return sb.ToString();
        }

        // ---- stat cards ----
        sb.Append("<div class=\"rep-stats\">");
        sb.Append(Card(students.ToString(), "Students active", "coral"));
        sb.Append(Card(quizzes.ToString(), "Quizzes attempted", ""));
        sb.Append(Card(answers.ToString(), "Total answers", ""));
        sb.Append(Card(okPct + "%", "Correct rate", "ok"));
        sb.Append(Card(badPct + "%", "Wrong rate", "bad"));
        sb.Append("</div>");

        // ---- correct vs wrong split ----
        sb.Append("<div class=\"rep-section-h\">Correct vs Wrong</div>");
        sb.Append("<div class=\"rep-split\"><div class=\"seg-ok\" style=\"width:").Append(okPct)
          .Append("%\"></div><div class=\"seg-bad\" style=\"width:").Append(badPct).Append("%\"></div></div>");
        sb.Append("<div class=\"rep-split-legend\">")
          .Append("<span><span class=\"dot ok\"></span>Correct <b>").Append(correct).Append("</b> (").Append(okPct).Append("%)</span>")
          .Append("<span><span class=\"dot bad\"></span>Wrong <b>").Append(wrong).Append("</b> (").Append(badPct).Append("%)</span></div>");

        // ---- activity chart across the period ----
        sb.Append("<div class=\"rep-section-h\">Activity over the period</div>");
        sb.Append(RenderChart(type, start, end));

        // ---- per-quiz breakdown ----
        sb.Append("<div class=\"rep-section-h\">All quizzes in this period</div>");
        sb.Append(RenderQuizTable(start, end));

        return sb.ToString();
    }

    // buckets: Weekly = 7 days, Monthly = 7-day chunks, Yearly = 12 months
    private string RenderChart(string type, DateTime start, DateTime end)
    {
        var labels = new System.Collections.Generic.List<string>();
        var buckStart = new System.Collections.Generic.List<DateTime>();
        var buckEnd = new System.Collections.Generic.List<DateTime>();

        if (type == "Yearly")
        {
            for (int m = 1; m <= 12; m++)
            {
                DateTime s = new DateTime(start.Year, m, 1);
                labels.Add(CultureInfo.InvariantCulture.DateTimeFormat.GetAbbreviatedMonthName(m));
                buckStart.Add(s); buckEnd.Add(s.AddMonths(1).AddDays(-1));
            }
        }
        else if (type == "Monthly")
        {
            DateTime s = start; int wk = 1;
            while (s <= end)
            {
                DateTime e = s.AddDays(6); if (e > end) e = end;
                labels.Add("W" + wk + " (" + s.Day + "-" + e.Day + ")");
                buckStart.Add(s); buckEnd.Add(e); s = s.AddDays(7); wk++;
            }
        }
        else // Weekly -> 7 days
        {
            string[] dn = { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
            for (int i = 0; i < 7; i++)
            {
                DateTime d = start.AddDays(i);
                labels.Add(d.ToString("ddd"));
                buckStart.Add(d); buckEnd.Add(d);
            }
        }

        int[] vals = new int[labels.Count];
        int max = 1;
        for (int i = 0; i < labels.Count; i++)
        {
            vals[i] = DistinctStudents(buckStart[i], buckEnd[i]);
            if (vals[i] > max) max = vals[i];
        }

        StringBuilder sb = new StringBuilder("<div class=\"rep-chart\">");
        for (int i = 0; i < labels.Count; i++)
        {
            int h = (int)Math.Round(100.0 * vals[i] / max);
            if (vals[i] > 0 && h < 6) h = 6;
            sb.Append("<div class=\"rep-col\"><div class=\"rep-bar-num\">").Append(vals[i]).Append("</div>")
              .Append("<div class=\"rep-bar-fill\" style=\"height:").Append(h).Append("%\"></div>")
              .Append("<div class=\"rep-bar-day\">").Append(HttpUtility.HtmlEncode(labels[i])).Append("</div></div>");
        }
        return sb.Append("</div>").ToString();
    }

    private int DistinctStudents(DateTime s, DateTime e)
    {
        using (SqlConnection cn = new SqlConnection(ConnStr))
        using (SqlCommand cmd = new SqlCommand(
            "SELECT ISNULL(COUNT(DISTINCT sa.StudentID),0) FROM StudentsAnswer sa " +
            "WHERE sa.AnsweredDate >= @s AND sa.AnsweredDate < @e", cn))
        {
            cmd.Parameters.AddWithValue("@s", s.Date);
            cmd.Parameters.AddWithValue("@e", e.Date.AddDays(1));
            cn.Open();
            return Convert.ToInt32(cmd.ExecuteScalar());
        }
    }

    private string RenderQuizTable(DateTime start, DateTime end)
    {
        DataTable dt = new DataTable();
        using (SqlConnection cn = new SqlConnection(ConnStr))
        using (SqlCommand cmd = new SqlCommand(
            "SELECT z.QuizID, z.QuizTitle, " +
            "  COUNT(DISTINCT sa.StudentID) AS Students, COUNT(*) AS Answers, " +
            "  SUM(CASE WHEN sa.QuestionStatus='Correct' THEN 1 ELSE 0 END) AS Correct " +
            "FROM StudentsAnswer sa JOIN Question q ON q.QuestionID=sa.QuestionID " +
            "JOIN Quiz z ON z.QuizID=q.QuizID " +
            "WHERE sa.AnsweredDate >= @s AND sa.AnsweredDate < @e " +
            "GROUP BY z.QuizID, z.QuizTitle ORDER BY Answers DESC", cn))
        {
            cmd.Parameters.AddWithValue("@s", start);
            cmd.Parameters.AddWithValue("@e", end.AddDays(1));
            using (SqlDataAdapter da = new SqlDataAdapter(cmd)) da.Fill(dt);
        }

        if (dt.Rows.Count == 0) return "<div class=\"adm-empty\">No quizzes were answered in this period.</div>";

        StringBuilder sb = new StringBuilder("<div class=\"adm-table-wrap\"><table class=\"adm-table\"><thead><tr>");
        sb.Append("<th>Quiz ID</th><th>Quiz Name</th><th>Students</th><th>Answers</th><th>Correct %</th><th>Wrong %</th></tr></thead><tbody>");
        foreach (DataRow r in dt.Rows)
        {
            int ans = Convert.ToInt32(r["Answers"]);
            int cor = r["Correct"] == DBNull.Value ? 0 : Convert.ToInt32(r["Correct"]);
            int okPct = ans > 0 ? (int)Math.Round(100.0 * cor / ans) : 0;
            int badPct = ans > 0 ? 100 - okPct : 0;
            sb.Append("<tr><td>#").Append(r["QuizID"]).Append("</td><td>")
              .Append(HttpUtility.HtmlEncode(Convert.ToString(r["QuizTitle"]))).Append("</td>")
              .Append("<td>").Append(r["Students"]).Append("</td>")
              .Append("<td>").Append(ans).Append("</td>")
              .Append("<td style=\"color:var(--olive);font-weight:600;\">").Append(okPct).Append("%</td>")
              .Append("<td style=\"color:#e0573f;font-weight:600;\">").Append(badPct).Append("%</td></tr>");
        }
        return sb.Append("</tbody></table></div>").ToString();
    }

    private string Card(string val, string label, string cls)
    {
        return "<div class=\"rep-card\"><div class=\"rep-val " + cls + "\">" + HttpUtility.HtmlEncode(val) +
               "</div><div class=\"rep-label\">" + HttpUtility.HtmlEncode(label) + "</div></div>";
    }


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

    private string GetInitials(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "A";
        string[] parts = name.Trim().Split(' ');
        if (parts.Length == 1) return parts[0].Substring(0, 1).ToUpper();
        return (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpper();
    }
}
