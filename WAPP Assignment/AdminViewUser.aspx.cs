using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

// Web Application project (CodeBehind + designer file, no namespace).
public partial class AdminViewUser : System.Web.UI.Page
{
    private string ConnStr
    {
        get { return ConfigurationManager.ConnectionStrings["SEAConnection"].ConnectionString; }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["AdminID"] == null) { Response.Redirect("Login.aspx"); return; }

        if (!IsPostBack)
        {
            litInitials.Text = GetInitials(Convert.ToString(Session["AdminName"]));
            hfActiveView.Value = "student";
            RenderActiveView(null);
        }
    }

    // ---------- table switches ----------
    protected void ShowStudents(object sender, EventArgs e) { SwitchTo("student"); }
    protected void ShowInstructors(object sender, EventArgs e) { SwitchTo("instructor"); }
    protected void ShowAdmins(object sender, EventArgs e) { SwitchTo("admin"); }

    private void SwitchTo(string view)
    {
        hfActiveView.Value = view;
        txtSearch.Text = "";
        hfSelectedId.Value = "";
        RenderActiveView(null);
    }

    // ---------- search ----------
    protected void btnSearch_Click(object sender, EventArgs e)
    {
        string term = txtSearch.Text.Trim();
        hfSelectedId.Value = "";
        RenderActiveView(string.IsNullOrEmpty(term) ? null : term);
    }

    // ---------- render whichever table is active ----------
    private void RenderActiveView(string search)
    {
        string v = hfActiveView.Value;
        if (v != "instructor" && v != "admin") v = "student";

        pnlStudentTable.Visible = (v == "student");
        pnlInstructorTable.Visible = (v == "instructor");
        pnlAdminTable.Visible = (v == "admin");

        lnkStudents.CssClass = "adm-tab" + (v == "student" ? " active" : "");
        lnkInstructors.CssClass = "adm-tab" + (v == "instructor" ? " active" : "");
        lnkAdmins.CssClass = "adm-tab" + (v == "admin" ? " active" : "");

        if (v == "student") BindStudents(search);
        else if (v == "instructor") BindInstructors(search);
        else BindAdmins(search);
    }

    private void BindStudents(string search)
    {
        string sql =
            "SELECT StudentID, StudentName, StudentDOB, StudentEmail, StudentPassword, " +
            "TotalScore, TotalPoint, Streak, LastActiveDate FROM Student ";
        if (!string.IsNullOrEmpty(search))
            sql += "WHERE StudentName LIKE @like OR CONVERT(NVARCHAR(20), StudentID) = @exact ";
        sql += "ORDER BY StudentID";
        DataTable dt = Query(sql, search);
        rptStudents.DataSource = dt; rptStudents.DataBind();
        pnlStudentEmpty.Visible = (dt.Rows.Count == 0);
    }

    private void BindInstructors(string search)
    {
        string sql =
            "SELECT InstructorID, InstructorName, InstructorDOB, InstructorEmail, InstructorPassword FROM Instructor ";
        if (!string.IsNullOrEmpty(search))
            sql += "WHERE InstructorName LIKE @like OR CONVERT(NVARCHAR(20), InstructorID) = @exact ";
        sql += "ORDER BY InstructorID";
        DataTable dt = Query(sql, search);
        rptInstructors.DataSource = dt; rptInstructors.DataBind();
        pnlInstructorEmpty.Visible = (dt.Rows.Count == 0);
    }

    private void BindAdmins(string search)
    {
        string sql =
            "SELECT AdminID, AdminName, AdminDOB, AdminEmail, AdminPassword FROM Administrator ";
        if (!string.IsNullOrEmpty(search))
            sql += "WHERE AdminName LIKE @like OR CONVERT(NVARCHAR(20), AdminID) = @exact ";
        sql += "ORDER BY AdminID";
        DataTable dt = Query(sql, search);
        rptAdmins.DataSource = dt; rptAdmins.DataBind();
        pnlAdminEmpty.Visible = (dt.Rows.Count == 0);
    }

    private DataTable Query(string sql, string search)
    {
        DataTable dt = new DataTable();
        using (SqlConnection cn = new SqlConnection(ConnStr))
        using (SqlCommand cmd = new SqlCommand(sql, cn))
        {
            if (!string.IsNullOrEmpty(search))
            {
                cmd.Parameters.AddWithValue("@like", "%" + search + "%");
                cmd.Parameters.AddWithValue("@exact", search);
            }
            using (SqlDataAdapter da = new SqlDataAdapter(cmd)) da.Fill(dt);
        }
        return dt;
    }

    // smallest unused positive id, so new rows continue the sequence (1,2,3,...9,10)
    private int NextId(SqlConnection cn, string table, string idCol)
    {
        string sql =
            "SELECT MIN(t.id) FROM (SELECT 1 AS id UNION ALL SELECT " + idCol + " + 1 FROM " + table + ") t " +
            "WHERE NOT EXISTS (SELECT 1 FROM " + table + " x WHERE x." + idCol + " = t.id)";
        using (SqlCommand cmd = new SqlCommand(sql, cn))
        {
            object o = cmd.ExecuteScalar();
            return (o == null || o == DBNull.Value) ? 1 : Convert.ToInt32(o);
        }
    }

    // ---------- ADD ----------
    protected void btnAddUser_Click(object sender, EventArgs e)
    {
        string role = ddlAddRole.SelectedValue;                 // Student | Instructor | Admin
        string name = txtAddName.Text.Trim();
        string email = txtAddEmail.Text.Trim();
        string pwd = txtAddPassword.Text;

        if (name == "" || email == "" || pwd == "")
        {
            Flash("Name, email and password are required.", false);
            RenderActiveView(null);
            return;
        }

        object dob = DateTime.TryParse(txtAddDob.Text, out DateTime d) ? (object)d : DBNull.Value;
        string hash = Security.Hash(pwd);

        string table, idCol, emailCol, view;
        switch (role)
        {
            case "Instructor": table = "Instructor"; idCol = "InstructorID"; emailCol = "InstructorEmail"; view = "instructor"; break;
            case "Admin": table = "Administrator"; idCol = "AdminID"; emailCol = "AdminEmail"; view = "admin"; break;
            default: role = "Student"; table = "Student"; idCol = "StudentID"; emailCol = "StudentEmail"; view = "student"; break;
        }

        // duplicate-email guard within the target table
        if (Scalar("SELECT COUNT(*) FROM " + table + " WHERE " + emailCol + " = @e", "@e", email) > 0)
        {
            Flash("That email is already registered as a " + role.ToLower() + ".", false);
            hfActiveView.Value = view;
            RenderActiveView(null);
            return;
        }

        using (SqlConnection cn = new SqlConnection(ConnStr))
        {
            cn.Open();
            int nextId = NextId(cn, table, idCol);

            string insert;
            if (table == "Student")
                insert = "SET IDENTITY_INSERT Student ON;" +
                         "INSERT INTO Student (StudentID, StudentName, StudentDOB, StudentEmail, StudentPassword, TotalScore, TotalPoint, Streak) " +
                         "VALUES (@id,@n,@dob,@e,@pw,0,0,0);SET IDENTITY_INSERT Student OFF;";
            else if (table == "Instructor")
                insert = "SET IDENTITY_INSERT Instructor ON;" +
                         "INSERT INTO Instructor (InstructorID, InstructorName, InstructorDOB, InstructorEmail, InstructorPassword) " +
                         "VALUES (@id,@n,@dob,@e,@pw);SET IDENTITY_INSERT Instructor OFF;";
            else
                insert = "SET IDENTITY_INSERT Administrator ON;" +
                         "INSERT INTO Administrator (AdminID, AdminName, AdminDOB, AdminEmail, AdminPassword) " +
                         "VALUES (@id,@n,@dob,@e,@pw);SET IDENTITY_INSERT Administrator OFF;";

            using (SqlCommand cmd = new SqlCommand(insert, cn))
            {
                cmd.Parameters.AddWithValue("@id", nextId);
                cmd.Parameters.AddWithValue("@n", name);
                cmd.Parameters.AddWithValue("@dob", dob);
                cmd.Parameters.AddWithValue("@e", email);
                cmd.Parameters.AddWithValue("@pw", hash);
                cmd.ExecuteNonQuery();
            }
        }

        hfActiveView.Value = view;
        hfSelectedId.Value = "";
        Flash(role + " \"" + name + "\" was added successfully.", true);
        RenderActiveView(null);
    }

    // ---------- EDIT (save) ----------
    protected void btnSaveEdit_Click(object sender, EventArgs e)
    {
        int id;
        if (!int.TryParse(hfEditId.Value, out id)) { RenderActiveView(null); return; }
        string role = hfEditRole.Value;   // student | instructor | admin

        string name = txtEditName.Text.Trim();
        string email = txtEditEmail.Text.Trim();
        object dob = DateTime.TryParse(txtEditDob.Text, out DateTime d) ? (object)d : DBNull.Value;

        using (SqlConnection cn = new SqlConnection(ConnStr))
        {
            cn.Open();
            SqlCommand cmd;
            if (role == "instructor")
                cmd = new SqlCommand("UPDATE Instructor SET InstructorName=@n, InstructorDOB=@dob, InstructorEmail=@e WHERE InstructorID=@id", cn);
            else if (role == "admin")
                cmd = new SqlCommand("UPDATE Administrator SET AdminName=@n, AdminDOB=@dob, AdminEmail=@e WHERE AdminID=@id", cn);
            else
            {
                int.TryParse(txtEditScore.Text, out int score);
                int.TryParse(txtEditPoint.Text, out int point);
                cmd = new SqlCommand("UPDATE Student SET StudentName=@n, StudentDOB=@dob, StudentEmail=@e, TotalScore=@sc, TotalPoint=@pt WHERE StudentID=@id", cn);
                cmd.Parameters.AddWithValue("@sc", score);
                cmd.Parameters.AddWithValue("@pt", point);
            }
            cmd.Parameters.AddWithValue("@n", name);
            cmd.Parameters.AddWithValue("@dob", dob);
            cmd.Parameters.AddWithValue("@e", email);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
            cmd.Dispose();
        }

        hfSelectedId.Value = "";
        Flash("Changes have been saved successfully.", true);
        RenderActiveView(null);
    }

    // ---------- DELETE ----------
    protected void btnDoDelete_Click(object sender, EventArgs e)
    {
        int id;
        if (!int.TryParse(hfSelectedId.Value, out id)) { RenderActiveView(null); return; }
        string view = hfActiveView.Value;   // student | instructor | admin

        // never let an admin delete the account they are logged in with
        if (view == "admin" && id == Convert.ToInt32(Session["AdminID"]))
        {
            Flash("You cannot delete the admin account you are currently logged in as.", false);
            RenderActiveView(null);
            return;
        }

        try
        {
            using (SqlConnection cn = new SqlConnection(ConnStr))
            {
                cn.Open();
                using (SqlTransaction tx = cn.BeginTransaction())
                {
                    if (view == "instructor")
                    {
                        const string quizzes = "(SELECT QuizID FROM Quiz WHERE InstructorID=@id)";
                        const string questions = "(SELECT QuestionID FROM Question WHERE QuizID IN " + quizzes + ")";
                        Exec(cn, tx, "DELETE FROM StudentsAnswer WHERE QuestionID IN " + questions, id);
                        Exec(cn, tx, "DELETE FROM MCQOption WHERE QuestionID IN " + questions, id);
                        TryExec(cn, tx, "DELETE FROM MatchOption WHERE QuestionID IN " + questions, id);
                        TryExec(cn, tx, "DELETE FROM PuzzleToken WHERE QuestionID IN " + questions, id);
                        TryExec(cn, tx, "DELETE FROM EssayOption WHERE QuestionID IN " + questions, id);
                        Exec(cn, tx, "DELETE FROM Question WHERE QuizID IN " + quizzes, id);
                        Exec(cn, tx, "DELETE FROM Quiz WHERE InstructorID=@id", id);
                        Exec(cn, tx, "DELETE FROM Instructor WHERE InstructorID=@id", id);
                    }
                    else if (view == "admin")
                    {
                        Exec(cn, tx, "DELETE FROM Administrator WHERE AdminID=@id", id);
                    }
                    else
                    {
                        Exec(cn, tx, "DELETE FROM StudentsAnswer WHERE StudentID=@id", id);
                        TryExec(cn, tx, "DELETE FROM Inventory WHERE StudentID=@id", id);
                        Exec(cn, tx, "DELETE FROM Student WHERE StudentID=@id", id);
                    }
                    tx.Commit();
                }
            }
            hfSelectedId.Value = "";
            Flash("User was deleted successfully.", true);
        }
        catch (Exception ex)
        {
            Flash("Could not delete this user: " + ex.Message, false);
        }
        RenderActiveView(null);
    }

    // ---------- small helpers ----------
    private void Exec(SqlConnection cn, SqlTransaction tx, string sql, int id)
    {
        using (SqlCommand cmd = new SqlCommand(sql, cn, tx))
        {
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }
    }

    private void TryExec(SqlConnection cn, SqlTransaction tx, string sql, int id)
    {
        try { Exec(cn, tx, sql, id); } catch { /* optional child table */ }
    }

    private int Scalar(string sql, string p, string v)
    {
        using (SqlConnection cn = new SqlConnection(ConnStr))
        using (SqlCommand cmd = new SqlCommand(sql, cn))
        {
            cmd.Parameters.AddWithValue(p, v);
            cn.Open();
            return Convert.ToInt32(cmd.ExecuteScalar());
        }
    }

    private void Flash(string msg, bool success)
    {
        pnlFlash.Visible = true;
        pnlFlash.CssClass = "adm-flash show" + (success ? "" : " err");
        litFlash.Text = Server.HtmlEncode(msg);
        if (!success)
        {
            pnlFlash.Style["background"] = "rgba(224,87,63,.14)";
            pnlFlash.Style["border-color"] = "#e0573f";
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
