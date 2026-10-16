using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Text;
using System.Web;

// Web Application project (CodeBehind + designer file, no namespace).
public partial class AdminQuizApproval : System.Web.UI.Page
{
    private string ConnStr
    {
        get { return ConfigurationManager.ConnectionStrings["SEAConnection"].ConnectionString; }
    }

    // used by the .aspx data-binding expression
    protected bool IsPending { get { return hfTab.Value == "Pending"; } }

    protected string RowActions(object quizId)
    {
        string id = Convert.ToString(quizId);
        return
            "<button type=\"button\" class=\"adm-btn ok\" style=\"padding:6px 12px;\" onclick=\"openApprove(" + id + ")\"><i class=\"ti ti-check\"></i> Approve</button> " +
            "<button type=\"button\" class=\"adm-btn danger\" style=\"padding:6px 12px;\" onclick=\"openReject(" + id + ")\"><i class=\"ti ti-x\"></i> Reject</button>";
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["AdminID"] == null) { Response.Redirect("Login.aspx"); return; }
        if (!IsPostBack)
        {
            EnsureSchema();
            litInitials.Text = GetInitials(Convert.ToString(Session["AdminName"]));
            hfTab.Value = "Pending";
            Bind();
        }
    }

    // The reject feature stores a reason on the quiz. Add the column the first
    // time this page is opened if the database doesn't have it yet.
    private void EnsureSchema()
    {
        try
        {
            using (SqlConnection cn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(
                "IF COL_LENGTH('dbo.Quiz','RejectReason') IS NULL " +
                "ALTER TABLE dbo.Quiz ADD RejectReason NVARCHAR(500) NULL; " +
                // every reject reason ever given is kept in this history table
                "IF OBJECT_ID('dbo.QuizRejectLog','U') IS NULL " +
                "CREATE TABLE dbo.QuizRejectLog (" +
                "  LogID INT IDENTITY(1,1) PRIMARY KEY," +
                "  QuizID INT NOT NULL," +
                "  QuizTitle NVARCHAR(200) NULL," +
                "  InstructorID INT NULL," +
                "  RejectReason NVARCHAR(500) NULL," +
                "  RejectedDate DATETIME NOT NULL DEFAULT GETDATE())", cn))
            {
                cn.Open();
                cmd.ExecuteNonQuery();
            }
        }
        catch { /* if it can't alter, approve still works; reject reason just won't store */ }
    }

    protected void ShowPending(object sender, EventArgs e) { hfTab.Value = "Pending"; hfSelectedQuiz.Value = ""; Bind(); }
    protected void ShowApproved(object sender, EventArgs e) { hfTab.Value = "Approved"; hfSelectedQuiz.Value = ""; Bind(); }
    protected void ShowRejected(object sender, EventArgs e) { hfTab.Value = "Rejected"; hfSelectedQuiz.Value = ""; Bind(); }

    private void Bind()
    {
        lnkPending.CssClass = "adm-tab" + (hfTab.Value == "Pending" ? " active" : "");
        lnkApproved.CssClass = "adm-tab" + (hfTab.Value == "Approved" ? " active" : "");
        lnkRejected.CssClass = "adm-tab" + (hfTab.Value == "Rejected" ? " active" : "");

        DataTable dt = new DataTable();
        using (SqlConnection cn = new SqlConnection(ConnStr))
        using (SqlCommand cmd = new SqlCommand(
            "SELECT z.QuizID, z.QuizTitle, ISNULL(i.InstructorName, '-') AS Instructor, CASE WHEN z.QuizStatus NOT IN ('Pending','Rejected') THEN 'Approved' ELSE z.QuizStatus END AS Status " +
            "FROM Quiz z LEFT JOIN Instructor i ON i.InstructorID = z.InstructorID " +
            (hfTab.Value == "Approved"
                ? "WHERE z.QuizStatus NOT IN ('Pending','Rejected') ORDER BY z.QuizID"
                : "WHERE z.QuizStatus = @st ORDER BY z.QuizID"), cn))
        {
            if (hfTab.Value != "Approved") cmd.Parameters.AddWithValue("@st", hfTab.Value);
            using (SqlDataAdapter da = new SqlDataAdapter(cmd)) da.Fill(dt);
        }
        rptQuizzes.DataSource = dt;
        rptQuizzes.DataBind();
        pnlEmpty.Visible = (dt.Rows.Count == 0);
    }

    // ---------- approve ----------
    protected void btnDoApprove_Click(object sender, EventArgs e)
    {
        int id;
        if (int.TryParse(hfTargetQuiz.Value, out id))
        {
            Update("UPDATE Quiz SET QuizStatus='Approved', RejectReason=NULL WHERE QuizID=@id", id, null);
            Flash("You have approved the quiz successfully.");
        }
        Bind();
    }

    // ---------- reject ----------
    protected void btnDoReject_Click(object sender, EventArgs e)
    {
        int id;
        if (int.TryParse(hfTargetQuiz.Value, out id))
        {
            string reason = txtRejectReason.Text.Trim();
            Update("UPDATE Quiz SET QuizStatus='Rejected', RejectReason=@r WHERE QuizID=@id", id, reason);
            LogReject(id, reason);
            Flash("You have rejected the quiz successfully.");
        }
        Bind();
    }

    // #3: keep EVERY reject reason in QuizRejectLog (the Quiz row only holds the latest).
    private void LogReject(int quizId, string reason)
    {
        try
        {
            using (SqlConnection cn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(
                "INSERT INTO QuizRejectLog (QuizID, QuizTitle, InstructorID, RejectReason) " +
                "SELECT z.QuizID, z.QuizTitle, z.InstructorID, @r FROM Quiz z WHERE z.QuizID=@id", cn))
            {
                cmd.Parameters.AddWithValue("@id", quizId);
                cmd.Parameters.AddWithValue("@r", (object)reason ?? DBNull.Value);
                cn.Open();
                cmd.ExecuteNonQuery();
            }
        }
        catch { }
    }

    private void Update(string sql, int id, string reason)
    {
        using (SqlConnection cn = new SqlConnection(ConnStr))
        using (SqlCommand cmd = new SqlCommand(sql, cn))
        {
            cmd.Parameters.AddWithValue("@id", id);
            if (reason != null) cmd.Parameters.AddWithValue("@r", reason);
            cn.Open();
            cmd.ExecuteNonQuery();
        }
    }

    // ---------- view (read-only) ----------
    protected void btnDoView_Click(object sender, EventArgs e)
    {
        int id;
        if (!int.TryParse(hfSelectedQuiz.Value, out id)) { Bind(); return; }

        string title = "", desc = "";
        using (SqlConnection cn = new SqlConnection(ConnStr))
        using (SqlCommand cmd = new SqlCommand("SELECT QuizTitle, QuizDescription FROM Quiz WHERE QuizID=@id", cn))
        {
            cmd.Parameters.AddWithValue("@id", id);
            cn.Open();
            using (SqlDataReader r = cmd.ExecuteReader())
                if (r.Read())
                {
                    title = Convert.ToString(r["QuizTitle"]);
                    desc = r["QuizDescription"] == DBNull.Value ? "" : Convert.ToString(r["QuizDescription"]);
                }
        }

        litViewTitle.Text = HttpUtility.HtmlEncode(title) + " <span style='color:var(--text-2);font-size:14px;'>#" + id + "</span>";

        StringBuilder sb = new StringBuilder();
        if (!string.IsNullOrEmpty(desc))
            sb.Append("<p class=\"quiz-desc\" style=\"margin-top:0;\">").Append(HttpUtility.HtmlEncode(desc)).Append("</p>");

        DataTable qs = QueryTable("SELECT QuestionID, QuestionText, QuestionType FROM Question WHERE QuizID=@id ORDER BY QuestionID", id);
        if (qs.Rows.Count == 0) sb.Append("<div class=\"adm-empty\">This quiz has no questions.</div>");
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
        litView.Text = sb.ToString();
        litStartup.Text = "<script>openOverlay('ovView');</script>";
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
            string model = (exp == null || exp == DBNull.Value) ? "(no model answer stored)" : Convert.ToString(exp);
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
    private void Flash(string msg)
    {
        pnlFlash.Visible = true;
        pnlFlash.CssClass = "adm-flash show";
        litFlash.Text = Server.HtmlEncode(msg);
    }
    private string GetInitials(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "A";
        string[] parts = name.Trim().Split(' ');
        if (parts.Length == 1) return parts[0].Substring(0, 1).ToUpper();
        return (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpper();
    }
}
