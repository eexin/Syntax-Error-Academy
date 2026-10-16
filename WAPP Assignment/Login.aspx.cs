using System;
using System.Configuration;
using System.Data.SqlClient;

// Web Application project (CodeBehind + designer file).
public partial class Login : System.Web.UI.Page
{
    private string ConnStr
    {
        get { return ConfigurationManager.ConnectionStrings["SEAConnection"].ConnectionString; }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            EnsureAdminAccount();   // make sure an admin login always exists

            // Already logged in? Send them to the right dashboard.
            if (Session["AdminID"] != null)
                Response.Redirect("AdminDashboard.aspx");
            else if (Session["InstructorID"] != null)
                Response.Redirect("InstructorDashboard.aspx");
            else if (Session["StudentID"] != null)
                Response.Redirect("StudentDashboard.aspx");
        }
    }

    // Seeds the default admin (admin@gmail.com / 123456) and the teaching staff
    // if they aren't there yet, so every copy of the database has working logins.
    private void EnsureAdminAccount()
    {
        const string pw = "8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92"; // 123456
        try
        {
            using (SqlConnection cn = new SqlConnection(ConnStr))
            {
                cn.Open();
                using (SqlCommand cmd = new SqlCommand(
                    "IF NOT EXISTS (SELECT 1 FROM Administrator WHERE AdminEmail='admin@gmail.com') " +
                    "INSERT INTO Administrator (AdminName, AdminDOB, AdminEmail, AdminPassword) " +
                    "VALUES ('admin','1995-01-01','admin@gmail.com','" + pw + "')", cn))
                    cmd.ExecuteNonQuery();

                // instructor accounts (all passwords: 123456)
                string[,] staff = {
                    { "Ms. Jas",  "jas@gmail.com",  "1990-06-15" },
                    { "Mr. Tan",  "tan@gmail.com",  "1987-03-22" },
                    { "Mr. Wang", "wang@gmail.com", "1992-11-08" },
                    { "Mr. Tay",  "tay@gmail.com",  "1985-09-30" },
                };
                for (int i = 0; i < staff.GetLength(0); i++)
                {
                    using (SqlCommand cmd = new SqlCommand(
                        "IF NOT EXISTS (SELECT 1 FROM Instructor WHERE InstructorEmail=@e) " +
                        "INSERT INTO Instructor (InstructorName, InstructorDOB, InstructorEmail, InstructorPassword) " +
                        "VALUES (@n,@d,@e,'" + pw + "')", cn))
                    {
                        cmd.Parameters.AddWithValue("@n", staff[i, 0]);
                        cmd.Parameters.AddWithValue("@d", staff[i, 2]);
                        cmd.Parameters.AddWithValue("@e", staff[i, 1]);
                        cmd.ExecuteNonQuery();
                    }
                }

                // the original seed account was named 'Jas' - align it with the staff list
                using (SqlCommand cmd = new SqlCommand(
                    "UPDATE Instructor SET InstructorName='Ms. Jas' " +
                    "WHERE InstructorEmail='jas@gmail.com' AND InstructorName='Jas'", cn))
                    cmd.ExecuteNonQuery();
            }
        }
        catch { }
    }

    protected void btnLogin_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid) return;

        string id = txtLoginId.Text.Trim();
        string hash = Security.Hash(txtLoginPwd.Text);

        // 1) Try the ADMIN account first (username or email + password).
        if (TryAdminLogin(id, hash)) return;

        // 2) Then try the INSTRUCTOR account.
        if (TryInstructorLogin(id, hash)) return;

        // 3) Otherwise fall back to the STUDENT account (unchanged behaviour).
        using (SqlConnection cn = new SqlConnection(ConnStr))
        using (SqlCommand cmd = new SqlCommand(
            "SELECT StudentID, StudentName FROM Student " +
            "WHERE (StudentEmail = @id OR StudentName = @id) AND StudentPassword = @pw", cn))
        {
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@pw", hash);
            cn.Open();
            using (SqlDataReader r = cmd.ExecuteReader())
            {
                if (r.Read())
                {
                    Session["StudentID"] = Convert.ToInt32(r["StudentID"]);
                    Session["StudentName"] = r["StudentName"].ToString();
                    Response.Redirect("StudentDashboard.aspx");
                    return;
                }
            }
        }

        // No match anywhere.
        lblError.Visible = true;
        lblError.Text = "Wrong email/username or password.";
    }

    // Returns true (and redirects) when the credentials match an instructor.
    private bool TryInstructorLogin(string id, string hash)
    {
        using (SqlConnection cn = new SqlConnection(ConnStr))
        using (SqlCommand cmd = new SqlCommand(
            "SELECT InstructorID, InstructorName FROM Instructor " +
            "WHERE (InstructorEmail = @id OR InstructorName = @id) AND InstructorPassword = @pw", cn))
        {
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@pw", hash);
            cn.Open();
            using (SqlDataReader r = cmd.ExecuteReader())
            {
                if (r.Read())
                {
                    Session.Clear();
                    Session["InstructorID"] = Convert.ToInt32(r["InstructorID"]);
                    Session["InstructorName"] = r["InstructorName"].ToString();
                    Response.Redirect("InstructorDashboard.aspx");
                    return true;
                }
            }
        }
        return false;
    }

    // Returns true (and redirects) when the credentials match an admin account.
    private bool TryAdminLogin(string id, string hash)
    {
        using (SqlConnection cn = new SqlConnection(ConnStr))
        using (SqlCommand cmd = new SqlCommand(
            "SELECT AdminID, AdminName FROM Administrator " +
            "WHERE (AdminEmail = @id OR AdminName = @id) AND AdminPassword = @pw", cn))
        {
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@pw", hash);
            cn.Open();
            using (SqlDataReader r = cmd.ExecuteReader())
            {
                if (r.Read())
                {
                    // Start a clean admin session (no leftover student keys).
                    Session.Clear();
                    Session["AdminID"] = Convert.ToInt32(r["AdminID"]);
                    Session["AdminName"] = r["AdminName"].ToString();
                    Response.Redirect("AdminDashboard.aspx");
                    return true;
                }
            }
        }
        return false;
    }
}
