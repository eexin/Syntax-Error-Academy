<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DailyChallenge.aspx.cs" Inherits="DailyChallenge" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Daily Challenge - Syntax Error Academy</title>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/@tabler/icons-webfont@3.11.0/dist/tabler-icons.min.css" />
    <link rel="stylesheet" href="Styles/sea-dashboard.css?v=3" />
    <link rel="stylesheet" href="Styles/sea-quiz.css?v=3" />
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
                <a class="sea-nlink" href="DailyChallenge.aspx" style="color:var(--coral);"><i class="ti ti-bolt" aria-hidden="true"></i> Daily</a>
                <a class="sea-nlink" href="Leaderboard.aspx"><i class="ti ti-trophy" aria-hidden="true"></i> Leaderboard</a>
                <a class="sea-nlink" href="ItemShop.aspx"><i class="ti ti-shopping-bag" aria-hidden="true"></i> Shop</a>
                <a href="Profile.aspx" class="sea-avatar"><asp:Literal ID="litInitials" runat="server" Text="U" /></a>
            </nav>
        </header>

        <div class="quiz-wrap">
            <a class="quiz-back" href="StudentDashboard.aspx"><i class="ti ti-arrow-left" aria-hidden="true"></i> Back to dashboard</a>
            <h1 class="quiz-title">Daily Challenge</h1>
            <p class="quiz-desc">A random question you've already met. Do it to keep your streak alive.</p>

            <asp:HiddenField ID="hfQid" runat="server" />

            <%-- Locked: no answered questions yet --%>
            <asp:Panel ID="pnlLocked" runat="server" Visible="false" CssClass="result">
                <h2>No challenge yet</h2>
                <p>Finish a quiz first — the daily challenge reviews questions you've already done.</p>
                <div style="margin-top:16px;"><a class="btn" href="StudentDashboard.aspx">Go to quizzes</a></div>
            </asp:Panel>

            <%-- Already done today --%>
            <asp:Panel ID="pnlDone" runat="server" Visible="false" CssClass="result">
                <h2>Done for today!</h2>
                <p>You've already completed today's challenge. Come back tomorrow to keep your streak going.</p>
                <div style="margin-top:16px;"><a class="btn" href="StudentDashboard.aspx">Back to dashboard</a></div>
            </asp:Panel>

            <%-- The challenge --%>
            <asp:Panel ID="pnlForm" runat="server">
                <div class="q-card">
                    <div class="q-head">
                        <span class="q-tag">Daily</span>
                    </div>
                    <p class="q-text"><asp:Literal ID="lblQ" runat="server" /></p>

                    <asp:Panel ID="pnlMCQ" runat="server" Visible="false">
                        <div class="opts">
                            <asp:RadioButtonList ID="rblOptions" runat="server" RepeatLayout="Flow" RepeatDirection="Vertical" />
                        </div>
                        <asp:RequiredFieldValidator ID="rfvMcq" runat="server" ControlToValidate="rblOptions"
                            Display="Dynamic" CssClass="vmsg" ErrorMessage="Please choose an answer." />
                    </asp:Panel>

                    <asp:Panel ID="pnlEssay" runat="server" Visible="false">
                        <asp:TextBox ID="txtEssay" runat="server" TextMode="MultiLine" CssClass="essay" />
                        <asp:RequiredFieldValidator ID="rfvEssay" runat="server" ControlToValidate="txtEssay"
                            Display="Dynamic" CssClass="vmsg" ErrorMessage="Please write your answer." />
                    </asp:Panel>

                    <asp:Panel ID="pnlMatch" runat="server" Visible="false">
                        <p class="hint">Match each item on the left to the correct option:</p>
                        <asp:Repeater ID="rptMatch" runat="server" OnItemDataBound="rptMatch_ItemDataBound">
                            <ItemTemplate>
                                <div class="pair">
                                    <span class="left"><asp:Literal ID="lblLeft" runat="server" /></span>
                                    <i class="ti ti-arrow-right" style="color:var(--text-2);" aria-hidden="true"></i>
                                    <asp:DropDownList ID="ddlMatch" runat="server" CssClass="ddl" />
                                    <asp:HiddenField ID="hfLeft" runat="server" />
                                    <asp:RequiredFieldValidator runat="server" ControlToValidate="ddlMatch"
                                        InitialValue="" Display="Dynamic" CssClass="vmsg" ErrorMessage="!" />
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </asp:Panel>

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

                <div class="quiz-actions">
                    <asp:Button ID="btnSubmit" runat="server" Text="Submit answer" CssClass="btn" OnClick="btnSubmit_Click" OnClientClick="return seaSubmit(this);" />
                </div>
            </asp:Panel>

            <%-- Result --%>
            <asp:Panel ID="pnlResult" runat="server" Visible="false" CssClass="result">
                <h2><asp:Literal ID="lblResult" runat="server" /></h2>
                <p><asp:Literal ID="lblFeedback" runat="server" /></p>
                <div style="margin-top:16px;"><a class="btn" href="StudentDashboard.aspx">Back to dashboard</a></div>
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
    <script src="Assets/js/sea-grading.js"></script>
</body>
</html>
