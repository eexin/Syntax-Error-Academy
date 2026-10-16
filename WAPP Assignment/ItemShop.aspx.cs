using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

// Web Application project (CodeBehind + designer file).
public partial class ItemShop : System.Web.UI.Page
{
    private string ConnStr
    {
        get { return ConfigurationManager.ConnectionStrings["SEAConnection"].ConnectionString; }
    }

    private int Sid { get { return Convert.ToInt32(Session["StudentID"]); } }

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
            LoadHeader();
            BindShop();
        }
    }

    private void LoadHeader()
    {
        using (SqlConnection cn = new SqlConnection(ConnStr))
        using (SqlCommand cmd = new SqlCommand(
            "SELECT StudentName, TotalPoint, Streak FROM Student WHERE StudentID = @sid", cn))
        {
            cmd.Parameters.AddWithValue("@sid", Sid);
            cn.Open();
            using (SqlDataReader r = cmd.ExecuteReader())
            {
                if (r.Read())
                {
                    litInitials.Text = Avatar.Render(Avatar.GetEquippedKeys(ConnStr, Sid), true);
                    int pts = Convert.ToInt32(r["TotalPoint"]);
                    lblCoins.Text = pts.ToString("N0");
                    lblBalance.Text = pts.ToString("N0");
                    lblStreak.Text = Convert.ToInt32(r["Streak"]).ToString();
                }
            }
        }
    }

    private int GetBalance()
    {
        using (SqlConnection cn = new SqlConnection(ConnStr))
        using (SqlCommand cmd = new SqlCommand("SELECT TotalPoint FROM Student WHERE StudentID = @sid", cn))
        {
            cmd.Parameters.AddWithValue("@sid", Sid);
            cn.Open();
            return Convert.ToInt32(cmd.ExecuteScalar());
        }
    }

    private void BindShop()
    {
        // Each item, plus whether this student owns it and has it equipped.
        const string sql =
            "SELECT i.ItemID, i.ItemName, i.ItemDescription, i.Type, i.Price, i.ItemPath, " +
            "  CASE WHEN inv.InventoryID IS NULL THEN 0 ELSE 1 END AS Owned, " +
            "  ISNULL(inv.EquipStatus,'') AS EquipStatus " +
            "FROM Item i " +
            "LEFT JOIN Inventory inv ON inv.ItemID = i.ItemID AND inv.StudentID = @sid " +
            "ORDER BY i.Price";

        DataTable dt = new DataTable();
        using (SqlConnection cn = new SqlConnection(ConnStr))
        using (SqlCommand cmd = new SqlCommand(sql, cn))
        {
            cmd.Parameters.AddWithValue("@sid", Sid);
            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                da.Fill(dt);
        }

        // Add display helper columns.
        dt.Columns.Add("Icon");
        dt.Columns.Add("Name");
        dt.Columns.Add("Description");
        dt.Columns.Add("Price2");
        foreach (DataRow row in dt.Rows)
        {
            row["Icon"] = IconFor(row["Type"].ToString());
            row["Name"] = Server.HtmlEncode(row["ItemName"].ToString());
            row["Description"] = Server.HtmlEncode(row["ItemDescription"].ToString());
            row["Price2"] = Convert.ToInt32(row["Price"]).ToString("N0");
        }

        rptShop.DataSource = dt;
        rptShop.DataBind();
    }

    protected void rptShop_ItemDataBound(object sender, RepeaterItemEventArgs e)
    {
        if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
            return;

        DataRowView drv = (DataRowView)e.Item.DataItem;
        bool owned = Convert.ToInt32(drv["Owned"]) == 1;
        bool equipped = drv["EquipStatus"].ToString() == "Equipped";
        int price = Convert.ToInt32(drv["Price"]);

        // Show the real wallpaper in the thumbnail instead of a generic icon.
        // Background items store their CSS background value in Item.ItemPath.
        HtmlGenericControl thumb = (HtmlGenericControl)e.Item.FindControl("thumb");
        string path = drv["ItemPath"] == DBNull.Value ? "" : drv["ItemPath"].ToString();
        if (drv["Type"].ToString() == "Background" && !string.IsNullOrEmpty(path))
        {
            thumb.Style["background"] = path;
            thumb.Attributes["class"] = "shop-thumb preview";   // hides the icon (CSS)
        }
        else if (path.StartsWith("avatar:"))
        {
            // Avatar cosmetics: preview the base avatar wearing just this item.
            Literal litPreview = (Literal)e.Item.FindControl("litPreview");
            litPreview.Text = Avatar.Render(new string[] { path }, false);
            thumb.Attributes["class"] = "shop-thumb preview avatar";
        }

        Button btnBuy = (Button)e.Item.FindControl("btnBuy");
        LinkButton btnEquip = (LinkButton)e.Item.FindControl("btnEquip");
        HtmlGenericControl lblOwned = (HtmlGenericControl)e.Item.FindControl("lblOwned");

        if (owned)
        {
            btnBuy.Visible = false;
            lblOwned.Visible = true;
            btnEquip.Visible = true;
            btnEquip.Text = equipped ? "Unequip" : "Equip";
            btnEquip.CommandName = equipped ? "Unequip" : "Equip";
        }
        else
        {
            lblOwned.Visible = false;
            btnEquip.Visible = false;
            btnBuy.Visible = true;
            btnBuy.Enabled = GetBalance() >= price;   // can only buy if affordable
            if (!btnBuy.Enabled) btnBuy.Text = "Not enough";
        }
    }

    protected void rptShop_ItemCommand(object source, RepeaterCommandEventArgs e)
    {
        int itemId = Convert.ToInt32(e.CommandArgument);

        if (e.CommandName == "Buy") BuyItem(itemId);
        else if (e.CommandName == "Equip") SetEquip(itemId, "Equipped");
        else if (e.CommandName == "Unequip") SetEquip(itemId, "Unequipped");

        // Re-apply so equipping a wallpaper updates the page background immediately.
        ApplyWallpaper();

        LoadHeader();
        BindShop();
    }

    private void BuyItem(int itemId)
    {
        int price;
        using (SqlConnection cn = new SqlConnection(ConnStr))
        using (SqlCommand cmd = new SqlCommand("SELECT Price FROM Item WHERE ItemID = @i", cn))
        {
            cmd.Parameters.AddWithValue("@i", itemId);
            cn.Open();
            object o = cmd.ExecuteScalar();
            if (o == null) return;
            price = Convert.ToInt32(o);
        }

        if (GetBalance() < price)
        {
            ShowMsg("You don't have enough points for that item.");
            return;
        }

        // INSERT into Inventory + UPDATE the student's points (the buy transaction).
        using (SqlConnection cn = new SqlConnection(ConnStr))
        {
            cn.Open();
            using (SqlCommand ins = new SqlCommand(
                "IF NOT EXISTS (SELECT 1 FROM Inventory WHERE StudentID=@s AND ItemID=@i) " +
                "INSERT INTO Inventory (StudentID, ItemID, EquipStatus) VALUES (@s, @i, 'Unequipped')", cn))
            {
                ins.Parameters.AddWithValue("@s", Sid);
                ins.Parameters.AddWithValue("@i", itemId);
                ins.ExecuteNonQuery();
            }
            using (SqlCommand upd = new SqlCommand(
                "UPDATE Student SET TotalPoint = TotalPoint - @p WHERE StudentID = @s", cn))
            {
                upd.Parameters.AddWithValue("@p", price);
                upd.Parameters.AddWithValue("@s", Sid);
                upd.ExecuteNonQuery();
            }
        }
        ShowMsg("Item purchased!");
    }

    private void SetEquip(int itemId, string status)
    {
        using (SqlConnection cn = new SqlConnection(ConnStr))
        {
            cn.Open();

            // Only one item per category at a time: equipping something unequips any
            // other item of the same Type (Background, Hair, Hat, ...) OR the same
            // avatar slot. The slot check compares the 'avatar:hat' / 'avatar:ear'
            // prefix of ItemPath, so two hats can never both stay equipped even if
            // their Type values in the database don't match.
            if (status == "Equipped")
            {
                using (SqlCommand pre = new SqlCommand(
                    "UPDATE inv SET inv.EquipStatus = 'Unequipped' " +
                    "FROM Inventory inv JOIN Item i ON i.ItemID = inv.ItemID " +
                    "CROSS APPLY (SELECT Type, ISNULL(ItemPath,'') AS Path FROM Item WHERE ItemID = @i) AS eq " +
                    "WHERE inv.StudentID = @s AND inv.ItemID <> @i " +
                    "AND ( i.Type = eq.Type " +
                    "      OR ( i.ItemPath LIKE 'avatar:%' AND eq.Path LIKE 'avatar:%' " +
                    "           AND LEFT(i.ItemPath, CHARINDEX('_', i.ItemPath + '_')) = " +
                    "               LEFT(eq.Path, CHARINDEX('_', eq.Path + '_')) ) )", cn))
                {
                    pre.Parameters.AddWithValue("@s", Sid);
                    pre.Parameters.AddWithValue("@i", itemId);
                    pre.ExecuteNonQuery();
                }
            }

            using (SqlCommand cmd = new SqlCommand(
                "UPDATE Inventory SET EquipStatus = @st WHERE StudentID = @s AND ItemID = @i", cn))
            {
                cmd.Parameters.AddWithValue("@st", status);
                cmd.Parameters.AddWithValue("@s", Sid);
                cmd.Parameters.AddWithValue("@i", itemId);
                cmd.ExecuteNonQuery();
            }
        }
    }

    private void ShowMsg(string text)
    {
        pnlMsg.Visible = true;
        lblMsg.Text = Server.HtmlEncode(text);
    }

    private string IconFor(string type)
    {
        switch (type)
        {
            case "Head": return "ti-mood-smile";
            case "Hair": return "ti-mood-smile";
            case "Hat": return "ti-crown";
            case "Earrings": return "ti-diamond";
            case "Glasses": return "ti-eyeglass";
            case "Clothes": return "ti-shirt";
            case "Background": return "ti-photo";
            case "Frame": return "ti-border-corners";
            default: return "ti-star";
        }
    }

    private string GetInitials(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "U";
        string[] parts = name.Trim().Split(' ');
        if (parts.Length == 1) return parts[0].Substring(0, 1).ToUpper();
        return (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpper();
    }

    // Apply the wallpaper the student bought + equipped in the Item Shop (if any).
    // Runs on every load (including postbacks) so the background is always current.
    private void ApplyWallpaper()
    {
        string wall = Wallpaper.GetEquipped(ConnStr, Sid);
        if (!string.IsNullOrEmpty(wall))
            pageBody.Style["background"] = wall;
    }

}