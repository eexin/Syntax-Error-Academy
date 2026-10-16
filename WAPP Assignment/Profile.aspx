<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Profile.aspx.cs" Inherits="Profile" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Profile - Syntax Error Academy</title>
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
                <a class="sea-nlink" href="ItemShop.aspx"><i class="ti ti-shopping-bag" aria-hidden="true"></i> Shop</a>
                <a href="Profile.aspx" class="sea-avatar" style="outline:2px solid var(--coral);"><asp:Literal ID="litInitials" runat="server" Text="U" /></a>
            </nav>
        </header>

        <div class="acc-wrap">
            <a class="quiz-back" href="StudentDashboard.aspx"><i class="ti ti-arrow-left" aria-hidden="true"></i> Back to dashboard</a>
            <h1 class="acc-title">My Profile</h1>
            <p class="acc-sub">Update your account details.</p>

            <asp:Panel ID="pnlOk" runat="server" Visible="false" CssClass="acc-ok">
                <asp:Literal ID="lblOk" runat="server" />
            </asp:Panel>
            <asp:Panel ID="pnlErr" runat="server" Visible="false" CssClass="acc-msg">
                <asp:Literal ID="lblErr" runat="server" />
            </asp:Panel>

            <div class="acc-layout">
            <div class="acc-card">
                <div class="acc-field">
                    <label>Username</label>
                    <asp:TextBox ID="txtName" runat="server" />
                    <asp:RequiredFieldValidator ID="rfvName" runat="server" ControlToValidate="txtName"
                        Display="Dynamic" CssClass="vmsg" ErrorMessage="Username is required." />
                </div>

                <div class="acc-field">
                    <label>Email</label>
                    <asp:TextBox ID="txtEmail" runat="server" TextMode="Email" />
                    <asp:RequiredFieldValidator ID="rfvEmail" runat="server" ControlToValidate="txtEmail"
                        Display="Dynamic" CssClass="vmsg" ErrorMessage="Email is required." />
                    <asp:RegularExpressionValidator ID="revEmail" runat="server" ControlToValidate="txtEmail"
                        Display="Dynamic" CssClass="vmsg"
                        ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"
                        ErrorMessage="Enter a valid email address." />
                </div>

                <div class="acc-field">
                    <label>Date of birth</label>
                    <asp:TextBox ID="txtDob" runat="server" TextMode="Date" />
                </div>

                <div class="acc-field">
                    <label>New password</label>
                    <asp:TextBox ID="txtPwd" runat="server" TextMode="Password" />
                    <asp:RegularExpressionValidator ID="revPwd" runat="server" ControlToValidate="txtPwd"
                        Display="Dynamic" CssClass="vmsg" ValidationExpression=".{6,}"
                        ErrorMessage="Password must be at least 6 characters." />
                    <div class="acc-hint">Leave blank to keep your current password.</div>
                </div>

                <div class="acc-actions">
                    <asp:Button ID="btnSave" runat="server" Text="Save changes" CssClass="acc-save" OnClick="btnSave_Click" />
                    <a class="acc-logout" href="Logout.aspx"><i class="ti ti-logout" aria-hidden="true" style="margin-right:6px;"></i> Log out</a>
                </div>

                <%-- Danger zone: permanently delete this account (DELETE operation) --%>
                <div class="acc-danger">
                    <h2 class="acc-danger-title"><i class="ti ti-alert-triangle" aria-hidden="true"></i> Delete account</h2>
                    <p class="acc-danger-text">
                        This permanently removes your account, your quiz progress and everything
                        you have bought in the shop. It cannot be undone.
                    </p>
                    <div class="acc-field">
                        <label for="<%= txtDelPwd.ClientID %>">Confirm your password to continue</label>
                        <asp:TextBox ID="txtDelPwd" runat="server" TextMode="Password"
                            CssClass="acc-input" ValidationGroup="del" placeholder="Your password" />
                        <asp:RequiredFieldValidator runat="server" ControlToValidate="txtDelPwd"
                            ValidationGroup="del" Display="Dynamic" CssClass="acc-err"
                            ErrorMessage="Enter your password to confirm." />
                    </div>
                    <asp:Button ID="btnDelete" runat="server" Text="Delete my account permanently"
                        CssClass="acc-delete" ValidationGroup="del" OnClick="btnDelete_Click"
                        OnClientClick="return confirm('Delete your account permanently?\n\nYour progress, points and items will all be lost. This cannot be undone.');" />
                </div>
            </div>

            <%-- HTML5 aside: the student's avatar, standing beside the account card --%>
            <aside class="acc-avatar-box">
                <asp:Literal ID="litAvatar" runat="server" />
                <div class="acc-avatar-caption">Your avatar</div>
                <a class="acc-avatar-link" href="ItemShop.aspx"><i class="ti ti-shopping-bag" aria-hidden="true"></i> Customize in the Shop</a>
            </aside>
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
