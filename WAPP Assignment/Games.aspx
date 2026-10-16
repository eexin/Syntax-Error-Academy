<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Games.aspx.cs" Inherits="Games" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Minigames - Syntax Error Academy</title>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/@tabler/icons-webfont@3.11.0/dist/tabler-icons.min.css" />
    <link rel="stylesheet" href="Styles/sea-dashboard.css?v=3" />
    <link rel="stylesheet" href="Styles/sea-games.css?v=1" />
    <script>
        if (localStorage.getItem('sea-theme') === 'light') {
            document.documentElement.classList.add('light');
        }
    </script>
    <%-- INTERNAL CSS: a small page-specific rule --%>
    <style>
        .games-note {
            margin-top: 18px;
            font-size: 12px;
            text-align: center;
            color: var(--text-2);
        }
    </style>
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
                <a class="sea-nlink" href="Games.aspx" style="color:var(--coral);"><i class="ti ti-device-gamepad-2" aria-hidden="true"></i> Minigames</a>
                <a class="sea-nlink" href="DailyChallenge.aspx"><i class="ti ti-bolt" aria-hidden="true"></i> Daily</a>
                <a class="sea-nlink" href="Leaderboard.aspx"><i class="ti ti-trophy" aria-hidden="true"></i> Leaderboard</a>
                <a class="sea-nlink" href="ItemShop.aspx"><i class="ti ti-shopping-bag" aria-hidden="true"></i> Shop</a>
                <a href="Profile.aspx" class="sea-avatar"><asp:Literal ID="litInitials" runat="server" Text="U" /></a>
            </nav>
        </header>

        <div class="games-wrap">
            <a class="game-back" href="StudentDashboard.aspx"><i class="ti ti-arrow-left" aria-hidden="true"></i> Back to dashboard</a>
            <h1 class="games-title">Minigames</h1>
            <p class="games-sub">Take a break &mdash; these games are just for fun and don't affect your points, streak or rank.</p>

            <section class="games-grid">
                <a class="game-card" href="GameCatcher.aspx">
                    <span class="game-ic"><i class="ti ti-basket" aria-hidden="true"></i></span>
                    <h3>Byte Catcher</h3>
                    <p>Steer the basket and catch the falling code bits before they hit the floor. Three misses and it's over.</p>
                    <span class="game-play">Play <i class="ti ti-player-play" aria-hidden="true"></i></span>
                </a>

                <a class="game-card" href="GameStack.aspx">
                    <span class="game-ic"><i class="ti ti-stack-2" aria-hidden="true"></i></span>
                    <h3>Stack Tower</h3>
                    <p>Drop the sliding block onto the tower. Whatever hangs over gets sliced off &mdash; how high can you build?</p>
                    <span class="game-play">Play <i class="ti ti-player-play" aria-hidden="true"></i></span>
                </a>

                <a class="game-card" href="GameArchery.aspx">
                    <span class="game-ic"><i class="ti ti-target-arrow" aria-hidden="true"></i></span>
                    <h3>Target Shot</h3>
                    <p>Draw the bow, judge the arc and the wind, and land your ten arrows as close to the bullseye as you can.</p>
                    <span class="game-play">Play <i class="ti ti-player-play" aria-hidden="true"></i></span>
                </a>
            </section>

            <p class="games-note"><i class="ti ti-info-circle" aria-hidden="true"></i> Scores here are session-only bragging rights &mdash; nothing is saved.</p>
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
