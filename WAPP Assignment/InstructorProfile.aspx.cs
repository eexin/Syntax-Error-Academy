using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;

// Web Application project (CodeBehind + designer file, no namespace).
// Instructor account page, mirroring the student Profile: editable details,
// the avatar beside the form, and a summary of the instructor's quizzes.
public partial class InstructorProfile : System.Web.UI.Page
{
    private string ConnStr
    {
        get { return ConfigurationManager.ConnectionStrings["SEAConnection"].ConnectionString; }
    }
    private int InstructorId { get { return Convert.ToInt32(Session["InstructorID"]); } }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["InstructorID"] == null) { Response.Redirect("Login.aspx"); return; }

        litNavAvatar.Text = NavAvatar();

        string wallBg = InstructorTheme.GetWallpaper(ConnStr, InstructorId);
        if (!string.IsNullOrEmpty(wallBg)) pageBody.Style["background"] = wallBg;

        if (IsPostBack) return;
        LoadDetails();
        LoadStats();
        litAvatar.Text = Avatar.Render(GetInsEquippedKeys());
    }

    private void LoadDetails()
    {
        using (SqlConnection cn = new SqlConnection(ConnStr))
        using (SqlCommand cmd = new SqlCommand(
            "SELECT InstructorName, InstructorEmail, InstructorDOB FROM Instructor WHERE InstructorID=@i", cn))
        {
            cmd.Parameters.AddWithValue("@i", InstructorId);
            cn.Open();
            using (SqlDataReader r = cmd.ExecuteReader())
                if (r.Read())
                {
                    txtName.Text = Convert.ToString(r["InstructorName"]);
                    txtEmail.Text = Convert.ToString(r["InstructorEmail"]);
                    if (r["InstructorDOB"] != DBNull.Value)
                        txtDob.Text = Convert.ToDateTime(r["InstructorDOB"]).ToString("yyyy-MM-dd");
                }
        }
    }

    private void LoadStats()
    {
        using (SqlConnection cn = new SqlConnection(ConnStr))
        {
            cn.Open();
            lblTotal.Text = Count(cn, "SELECT COUNT(*) FROM Quiz WHERE InstructorID=@i");
            lblPublished.Text = Count(cn, "SELECT COUNT(*) FROM Quiz WHERE InstructorID=@i AND QuizStatus NOT IN ('Pending','Rejected')");
            lblPending.Text = Count(cn, "SELECT COUNT(*) FROM Quiz WHERE InstructorID=@i AND QuizStatus='Pending'");
            lblRejected.Text = Count(cn, "SELECT COUNT(*) FROM Quiz WHERE InstructorID=@i AND QuizStatus='Rejected'");
        }
    }

    protected void btnSave_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid) return;

        string name = txtName.Text.Trim();
        string email = txtEmail.Text.Trim();
        if (name == "" || email == "") { ShowErr("Username and email are required."); return; }

        // the email must not belong to another instructor
        using (SqlConnection cn = new SqlConnection(ConnStr))
        using (SqlCommand chk = new SqlCommand(
            "SELECT COUNT(*) FROM Instructor WHERE InstructorEmail=@e AND InstructorID<>@i", cn))
        {
            chk.Parameters.AddWithValue("@e", email);
            chk.Parameters.AddWithValue("@i", InstructorId);
            cn.Open();
            if (Convert.ToInt32(chk.ExecuteScalar()) > 0)
            {
                ShowErr("That email is already used by another instructor.");
                return;
            }
        }

        DateTime dob;
        object dobValue = DateTime.TryParse(txtDob.Text, out dob) ? (object)dob : DBNull.Value;
        bool changePwd = !string.IsNullOrWhiteSpace(txtPwd.Text);

        string sql = "UPDATE Instructor SET InstructorName=@n, InstructorEmail=@e, InstructorDOB=@d" +
                     (changePwd ? ", InstructorPassword=@pw" : "") +
                     " WHERE InstructorID=@i";
        using (SqlConnection cn = new SqlConnection(ConnStr))
        using (SqlCommand cmd = new SqlCommand(sql, cn))
        {
            cmd.Parameters.AddWithValue("@n", name);
            cmd.Parameters.AddWithValue("@e", email);
            cmd.Parameters.AddWithValue("@d", dobValue);
            if (changePwd) cmd.Parameters.AddWithValue("@pw", Security.Hash(txtPwd.Text));
            cmd.Parameters.AddWithValue("@i", InstructorId);
            cn.Open();
            cmd.ExecuteNonQuery();
        }

        Session["InstructorName"] = name;
        txtPwd.Text = "";
        pnlErr.Visible = false;
        pnlOk.Visible = true;
        lblOk.Text = changePwd ? "Profile and password updated." : "Profile updated.";
        litNavAvatar.Text = NavAvatar();
    }

    private void ShowErr(string msg)
    {
        pnlOk.Visible = false;
        pnlErr.Visible = true;
        lblErr.Text = Server.HtmlEncode(msg);
    }

    private string Count(SqlConnection cn, string sql)
    {
        using (SqlCommand cmd = new SqlCommand(sql, cn))
        {
            cmd.Parameters.AddWithValue("@i", InstructorId);
            object o = cmd.ExecuteScalar();
            return (o == null || o == DBNull.Value) ? "0" : Convert.ToString(o);
        }
    }

    // instructor's equipped cosmetics -> avatar
    private List<string> GetInsEquippedKeys()
    {
        List<string> keys = new List<string>();
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
