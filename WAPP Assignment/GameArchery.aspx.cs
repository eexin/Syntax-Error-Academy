using System;
using System.Configuration;
using System.Data.SqlClient;

// Web Application project (CodeBehind + designer file).
// Target Shot: part of the recreational Minigames module. The games are
// client-side only - nothing on this page writes to the database, and
// playing never changes TotalPoint, TotalScore, Streak or Inventory.
public partial class GameArchery : System.Web.UI.Page
{
    private string ConnStr
    {
        get { return ConfigurationManager.ConnectionStrings["SEAConnection"].ConnectionString; }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        // Registered-member area: a valid student login is required.
        if (Session["StudentID"] == null)
        {
            Response.Redirect("Login.aspx");
            return;
        }

        ApplyWallpaper();

        if (!IsPostBack)
        {
            LoadHeader(Convert.ToInt32(Session["StudentID"]));
        }
    }

    // Top-bar data: avatar, coins, streak (same pattern as the other student pages).
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

    // Apply the wallpaper the student bought + equipped in the Item Shop.
    private void ApplyWallpaper()
    {
        string wall = Wallpaper.GetEquipped(ConnStr, Convert.ToInt32(Session["StudentID"]));
        if (!string.IsNullOrEmpty(wall))
            pageBody.Style["background"] = wall;
    }
}
