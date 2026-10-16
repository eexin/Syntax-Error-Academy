using System;

// Web Application project (CodeBehind + designer file, no namespace).
public partial class AdminReport : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["AdminID"] == null) { Response.Redirect("Login.aspx"); return; }
        if (!IsPostBack)
        {
            string name = Convert.ToString(Session["AdminName"]);
            litInitials.Text = string.IsNullOrWhiteSpace(name) ? "A"
                : (name.Trim().Split(' ').Length == 1
                    ? name.Trim().Substring(0, 1).ToUpper()
                    : (name.Trim().Split(' ')[0].Substring(0, 1) + name.Trim().Split(' ')[name.Trim().Split(' ').Length - 1].Substring(0, 1)).ToUpper());
        }
    }
}
