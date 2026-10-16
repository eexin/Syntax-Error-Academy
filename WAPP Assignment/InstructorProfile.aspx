<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="InstructorProfile.aspx.cs" Inherits="InstructorProfile" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Profile - Instructor - SEA</title>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/@tabler/icons-webfont@3.11.0/dist/tabler-icons.min.css" />
    <link rel="stylesheet" href="Styles/sea-dashboard.css?v=7" />
    <link rel="stylesheet" href="Styles/sea-account.css?v=3" />
    <link rel="stylesheet" href="Styles/sea-instructor.css?v=2" />
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
                <a href="InstructorProfile.aspx" class="sea-avatar" style="outline:2px solid var(--coral);"><asp:Literal ID="litNavAvatar" runat="server" Text="I" /></a>
            </nav>
        </header>

        <div class="acc-wrap">
            <a class="quiz-back" href="InstructorDashboard.aspx"><i class="ti ti-arrow-left" aria-hidden="true"></i> Back to dashboard</a>
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
                </div>

                <%-- HTML5 aside: the instructor's avatar, standing beside the account card --%>
                <aside class="acc-avatar-box">
                    <asp:Literal ID="litAvatar" runat="server" />
                    <div class="acc-avatar-caption">Your avatar</div>
                    <a class="acc-avatar-link" href="InstructorShop.aspx"><i class="ti ti-shopping-bag" aria-hidden="true"></i> Customize in the Shop</a>
                </aside>
            </div>

            <%-- quiz summary --%>
            <h2 class="acc-title" style="font-size:18px;margin-top:34px;">My Quizzes</h2>
            <p class="acc-sub">A summary of the quizzes you have created.</p>
            <div class="ins-grid" style="grid-template-columns:repeat(auto-fill,minmax(180px,1fr));">
                <div class="ins-card" style="min-height:0;">
                    <h3 class="ins-card-title" style="font-size:24px;"><asp:Literal ID="lblTotal" runat="server" Text="0" /></h3>
                    <p class="ins-card-desc">Quizzes created</p>
                </div>
                <div class="ins-card" style="min-height:0;">
                    <h3 class="ins-card-title" style="font-size:24px;color:var(--olive);"><asp:Literal ID="lblPublished" runat="server" Text="0" /></h3>
                    <p class="ins-card-desc">Published</p>
                </div>
                <div class="ins-card" style="min-height:0;">
                    <h3 class="ins-card-title" style="font-size:24px;color:#e0a83a;"><asp:Literal ID="lblPending" runat="server" Text="0" /></h3>
                    <p class="ins-card-desc">Pending approval</p>
                </div>
                <div class="ins-card" style="min-height:0;">
                    <h3 class="ins-card-title" style="font-size:24px;color:#e0573f;"><asp:Literal ID="lblRejected" runat="server" Text="0" /></h3>
                    <p class="ins-card-desc">Rejected</p>
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
