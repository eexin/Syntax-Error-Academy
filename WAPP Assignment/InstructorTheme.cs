using System;
using System.Data.SqlClient;

// Returns the CSS background of the instructor's equipped "Background" shop item
// (mirrors Wallpaper.cs, which does the same for students).
// Web Application: place this file in the PROJECT ROOT (not App_Code).
public static class InstructorTheme
{
    public static string GetWallpaper(string connStr, int instructorId)
    {
        try
        {
            using (SqlConnection cn = new SqlConnection(connStr))
            using (SqlCommand cmd = new SqlCommand(
                "SELECT TOP 1 i.ItemPath FROM InstructorInventory inv " +
                "JOIN Item i ON i.ItemID = inv.ItemID " +
                "WHERE inv.InstructorID = @i AND inv.EquipStatus = 'Equipped' AND i.Type = 'Background'", cn))
            {
                cmd.Parameters.AddWithValue("@i", instructorId);
                cn.Open();
                object o = cmd.ExecuteScalar();
                return (o == null || o == DBNull.Value) ? null : o.ToString();
            }
        }
        catch { return null; } // table may not exist until the shop is opened once
    }
}
