<%@ WebHandler Language="C#" Class="HintHandler" %>

using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web;
using System.Web.SessionState;
using System.Web.Script.Serialization;

// ============================================================
//  HintHandler.ashx
//  Called by fetch() from Quiz.aspx when a student asks for a
//  hint. Returns { ok, hint } as JSON.
//
//  Using a handler instead of a postback means the page never
//  reloads, so answers already typed in are not lost.
//
//  The question text is looked up here on the SERVER from the
//  QuestionID, so a student cannot tamper with what gets sent.
//  The correct answer is deliberately NEVER sent to the AI or
//  to the browser - the hint is generated from the question
//  alone, so it can nudge without giving the answer away.
// ============================================================
public class HintHandler : IHttpHandler, IRequiresSessionState
{
    private string ConnStr
    {
        get { return ConfigurationManager.ConnectionStrings["SEAConnection"].ConnectionString; }
    }

    public void ProcessRequest(HttpContext ctx)
    {
        ctx.Response.ContentType = "application/json";
        ctx.Response.Cache.SetCacheability(HttpCacheability.NoCache);

        // Only a logged-in student may ask for a hint.
        if (ctx.Session["StudentID"] == null)
        {
            Write(ctx, false, "Please log in again to use hints.");
            return;
        }

        int qid;
        if (!int.TryParse(ctx.Request["qid"], out qid))
        {
            Write(ctx, false, "No question was specified.");
            return;
        }

        // Simple per-session throttle so the free AI quota is not burned
        // by holding the button down.
        object last = ctx.Session["LastHintUtc"];
        if (last != null && (DateTime.UtcNow - (DateTime)last).TotalSeconds < 3)
        {
            Write(ctx, false, "Just a moment before asking for another hint.");
            return;
        }
        ctx.Session["LastHintUtc"] = DateTime.UtcNow;

        string question = LookupQuestion(qid);
        if (string.IsNullOrEmpty(question))
        {
            Write(ctx, false, "That question could not be found.");
            return;
        }

        AiHint.HintResult r = AiHint.GetHint(question);
        Write(ctx, r.Available, r.Available ? r.Hint : r.Hint);
    }

    private string LookupQuestion(int qid)
    {
        using (SqlConnection cn = new SqlConnection(ConnStr))
        using (SqlCommand cmd = new SqlCommand(
            "SELECT QuestionText FROM Question WHERE QuestionID = @q", cn))
        {
            cmd.Parameters.AddWithValue("@q", qid);
            cn.Open();
            object o = cmd.ExecuteScalar();
            return (o == null || o == DBNull.Value) ? "" : o.ToString();
        }
    }

    private void Write(HttpContext ctx, bool ok, string hint)
    {
        var payload = new { ok = ok, hint = hint };
        ctx.Response.Write(new JavaScriptSerializer().Serialize(payload));
    }

    public bool IsReusable { get { return false; } }
}
