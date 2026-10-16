<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ItemShop.aspx.cs" Inherits="ItemShop" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Item Shop - Syntax Error Academy</title>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/@tabler/icons-webfont@3.11.0/dist/tabler-icons.min.css" />
    <link rel="stylesheet" href="Styles/sea-dashboard.css?v=3" />
    <link rel="stylesheet" href="Styles/sea-account.css?v=3" />
    <script>
        if (localStorage.getItem('sea-theme') === 'light') {
            document.documentElement.classList.add('light');
        }
    </script>
</head>
<body id="pageBody" runat="server">
    <form id="form1" runat="server">

        <header class="sea-top">
            <div class="sea-left">
                <a href="StudentDashboard.aspx" class="sea-brand" style="text-decoration:none;">SEA<img src="Assets/sea-octopus.svg" alt="octopus" style="height:2em;width:auto;vertical-align:middle;margin-left:6px;margin-bottom:-3px;" /></a>
                <span class="sea-pill streak">
                    <i class="ti ti-flame" aria-hidden="true"></i>
                    <asp:Literal ID="lblStreak" runat="server" Text="0" />
                </span>
            </div>
            <nav class="sea-nav">
                <span class="sea-pill coin">
                    <i class="ti ti-coin" aria-hidden="true"></i>
                    <asp:Literal ID="lblCoins" runat="server" Text="0" />
                </span>
                <span class="sea-sep"></span>
                <a class="sea-nlink" href="Games.aspx"><i class="ti ti-device-gamepad-2" aria-hidden="true"></i> Minigames</a>
                <a class="sea-nlink" href="DailyChallenge.aspx"><i class="ti ti-bolt" aria-hidden="true"></i> Daily</a>
                <a class="sea-nlink" href="Leaderboard.aspx"><i class="ti ti-trophy" aria-hidden="true"></i> Leaderboard</a>
                <a class="sea-nlink" href="ItemShop.aspx" style="color:var(--coral);"><i class="ti ti-shopping-bag" aria-hidden="true"></i> Shop</a>
                <a href="Profile.aspx" class="sea-avatar"><asp:Literal ID="litInitials" runat="server" Text="U" /></a>
            </nav>
        </header>

        <div class="acc-wrap">
            <a class="quiz-back" href="StudentDashboard.aspx"><i class="ti ti-arrow-left" aria-hidden="true"></i> Back to dashboard</a>
            <h1 class="acc-title">Item Shop</h1>
            <p class="acc-sub">Spend your points on cosmetics. You have <asp:Literal ID="lblBalance" runat="server" Text="0" /> points.</p>

            <asp:Panel ID="pnlMsg" runat="server" Visible="false" CssClass="acc-msg">
                <asp:Literal ID="lblMsg" runat="server" />
            </asp:Panel>

            <div class="shop-grid">
                <asp:Repeater ID="rptShop" runat="server"
                    OnItemDataBound="rptShop_ItemDataBound" OnItemCommand="rptShop_ItemCommand">
                    <ItemTemplate>
                        <div class="shop-card">
                            <div class="shop-thumb" id="thumb" runat="server"><asp:Literal ID="litPreview" runat="server" /><i class='ti <%# Eval("Icon") %>' aria-hidden="true"></i></div>
                            <h3 class="shop-name"><%# Eval("Name") %></h3>
                            <div class="shop-type"><%# Eval("Type") %></div>
                            <div class="shop-desc"><%# Eval("Description") %></div>
                            <div class="shop-foot">
                                <span class="shop-price"><i class="ti ti-coin" aria-hidden="true"></i> <%# Eval("Price") %></span>

                                <asp:Button ID="btnBuy" runat="server" CssClass="shop-btn" Text="Buy"
                                    CommandName="Buy" CommandArgument='<%# Eval("ItemID") %>' />
                                <asp:LinkButton ID="btnEquip" runat="server" CssClass="shop-btn ghost"
                                    CommandArgument='<%# Eval("ItemID") %>' />
                                <span class="shop-owned" id="lblOwned" runat="server" visible="false"><i class="ti ti-check" aria-hidden="true"></i> Owned</span>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </div>

        <button type="button" id="seaToggle" class="sea-toggle" aria-label="Toggle dark and light mode">
            <i class="ti ti-sun" aria-hidden="true"></i>
        </button>
    </form>

    <script>
        (function () {
            var btn = document.getElementById('seaToggle');
            function paintIcon() {
                var light = document.documentElement.classList.contains('light');
                btn.innerHTML = light ? '<i class="ti ti-moon"></i>' : '<i class="ti ti-sun"></i>';
            }
            paintIcon();
            btn.addEventListener('click', function () {
                var light = document.documentElement.classList.toggle('light');
                localStorage.setItem('sea-theme', light ? 'light' : 'dark');
                paintIcon();
            });
        })();
    </script>
</body>
</html>
