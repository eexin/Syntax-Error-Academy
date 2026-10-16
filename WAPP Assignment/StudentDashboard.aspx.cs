using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Text;

// Web Application project (CodeBehind + designer file, no namespace).
public partial class StudentDashboard : System.Web.UI.Page
{
    private string ConnStr
    {
        get { return ConfigurationManager.ConnectionStrings["SEAConnection"].ConnectionString; }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        // ---- Authentication gate: only logged-in students may view this page ----
        if (Session["StudentID"] == null)
        {
            Response.Redirect("Login.aspx");
            return;
        }

        if (!IsPostBack)
        {
            int sid = Convert.ToInt32(Session["StudentID"]);
            Streak.EnsureCurrent(ConnStr, sid);   // reset to 0 if a day was missed

            // Apply the wallpaper the student bought + equipped in the Item Shop (if any).
            string wall = Wallpaper.GetEquipped(ConnStr, sid);
            if (!string.IsNullOrEmpty(wall))
                pageBody.Style["background"] = wall;

            LoadHeader(sid);
            BindDashboard(sid);
        }
    }

    // ---------- Top bar: initials, coins (Total Point), streak ----------
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

    private class LevelInfo
    {
        public int LevelNumber;
        public int Total;       // quizzes in this level
        public int Solved;      // quizzes fully answered CORRECTLY
        public bool Unlocked;
        public bool Completed;
    }

    private void BindDashboard(int studentId)
    {
        List<LevelInfo> levels = LoadLevels(studentId);

        // Level 1 is always open; a later level opens once the student has done
        // at least ONE quiz in the previous level. Because this is based on saved
        // answers, an unlocked level stays unlocked.
        bool prevHasProgress = true;
        foreach (LevelInfo lv in levels)            // ordered by level number
        {
            lv.Unlocked = (lv.LevelNumber == 1) || prevHasProgress;
            lv.Completed = lv.Total > 0 && lv.Solved == lv.Total;
            prevHasProgress = lv.Solved >= 1;       // one solved quiz unlocks the next level
        }

        int selected;
        if (!int.TryParse(Request.QueryString["level"], out selected))
            selected = 0;

        LevelInfo current = levels.Find(x => x.LevelNumber == selected && x.Unlocked);
        if (current == null)
        {
            current = levels.Find(x => x.Unlocked && !x.Completed)
                   ?? levels.Find(x => x.Unlocked)
                   ?? (levels.Count > 0 ? levels[0] : null);
        }
        if (current == null) return;
        selected = current.LevelNumber;

        BindLevelRail(levels, selected);
        BindTrack(studentId);
        BindQuizzes(studentId, selected);
    }

    // ---------- Personal level track (battle-pass style, above the quizzes) ----------
    // The student has an infinite personal level, separate from the quiz levels in
    // the sidebar. Completing a whole quiz advances the personal level by that
    // quiz's Level: a Level 1 quiz = +1 move, a Level 2 quiz = +2 moves, etc.
    // Personal level = 1 + total moves earned.
    //
    // The track shows a sliding 7-tier window around the current tier, so it
    // works no matter how high the level gets. Past tiers show a check, the
    // current tier holds the avatar pin, and upcoming tiers are dimmed.
    private const int TrackWindow = 7;

    private void BindTrack(int studentId)
    {
        int personal = 1 + GetPersonalMoves(studentId);

        // Where was the pin the last time this student saw the dashboard?
        int from = GetLastShownLevel(studentId);
        if (from < 1) from = 1;
        if (from > personal) from = personal;        // never slide backwards

        // Sliding window: keep the current tier at the 5th of 7 stops once
        // possible, leaving two upcoming tiers visible ahead of the pin.
        int windowStart = Math.Max(1, personal - 4);

        // The client script works with stop indices inside the window.
        int toIdx = personal - windowStart + 1;
        int fromIdx = Math.Max(1, from - windowStart + 1);
        trackWrap.Attributes["data-pin-level"] = toIdx.ToString();
        trackWrap.Attributes["data-from-level"] = fromIdx.ToString();

        // The avatar head inside the pin marker.
        litPinAvatar.Text = Avatar.Render(Avatar.GetEquippedKeys(ConnStr, studentId), true);

        // Current tier badge in the track's corner label.
        litTrackLevel.Text = personal.ToString();

        // Build the visible tiers.
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < TrackWindow; i++)
        {
            int tier = windowStart + i;
            string cls = "sea-stop";
            if (tier < personal) cls += " done";
            else if (tier == personal) cls += " current";

            // Every dot shows its tier number; the coral fill marks completed tiers.
            sb.AppendFormat(
                "<span class='{0}'><span class='sea-stop-dot'>{1}</span></span>",
                cls, tier);
        }
        litTrackStops.Text = sb.ToString();

