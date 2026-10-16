<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="InstructorShop.aspx.cs" Inherits="InstructorShop" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Shop - Instructor - SEA</title>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/@tabler/icons-webfont@3.11.0/dist/tabler-icons.min.css" />
    <link rel="stylesheet" href="Styles/sea-dashboard.css?v=7" />
    <link rel="stylesheet" href="Styles/sea-instructor.css?v=1" />
    <script>
        if (localStorage.getItem('sea-theme') === 'light') { document.documentElement.classList.add('light'); }
    </script>
</head>
<body id="pageBody" runat="server">
    <form id="form1" runat="server">

        <header class="sea-top">
            <div class="sea-left">
                <a href="InstructorDashboard.aspx" class="sea-brand" style="text-decoration:none;">SEA<img src="Assets/sea-octopus.svg" alt="octopus" style="height:2em;width:auto;vertical-align:middle;margin-left:6px;margin-bottom:-3px;" /></a>
                <span class="sea-pill ins-badge-pill"><i class="ti ti-chalkboard" aria-hidden="true"></i> INSTRUCTOR</span>
            </div>
            <nav class="sea-nav">
                <a class="sea-nlink" href="InstructorCreateQuiz.aspx"><i class="ti ti-plus" aria-hidden="true"></i> Create Quiz</a>
                <a class="sea-nlink" href="InstructorShop.aspx"><i class="ti ti-shopping-bag" aria-hidden="true"></i> Shop</a>
                <span class="sea-sep"></span>
                <a class="sea-nlink" href="Logout.aspx"><i class="ti ti-logout" aria-hidden="true"></i> Logout</a>
                <a href="InstructorProfile.aspx" class="sea-avatar"><asp:Literal ID="litNavAvatar" runat="server" Text="I" /></a>
            </nav>
        </header>

        <div class="quiz-wrap" style="max-width:1000px;margin:0 auto;padding:24px 20px 70px;">
            <a class="quiz-back" href="InstructorDashboard.aspx" style="display:inline-flex;align-items:center;gap:6px;color:var(--text-2);font-size:13px;font-weight:600;margin-bottom:14px;">
                <i class="ti ti-arrow-left"></i> Back to dashboard
            </a>

            <h1 class="sea-h">Shop</h1>
            <p class="sea-sub">Customise your avatar and page wallpaper. All items are free &mdash; just equip what you like.</p>

            <div style="display:flex;gap:24px;flex-wrap:wrap;align-items:flex-start;margin-bottom:22px;">
                <div style="background:var(--surf);border:1px solid var(--border);border-radius:16px;padding:18px;text-align:center;min-width:200px;">
                    <div style="font-size:11px;color:var(--text-2);letter-spacing:.06em;margin-bottom:8px;">YOUR AVATAR</div>
                    <asp:Literal ID="litAvatar" runat="server" />
                </div>
                <div style="flex:1;min-width:260px;">
                    <asp:Panel ID="pnlFlash" runat="server" Visible="false"
                        Style="background:rgba(143,174,126,.15);border:1px solid var(--olive);border-radius:10px;padding:11px 14px;font-size:13px;margin-bottom:12px;">
                        <asp:Literal ID="litFlash" runat="server" />
                    </asp:Panel>

                    <section class="ins-shop-grid">
                        <asp:Repeater ID="rptItems" runat="server" OnItemCommand="rptItems_ItemCommand">
                            <ItemTemplate>
                                <div class='ins-shop-card <%# (string)Eval("EquipStatus") == "Equipped" ? "equipped" : "" %>'>
                                    <div class="ins-shop-ic"><%# (string)Eval("Swatch") != "" ? Eval("Swatch") : "<i class='ti " + Eval("Icon") + "'></i>" %></div>
                                    <div class="ins-shop-name"><%# Eval("ItemName") %></div>
                                    <div class="ins-shop-type"><%# Eval("Type") %></div>
                                    <asp:Button runat="server" CssClass='<%# (string)Eval("EquipStatus") == "Equipped" ? "ins-btn ghost" : "ins-btn" %>'
                                        CommandName='<%# (string)Eval("EquipStatus") == "Equipped" ? "Unequip" : "Equip" %>'
                                        CommandArgument='<%# Eval("ItemID") %>'
                                        Text='<%# (string)Eval("EquipStatus") == "Equipped" ? "Unequip" : "Equip" %>' />
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </section>

                    <asp:Panel ID="pnlEmpty" runat="server" Visible="false" CssClass="ins-panel" Style="text-align:center;color:var(--text-2);">
                        No avatar items are available yet.
                    </asp:Panel>
                </div>
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
    </script>
</body>
</html>
