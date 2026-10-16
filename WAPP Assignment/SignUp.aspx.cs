using System;
using System.Configuration;
using System.Data.SqlClient;

// Web Application project (CodeBehind + designer file).
public partial class SignUp : System.Web.UI.Page
{
    private string ConnStr
    {
        get { return ConfigurationManager.ConnectionStrings["SEAConnection"].ConnectionString; }
    }

    protected void Page_Load(object sender, EventArgs e) { }

    protected void btnRegister_Click(object sender, EventArgs e)
    {
        // Server-side validation gate (validators also run on the client).
        if (!Page.IsValid) return;

        string email = txtEmail.Text.Trim();
        string username = txtUsername.Text.Trim();

        // Don't allow a duplicate email.
        if (EmailExists(email))
        {
            lblError.Visible = true;
            lblError.Text = "That email is already registered.";
            return;
        }

        string hash = Security.Hash(txtPwd.Text);

        DateTime dob;
        object dobValue = DateTime.TryParse(txtDob.Text, out dob) ? (object)dob : DBNull.Value;

        int newId;
        using (SqlConnection cn = new SqlConnection(ConnStr))
        using (SqlCommand cmd = new SqlCommand(
            // Take the LOWEST unused StudentID so self-registered accounts continue
            // the sequence (9, 10, 11...) instead of jumping to 1001 after restarts.
            "DECLARE @id INT = (SELECT MIN(t.id) FROM (SELECT 1 AS id UNION ALL SELECT StudentID + 1 FROM Student) t " +
            "                   WHERE NOT EXISTS (SELECT 1 FROM Student x WHERE x.StudentID = t.id));" +
            "SET IDENTITY_INSERT Student ON;" +
            "INSERT INTO Student (StudentID, StudentName, StudentDOB, StudentEmail, StudentPassword, TotalScore, TotalPoint, Streak) " +
            "VALUES (@id, @name, @dob, @email, @pw, 0, 0, 0);" +
            "SET IDENTITY_INSERT Student OFF;" +
            "SELECT @id;", cn))
        {
            cmd.Parameters.AddWithValue("@name", username);
            cmd.Parameters.AddWithValue("@dob", dobValue);
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@pw", hash);
            cn.Open();
            newId = Convert.ToInt32(cmd.ExecuteScalar());
        }

        // Log the new student in and send them to the dashboard.
        Session["StudentID"] = newId;
        Session["StudentName"] = username;
        Response.Redirect("StudentDashboard.aspx");
    }

    protected void btnBack_Click(object sender, EventArgs e)
    {
        Response.Redirect("Login.aspx");
    }

    private bool EmailExists(string email)
    {
        using (SqlConnection cn = new SqlConnection(ConnStr))
        using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Student WHERE StudentEmail = @e", cn))
        {
            cmd.Parameters.AddWithValue("@e", email);
            cn.Open();
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }
    }
}