        // Persist the new position so the slide animation plays exactly once,
        // and pay the battle-pass reward: 10 item points per tier gained.
        if (from != personal)
        {
            int reward = (personal - from) * 10;
            AddTierReward(studentId, reward);
            trackWrap.Attributes["data-reward"] = reward.ToString();
            SetLastShownLevel(studentId, personal);
        }
    }

    // Reward goes to TotalPoint (the spendable shop currency) only, so the
    // leaderboard score (TotalScore) still reflects quiz answers alone.
    private void AddTierReward(int studentId, int points)
    {
        using (SqlConnection cn = new SqlConnection(ConnStr))
        using (SqlCommand cmd = new SqlCommand(
            "UPDATE Student SET TotalPoint = TotalPoint + @p WHERE StudentID = @sid", cn))
        {
            cmd.Parameters.AddWithValue("@p", points);
            cmd.Parameters.AddWithValue("@sid", studentId);
            cn.Open();
            cmd.ExecuteNonQuery();
        }
    }

    // Total moves earned = SUM of Quiz.Level over every quiz where the student
    // has answered EVERY question correctly. Wrong answers never advance the
    // track; a question counts from the first time it becomes correct (same
    // "first correct answer" rule the quiz uses for awarding question points).
    private int GetPersonalMoves(int studentId)
    {
        const string sql =
            "SELECT ISNULL(SUM(z.Level), 0) FROM Quiz z " +
            "WHERE (SELECT COUNT(*) FROM Question q WHERE q.QuizID = z.QuizID) > 0 " +
            "  AND (SELECT COUNT(*) FROM Question q WHERE q.QuizID = z.QuizID) = " +
            "      (SELECT COUNT(*) FROM Question q " +
            "         JOIN StudentsAnswer sa ON sa.QuestionID = q.QuestionID AND sa.StudentID = @sid " +
            "              AND sa.QuestionStatus = 'Correct' " +
            "         WHERE q.QuizID = z.QuizID)";

        using (SqlConnection cn = new SqlConnection(ConnStr))
        using (SqlCommand cmd = new SqlCommand(sql, cn))
        {
            cmd.Parameters.AddWithValue("@sid", studentId);
            cn.Open();
            return Convert.ToInt32(cmd.ExecuteScalar());
        }
    }

    private int GetLastShownLevel(int studentId)
    {
        using (SqlConnection cn = new SqlConnection(ConnStr))
        using (SqlCommand cmd = new SqlCommand(
            "SELECT ISNULL(LastShownLevel, 1) FROM Student WHERE StudentID = @sid", cn))
        {
            cmd.Parameters.AddWithValue("@sid", studentId);
            cn.Open();
            object o = cmd.ExecuteScalar();
            return o == null || o == DBNull.Value ? 1 : Convert.ToInt32(o);
        }
    }

    private void SetLastShownLevel(int studentId, int level)
    {
        using (SqlConnection cn = new SqlConnection(ConnStr))
        using (SqlCommand cmd = new SqlCommand(
            "UPDATE Student SET LastShownLevel = @lvl WHERE StudentID = @sid", cn))
        {
            cmd.Parameters.AddWithValue("@lvl", level);
            cmd.Parameters.AddWithValue("@sid", studentId);
            cn.Open();
            cmd.ExecuteNonQuery();
        }
    }

    // Per-level totals: how many quizzes exist, and how many the student finished.
    // A quiz is "completed" when the student has answered every question in it.
    private List<LevelInfo> LoadLevels(int studentId)
    {
        const string sql =
            "WITH QuizProgress AS ( " +
            "  SELECT z.QuizID, z.Level, " +
            "    (SELECT COUNT(*) FROM Question q WHERE q.QuizID = z.QuizID) AS QCount, " +
            "    (SELECT COUNT(*) FROM Question q " +
            "       JOIN StudentsAnswer sa ON sa.QuestionID = q.QuestionID AND sa.StudentID = @sid " +
            "            AND sa.QuestionStatus = 'Correct' " +
            "       WHERE q.QuizID = z.QuizID) AS AnsCount " +
            "  FROM Quiz z WHERE z.QuizStatus NOT IN ('Pending','Rejected') ) " +
            "SELECT Level, COUNT(*) AS Total, " +
            "  SUM(CASE WHEN QCount > 0 AND AnsCount = QCount THEN 1 ELSE 0 END) AS Solved " +
            "FROM QuizProgress GROUP BY Level ORDER BY Level";

        List<LevelInfo> list = new List<LevelInfo>();
        using (SqlConnection cn = new SqlConnection(ConnStr))
        using (SqlCommand cmd = new SqlCommand(sql, cn))
        {
            cmd.Parameters.AddWithValue("@sid", studentId);
            cn.Open();
            using (SqlDataReader r = cmd.ExecuteReader())
            {
                while (r.Read())
                {
                    list.Add(new LevelInfo
                    {
                        LevelNumber = Convert.ToInt32(r["Level"]),
                        Total = Convert.ToInt32(r["Total"]),
                        Solved = Convert.ToInt32(r["Solved"])
                    });
                }
            }
        }
        return list;
    }

    private void BindLevelRail(List<LevelInfo> levels, int selected)
    {
        DataTable dt = new DataTable();
        dt.Columns.Add("LevelNumber", typeof(int));
        dt.Columns.Add("Url");
        dt.Columns.Add("StateClass");
        dt.Columns.Add("IconClass");

        foreach (LevelInfo lv in levels)
        {
            string state;
            if (lv.LevelNumber == selected) state = "active";
            else if (lv.Completed) state = "done";
            else if (!lv.Unlocked) state = "locked";
            else state = "";

            string icon = lv.Completed ? "ti-circle-check"
                        : (!lv.Unlocked ? "ti-lock" : "ti-player-play");

            string url = lv.Unlocked ? "StudentDashboard.aspx?level=" + lv.LevelNumber : "#";

            dt.Rows.Add(lv.LevelNumber, url, state, icon);
        }

        rptLevels.DataSource = dt;
        rptLevels.DataBind();
    }

    private void BindQuizzes(int studentId, int level)
    {
        // Each card = one quiz. Points = sum of its question scores.
        // Status: 2 = done (every question answered CORRECTLY),
        //         1 = attempted but not all correct yet (retry),
        //         0 = not started.
        const string sql =
            "SELECT z.QuizID, z.QuizTitle, " +
            "  (SELECT ISNULL(SUM(q.Score),0) FROM Question q WHERE q.QuizID = z.QuizID) AS Points, " +
            "  CASE " +
            "    WHEN (SELECT COUNT(*) FROM Question q WHERE q.QuizID = z.QuizID) > 0 " +
            "     AND (SELECT COUNT(*) FROM Question q WHERE q.QuizID = z.QuizID) = " +
            "         (SELECT COUNT(*) FROM Question q " +
            "            JOIN StudentsAnswer sa ON sa.QuestionID = q.QuestionID AND sa.StudentID = @sid " +
            "                 AND sa.QuestionStatus = 'Correct' " +
            "            WHERE q.QuizID = z.QuizID) THEN 2 " +
            "    WHEN EXISTS (SELECT 1 FROM Question q " +
            "                   JOIN StudentsAnswer sa ON sa.QuestionID = q.QuestionID AND sa.StudentID = @sid " +
            "                   WHERE q.QuizID = z.QuizID) THEN 1 " +
            "    ELSE 0 END AS Status " +
            "FROM Quiz z WHERE z.Level = @lvl AND z.QuizStatus NOT IN ('Pending','Rejected') ORDER BY z.QuizID";

        DataTable dt = new DataTable();
        using (SqlConnection cn = new SqlConnection(ConnStr))
        using (SqlCommand cmd = new SqlCommand(sql, cn))
        {
            cmd.Parameters.AddWithValue("@sid", studentId);
            cmd.Parameters.AddWithValue("@lvl", level);
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                da.Fill(dt);
        }

        int total = dt.Rows.Count;
        int done = 0;
        foreach (DataRow row in dt.Rows)
            if (Convert.ToInt32(row["Status"]) == 2) done++;

        int earned = GetEarnedPoints(studentId, level);

        lblLevelTitle.Text = "Level " + level;
        lblLevelSub.Text = done + " / " + total + " quizzes solved &middot; " + earned + " pts earned";

        // INLINE CSS written from live data (the progress fill width).
        int percent = total > 0 ? (int)Math.Round(100.0 * done / total) : 0;
        barFill.Style["width"] = percent + "%";

        rptQuizzes.DataSource = dt;
        rptQuizzes.DataBind();
        pnlEmpty.Visible = (total == 0);
    }

    // Points earned in a level = sum of Score for questions answered Correct.
    private int GetEarnedPoints(int studentId, int level)
    {
        const string sql =
            "SELECT ISNULL(SUM(q.Score),0) FROM Question q " +
            "JOIN Quiz z ON z.QuizID = q.QuizID " +
            "JOIN StudentsAnswer sa ON sa.QuestionID = q.QuestionID " +
            "  AND sa.StudentID = @sid AND sa.QuestionStatus = 'Correct' " +
            "WHERE z.Level = @lvl AND z.QuizStatus NOT IN ('Pending','Rejected')";

        using (SqlConnection cn = new SqlConnection(ConnStr))
        using (SqlCommand cmd = new SqlCommand(sql, cn))
        {
            cmd.Parameters.AddWithValue("@sid", studentId);
            cmd.Parameters.AddWithValue("@lvl", level);
            cn.Open();
            return Convert.ToInt32(cmd.ExecuteScalar());
        }
    }

    // ---------- Helpers used by the .aspx data-binding ----------
    protected string GetStatusIcon(object status)
    {
        switch (Convert.ToInt32(status))
        {
            case 2: return "ti-circle-check icon-ok";     // all questions correct
            case 1: return "ti-refresh icon-retry";       // attempted, has wrong answers
            default: return "ti-player-play icon-go";     // not started
        }
    }

    private string GetInitials(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "U";
        string[] parts = name.Trim().Split(' ');
        if (parts.Length == 1) return parts[0].Substring(0, 1).ToUpper();
        return (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpper();
    }
}