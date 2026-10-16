<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="StudentDashboard.aspx.cs" Inherits="StudentDashboard" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Dashboard - Syntax Error Academy</title>

    <%-- Tabler outline icon font --%>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/@tabler/icons-webfont@3.11.0/dist/tabler-icons.min.css" />

    <%-- EXTERNAL CSS --%>
    <link rel="stylesheet" href="Styles/sea-dashboard.css?v=7" />

    <%-- Apply the saved theme before paint so there is no colour flash --%>
    <script>
        if (localStorage.getItem('sea-theme') === 'light') {
            document.documentElement.classList.add('light');
        }
    </script>

    <%-- INTERNAL CSS: a small page-specific rule --%>
    <style>
        .sea-empty {
            border: 1px dashed var(--border);
            border-radius: 12px;
            padding: 28px;
            text-align: center;
            color: var(--text-2);
            font-size: 14px;
        }
    </style>
</head>
<body id="pageBody" runat="server">
    <form id="form1" runat="server">

        <%-- HTML5 header: top navigation bar --%>
        <header class="sea-top">
            <div class="sea-left">
                <span class="sea-brand">SEA<img src="Assets/sea-octopus.svg" alt="octopus" style="height:2em;width:auto;vertical-align:middle;margin-left:6px;margin-bottom:-3px;" /></span>
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
                <a class="sea-nlink" href="ItemShop.aspx"><i class="ti ti-shopping-bag" aria-hidden="true"></i> Shop</a>
                <a href="Profile.aspx" class="sea-avatar"><asp:Literal ID="litInitials" runat="server" Text="U" /></a>
            </nav>
        </header>

        <div class="sea-body" id="seaBody" runat="server">

            <%-- HTML5 aside: the level navigation rail --%>
            <aside class="sea-side">
                <div class="sea-side-label">LEVELS</div>
                <asp:Repeater ID="rptLevels" runat="server">
                    <ItemTemplate>
                        <a href='<%# Eval("Url") %>' class='lvl <%# Eval("StateClass") %>'>
                            <i class='ti <%# Eval("IconClass") %>' aria-hidden="true"></i>
                            <span>Level <%# Eval("LevelNumber") %></span>
                        </a>
                    </ItemTemplate>
                </asp:Repeater>
            </aside>

            <%-- HTML5 main: the selected level's quizzes --%>
            <main class="sea-main">
                <a class="sea-daily" href="DailyChallenge.aspx">
                    <span class="sea-daily-ic"><i class="ti ti-bolt" aria-hidden="true"></i></span>
                    <span class="sea-daily-tx">
                        <strong>Daily Challenge</strong>
                        <span>Answer one question to keep your streak alive</span>
                    </span>
                    <i class="ti ti-chevron-right sea-daily-go" aria-hidden="true"></i>
                </a>

                <%-- Personal level track (battle-pass): completing a quiz advances the
                     pin by that quiz's level. Shows a sliding window of tiers. --%>
                <div class="sea-track" id="trackWrap" runat="server" data-pin-level="1" data-from-level="1">
                    <div class="sea-track-label">PERSONAL GRADE <b><asp:Literal ID="litTrackLevel" runat="server" Text="1" /></b></div>
                    <div class="sea-road"><div class="sea-road-fill" id="seaRoadFill"></div></div>
                    <div class="sea-stops"><asp:Literal ID="litTrackStops" runat="server" /></div>
                    <div class="sea-pin" id="seaPin">
                        <span class="sea-pin-caption" id="seaPinCaption">Level Up!</span>
                        <span class="sea-pin-float">
                            <span class="sea-pin-body">
                                <span class="sea-pin-avatar"><asp:Literal ID="litPinAvatar" runat="server" /></span>
                            </span>
                        </span>
                    </div>
                </div>

                <h1 class="sea-h"><asp:Literal ID="lblLevelTitle" runat="server" /></h1>
                <p class="sea-sub"><asp:Literal ID="lblLevelSub" runat="server" /></p>

                <%-- INLINE CSS: bar width is written by the code-behind from live data --%>
                <div class="sea-bar">
                    <div id="barFill" runat="server" class="sea-bar-fill" style="width:0%;"></div>
                </div>

                <section class="sea-grid">
                    <asp:Repeater ID="rptQuizzes" runat="server">
                        <ItemTemplate>
                            <%-- each quiz card is an HTML5 article: title + points only --%>
                            <a class="sea-card" href='<%# "Quiz.aspx?id=" + Eval("QuizID") %>'>
                                <article>
                                    <div class="sea-card-row" style="justify-content:flex-end;">
                                        <i class='ti <%# GetStatusIcon(Eval("Status")) %>' aria-hidden="true"></i>
                                    </div>
                                    <h3 class="sea-card-title"><%# Eval("QuizTitle") %></h3>
                                    <div class="sea-meta"><b><%# Eval("Points") %></b> pts</div>
                                </article>
                            </a>
                        </ItemTemplate>
                    </asp:Repeater>
                </section>

                <asp:Panel ID="pnlEmpty" runat="server" Visible="false" CssClass="sea-empty">
                    No quizzes in this level yet.
                </asp:Panel>
            </main>
        </div>

        <%-- Dark / light toggle (client-side) --%>
        <button type="button" id="seaToggle" class="sea-toggle" aria-label="Toggle dark and light mode">
            <i class="ti ti-sun" aria-hidden="true"></i>
        </button>
    </form>

    <script>
        (function () {
            var btn = document.getElementById('seaToggle');
            function paintIcon() {
                var light = document.documentElement.classList.contains('light');
                btn.innerHTML = light
                    ? '<i class="ti ti-moon" aria-hidden="true"></i>'
                    : '<i class="ti ti-sun" aria-hidden="true"></i>';
            }
            paintIcon();
            btn.addEventListener('click', function () {
                var light = document.documentElement.classList.toggle('light');
                localStorage.setItem('sea-theme', light ? 'light' : 'dark');
                paintIcon();
            });
        })();
    </script>

    <%-- Level track: place the pin, slide it on level-up, keep it aligned on resize --%>
    <script>
        (function () {
            var wrap = document.getElementById('trackWrap');
            if (!wrap) return;
            var pin = document.getElementById('seaPin');
            var fill = document.getElementById('seaRoadFill');
            var caption = document.getElementById('seaPinCaption');
            var stops = wrap.querySelectorAll('.sea-stop');
            if (!stops.length) return;

            var to = parseInt(wrap.getAttribute('data-pin-level'), 10) || 1;
            var from = parseInt(wrap.getAttribute('data-from-level'), 10) || to;
            var reward = parseInt(wrap.getAttribute('data-reward'), 10) || 0;
            if (from > to) from = to;

            function stopX(n) {
                var s = stops[Math.min(n, stops.length) - 1];
                return s.offsetLeft + s.offsetWidth / 2;
            }
            function place(n) {
                pin.style.left = stopX(n) + 'px';
                fill.style.width = ((n - 1) / (stops.length - 1)) * 100 + '%';
            }

            // initial placement without any transition, then reveal
            pin.classList.add('no-anim');
            fill.style.transition = 'none';
            place(from);
            void pin.offsetWidth;                       // force reflow
            pin.classList.remove('no-anim');
            fill.style.transition = '';
            pin.style.opacity = '1';

            if (to > from) {
                // level up! slide to the new stop, then pulse + caption
                setTimeout(function () {
                    place(to);
                    var fired = false;
                    pin.addEventListener('transitionend', function onEnd(ev) {
                        if (ev.propertyName !== 'left' || fired) return;
                        fired = true;
                        pin.removeEventListener('transitionend', onEnd);
                        pin.classList.add('pulse');
                        caption.classList.add('show');
                        if (reward > 0) {
                            spawnRewardChip(reward);
                            animateCoins(reward);
                        }
                        setTimeout(function () {
                            caption.classList.remove('show');
                            pin.classList.remove('pulse');
                        }, 1700);
                    });
                }, 600);
            }

            // ---- item-point reward: floating "+N pts" chip over the pin ----
            function spawnRewardChip(gain) {
                var chip = document.createElement('span');
                chip.className = 'sea-reward-float';
                chip.textContent = '+' + gain + ' pts';
                pin.appendChild(chip);
                setTimeout(function () { if (chip.parentNode) chip.parentNode.removeChild(chip); }, 2000);
            }

            // ---- item-point reward: the nav coin counter counts up to the new total ----
            function coinTextNode() {
                var pill = document.querySelector('.sea-coin');
                if (!pill) return null;
                for (var i = pill.childNodes.length - 1; i >= 0; i--) {
                    var n = pill.childNodes[i];
                    if (n.nodeType === 3 && /\d/.test(n.nodeValue)) return n;
                }
                return null;
            }
            function animateCoins(gain) {
                var node = coinTextNode();
                if (!node) return;
                var pill = document.querySelector('.sea-coin');
                var startVal = parseInt(node.nodeValue.replace(/[^0-9]/g, ''), 10) || 0;
                var endVal = startVal + gain;
                var t0 = null, dur = 900;
                if (pill) pill.classList.add('bump');
                function step(ts) {
                    if (t0 === null) t0 = ts;
                    var p = Math.min((ts - t0) / dur, 1);
                    var eased = 1 - Math.pow(1 - p, 3);            // ease-out
                    var v = Math.round(startVal + (endVal - startVal) * eased);
                    node.nodeValue = ' ' + v.toLocaleString('en-US');
                    if (p < 1) requestAnimationFrame(step);
                    else if (pill) setTimeout(function () { pill.classList.remove('bump'); }, 250);
                }
                requestAnimationFrame(step);
            }

            window.addEventListener('resize', function () {
                pin.classList.add('no-anim');
                fill.style.transition = 'none';
                place(to);
                void pin.offsetWidth;
                pin.classList.remove('no-anim');
                fill.style.transition = '';
            });
        })();
    </script>
</body>
</html>
