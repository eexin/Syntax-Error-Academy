using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

// Web Application project (CodeBehind + designer file, no namespace).
public partial class InstructorDashboard : System.Web.UI.Page
{
    private string ConnStr
    {
        get { return ConfigurationManager.ConnectionStrings["SEAConnection"].ConnectionString; }
    }

    private int InstructorId { get { return Convert.ToInt32(Session["InstructorID"]); } }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["InstructorID"] == null) { Response.Redirect("Login.aspx"); return; }
        if (IsPostBack) return;

        litNavAvatar.Text = NavAvatar();

        string wallBg = InstructorTheme.GetWallpaper(ConnStr, InstructorId);
        if (!string.IsNullOrEmpty(wallBg)) pageBody.Style["background"] = wallBg;

        int level;
        if (!int.TryParse(Request.QueryString["level"], out level) || level < 1) level = 1;
        lblLevel.Text = level.ToString();

        BuildLevels(level);
        BindQuizzes(level);
    }

    private void BuildLevels(int active)
    {
        DataTable dt = new DataTable();
        dt.Columns.Add("Level", typeof(int));
        dt.Columns.Add("Active", typeof(string));
        for (int i = 1; i <= 5; i++)
            dt.Rows.Add(i, i == active ? "active" : "");
        rptLevels.DataSource = dt;
        rptLevels.DataBind();
    }

    private void BindQuizzes(int level)
    {
        DataTable dt = new DataTable();
        using (SqlConnection cn = new SqlConnection(ConnStr))
        using (SqlCommand cmd = new SqlCommand(
            "SELECT QuizID, QuizTitle, ISNULL(QuizDescription,'') AS QuizDescription, QuizStatus " +
            "FROM Quiz WHERE InstructorID = @i AND Level = @l ORDER BY QuizID DESC", cn))
        {
            cmd.Parameters.AddWithValue("@i", InstructorId);
            cmd.Parameters.AddWithValue("@l", level);
            using (SqlDataAdapter da = new SqlDataAdapter(cmd)) da.Fill(dt);
        }
        rptQuizzes.DataSource = dt;
        rptQuizzes.DataBind();
        pnlEmpty.Visible = (dt.Rows.Count == 0);
    }

    // ---- status colour coding (used by the .aspx) ----
    protected string StatusClass(object status)
    {
        string s = Convert.ToString(status).Trim().ToLower();
        if (s == "rejected") return "red";
        if (s == "loginonly" || s == "pending") return "yellow";
        return "green"; // published / approved / guest / anything else live
    }

    protected string StatusText(object status)
    {
        string s = Convert.ToString(status).Trim();
        if (string.IsNullOrEmpty(s)) return "Published";
        string low = s.ToLower();
        if (low == "loginonly") return "Login Only";
        if (low == "approved" || low == "guest") return "Published";
        // capitalise first letter
        return char.ToUpper(s[0]) + s.Substring(1).ToLower();
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
