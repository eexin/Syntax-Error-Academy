<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="GameCatcher.aspx.cs" Inherits="GameCatcher" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Byte Catcher - Syntax Error Academy</title>
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
        .game-title .ti { color: var(--coral); }
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
            <div class="game-head">
                <a class="game-back" href="Games.aspx"><i class="ti ti-arrow-left" aria-hidden="true"></i> Minigames</a>
                <button type="button" id="btnMute" class="game-mute" aria-label="Toggle sound"><i class="ti ti-volume" aria-hidden="true"></i></button>
            </div>

            <div class="game-panel">
                <div class="game-top">
                    <h1 class="game-title"><i class="ti ti-basket" aria-hidden="true"></i> Byte Catcher</h1>
                    <div class="game-hud">
                        <span class="hud-chip">Score <b id="hudScore">0</b></span>
                        <span class="hud-chip">Lives <b id="hudLives">3</b></span>
                        <span class="hud-chip">Best <b id="hudBest">0</b></span>
                    </div>
                </div>

                <div class="game-stage">
                    <canvas id="gameCanvas" width="720" height="480"></canvas>
                    <div class="game-overlay" id="gameOverlay">
                        <h2 id="ovTitle">Byte Catcher</h2>
                        <p id="ovMsg">Code bits are falling! Steer the basket and catch them. Every miss costs a life &mdash; you have three.</p>
                        <button type="button" id="btnStart" class="game-btn">Play</button>
                    </div>
                </div>

                <p class="game-hint">Move with <b>&larr;</b> <b>&rarr;</b> (or <b>A</b>/<b>D</b>), your mouse, or a finger.</p>
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
    <script src="Assets/Games/catcher/catcher.js"></script>
</body>
</html>
