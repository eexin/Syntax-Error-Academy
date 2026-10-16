using System;
using System.Data.SqlClient;

// Returns the CSS background of the student's equipped "Background" shop item.
// We store the CSS value (a gradient, or a url(...) once you add image files) in
// Item.ItemPath for Background-type items, so no image assets are required to start.
//
// Web Application: place this file in the PROJECT ROOT (not App_Code).
public static class Wallpaper
{
    public static string GetEquipped(string connStr, int sid)
    {
        using (SqlConnection cn = new SqlConnection(connStr))
        using (SqlCommand cmd = new SqlCommand(
            "SELECT TOP 1 i.ItemPath FROM Inventory inv " +
            "JOIN Item i ON i.ItemID = inv.ItemID " +
            "WHERE inv.StudentID = @s AND inv.EquipStatus = 'Equipped' AND i.Type = 'Background'", cn))
        {
            cmd.Parameters.AddWithValue("@s", sid);
            cn.Open();
            object o = cmd.ExecuteScalar();
            return (o == null || o == DBNull.Value) ? null : o.ToString();
        }
    }
}
