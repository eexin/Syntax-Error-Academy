using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

// ============================================================
//  AiHint
//  Asks the n8n "hint" workflow for a nudge on a quiz question.
//
//  Built the same way as EssayGrader: this app stores only the
//  webhook URL, never an AI provider key. The key lives inside
//  n8n, so it is never deployed with the website.
//
//  Web.config appSettings used:
//    N8nHintWebhookUrl - the n8n Webhook URL for the hint flow
//    N8nAuthToken      - optional; reused from the grading flow
// ============================================================
public static class AiHint
{
    public class HintResult
    {
        public bool Available = false;
        public string Hint = "";
    }

    private static readonly HttpClient http = new HttpClient();

    static AiHint()
    {
        try { ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12; } catch { }
        http.Timeout = TimeSpan.FromSeconds(30);
    }

    public static HintResult GetHint(string question)
    {
        HintResult result = new HintResult();

        string url = ConfigurationManager.AppSettings["N8nHintWebhookUrl"];
        if (string.IsNullOrWhiteSpace(url))
        {
            result.Hint = "Hints are not switched on at the moment.";
            return result;
        }

        try
        {
            // Task.Run keeps the blocking wait off the ASP.NET request
            // thread, which avoids an async deadlock.
            string body = Task.Run(() => CallAsync(url, question)).GetAwaiter().GetResult();
            result.Hint = ParseHint(body);
            result.Available = !string.IsNullOrWhiteSpace(result.Hint);
            if (!result.Available) result.Hint = "No hint came back - try again in a moment.";
        }
        catch
        {
            // Never surface the raw error to a student.
            result.Available = false;
            result.Hint = "The hint service is busy right now. Please try again shortly.";
        }
        return result;
    }

    private static async Task<string> CallAsync(string url, string question)
    {
        var payload = new { question = question ?? "" };
        string json = new JavaScriptSerializer().Serialize(payload);

        HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, url);

        string token = ConfigurationManager.AppSettings["N8nAuthToken"];
        if (!string.IsNullOrWhiteSpace(token))
            req.Headers.Add("Authorization", token);

        req.Content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage resp = await http.SendAsync(req).ConfigureAwait(false);
        string text = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
        if (!resp.IsSuccessStatusCode)
            throw new Exception("Webhook returned " + (int)resp.StatusCode);
        return text;
    }

    private static string ParseHint(string responseBody)
    {
        try
        {
            JavaScriptSerializer ser = new JavaScriptSerializer();
            object parsed = ser.DeserializeObject(responseBody);

            // n8n may return the object directly or wrapped in an array.
            Dictionary<string, object> obj = parsed as Dictionary<string, object>;
            if (obj == null && parsed is object[] && ((object[])parsed).Length > 0)
                obj = ((object[])parsed)[0] as Dictionary<string, object>;

            if (obj != null && obj.ContainsKey("hint"))
                return Convert.ToString(obj["hint"]).Trim();
        }
        catch { /* fall through */ }

        // Fallback: treat the whole body as the hint, trimmed to a sane length.
        string plain = (responseBody ?? "").Trim();
        if (plain.Length > 300) plain = plain.Substring(0, 300);
        return plain;
    }
}
