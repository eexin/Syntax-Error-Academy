<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Quiz.aspx.cs" Inherits="Quiz" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Quiz - Syntax Error Academy</title>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/@tabler/icons-webfont@3.11.0/dist/tabler-icons.min.css" />
    <link rel="stylesheet" href="Styles/sea-dashboard.css?v=3" />
    <link rel="stylesheet" href="Styles/sea-quiz.css?v=4" />
    <link rel="stylesheet" href="Styles/sea-grading.css?v=1" />
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
                <a class="sea-nlink" href="ItemShop.aspx"><i class="ti ti-shopping-bag" aria-hidden="true"></i> Shop</a>
                <a href="Profile.aspx" class="sea-avatar"><asp:Literal ID="litInitials" runat="server" Text="U" /></a>
            </nav>
        </header>

        <div class="quiz-wrap">
            <a class="quiz-back" href="StudentDashboard.aspx"><i class="ti ti-arrow-left" aria-hidden="true"></i> Back to dashboard</a>

            <h1 class="quiz-title"><asp:Literal ID="lblQuizTitle" runat="server" /></h1>
            <p class="quiz-desc"><asp:Literal ID="lblQuizDesc" runat="server" /></p>

            <asp:HiddenField ID="hfQuizId" runat="server" />

            <%-- The question form --%>
            <asp:Panel ID="pnlForm" runat="server">
                <asp:Repeater ID="rptQuestions" runat="server" OnItemDataBound="rptQuestions_ItemDataBound">
                    <ItemTemplate>
                        <div class="q-card">
                            <div class="q-head">
                                <span class="q-num">Q<%# Container.ItemIndex + 1 %></span>
                                <span class="q-tag"><%# Eval("QuestionType") %></span>
                            </div>
                            <p class="q-text"><%# Eval("QuestionText") %></p>

                            <%-- AI hint: fetched without a postback so typed answers survive --%>
                            <div class="hint-row">
                                <button type="button" class="hint-btn" data-qid='<%# Eval("QuestionID") %>'>
                                    <i class="ti ti-bulb" aria-hidden="true"></i> Need a hint?
                                </button>
                                <div class="hint-bubble" role="status" aria-live="polite"></div>
                            </div>

                            <asp:HiddenField ID="hfQid" runat="server" />
                            <asp:HiddenField ID="hfType" runat="server" />

                            <%-- MCQ --%>
                            <asp:Panel ID="pnlMCQ" runat="server" Visible="false">
                                <div class="opts">
                                    <asp:RadioButtonList ID="rblOptions" runat="server"
                                        RepeatLayout="Flow" RepeatDirection="Vertical" />
                                </div>
                                <asp:RequiredFieldValidator ID="rfvMcq" runat="server"
                                    ControlToValidate="rblOptions" Display="Dynamic"
                                    CssClass="vmsg" ErrorMessage="Please choose an answer." />
                            </asp:Panel>

                            <%-- Essay --%>
                            <asp:Panel ID="pnlEssay" runat="server" Visible="false">
                                <asp:TextBox ID="txtEssay" runat="server" TextMode="MultiLine" CssClass="essay" />
                                <asp:RequiredFieldValidator ID="rfvEssay" runat="server"
                                    ControlToValidate="txtEssay" Display="Dynamic"
                                    CssClass="vmsg" ErrorMessage="Please write your answer." />
                            </asp:Panel>

                            <%-- Matching (Duolingo-style tap to match) --%>
                            <asp:Panel ID="pnlMatch" runat="server" Visible="false">
                                <p class="hint">Tap an item on the left, then tap its match on the right. Correct pairs lock in; wrong pairs reset so you can try again.</p>
                                <asp:Literal ID="litMatchUI" runat="server" />
                                <asp:HiddenField ID="hfMatchResult" runat="server" />
                            </asp:Panel>

                            <%-- Puzzle --%>
                            <asp:Panel ID="pnlPuzzle" runat="server" Visible="false">
                                <p class="hint">Set the correct position for each piece:</p>
                                <asp:Repeater ID="rptPuzzle" runat="server" OnItemDataBound="rptPuzzle_ItemDataBound">
                                    <ItemTemplate>
                                        <div class="pair">
                                            <span class="left"><asp:Literal ID="lblToken" runat="server" /></span>
                                            <asp:DropDownList ID="ddlPos" runat="server" CssClass="ddl" />
                                            <asp:HiddenField ID="hfToken" runat="server" />
                                            <asp:RequiredFieldValidator runat="server" ControlToValidate="ddlPos"
                                                InitialValue="" Display="Dynamic" CssClass="vmsg" ErrorMessage="!" />
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </asp:Panel>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>

                <div class="quiz-actions">
                    <asp:Button ID="btnSubmit" runat="server" Text="Submit answers"
                        CssClass="btn" OnClick="btnSubmit_Click" OnClientClick="return seaSubmit(this);" />
                </div>
            </asp:Panel>

            <%-- Result (shown after submit) --%>
            <asp:Panel ID="pnlResult" runat="server" Visible="false" CssClass="result">
                <h2>Quiz complete</h2>
                <div class="score"><asp:Literal ID="lblScore" runat="server" /></div>
                <p><asp:Literal ID="lblEarned" runat="server" /></p>
                <asp:Literal ID="lblFeedback" runat="server" />
                <div style="margin-top:18px; display:flex; gap:10px; justify-content:center;">
                    <asp:HyperLink ID="lnkRetry" runat="server" CssClass="btn" Visible="false">
                        <i class="ti ti-refresh" aria-hidden="true" style="margin-right:6px;"></i>Retry quiz</asp:HyperLink>
                    <a class="btn btn-ghost" href="StudentDashboard.aspx">Back to dashboard</a>
                </div>
            </asp:Panel>
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

    <%-- Duolingo-style tap-to-match for Matching questions --%>
    <script>
        (function () {
            function setup(widget) {
                var target = document.getElementById(widget.getAttribute('data-target'));
                var chips = Array.prototype.slice.call(widget.querySelectorAll('.chip'));
                var leftTotal = widget.querySelectorAll('.chip[data-side="L"]').length;
                var sel = null;          // currently selected chip (either column)
                var matched = {};        // leftText -> rightText (correct pairs only)

                function record() {
                    // URL-encode so values like "List<T>" don't trip request validation
                    if (target) target.value = encodeURIComponent(JSON.stringify(matched));
                }
                function done() { return Object.keys(matched).length === leftTotal; }
                function clearSel() { if (sel) { sel.classList.remove('selected'); sel = null; } }

                chips.forEach(function (chip) {
                    chip.addEventListener('click', function () {
                        if (chip.classList.contains('correct')) return;     // already locked
                        if (sel === chip) { clearSel(); return; }           // tap again to deselect
                        if (!sel) { sel = chip; chip.classList.add('selected'); return; }
                        if (sel.getAttribute('data-side') === chip.getAttribute('data-side')) {
                            clearSel(); sel = chip; chip.classList.add('selected'); return; // switch within a column
                        }

                        var L = sel.getAttribute('data-side') === 'L' ? sel : chip;
                        var R = sel.getAttribute('data-side') === 'R' ? sel : chip;
                        var ok = R.getAttribute('data-right') === L.getAttribute('data-answer');

                        if (ok) {
                            L.classList.remove('selected'); R.classList.remove('selected');
                            L.classList.add('correct'); R.classList.add('correct');
                            L.disabled = true; R.disabled = true;
                            matched[L.getAttribute('data-left')] = R.getAttribute('data-right');
                            sel = null; record();
                            if (done()) widget.classList.add('done');
                        } else {
                            L.classList.add('wrong'); R.classList.add('wrong');
                            var a = L, b = R;
                            setTimeout(function () {
                                a.classList.remove('wrong', 'selected');
                                b.classList.remove('wrong', 'selected');
                            }, 650);
                            sel = null;
                        }
                    });
                });
            }

            function init() {
                Array.prototype.slice.call(document.querySelectorAll('.match')).forEach(setup);
            }
            if (document.readyState !== 'loading') init();
            else document.addEventListener('DOMContentLoaded', init);

            // Submit guard: every matching question must be fully matched first.
            window.seaMatchReady = function () {
                var widgets = document.querySelectorAll('.match');
                for (var i = 0; i < widgets.length; i++) {
                    if (!widgets[i].classList.contains('done')) {
                        var w = widgets[i];
                        w.classList.add('shake');
                        setTimeout(function () { w.classList.remove('shake'); }, 500);
                        w.scrollIntoView({ behavior: 'smooth', block: 'center' });
                        return false;
                    }
                }
                return true;
            };
        })();
    </script>
    <script src="Assets/js/sea-grading.js"></script>

    <%-- AI hint buttons: fetch a nudge without reloading the page --%>
    <script>
        (function () {
            document.addEventListener('click', function (ev) {
                var btn = ev.target.closest ? ev.target.closest('.hint-btn') : null;
                if (!btn) return;

                var bubble = btn.parentNode.querySelector('.hint-bubble');
                var qid = btn.getAttribute('data-qid');
                if (!bubble || !qid) return;

                // Second click closes an open hint.
                if (bubble.classList.contains('show') && bubble.dataset.done === '1') {
                    bubble.classList.remove('show');
                    bubble.dataset.done = '0';
                    return;
                }

                btn.disabled = true;
                bubble.textContent = 'Thinking of a hint\u2026';
                bubble.classList.add('show', 'loading');
                bubble.dataset.done = '0';

                fetch('HintHandler.ashx?qid=' + encodeURIComponent(qid), {
                    credentials: 'same-origin'
                })
                .then(function (r) { return r.json(); })
                .then(function (d) {
                    bubble.classList.remove('loading');
                    bubble.textContent = d.hint || 'No hint available right now.';
                    bubble.dataset.done = '1';
                    btn.disabled = false;
                })
                .catch(function () {
                    bubble.classList.remove('loading');
                    bubble.textContent = 'Could not reach the hint service.';
                    bubble.dataset.done = '1';
                    btn.disabled = false;
                });
            });
        })();
    </script>
</body>
</html>
