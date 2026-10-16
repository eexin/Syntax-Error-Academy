using System;
using System.Data.SqlClient;

// Daily-streak logic.
//   - RecordActivity: call when a student completes a quiz/challenge. Same day = no
//     change; yesterday = +1; otherwise (first time, or a day was missed) = restart at 1.
//   - EnsureCurrent: call on page load. If the last activity was before yesterday, the
//     streak is broken, so reset it to 0.
//
// Web Application: place this file in the PROJECT ROOT (not App_Code) so it compiles.
public static class Streak
{
    private static void Read(string connStr, int sid, out DateTime? last, out int streak)
    {
        last = null; streak = 0;
        using (SqlConnection cn = new SqlConnection(connStr))
        using (SqlCommand cmd = new SqlCommand(
            "SELECT LastActiveDate, Streak FROM Student WHERE StudentID = @s", cn))
        {
            cmd.Parameters.AddWithValue("@s", sid);
            cn.Open();
            using (SqlDataReader r = cmd.ExecuteReader())
            {
                if (!r.Read()) return;
                last = r["LastActiveDate"] == DBNull.Value
                    ? (DateTime?)null
                    : Convert.ToDateTime(r["LastActiveDate"]).Date;
                streak = Convert.ToInt32(r["Streak"]);
            }
        }
    }

    public static void EnsureCurrent(string connStr, int sid)
    {
        DateTime? last; int streak;
        Read(connStr, sid, out last, out streak);

        DateTime today = DateTime.Today;
        bool alive = last.HasValue && (last.Value == today || last.Value == today.AddDays(-1));

        if (!alive && streak != 0)
        {
            using (SqlConnection cn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand("UPDATE Student SET Streak = 0 WHERE StudentID = @s", cn))
            {
                cmd.Parameters.AddWithValue("@s", sid);
                cn.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }

    public static void RecordActivity(string connStr, int sid)
    {
        DateTime? last; int streak;
        Read(connStr, sid, out last, out streak);

        DateTime today = DateTime.Today;
        int newStreak;
        if (last.HasValue && last.Value == today) newStreak = streak;                 // already counted today
        else if (last.HasValue && last.Value == today.AddDays(-1)) newStreak = streak + 1; // continued
        else newStreak = 1;                                                            // first time or broken

        using (SqlConnection cn = new SqlConnection(connStr))
        using (SqlCommand cmd = new SqlCommand(
            "UPDATE Student SET Streak = @st, LastActiveDate = @d WHERE StudentID = @s", cn))
        {
            cmd.Parameters.AddWithValue("@st", newStreak);
            cmd.Parameters.AddWithValue("@d", today);
            cmd.Parameters.AddWithValue("@s", sid);
            cn.Open();
            cmd.ExecuteNonQuery();
        }
    }
}
