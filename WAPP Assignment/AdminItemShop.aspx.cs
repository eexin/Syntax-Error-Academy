using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web;
using System.Web.UI.WebControls;

// Web Application project (CodeBehind + designer file, no namespace).
// Lets the admin add new shop items (wallpapers or avatar cosmetics) and
// remove existing ones. Students buy these in ItemShop.aspx.
public partial class AdminItemShop : System.Web.UI.Page
{
    private string ConnStr
    {
        get { return ConfigurationManager.ConnectionStrings["SEAConnection"].ConnectionString; }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["AdminID"] == null) { Response.Redirect("Login.aspx"); return; }
        if (!IsPostBack)
        {
            EnsureSequentialItemIds();   // fix jumped IDs like 1002 -> next in sequence
            litInitials.Text = GetInitials(Convert.ToString(Session["AdminName"]));
            Bind();
        }
    }

    protected void ddlType_Changed(object sender, EventArgs e)
    {
        bool bg = ddlType.SelectedValue == "Background";
        pnlBg.Visible = bg;
        pnlPresets.Visible = bg;
        pnlAvatar.Visible = !bg;
        pnlSvg.Visible = !bg;
        Bind();
    }

    private void Bind()
    {
        DataTable dt = new DataTable();
        using (SqlConnection cn = new SqlConnection(ConnStr))
        using (SqlCommand cmd = new SqlCommand(
            "SELECT ItemID, ItemName, ISNULL(ItemDescription,'') AS ItemDescription, " +
            "ISNULL(ItemPath,'') AS ItemPath, Type, Price FROM Item " +
            "ORDER BY CASE WHEN Type='Background' THEN 1 ELSE 0 END, ItemID", cn))
        using (SqlDataAdapter da = new SqlDataAdapter(cmd)) da.Fill(dt);

        dt.Columns.Add("Preview", typeof(string));
        foreach (DataRow r in dt.Rows)
        {
            string path = Convert.ToString(r["ItemPath"]);
            if (Convert.ToString(r["Type"]) == "Background" && path != "")
                r["Preview"] = "<span style=\"display:inline-block;width:46px;height:30px;border-radius:6px;border:1px solid var(--border);background:" +
                               HttpUtility.HtmlEncode(path) + ";\"></span>";
            else if (path.StartsWith("avatar:img:"))
            {
                string[] pp = path.Split(new char[] { ':' }, 4);
                string url = pp.Length == 4 ? pp[3] : "";
                r["Preview"] = "<img src=\"" + HttpUtility.HtmlEncode(url) +
                               "\" style=\"width:44px;height:44px;object-fit:contain;border-radius:8px;border:1px solid var(--border);background:var(--surf-2);\" />";
            }
            else if (path.StartsWith("avatar:"))
                r["Preview"] = "<i class=\"ti ti-user-circle\" style=\"font-size:24px;color:var(--coral);\"></i>";
            else
                r["Preview"] = "<i class=\"ti ti-photo\" style=\"font-size:22px;color:var(--text-2);\"></i>";
            // encode text columns for safe display
            r["ItemName"] = HttpUtility.HtmlEncode(Convert.ToString(r["ItemName"]));
            r["ItemDescription"] = HttpUtility.HtmlEncode(Convert.ToString(r["ItemDescription"]));
        }

        rptItems.DataSource = dt;
        rptItems.DataBind();
    }

    protected void btnAdd_Click(object sender, EventArgs e)
    {
        string name = txtName.Text.Trim();
        string desc = txtDesc.Text.Trim();
        int price; if (!int.TryParse(ddlPrice.SelectedValue, out price) || price < 0) price = 0;

        if (name == "")
        {
            Flash("Item name is required.", false); Bind(); return;
        }

        string type, path;
        string uploaded = SaveUpload();          // null when no file was chosen
        if (uploaded == "INVALID")
        {
            Flash("Only PNG, JPG, GIF, SVG or WEBP images can be uploaded.", false); Bind(); return;
        }

        if (ddlType.SelectedValue == "Background")
        {
            type = "Background";
            if (uploaded != null)
                path = "url('" + uploaded + "') center / cover no-repeat";   // uploaded wallpaper image
            else
            {
                path = txtCss.Text.Trim();
                if (path == "")
                {
                    Flash("Paste a background CSS value, pick a preset, or upload an image.", false); Bind(); return;
                }
            }
        }
        else
        {
            string slot = ddlPart.SelectedValue;                              // hat / glasses / frame / hair / ear / clothes
            string svg = txtSvg.Text.Trim();

            if (uploaded != null)
            {
                // custom image cosmetic, positioned by the chosen body part
                path = "avatar:img:" + slot + ":" + uploaded;
                type = TypeOfSlot(slot);
            }
            else if (svg != "")
            {
                // pasted SVG code: saved as an .svg file, then worn like an uploaded image
                if (!svg.StartsWith("<svg", StringComparison.OrdinalIgnoreCase))
                {
                    Flash("Pasted code must be SVG (it should start with <svg>).", false); Bind(); return;
                }
                string dir = Server.MapPath("~/Assets/Items/Uploads");
                System.IO.Directory.CreateDirectory(dir);
                string svgFile = "item_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".svg";
                System.IO.File.WriteAllText(System.IO.Path.Combine(dir, svgFile), svg);
                path = "avatar:img:" + slot + ":Assets/Items/Uploads/" + svgFile;
                type = TypeOfSlot(slot);
            }
            else
            {
                Flash("Upload an image or paste SVG code for the cosmetic.", false); Bind(); return;
            }
        }

        // duplicate-name guard
        using (SqlConnection cn = new SqlConnection(ConnStr))
        {
            cn.Open();
            using (SqlCommand chk = new SqlCommand("SELECT COUNT(*) FROM Item WHERE ItemName=@n", cn))
            {
                chk.Parameters.AddWithValue("@n", name);
                if (Convert.ToInt32(chk.ExecuteScalar()) > 0)
                {
                    Flash("An item with that name already exists.", false); Bind(); return;
                }
            }
            int nextId;
            using (SqlCommand idCmd = new SqlCommand(
                "SELECT MIN(t.id) FROM (SELECT 1 AS id UNION ALL SELECT ItemID + 1 FROM Item) t " +
                "WHERE NOT EXISTS (SELECT 1 FROM Item x WHERE x.ItemID = t.id)", cn))
            {
                object o = idCmd.ExecuteScalar();
                nextId = (o == null || o == DBNull.Value) ? 1 : Convert.ToInt32(o);
            }
            using (SqlCommand cmd = new SqlCommand(
                "SET IDENTITY_INSERT Item ON;" +
                "INSERT INTO Item (ItemID, ItemName, ItemDescription, ItemPath, Type, Price) " +
                "VALUES (@id,@n,@d,@p,@t,@pr);" +
                "SET IDENTITY_INSERT Item OFF;", cn))
            {
                cmd.Parameters.AddWithValue("@id", nextId);
                cmd.Parameters.AddWithValue("@n", name);
                cmd.Parameters.AddWithValue("@d", desc == "" ? (object)DBNull.Value : desc);
                cmd.Parameters.AddWithValue("@p", path);
                cmd.Parameters.AddWithValue("@t", type);
                cmd.Parameters.AddWithValue("@pr", price);
                cmd.ExecuteNonQuery();
            }
        }

        txtName.Text = ""; txtDesc.Text = ""; txtCss.Text = ""; txtSvg.Text = "";
        ddlPrice.SelectedIndex = 0;
        EnsureSequentialItemIds();   // keep the grouped sequence after adding
        Flash("\"" + name + "\" was added to the shop.", true);
        Bind();
    }

    protected void rptItems_ItemCommand(object source, RepeaterCommandEventArgs e)
    {
        if (e.CommandName != "Del") return;
        int id;
        if (!int.TryParse(Convert.ToString(e.CommandArgument), out id)) return;

        using (SqlConnection cn = new SqlConnection(ConnStr))
        {
            cn.Open();
            using (SqlTransaction tx = cn.BeginTransaction())
            {
                Exec(cn, tx, "DELETE FROM Inventory WHERE ItemID=@id", id);
                TryExec(cn, tx, "DELETE FROM InstructorInventory WHERE ItemID=@id", id);
                Exec(cn, tx, "DELETE FROM Item WHERE ItemID=@id", id);
                tx.Commit();
            }
        }
        EnsureSequentialItemIds();   // close the gap left by the deleted item
        Flash("Item deleted.", true);
        Bind();
    }

    // ---------- helpers ----------
    // Keeps item IDs in a clean grouped sequence: avatar cosmetics first (1..k),
    // then backgrounds (k+1..n). Runs after every change, so jumped IDs like 1002
    // are folded back into the sequence and inventory links are moved with them.
    private void EnsureSequentialItemIds()
    {
        try
        {
            using (SqlConnection cn = new SqlConnection(ConnStr))
            {
                cn.Open();

                var ids = new System.Collections.Generic.List<int>();
                using (SqlCommand q = new SqlCommand(
                    "SELECT ItemID FROM Item " +
                    "ORDER BY CASE WHEN Type='Background' THEN 1 ELSE 0 END, ItemID", cn))
                using (SqlDataReader r = q.ExecuteReader())
                    while (r.Read()) ids.Add(r.GetInt32(0));

                bool ok = true;
                for (int i = 0; i < ids.Count; i++)
                    if (ids[i] != i + 1) { ok = false; break; }
                if (ok) return;

                const int OFF = 100000;   // temporary range so moves can never collide
                using (SqlTransaction tx = cn.BeginTransaction())
                {
                    // phase 1: park every out-of-place row in the temp range
                    for (int i = 0; i < ids.Count; i++)
                        if (ids[i] != i + 1) MoveItem(cn, tx, ids[i], i + 1 + OFF);

                    // phase 2: bring them down to their final sequential IDs
                    for (int i = 0; i < ids.Count; i++)
                        if (ids[i] != i + 1) MoveItem(cn, tx, i + 1 + OFF, i + 1);

                    using (SqlCommand rs = new SqlCommand(
                        "DECLARE @m INT = (SELECT ISNULL(MAX(ItemID),0) FROM Item);" +
                        "DBCC CHECKIDENT('Item', RESEED, @m);", cn, tx))
                        rs.ExecuteNonQuery();
                    tx.Commit();
                }
            }
        }
        catch { /* never block the page; the shop still works with jumped IDs */ }
    }

    // copies an item to a new ID, points all inventories at it, removes the old row
    private void MoveItem(SqlConnection cn, SqlTransaction tx, int oldId, int newId)
    {
        using (SqlCommand c = new SqlCommand(
            "SET IDENTITY_INSERT Item ON;" +
            "INSERT INTO Item (ItemID, ItemName, ItemDescription, ItemPath, Type, Price) " +
            "SELECT @new, ItemName, ItemDescription, ItemPath, Type, Price FROM Item WHERE ItemID=@old;" +
            "SET IDENTITY_INSERT Item OFF;" +
            "UPDATE Inventory SET ItemID=@new WHERE ItemID=@old;" +
            "IF OBJECT_ID('dbo.InstructorInventory','U') IS NOT NULL " +
            "  UPDATE InstructorInventory SET ItemID=@new WHERE ItemID=@old;" +
            "DELETE FROM Item WHERE ItemID=@old;", cn, tx))
        {
            c.Parameters.AddWithValue("@new", newId);
            c.Parameters.AddWithValue("@old", oldId);
            c.ExecuteNonQuery();
        }
    }

    // saves the chosen file into Assets/Items/Uploads and returns its relative path,
    // null when nothing was uploaded, or "INVALID" for a disallowed extension
    private string SaveUpload()
    {
        if (!fuImage.HasFile) return null;
        string ext = System.IO.Path.GetExtension(fuImage.FileName).ToLowerInvariant();
        if (ext != ".png" && ext != ".jpg" && ext != ".jpeg" && ext != ".gif" && ext != ".svg" && ext != ".webp")
            return "INVALID";

        string dir = Server.MapPath("~/Assets/Items/Uploads");
        System.IO.Directory.CreateDirectory(dir);
        string name = "item_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ext;
        fuImage.SaveAs(System.IO.Path.Combine(dir, name));
        return "Assets/Items/Uploads/" + name;
    }

    private string TypeOfSlot(string slot)
    {
        switch (slot)
        {
            case "hat": return "Head";
            case "glasses": return "Glasses";
            case "frame": return "Frame";
            case "hair": return "Hair";
            case "ear": return "Ear";
            case "clothes": return "Clothes";
            default: return "Head";
        }
    }

    private void Exec(SqlConnection cn, SqlTransaction tx, string sql, int id)
    {
        using (SqlCommand cmd = new SqlCommand(sql, cn, tx)) { cmd.Parameters.AddWithValue("@id", id); cmd.ExecuteNonQuery(); }
    }
    private void TryExec(SqlConnection cn, SqlTransaction tx, string sql, int id)
    {
        try { Exec(cn, tx, sql, id); } catch { }
    }
    private void Flash(string msg, bool ok)
    {
        pnlFlash.Visible = true;
        litFlash.Text = HttpUtility.HtmlEncode(msg);
        pnlFlash.Style["background"] = ok ? "rgba(143,174,126,.15)" : "rgba(224,87,63,.14)";
        pnlFlash.Style["border"] = "1px solid " + (ok ? "var(--olive)" : "#e0573f");
        pnlFlash.Style["border-radius"] = "10px";
        pnlFlash.Style["padding"] = "11px 14px";
        pnlFlash.Style["font-size"] = "13px";
        pnlFlash.Style["margin-bottom"] = "14px";
    }
    private string GetInitials(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "A";
        string[] parts = name.Trim().Split(' ');
        if (parts.Length == 1) return parts[0].Substring(0, 1).ToUpper();
        return (parts[0].Substring(0, 1) + parts[parts.Length - 1].Substring(0, 1)).ToUpper();
    }
}
