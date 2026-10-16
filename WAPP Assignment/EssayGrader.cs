using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;   // System.Web.Extensions

// Grades a free-text (Essay) answer by sending it to an n8n workflow.
// The AI API key lives inside n8n - this app stores no key, only the webhook URL.
//
// Web Site project: place this file in App_Code (auto-compiled).
// Web Application project: just include the .cs anywhere in the project.
//
// Config (web.config <appSettings>):
//   N8nEssayWebhookUrl - paste your n8n Webhook URL here
//   N8nAuthToken       - optional; sent as an Authorization header if your
//                        webhook is protected (leave blank if it isn't)
//
// Contract between this app and n8n:
//   POST  { "question": "...", "expected": "...", "answer": "..." }
//   reply { "verdict": "Correct" | "Wrong", "feedback": "one short sentence" }
public static class EssayGrader
{
    public class GradeResult
    {
        public bool Available;     // false if no URL / the call failed
        public bool IsCorrect;
        public string Feedback = "";
    }

    private static readonly HttpClient http = new HttpClient();

    static EssayGrader()
    {
        // Some servers need TLS 1.2 enabled explicitly for the HTTPS call.
        try { ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12; } catch { }
        http.Timeout = TimeSpan.FromSeconds(60);   // n8n + the model can take a few seconds
    }

    public static GradeResult Grade(string question, string expectedAnswer, string studentAnswer)
    {
        GradeResult result = new GradeResult();

        string url = ConfigurationManager.AppSettings["N8nEssayWebhookUrl"];
        if (string.IsNullOrWhiteSpace(url))
        {
            result.Available = false;
            result.Feedback = "No n8n webhook URL configured.";
            return result;
        }

        try
        {
            // Task.Run keeps the blocking call off the ASP.NET request thread,
            // which avoids an async deadlock in Web Forms.
            string body = Task.Run(() =>
                CallWebhookAsync(url, question, expectedAnswer, studentAnswer)
            ).GetAwaiter().GetResult();

            ParseVerdict(body, result);
            result.Available = true;
        }
        catch (Exception ex)
        {
            result.Available = false;
            result.Feedback = "AI grading error: " + ex.Message;
        }
        return result;
    }

    private static async Task<string> CallWebhookAsync(string url, string question, string expected, string answer)
    {
        var payload = new { question = question ?? "", expected = expected ?? "", answer = answer ?? "" };
        string json = new JavaScriptSerializer().Serialize(payload);

        HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, url);

        string token = ConfigurationManager.AppSettings["N8nAuthToken"];
        if (!string.IsNullOrWhiteSpace(token))
            req.Headers.Add("Authorization", token);

        req.Content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage resp = await http.SendAsync(req).ConfigureAwait(false);
        string respText = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
        if (!resp.IsSuccessStatusCode)
            throw new Exception("Webhook returned " + (int)resp.StatusCode + ": " + respText);
        return respText;
    }

    private static void ParseVerdict(string responseBody, GradeResult result)
    {
        try
        {
            string jsonPart = ExtractJson(responseBody);
            JavaScriptSerializer ser = new JavaScriptSerializer();
            object parsed = ser.DeserializeObject(jsonPart);

            // n8n may return the object directly, or wrapped in an array.
            Dictionary<string, object> obj = parsed as Dictionary<string, object>;
            if (obj == null && parsed is object[] && ((object[])parsed).Length > 0)
                obj = ((object[])parsed)[0] as Dictionary<string, object>;

            if (obj != null && obj.ContainsKey("verdict"))
            {
                string verdict = Convert.ToString(obj["verdict"]);
                result.IsCorrect = verdict.Trim().Equals("Correct", StringComparison.OrdinalIgnoreCase);
                result.Feedback = obj.ContainsKey("feedback") ? Convert.ToString(obj["feedback"]) : "";
                return;
            }
        }
        catch { /* fall through to the text check below */ }

        // Fallback: read the verdict straight from the text.
        string lower = responseBody.ToLowerInvariant();
        if (lower.Contains("incorrect") || lower.Contains("wrong")) result.IsCorrect = false;
        else if (lower.Contains("correct")) result.IsCorrect = true;
        result.Feedback = responseBody.Trim();
    }

    private static string ExtractJson(string s)
    {
        int objStart = s.IndexOf('{');
        int arrStart = s.IndexOf('[');
        int start = (arrStart >= 0 && (objStart < 0 || arrStart < objStart)) ? arrStart : objStart;
        int end = Math.Max(s.LastIndexOf('}'), s.LastIndexOf(']'));
        if (start >= 0 && end > start) return s.Substring(start, end - start + 1);
        return s;
    }
}
