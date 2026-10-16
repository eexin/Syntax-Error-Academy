<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AdminItemShop.aspx.cs" Inherits="AdminItemShop" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Item Shop - Admin - SEA</title>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/@tabler/icons-webfont@3.11.0/dist/tabler-icons.min.css" />
    <link rel="stylesheet" href="Styles/sea-dashboard.css?v=7" />
    <link rel="stylesheet" href="Styles/sea-admin.css?v=2" />
    <script>
        if (localStorage.getItem('sea-theme') === 'light') { document.documentElement.classList.add('light'); }
    </script>
</head>
<body>
    <form id="form1" runat="server">

        <header class="sea-top">
            <div class="sea-left">
                <a href="AdminDashboard.aspx" class="sea-brand" style="text-decoration:none;" title="Back to dashboard">SEA<img src="Assets/sea-octopus.svg" alt="octopus" style="height:2em;width:auto;vertical-align:middle;margin-left:6px;margin-bottom:-3px;" /></a>
                <span class="sea-pill streak" style="background:var(--coral-bg);color:var(--coral);">
                    <i class="ti ti-shield-lock" aria-hidden="true"></i> ADMIN
                </span>
            </div>
            <nav class="adm-nav">
                <a class="adm-nlink" href="AdminViewUser.aspx"><i class="ti ti-users" aria-hidden="true"></i> View User</a>

                <div class="adm-drop">
                    <span class="adm-nlink"><i class="ti ti-list-check" aria-hidden="true"></i> Quiz Management <i class="ti ti-chevron-down" style="font-size:14px;"></i></span>
                    <div class="adm-drop-menu">
                        <a href="AdminViewQuestion.aspx"><i class="ti ti-eye" aria-hidden="true"></i> View Quiz</a>
                        <a href="AdminQuizApproval.aspx"><i class="ti ti-checkup-list" aria-hidden="true"></i> Quiz Approval</a>
                    </div>
                </div>

                <div class="adm-drop">
                    <span class="adm-nlink"><i class="ti ti-report-analytics" aria-hidden="true"></i> Generate Report <i class="ti ti-chevron-down" style="font-size:14px;"></i></span>
                    <div class="adm-drop-menu">
                        <a href="AdminQuizReport.aspx"><i class="ti ti-chart-bar" aria-hidden="true"></i> Quiz Report</a>
                        <a href="AdminSummaryReport.aspx"><i class="ti ti-file-analytics" aria-hidden="true"></i> Summary Report</a>
                    </div>
                </div>

                <a class="adm-nlink active" href="AdminItemShop.aspx"><i class="ti ti-building-store" aria-hidden="true"></i> Item Shop</a>

                <span class="sea-sep"></span>
                <a class="sea-nlink" href="Logout.aspx"><i class="ti ti-logout" aria-hidden="true"></i> Logout</a>
                <span class="sea-avatar"><asp:Literal ID="litInitials" runat="server" Text="A" /></span>
            </nav>
        </header>

        <div class="adm-wrap">
            <h2 class="adm-h">Item Shop</h2>
            <p class="adm-sub">Add new items for students to buy, or remove existing ones.</p>

            <asp:Panel ID="pnlFlash" runat="server" Visible="false" CssClass="adm-flash show">
                <asp:Literal ID="litFlash" runat="server" />
            </asp:Panel>

            <%-- ============ ADD ITEM ============ --%>
            <div class="adm-toolbar" style="align-items:flex-end;flex-wrap:wrap;gap:12px;">
                <div class="adm-toolbar-left" style="align-items:flex-end;flex-wrap:wrap;gap:12px;">
                    <div class="sum-field show">
                        <label>Item name</label>
                        <asp:TextBox ID="txtName" runat="server" placeholder="e.g. Ocean Waves" />
                    </div>
                    <div class="sum-field show">
                        <label>Description</label>
                        <asp:TextBox ID="txtDesc" runat="server" placeholder="Short description" Style="min-width:220px;" />
                    </div>
                    <div class="sum-field show">
                        <label>Type</label>
                        <asp:DropDownList ID="ddlType" runat="server" ClientIDMode="Static" AutoPostBack="true" OnSelectedIndexChanged="ddlType_Changed">
                            <asp:ListItem Value="Background" Text="Background (wallpaper)" />
                            <asp:ListItem Value="Avatar" Text="Avatar cosmetic" />
                        </asp:DropDownList>
                    </div>
                    <asp:Panel ID="pnlBg" runat="server" CssClass="sum-field show">
                        <label>Background CSS &mdash; paste a value, pick a preset, or upload an image below</label>
                        <asp:TextBox ID="txtCss" runat="server" ClientIDMode="Static" placeholder="linear-gradient(...)" Style="min-width:290px;" />
                    </asp:Panel>
                    <asp:Panel ID="pnlAvatar" runat="server" CssClass="sum-field show" Visible="false">
                        <label>Body part (where it is worn)</label>
                        <asp:DropDownList ID="ddlPart" runat="server" Style="min-width:150px;">
                            <asp:ListItem Value="hat" Text="Hat" />
                            <asp:ListItem Value="glasses" Text="Glasses" />
                            <asp:ListItem Value="frame" Text="Frame" />
                            <asp:ListItem Value="hair" Text="Hair" />
                            <asp:ListItem Value="ear" Text="Earrings" />
                            <asp:ListItem Value="clothes" Text="Clothes" />
                        </asp:DropDownList>
                    </asp:Panel>
                    <asp:Panel ID="pnlSvg" runat="server" CssClass="sum-field show" Visible="false">
                        <label>Or paste SVG code (starts with &lt;svg&gt;)</label>
                        <asp:TextBox ID="txtSvg" runat="server" TextMode="MultiLine" Rows="2"
                            placeholder="<svg viewBox=&quot;0 0 100 100&quot;>...</svg>" Style="min-width:280px;font-family:monospace;font-size:11px;" />
                    </asp:Panel>
                    <div class="sum-field show">
                        <label>Upload image (PNG/JPG/SVG, transparent PNG works best)</label>
                        <asp:FileUpload ID="fuImage" runat="server" Style="font-size:12px;color:var(--text-2);" />
                    </div>
                    <div class="sum-field show">
                        <label>Price (points)</label>
                        <asp:DropDownList ID="ddlPrice" runat="server" Style="min-width:110px;">
                            <asp:ListItem Value="0" Text="Free" />
                            <asp:ListItem Value="50" Text="50" /><asp:ListItem Value="100" Text="100" />
                            <asp:ListItem Value="150" Text="150" /><asp:ListItem Value="200" Text="200" />
                            <asp:ListItem Value="250" Text="250" /><asp:ListItem Value="300" Text="300" />
                            <asp:ListItem Value="350" Text="350" /><asp:ListItem Value="400" Text="400" />
                            <asp:ListItem Value="450" Text="450" /><asp:ListItem Value="500" Text="500" />
                            <asp:ListItem Value="550" Text="550" /><asp:ListItem Value="600" Text="600" />
                            <asp:ListItem Value="650" Text="650" /><asp:ListItem Value="700" Text="700" />
                            <asp:ListItem Value="750" Text="750" /><asp:ListItem Value="800" Text="800" />
                            <asp:ListItem Value="850" Text="850" /><asp:ListItem Value="900" Text="900" />
                            <asp:ListItem Value="950" Text="950" /><asp:ListItem Value="1000" Text="1000" />
                        </asp:DropDownList>
                    </div>
                </div>
                <div class="adm-toolbar-right">
                    <asp:Button ID="btnAdd" runat="server" CssClass="adm-btn" Text="Add Item" OnClick="btnAdd_Click" />
                </div>
            </div>

            <asp:Panel ID="pnlPresets" runat="server" Style="display:flex;gap:8px;flex-wrap:wrap;margin:-6px 0 18px;">
                <span style="font-size:11px;color:var(--text-2);align-self:center;">Presets:</span>
                <button type="button" class="adm-tab" onclick="setCss('linear-gradient(160deg,#0e2a3a,#123f52 55%,#0a2130)')">Deep Ocean</button>
                <button type="button" class="adm-tab" onclick="setCss('radial-gradient(50% 60% at 25% 20%,rgba(255,200,90,.35),transparent 60%),linear-gradient(160deg,#3a2a12,#241a0c)')">Golden Hour</button>
                <button type="button" class="adm-tab" onclick="setCss('repeating-linear-gradient(45deg,rgba(255,255,255,.05) 0 12px,transparent 12px 24px),linear-gradient(160deg,#1d1d28,#12121a)')">Diagonal Lines</button>
                <button type="button" class="adm-tab" onclick="setCss('radial-gradient(circle,rgba(255,255,255,.18) 1px,transparent 1.5px) 0 0/22px 22px,linear-gradient(160deg,#101018,#0a0a12)')">Dotted Night</button>
            </asp:Panel>

            <%-- ============ CURRENT ITEMS ============ --%>
            <div class="adm-table-wrap">
                <table class="adm-table">
                    <thead>
                        <tr><th style="width:70px;">Preview</th><th>ID</th><th>Name</th><th>Description</th><th>Type</th><th>Price</th><th style="width:90px;"></th></tr>
                    </thead>
                    <tbody>
                        <asp:Repeater ID="rptItems" runat="server" OnItemCommand="rptItems_ItemCommand">
                            <ItemTemplate>
                                <tr>
                                    <td><%# Eval("Preview") %></td>
                                    <td><%# Eval("ItemID") %></td>
                                    <td><%# Eval("ItemName") %></td>
                                    <td style="color:var(--text-2);"><%# Eval("ItemDescription") %></td>
                                    <td><%# Eval("Type") %></td>
                                    <td><%# Eval("Price") %> pts</td>
                                    <td>
                                        <asp:LinkButton runat="server" CssClass="adm-mini-btn danger" CommandName="Del"
                                            CommandArgument='<%# Eval("ItemID") %>'
                                            OnClientClick="return confirm('Delete this item? Students who own it will lose it.');">
                                            <i class="ti ti-trash"></i> Delete
                                        </asp:LinkButton>
                                    </td>
                                </tr>
                            </ItemTemplate>
                        </asp:Repeater>
                    </tbody>
                </table>
            </div>
        </div>

        <button type="button" id="seaToggle" class="sea-toggle" aria-label="Toggle dark and light mode">
            <i class="ti ti-sun" aria-hidden="true"></i>
        </button>
    </form>

    <script>
        (function () {
            var btn = document.getElementById('seaToggle');
            function paint() { btn.innerHTML = document.documentElement.classList.contains('light') ? '<i class="ti ti-moon"></i>' : '<i class="ti ti-sun"></i>'; }
            paint();
            btn.addEventListener('click', function () {
                var light = document.documentElement.classList.toggle('light');
                localStorage.setItem('sea-theme', light ? 'light' : 'dark'); paint();
            });
        })();
        function setCss(v) {
            var t = document.getElementById('txtCss');
            if (t) { t.value = v; }
        }
    </script>
</body>
</html>
