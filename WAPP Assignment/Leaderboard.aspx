<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Leaderboard.aspx.cs" Inherits="Leaderboard" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Leaderboard - Syntax Error Academy</title>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/@tabler/icons-webfont@3.11.0/dist/tabler-icons.min.css" />
    <link rel="stylesheet" href="Styles/sea-dashboard.css?v=3" />
    <link rel="stylesheet" href="Styles/sea-leaderboard.css?v=4" />
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
                <a class="sea-nlink" href="Leaderboard.aspx" style="color:var(--coral);"><i class="ti ti-trophy" aria-hidden="true"></i> Leaderboard</a>
                <a class="sea-nlink" href="ItemShop.aspx"><i class="ti ti-shopping-bag" aria-hidden="true"></i> Shop</a>
                <a href="Profile.aspx" class="sea-avatar"><asp:Literal ID="litInitials" runat="server" Text="U" /></a>
            </nav>
        </header>

        <div class="lb-wrap">
            <a class="quiz-back" href="StudentDashboard.aspx"><i class="ti ti-arrow-left" aria-hidden="true"></i> Back to dashboard</a>
            <h1 class="lb-title">Leaderboard</h1>
            <p class="lb-sub"><asp:Literal ID="lblSub" runat="server" /></p>

            <%-- Winner's podium: HTML is built server-side from the top 3 --%>
            <asp:Literal ID="litPodium" runat="server" />

            <div class="lb-list">
                <asp:Repeater ID="rptBoard" runat="server">
                    <ItemTemplate>
                        <div class='lb-row <%# Eval("RowClass") %>'>
                            <span class="lb-rank"><%# Eval("Rank") %></span>
                            <span class="lb-avatar"><%# Eval("AvatarHtml") %></span>
                            <span class="lb-name"><%# Eval("Name") %><%# (bool)Eval("IsMe") ? "<span class=\"lb-you\">You</span>" : "" %></span>
                            <span class="lb-streak"><i class="ti ti-flame" aria-hidden="true"></i> <%# Eval("Streak") %></span>
                            <span class="lb-score"><%# Eval("Score") %></span>
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
