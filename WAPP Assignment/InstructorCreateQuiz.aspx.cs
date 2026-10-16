using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.Script.Serialization;

// Web Application project (CodeBehind + designer file, no namespace).
public partial class InstructorCreateQuiz : System.Web.UI.Page
{
    private string ConnStr
    {
        get { return ConfigurationManager.ConnectionStrings["SEAConnection"].ConnectionString; }
    }
    private int InstructorId { get { return Convert.ToInt32(Session["InstructorID"]); } }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["InstructorID"] == null) { Response.Redirect("Login.aspx"); return; }
        if (!IsPostBack)
            litNavAvatar.Text = NavAvatar();

        string wallBg = InstructorTheme.GetWallpaper(ConnStr, InstructorId);
        if (!string.IsNullOrEmpty(wallBg)) pageBody.Style["background"] = wallBg;
    }

    protected void btnCreate_Click(object sender, EventArgs e)
    {
        if (Session["InstructorID"] == null) { Response.Redirect("Login.aspx"); return; }

        string name = (hfName.Value ?? "").Trim();
        string desc = (hfDesc.Value ?? "").Trim();
        int level; if (!int.TryParse(hfLevel.Value, out level) || level < 1) level = 1;
        if (name == "") { Response.Redirect("InstructorCreateQuiz.aspx"); return; }

        object[] questions;
        try { questions = (object[])new JavaScriptSerializer().DeserializeObject(hfQuestions.Value); }
        catch { questions = new object[0]; }

        using (SqlConnection cn = new SqlConnection(ConnStr))
        {
            cn.Open();
            using (SqlTransaction tx = cn.BeginTransaction())
            {
                // 1) the quiz  (new instructor quizzes start as Pending for admin approval)
                int quizId;
                using (SqlCommand cmd = new SqlCommand(
                    "INSERT INTO Quiz (QuizTitle, QuizDescription, InstructorID, Level, QuizStatus) " +
                    "VALUES (@t,@d,@i,@l,'Pending'); SELECT SCOPE_IDENTITY();", cn, tx))
                {
                    cmd.Parameters.AddWithValue("@t", name);
                    cmd.Parameters.AddWithValue("@d", (object)desc ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@i", InstructorId);
                    cmd.Parameters.AddWithValue("@l", level);
                    quizId = Convert.ToInt32(cmd.ExecuteScalar());
                }

                // 2) each question + its type-specific rows
                foreach (object qo in questions)
                {
                    var q = qo as Dictionary<string, object>;
                    if (q == null) continue;
                    string type = Str(q, "type");
                    string text = Str(q, "text");
                    if (text == "") continue;

                    int qid;
                    using (SqlCommand cmd = new SqlCommand(
                        "INSERT INTO Question (QuestionText, QuestionType, Score, QuizID) " +
                        "VALUES (@x,@ty,100,@qz); SELECT SCOPE_IDENTITY();", cn, tx))
                    {
                        cmd.Parameters.AddWithValue("@x", text);
                        cmd.Parameters.AddWithValue("@ty", type);
                        cmd.Parameters.AddWithValue("@qz", quizId);
                        qid = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    if (type == "MCQ")
                    {
                        object[] opts = Arr(q, "options");
                        int correct = 0; int.TryParse(Convert.ToString(q.ContainsKey("correct") ? q["correct"] : 0), out correct);
                        for (int k = 0; k < opts.Length; k++)
                            using (SqlCommand cmd = new SqlCommand(
                                "INSERT INTO MCQOption (QuestionID, OptionText, IsCorrect) VALUES (@q,@o,@c)", cn, tx))
                            {
                                cmd.Parameters.AddWithValue("@q", qid);
                                cmd.Parameters.AddWithValue("@o", Convert.ToString(opts[k]));
                                cmd.Parameters.AddWithValue("@c", k == correct ? 1 : 0);
                                cmd.ExecuteNonQuery();
                            }
                    }
                    else if (type == "Matching")
                    {
                        foreach (object po in Arr(q, "pairs"))
                        {
                            object[] pair = po as object[];
                            if (pair == null || pair.Length < 2) continue;
                            using (SqlCommand cmd = new SqlCommand(
                                "INSERT INTO MatchOption (QuestionID, LeftItem, RightItem) VALUES (@q,@l,@r)", cn, tx))
                            {
                                cmd.Parameters.AddWithValue("@q", qid);
                                cmd.Parameters.AddWithValue("@l", Convert.ToString(pair[0]));
                                cmd.Parameters.AddWithValue("@r", Convert.ToString(pair[1]));
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                    else if (type == "Puzzle")
                    {
                        object[] toks = Arr(q, "tokens");
                        for (int k = 0; k < toks.Length; k++)
                            using (SqlCommand cmd = new SqlCommand(
                                "INSERT INTO PuzzleToken (QuestionID, TokenText, PositionOrder) VALUES (@q,@t,@p)", cn, tx))
                            {
                                cmd.Parameters.AddWithValue("@q", qid);
                                cmd.Parameters.AddWithValue("@t", Convert.ToString(toks[k]));
                                cmd.Parameters.AddWithValue("@p", k + 1);
                                cmd.ExecuteNonQuery();
                            }
                    }
                    else if (type == "Essay")
                    {
                        // Essay questions only need the question itself - no answer is
                        // supplied by the instructor. EssayTitle mirrors the question text
                        // so the existing EssayOption row stays valid.
                        using (SqlCommand cmd = new SqlCommand(
                            "INSERT INTO EssayOption (QuestionID, EssayTitle) VALUES (@q,@t)", cn, tx))
                        {
                            cmd.Parameters.AddWithValue("@q", qid);
                            cmd.Parameters.AddWithValue("@t", text);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                tx.Commit();
            }
        }

        Response.Redirect("InstructorDashboard.aspx?level=" + level);
    }

    private static string Str(Dictionary<string, object> d, string k)
    {
        return (d != null && d.ContainsKey(k) && d[k] != null) ? Convert.ToString(d[k]) : "";
    }
    private static object[] Arr(Dictionary<string, object> d, string k)
    {
        if (d != null && d.ContainsKey(k) && d[k] is object[]) return (object[])d[k];
        return new object[0];
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
