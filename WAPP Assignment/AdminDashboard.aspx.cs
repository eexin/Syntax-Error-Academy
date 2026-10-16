using System;
using System.Configuration;
using System.Data.SqlClient;

// Web Application project (CodeBehind + designer file, no namespace).
public partial class AdminDashboard : System.Web.UI.Page
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
            EnsureSetup();   // self-healing: add admin columns + demo data if missing

            string name = Convert.ToString(Session["AdminName"]);
            lblWelcome.Text = Server.HtmlEncode(string.IsNullOrEmpty(name) ? "admin" : name);
            litInitials.Text = GetInitials(name);
        }
    }

    // Makes any copy of the database work with the admin panel:
    //  - adds the RejectReason / AnsweredDate columns the admin features use
    //  - seeds instructor Jas + 5 PENDING C# quizzes so Approval & Reports have data
    private void EnsureSetup()
    {
        try
        {
            using (SqlConnection cn = new SqlConnection(ConnStr))
            {
                cn.Open();
                Run(cn, "IF COL_LENGTH('dbo.Quiz','RejectReason') IS NULL ALTER TABLE dbo.Quiz ADD RejectReason NVARCHAR(500) NULL");
                Run(cn, "IF COL_LENGTH('dbo.StudentsAnswer','AnsweredDate') IS NULL ALTER TABLE dbo.StudentsAnswer ADD AnsweredDate DATETIME NULL");
                SeedDemo(cn);
            }
        }
        catch { /* never block the dashboard if the DB user lacks ALTER rights */ }
    }

    private void Run(SqlConnection cn, string sql)
    {
        using (SqlCommand cmd = new SqlCommand(sql, cn)) cmd.ExecuteNonQuery();
    }

    private void SeedDemo(SqlConnection cn)
    {
        using (SqlCommand chk = new SqlCommand("SELECT COUNT(*) FROM Instructor WHERE InstructorEmail='jas@gmail.com'", cn))
            if (Convert.ToInt32(chk.ExecuteScalar()) > 0) return;

        int jas;
        using (SqlCommand cmd = new SqlCommand(
            "INSERT INTO Instructor (InstructorName, InstructorDOB, InstructorEmail, InstructorPassword) " +
            "VALUES ('Jas','1990-06-15','jas@gmail.com'," +
            "'8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92'); SELECT SCOPE_IDENTITY();", cn))
            jas = Convert.ToInt32(cmd.ExecuteScalar());

        SeedQuiz[] quizzes = new SeedQuiz[]
        {
            new SeedQuiz("C# Basics","Variables, data types and printing output in C#.",1, new SeedQ[]{
                new SeedQ("Which keyword declares a variable whose type is inferred by the compiler?", new string[]{"var","dynamic","object","auto"},0),
                new SeedQ("Which method prints a line of text to the console?", new string[]{"Console.WriteLine()","Console.Print()","System.Out()","Console.Log()"},0),
                new SeedQ("Which data type best stores a whole number like 42?", new string[]{"int","string","bool","char"},0),
                new SeedQ("How do you write a single-line comment in C#?", new string[]{"// comment","# comment","<!-- comment -->","** comment"},0),
                new SeedQ("Which type holds the value true or false?", new string[]{"bool","byte","flag","bit"},0),
            }),
            new SeedQuiz("C# Operators & Expressions","Arithmetic, comparison and logical operators.",2, new SeedQ[]{
                new SeedQ("What is the result of 7 % 3 in C#?", new string[]{"1","2","3","0"},0),
                new SeedQ("Which operator checks equality of two values?", new string[]{"==","=","!=","=>"},0),
                new SeedQ("Which operator means logical AND?", new string[]{"&&","||","&","AND"},0),
                new SeedQ("What does the ++ operator do to an int variable?", new string[]{"Increases it by 1","Doubles it","Resets it to 0","Decreases it by 1"},0),
                new SeedQ("What is the value of 5 > 2 && 2 > 3 ?", new string[]{"false","true","5","error"},0),
            }),
            new SeedQuiz("C# Control Flow","if / else, switch and loops.",3, new SeedQ[]{
                new SeedQ("Which statement runs a block while a condition stays true?", new string[]{"while","if","switch","break"},0),
                new SeedQ("Which keyword exits a loop immediately?", new string[]{"break","continue","return","exit"},0),
                new SeedQ("A for loop with i from 0 while i < 3 runs how many times?", new string[]{"3","2","4","Infinite"},0),
                new SeedQ("Which keyword provides a default branch in a switch?", new string[]{"default","else","case","otherwise"},0),
                new SeedQ("Which keyword skips to the next loop iteration?", new string[]{"continue","break","skip","next"},0),
            }),
            new SeedQuiz("C# Methods & Parameters","Defining methods, return types and parameters.",4, new SeedQ[]{
                new SeedQ("Which return type means a method returns no value?", new string[]{"void","null","empty","none"},0),
                new SeedQ("What keyword sends a value back from a method?", new string[]{"return","send","yield","give"},0),
                new SeedQ("What are the values passed into a method call called?", new string[]{"Arguments","Fields","Properties","Events"},0),
                new SeedQ("Which modifier lets a method be called without creating an object?", new string[]{"static","public","virtual","const"},0),
                new SeedQ("Two methods with the same name but different parameters is called?", new string[]{"Overloading","Overriding","Hiding","Casting"},0),
            }),
            new SeedQuiz("C# Arrays & Collections","Arrays, indexing and the List collection.",5, new SeedQ[]{
                new SeedQ("What is the index of the first element in a C# array?", new string[]{"0","1","-1","null"},0),
                new SeedQ("How do you declare an int array of size 5?", new string[]{"int[] a = new int[5];","int a = new int(5);","array<int> a = 5;","int a[5];"},0),
                new SeedQ("Which property gives the number of elements in an array?", new string[]{"Length","Size","Count()","Total"},0),
                new SeedQ("Which collection can grow and shrink dynamically?", new string[]{"List<T>","Array","int[]","string"},0),
                new SeedQ("Which method adds an item to a List?", new string[]{"Add()","Push()","Insert()","Append()"},0),
            }),
        };

        foreach (SeedQuiz z in quizzes)
        {
            int quizId;
            using (SqlCommand cmd = new SqlCommand(
                "INSERT INTO Quiz (QuizTitle, QuizDescription, InstructorID, Level, QuizStatus) " +
                "VALUES (@t,@d,@i,@l,'Pending'); SELECT SCOPE_IDENTITY();", cn))
            {
                cmd.Parameters.AddWithValue("@t", z.Title);
                cmd.Parameters.AddWithValue("@d", z.Desc);
                cmd.Parameters.AddWithValue("@i", jas);
                cmd.Parameters.AddWithValue("@l", z.Level);
                quizId = Convert.ToInt32(cmd.ExecuteScalar());
            }
            foreach (SeedQ q in z.Qs)
            {
                int qid;
                using (SqlCommand cmd = new SqlCommand(
                    "INSERT INTO Question (QuestionText, QuestionType, Score, QuizID) " +
                    "VALUES (@x,'MCQ',100,@qz); SELECT SCOPE_IDENTITY();", cn))
                {
                    cmd.Parameters.AddWithValue("@x", q.Text);
                    cmd.Parameters.AddWithValue("@qz", quizId);
                    qid = Convert.ToInt32(cmd.ExecuteScalar());
                }
                for (int i = 0; i < q.Opts.Length; i++)
                    using (SqlCommand cmd = new SqlCommand(
                        "INSERT INTO MCQOption (QuestionID, OptionText, IsCorrect) VALUES (@q,@o,@c)", cn))
                    {
                        cmd.Parameters.AddWithValue("@q", qid);
                        cmd.Parameters.AddWithValue("@o", q.Opts[i]);
                        cmd.Parameters.AddWithValue("@c", i == q.Correct ? 1 : 0);
                        cmd.ExecuteNonQuery();
                    }
            }
        }
    }

    private class SeedQuiz
    {
        public string Title, Desc; public int Level; public SeedQ[] Qs;
        public SeedQuiz(string t, string d, int l, SeedQ[] qs) { Title = t; Desc = d; Level = l; Qs = qs; }
    }
    private class SeedQ
    {
        public string Text; public string[] Opts; public int Correct;
        public SeedQ(string t, string[] o, int c) { Text = t; Opts = o; Correct = c; }
    }

    private string GetInitials(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "A";
        string[] parts = name.Trim().Split(' ');
        if (parts.Length == 1) return parts[0].Substring(0, 1).ToUpper();
        return (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpper();
    }
}
