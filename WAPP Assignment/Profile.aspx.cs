using System;
using System.Configuration;
using System.Data.SqlClient;

// Web Application project (CodeBehind + designer file).
public partial class Profile : System.Web.UI.Page
{
    private string ConnStr
    {
        get { return ConfigurationManager.ConnectionStrings["SEAConnection"].ConnectionString; }
    }

    private int Sid { get { return Convert.ToInt32(Session["StudentID"]); } }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["StudentID"] == null)
        {
            Response.Redirect("Login.aspx");
            return;
        }

        ApplyWallpaper();

        if (!IsPostBack)
            LoadProfile();
    }

    private void LoadProfile()
    {
        using (SqlConnection cn = new SqlConnection(ConnStr))
        using (SqlCommand cmd = new SqlCommand(
            "SELECT StudentName, StudentEmail, StudentDOB, TotalPoint, Streak FROM Student WHERE StudentID = @sid", cn))
        {
            cmd.Parameters.AddWithValue("@sid", Sid);
            cn.Open();
            using (SqlDataReader r = cmd.ExecuteReader())
            {
                if (r.Read())
                {
                    string name = r["StudentName"].ToString();
                    txtName.Text = name;
                    txtEmail.Text = r["StudentEmail"].ToString();
                    if (r["StudentDOB"] != DBNull.Value)
                        txtDob.Text = Convert.ToDateTime(r["StudentDOB"]).ToString("yyyy-MM-dd");

                    litInitials.Text = Avatar.Render(Avatar.GetEquippedKeys(ConnStr, Sid), true);
                    litAvatar.Text = Avatar.Render(Avatar.GetEquippedKeys(ConnStr, Sid), false);
                    lblCoins.Text = Convert.ToInt32(r["TotalPoint"]).ToString("N0");
                    lblStreak.Text = Convert.ToInt32(r["Streak"]).ToString();
                }
            }
        }
    }

    protected void btnSave_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid) return;

        string name = txtName.Text.Trim();
        string email = txtEmail.Text.Trim();

        // Make sure the email isn't taken by a different student.
        if (EmailTakenByOther(email))
        {
            pnlErr.Visible = true;
            lblErr.Text = "That email is already used by another account.";
            return;
        }

        DateTime dob;
        object dobValue = DateTime.TryParse(txtDob.Text, out dob) ? (object)dob : DBNull.Value;

        // Build the UPDATE. Only change the password if a new one was typed.
        bool changePwd = !string.IsNullOrWhiteSpace(txtPwd.Text);
        string sql = "UPDATE Student SET StudentName=@name, StudentEmail=@email, StudentDOB=@dob" +
                     (changePwd ? ", StudentPassword=@pw" : "") +
                     " WHERE StudentID=@sid";

        using (SqlConnection cn = new SqlConnection(ConnStr))
        using (SqlCommand cmd = new SqlCommand(sql, cn))
        {
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@dob", dobValue);
            if (changePwd) cmd.Parameters.AddWithValue("@pw", Security.Hash(txtPwd.Text));
            cmd.Parameters.AddWithValue("@sid", Sid);
            cn.Open();
            cmd.ExecuteNonQuery();
        }

        Session["StudentName"] = name;
        litInitials.Text = Avatar.Render(Avatar.GetEquippedKeys(ConnStr, Sid), true);
        txtPwd.Text = "";

        pnlErr.Visible = false;
        pnlOk.Visible = true;
        lblOk.Text = "Your profile has been updated.";
    }

    private bool EmailTakenByOther(string email)
    {
        using (SqlConnection cn = new SqlConnection(ConnStr))
        using (SqlCommand cmd = new SqlCommand(
            "SELECT COUNT(*) FROM Student WHERE StudentEmail = @e AND StudentID <> @sid", cn))
        {
            cmd.Parameters.AddWithValue("@e", email);
            cmd.Parameters.AddWithValue("@sid", Sid);
            cn.Open();
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
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
    // ------------------------------------------------------------------
    //  DELETE: permanently remove this student's account.
    //  The password is re-checked first, then the child rows are removed
    //  before the parent row so the foreign keys stay valid. Everything
    //  runs in one transaction, so a failure part-way leaves no orphans.
    // ------------------------------------------------------------------
    protected void btnDelete_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid) return;

        if (!PasswordMatches(txtDelPwd.Text))
        {
            pnlOk.Visible = false;
            pnlErr.Visible = true;
            lblErr.Text = "That password is not correct, so the account was not deleted.";
            txtDelPwd.Text = "";
            return;
        }

        using (SqlConnection cn = new SqlConnection(ConnStr))
        {
            cn.Open();
            using (SqlTransaction tx = cn.BeginTransaction())
            {
                try
                {
                    Exec(cn, tx, "DELETE FROM StudentsAnswer WHERE StudentID = @sid");
                    Exec(cn, tx, "DELETE FROM Inventory      WHERE StudentID = @sid");
                    Exec(cn, tx, "DELETE FROM Student        WHERE StudentID = @sid");
                    tx.Commit();
                }
                catch
                {
                    tx.Rollback();
                    pnlOk.Visible = false;
                    pnlErr.Visible = true;
                    lblErr.Text = "Your account could not be deleted. Please try again.";
                    return;
                }
            }
        }

        // The account is gone, so end the session and send them to the login page.
        Session.Clear();
        Session.Abandon();
        Response.Redirect("Login.aspx?deleted=1");
    }

    private void Exec(SqlConnection cn, SqlTransaction tx, string sql)
    {
        using (SqlCommand cmd = new SqlCommand(sql, cn, tx))
        {
            cmd.Parameters.AddWithValue("@sid", Sid);
            cmd.ExecuteNonQuery();
        }
    }

    private bool PasswordMatches(string plain)
    {
        using (SqlConnection cn = new SqlConnection(ConnStr))
        using (SqlCommand cmd = new SqlCommand(
            "SELECT COUNT(*) FROM Student WHERE StudentID = @sid AND StudentPassword = @pw", cn))
        {
            cmd.Parameters.AddWithValue("@sid", Sid);
            cmd.Parameters.AddWithValue("@pw", Security.Hash(plain));
            cn.Open();
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }
    }

    private void ApplyWallpaper()
    {
        string wall = Wallpaper.GetEquipped(ConnStr, Sid);
        if (!string.IsNullOrEmpty(wall))
            pageBody.Style["background"] = wall;
    }

}