using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI.WebControls;

// Web Application project (CodeBehind + designer file, no namespace).
// Instructor shop: ONLY avatar/skin items (hair, hat, glasses, clothes, earrings).
// Everything is free - the instructor just equips what they want.
public partial class InstructorShop : System.Web.UI.Page
{
    private string ConnStr
    {
        get { return ConfigurationManager.ConnectionStrings["SEAConnection"].ConnectionString; }
    }
    private int InstructorId { get { return Convert.ToInt32(Session["InstructorID"]); } }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["InstructorID"] == null) { Response.Redirect("Login.aspx"); return; }
        EnsureTable();
        if (!IsPostBack)
        {
            litNavAvatar.Text = NavAvatar();
            Bind();
        }
    }

    // Instructors need their own equip table (Inventory is keyed to StudentID).
    private void EnsureTable()
    {
        try
        {
            using (SqlConnection cn = new SqlConnection(ConnStr))
            using (SqlCommand cmd = new SqlCommand(
                "IF OBJECT_ID('dbo.InstructorInventory','U') IS NULL " +
                "CREATE TABLE dbo.InstructorInventory (" +
                "  InstructorInventoryID INT IDENTITY(1,1) PRIMARY KEY," +
                "  InstructorID INT NOT NULL," +
                "  ItemID INT NOT NULL," +
                "  EquipStatus NVARCHAR(20) NOT NULL DEFAULT 'Unequipped')", cn))
            {
                cn.Open();
                cmd.ExecuteNonQuery();
            }
        }
        catch { }
    }

    private void Bind()
    {
        DataTable dt = new DataTable();
        using (SqlConnection cn = new SqlConnection(ConnStr))
        using (SqlCommand cmd = new SqlCommand(
            "SELECT i.ItemID, i.ItemName, i.Type, ISNULL(i.ItemPath,'') AS ItemPath, " +
            "  ISNULL(inv.EquipStatus,'') AS EquipStatus " +
            "FROM Item i " +
            "LEFT JOIN InstructorInventory inv ON inv.ItemID = i.ItemID AND inv.InstructorID = @i " +
            // cosmetics AND wallpapers - everything except quiz boosters
            "WHERE (i.ItemPath LIKE 'avatar:%' OR i.Type = 'Background') " +
            "AND i.Type NOT LIKE '%Booster%' " +
            "ORDER BY CASE WHEN i.Type='Background' THEN 1 ELSE 0 END, i.Type, i.ItemID", cn))
        {
            cmd.Parameters.AddWithValue("@i", InstructorId);
            using (SqlDataAdapter da = new SqlDataAdapter(cmd)) da.Fill(dt);
        }

        dt.Columns.Add("Icon", typeof(string));
        dt.Columns.Add("Swatch", typeof(string));
        foreach (DataRow r in dt.Rows)
        {
            string ty = Convert.ToString(r["Type"]);
            r["Icon"] = IconFor(ty);
            r["Swatch"] = (ty == "Background")
                ? "<span style=\"display:inline-block;width:64px;height:38px;border-radius:8px;border:1px solid var(--border);background:" +
                  System.Web.HttpUtility.HtmlEncode(Convert.ToString(r["ItemPath"])) + ";\"></span>"
                : "";
        }

        rptItems.DataSource = dt;
        rptItems.DataBind();
        pnlEmpty.Visible = (dt.Rows.Count == 0);

        litAvatar.Text = Avatar.Render(GetEquippedKeys());

        // paint the equipped wallpaper behind the page, like the student shop
        string wall = InstructorTheme.GetWallpaper(ConnStr, InstructorId);
        if (!string.IsNullOrEmpty(wall)) pageBody.Style["background"] = wall;
        else pageBody.Style.Remove("background");
    }

    private List<string> GetEquippedKeys()
    {
        List<string> keys = new List<string>();
        using (SqlConnection cn = new SqlConnection(ConnStr))
        using (SqlCommand cmd = new SqlCommand(
            "SELECT i.ItemPath FROM InstructorInventory inv " +
            "JOIN Item i ON i.ItemID = inv.ItemID " +
            "WHERE inv.InstructorID = @i AND inv.EquipStatus = 'Equipped' AND i.ItemPath LIKE 'avatar:%'", cn))
        {
            cmd.Parameters.AddWithValue("@i", InstructorId);
            cn.Open();
            using (SqlDataReader r = cmd.ExecuteReader())
                while (r.Read()) keys.Add(Convert.ToString(r[0]));
        }
        return keys;
    }

    protected void rptItems_ItemCommand(object source, RepeaterCommandEventArgs e)
    {
        int itemId;
        if (!int.TryParse(Convert.ToString(e.CommandArgument), out itemId)) return;

        if (e.CommandName == "Equip") SetEquip(itemId, "Equipped");
        else if (e.CommandName == "Unequip") SetEquip(itemId, "Unequipped");

        Bind();
    }

    private void SetEquip(int itemId, string status)
    {
        using (SqlConnection cn = new SqlConnection(ConnStr))
        {
            cn.Open();

            // make sure the instructor "owns" the row (free - no purchase needed)
            using (SqlCommand cmd = new SqlCommand(
                "IF NOT EXISTS (SELECT 1 FROM InstructorInventory WHERE InstructorID=@i AND ItemID=@it) " +
                "INSERT INTO InstructorInventory (InstructorID, ItemID, EquipStatus) VALUES (@i,@it,'Unequipped')", cn))
            {
                cmd.Parameters.AddWithValue("@i", InstructorId);
                cmd.Parameters.AddWithValue("@it", itemId);
                cmd.ExecuteNonQuery();
            }

            if (status == "Equipped")
            {
                // only one item per avatar slot (hair / hat / glasses / clothes / ear)
                using (SqlCommand cmd = new SqlCommand(
                    "UPDATE inv SET inv.EquipStatus='Unequipped' " +
                    "FROM InstructorInventory inv JOIN Item i ON i.ItemID = inv.ItemID " +
                    "CROSS APPLY (SELECT Type, ISNULL(ItemPath,'') AS Path FROM Item WHERE ItemID=@it) AS eq " +
                    "WHERE inv.InstructorID=@i AND inv.ItemID <> @it " +
                    "AND ( i.Type = eq.Type " +
                    "   OR ( i.ItemPath LIKE 'avatar:%' AND eq.Path LIKE 'avatar:%' " +
                    "        AND LEFT(SUBSTRING(i.ItemPath, 8, 100), CHARINDEX('_', SUBSTRING(i.ItemPath, 8, 100) + '_') - 1) " +
                    "          = LEFT(SUBSTRING(eq.Path, 8, 100), CHARINDEX('_', SUBSTRING(eq.Path, 8, 100) + '_') - 1) ) )", cn))
                {
                    cmd.Parameters.AddWithValue("@i", InstructorId);
                    cmd.Parameters.AddWithValue("@it", itemId);
                    cmd.ExecuteNonQuery();
                }
            }

            using (SqlCommand cmd = new SqlCommand(
                "UPDATE InstructorInventory SET EquipStatus=@st WHERE InstructorID=@i AND ItemID=@it", cn))
            {
                cmd.Parameters.AddWithValue("@st", status);
                cmd.Parameters.AddWithValue("@i", InstructorId);
                cmd.Parameters.AddWithValue("@it", itemId);
                cmd.ExecuteNonQuery();
            }
        }

        pnlFlash.Visible = true;
        litFlash.Text = (status == "Equipped") ? "Item equipped." : "Item unequipped.";
    }

    private string IconFor(string type)
    {
        switch ((type ?? "").ToLower())
        {
            case "head": return "ti-hat";
            case "hair": return "ti-user";
            case "clothes": return "ti-shirt";
            case "frame": return "ti-frame";
            case "background": return "ti-photo";
            case "glasses": return "ti-eyeglass";
            default: return "ti-sparkles";
        }
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
