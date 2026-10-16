using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Text;

// Web Application project (CodeBehind + designer file).
public partial class Leaderboard : System.Web.UI.Page
{
    private string ConnStr
    {
        get { return ConfigurationManager.ConnectionStrings["SEAConnection"].ConnectionString; }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["StudentID"] == null)
        {
            Response.Redirect("Login.aspx");
            return;
        }

        ApplyWallpaper();

        if (!IsPostBack)
        {
            int sid = Convert.ToInt32(Session["StudentID"]);
            LoadHeader(sid);
            LoadBoard(sid);
        }
    }

    private void LoadHeader(int studentId)
    {
        using (SqlConnection cn = new SqlConnection(ConnStr))
        using (SqlCommand cmd = new SqlCommand(
            "SELECT StudentName, TotalPoint, Streak FROM Student WHERE StudentID = @sid", cn))
        {
            cmd.Parameters.AddWithValue("@sid", studentId);
            cn.Open();
            using (SqlDataReader r = cmd.ExecuteReader())
            {
                if (r.Read())
                {
                    litInitials.Text = Avatar.Render(Avatar.GetEquippedKeys(ConnStr, studentId), true);
                    lblCoins.Text = Convert.ToInt32(r["TotalPoint"]).ToString("N0");
                    lblStreak.Text = Convert.ToInt32(r["Streak"]).ToString();
                }
            }
        }
    }

    private void LoadBoard(int meId)
    {
        DataTable src = new DataTable();
        using (SqlConnection cn = new SqlConnection(ConnStr))
        using (SqlCommand cmd = new SqlCommand(
            "SELECT TOP 100 StudentID, StudentName, TotalScore, Streak " +
            "FROM Student ORDER BY TotalScore DESC, StudentName", cn))
        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
            da.Fill(src);

        // ONE query for every listed student's equipped avatar cosmetics
        // (per-student Avatar.GetEquippedKeys calls would be 100 round-trips).
        Dictionary<int, List<string>> keyMap = LoadAvatarKeyMap();

        // Winner's podium from the top 3.
        litPodium.Text = BuildPodium(src, meId, keyMap);

        // The list continues from rank #4 (the podium already shows 1-3).
        DataTable dt = new DataTable();
        dt.Columns.Add("Rank", typeof(int));
        dt.Columns.Add("AvatarHtml");
        dt.Columns.Add("Name");
        dt.Columns.Add("Streak", typeof(int));
        dt.Columns.Add("Score");
        dt.Columns.Add("RowClass");
        dt.Columns.Add("IsMe", typeof(bool));

        int rank = 0, myRank = 0, total = src.Rows.Count;
        foreach (DataRow row in src.Rows)
        {
            rank++;
            int sid = Convert.ToInt32(row["StudentID"]);
            bool isMe = sid == meId;
            if (isMe) myRank = rank;         // rank counts everyone, podium included
            if (rank <= 3) continue;         // top 3 live on the podium only

            dt.Rows.Add(
                rank,
                Avatar.Render(GetKeys(keyMap, sid), true),
                Server.HtmlEncode(row["StudentName"].ToString()),
                Convert.ToInt32(row["Streak"]),
                Convert.ToInt32(row["TotalScore"]).ToString("N0"),
                isMe ? "me" : "",
                isMe);
        }

        rptBoard.DataSource = dt;
        rptBoard.DataBind();

        lblSub.Text = myRank > 0
            ? ("You're ranked #" + myRank + " of " + total + " students")
            : (total + " students ranked by score");
    }

    // All equipped avatar layers for the TOP 100, grouped per student, in one query.
    private Dictionary<int, List<string>> LoadAvatarKeyMap()
    {
        Dictionary<int, List<string>> map = new Dictionary<int, List<string>>();
        using (SqlConnection cn = new SqlConnection(ConnStr))
        using (SqlCommand cmd = new SqlCommand(
            "SELECT inv.StudentID, i.ItemPath FROM Inventory inv " +
            "JOIN Item i ON i.ItemID = inv.ItemID " +
            "WHERE inv.EquipStatus = 'Equipped' AND i.ItemPath LIKE 'avatar:%' " +
            "AND inv.StudentID IN (SELECT TOP 100 StudentID FROM Student " +
            "ORDER BY TotalScore DESC, StudentName)", cn))
        {
            cn.Open();
            using (SqlDataReader r = cmd.ExecuteReader())
            {
                while (r.Read())
                {
                    int sid = Convert.ToInt32(r["StudentID"]);
                    if (!map.ContainsKey(sid)) map[sid] = new List<string>();
                    map[sid].Add(r["ItemPath"].ToString());
                }
            }
        }
        return map;
    }

    private static List<string> GetKeys(Dictionary<int, List<string>> map, int sid)
    {
        return map.ContainsKey(sid) ? map[sid] : new List<string>();
    }

    // ------------------------------------------------------------------
    //  Winner's podium (2nd | 1st | 3rd) built as HTML. Handles 0/1/2/3
    //  students gracefully: only the columns that exist are rendered.
    // ------------------------------------------------------------------
    private string BuildPodium(DataTable src, int meId, Dictionary<int, List<string>> keyMap)
    {
        int n = Math.Min(3, src.Rows.Count);
        if (n == 0) return "";

        StringBuilder sb = new StringBuilder();
        sb.Append("<div class='pd-stage'>");
        sb.Append("<div class='pd-spot'></div><div class='pd-halo'></div>");

        // celebratory confetti (looping CSS animation, negative delays = mid-air on load)
        string[] confetti = {
            "left:8%;background:#ffd166;animation-duration:5.6s;animation-delay:-2.1s",
            "left:20%;background:#6fb3d2;animation-duration:6.4s;animation-delay:-4.4s",
            "left:31%;background:#ff9485;animation-duration:5.1s;animation-delay:-1.2s",
            "left:43%;background:#b39ddb;animation-duration:6.8s;animation-delay:-3.3s",
            "left:55%;background:#ffd166;animation-duration:5.4s;animation-delay:-0.6s",
            "left:66%;background:#8fae7e;animation-duration:6.1s;animation-delay:-5.0s",
            "left:78%;background:#6fb3d2;animation-duration:5.8s;animation-delay:-2.8s",
            "left:90%;background:#ff9485;animation-duration:6.6s;animation-delay:-1.7s"
        };
        for (int i = 0; i < confetti.Length; i++)
            sb.Append("<i class='pd-cf").Append(i % 3 == 2 ? " dot" : "")
              .Append("' style='").Append(confetti[i]).Append("'></i>");

        sb.Append("<div class='pd-row'>");
        if (n >= 2) AppendPodiumColumn(sb, 2, src.Rows[1], meId, keyMap);
        AppendPodiumColumn(sb, 1, src.Rows[0], meId, keyMap);
        if (n >= 3) AppendPodiumColumn(sb, 3, src.Rows[2], meId, keyMap);
        sb.Append("</div></div>");
        return sb.ToString();
    }

    private void AppendPodiumColumn(StringBuilder sb, int rank, DataRow row, int meId,
                                    Dictionary<int, List<string>> keyMap)
    {
        int sid = Convert.ToInt32(row["StudentID"]);
        bool isMe = sid == meId;
        List<string> keys = GetKeys(keyMap, sid);
        string name = Server.HtmlEncode(row["StudentName"].ToString());
        string score = Convert.ToInt32(row["TotalScore"]).ToString("N0");

        sb.Append("<div class='pd-col pd-c").Append(rank).Append("'>");

        // ---- avatar (full body) with rank decorations layered over it
        sb.Append("<div class='pd-avatar'>");
        if (rank == 1)
        {
            sb.Append("<svg class='pd-spark pd-s1' viewBox='0 0 20 20' aria-hidden='true'><path d='" + STAR + "' fill='#ffd166'/></svg>");
            sb.Append("<svg class='pd-spark pd-s2' viewBox='0 0 20 20' aria-hidden='true'><path d='" + STAR + "' fill='#ffd166'/></svg>");
            sb.Append("<svg class='pd-spark pd-s3' viewBox='0 0 20 20' aria-hidden='true'><path d='" + STAR + "' fill='#ffd166'/></svg>");
        }
        sb.Append("<span class='pd-avbox'>").Append(Avatar.Render(keys, false)).Append("</span>");
        if (rank == 1)
        {
            // crown only when no hat is equipped (a crown on a cap looks odd)
            bool hasHat = keys.Exists(delegate (string k) { return k.StartsWith("avatar:hat_"); });
            if (!hasHat) sb.Append(CROWN_SVG);
            sb.Append(TROPHY_SVG);   // held in front of the torso
        }
        else
        {
            sb.Append(rank == 2 ? MEDAL_SILVER : MEDAL_BRONZE);
        }
        sb.Append("</div>");

        // ---- the block with its rank numeral
        sb.Append("<div class='pd-block pd-b").Append(rank)
          .Append("'><span class='pd-num'>").Append(rank).Append("</span></div>");

        // ---- glass nameplate (marked when it's the logged-in student)
        sb.Append("<div class='pd-plate").Append(isMe ? " pd-me" : "").Append("'>");
        if (isMe) sb.Append("<span class='pd-you'>You</span>");
        sb.Append("<span class='pd-name'>").Append(name).Append("</span>");
        sb.Append("<span class='pd-score'>").Append(score).Append("</span>");
        sb.Append("</div>");

        sb.Append("</div>");
    }

    // ---- original inline SVG decorations (no image files, no icon font) ----
    private const string STAR = "M10 0 l2.6 7.4 7.4 2.6 -7.4 2.6 -2.6 7.4 -2.6 -7.4 -7.4 -2.6 7.4 -2.6 z";

    private const string TROPHY_SVG =
        "<svg class='pd-trophy' viewBox='0 0 60 46' aria-hidden='true'>" +
        "<path d='M12 8 q-11 3 -2 14' stroke='#e3a93d' stroke-width='4' fill='none'/>" +
        "<path d='M48 8 q11 3 2 14' stroke='#e3a93d' stroke-width='4' fill='none'/>" +
        "<rect x='12' y='3' width='36' height='7' rx='3' fill='#ffd166'/>" +
        "<path d='M14 10 h32 q-1 17 -16 20 q-15 -3 -16 -20 z' fill='#ffd166'/>" +
        "<path d='M20 15 q1 8 6 10' stroke='rgba(255,255,255,.6)' stroke-width='2.5' fill='none' stroke-linecap='round'/>" +
        "<rect x='26' y='30' width='8' height='6' fill='#e3a93d'/>" +
        "<rect x='18' y='36' width='24' height='6' rx='2.5' fill='#e3a93d'/>" +
        "<circle cx='13' cy='16' r='5' fill='#d9a679'/>" +
        "<circle cx='47' cy='16' r='5' fill='#d9a679'/>" +
        "</svg>";

    private const string CROWN_SVG =
        "<svg class='pd-crown' viewBox='0 0 40 22' aria-hidden='true'>" +
        "<path d='M3 18 L7 4 L14 11 L20 2 L26 11 L33 4 L37 18 Z' fill='#ffd166' stroke='#e3a93d' stroke-width='1'/>" +
        "<rect x='3' y='16' width='34' height='5' rx='2' fill='#e3a93d'/>" +
        "</svg>";

    private const string MEDAL_SILVER =
        "<svg class='pd-medal' viewBox='0 0 24 34' aria-hidden='true'>" +
        "<polygon points='7,0 17,0 12,12' fill='#d8543c'/>" +
        "<polygon points='10,0 14,0 12,7' fill='#a83f2e'/>" +
        "<circle cx='12' cy='18' r='8.5' fill='#d7d7d7' stroke='#9e9e9e' stroke-width='1.6'/>" +
        "<text x='12' y='22' text-anchor='middle' font-family='Consolas,monospace' font-size='10' font-weight='700' fill='#6d6d6d'>2</text>" +
        "</svg>";

    private const string MEDAL_BRONZE =
        "<svg class='pd-medal' viewBox='0 0 24 34' aria-hidden='true'>" +
        "<polygon points='7,0 17,0 12,12' fill='#d8543c'/>" +
        "<polygon points='10,0 14,0 12,7' fill='#a83f2e'/>" +
        "<circle cx='12' cy='18' r='8.5' fill='#d08b4e' stroke='#a8672a' stroke-width='1.6'/>" +
        "<text x='12' y='22' text-anchor='middle' font-family='Consolas,monospace' font-size='10' font-weight='700' fill='#7a4a1d'>3</text>" +
        "</svg>";

    // Apply the wallpaper the student bought + equipped in the Item Shop (if any).
    // Runs on every load (including postbacks) so the background is always current.
    private void ApplyWallpaper()
    {
        string wall = Wallpaper.GetEquipped(ConnStr, Convert.ToInt32(Session["StudentID"]));
        if (!string.IsNullOrEmpty(wall))
            pageBody.Style["background"] = wall;
    }
}
