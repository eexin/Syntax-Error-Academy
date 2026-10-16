<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AdminDashboard.aspx.cs" Inherits="AdminDashboard" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Admin - Syntax Error Academy</title>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/@tabler/icons-webfont@3.11.0/dist/tabler-icons.min.css" />
    <link rel="stylesheet" href="Styles/sea-dashboard.css" />
    <link rel="stylesheet" href="Styles/sea-admin.css" />
    <script>
        if (localStorage.getItem('sea-theme') === 'light') {
            document.documentElement.classList.add('light');
        }
    </script>
</head>
<body>
    <form id="form1" runat="server">

        <%-- ============ Shared admin top bar ============ --%>
        <header class="sea-top">
            <div class="sea-left">
                <a href="AdminDashboard.aspx" class="sea-brand" style="text-decoration:none;" title="Back to dashboard">SEA<img src="Assets/sea-octopus.svg" alt="octopus" style="height:2em;width:auto;vertical-align:middle;margin-left:6px;margin-bottom:-3px;" /></a>
                <span class="sea-pill streak" style="background:var(--coral-bg);color:var(--coral);">
                    <i class="ti ti-shield-lock" aria-hidden="true"></i> ADMIN
                </span>
            </div>

            <nav class="adm-nav">
                <a class="adm-nlink" href="AdminViewUser.aspx"><i class="ti ti-users" aria-hidden="true"></i> View User</a>

                <div class="adm-drop">
                    <span class="adm-nlink"><i class="ti ti-list-check" aria-hidden="true"></i> Quiz Management <i class="ti ti-chevron-down" style="font-size:14px;"></i></span>
                    <div class="adm-drop-menu">
                        <a href="AdminViewQuestion.aspx"><i class="ti ti-eye" aria-hidden="true"></i> View Quiz</a>
                        <a href="AdminQuizApproval.aspx"><i class="ti ti-checkup-list" aria-hidden="true"></i> Quiz Approval</a>
                    </div>
                </div>

                <div class="adm-drop">
                    <span class="adm-nlink"><i class="ti ti-report-analytics" aria-hidden="true"></i> Generate Report <i class="ti ti-chevron-down" style="font-size:14px;"></i></span>
                    <div class="adm-drop-menu">
                        <a href="AdminQuizReport.aspx"><i class="ti ti-chart-bar" aria-hidden="true"></i> Quiz Report</a>
                        <a href="AdminSummaryReport.aspx"><i class="ti ti-file-analytics" aria-hidden="true"></i> Summary Report</a>
                    </div>
                </div>

                <a class="adm-nlink" href="AdminItemShop.aspx"><i class="ti ti-building-store" aria-hidden="true"></i> Item Shop</a>

                <span class="sea-sep"></span>
                <a class="sea-nlink" href="Logout.aspx"><i class="ti ti-logout" aria-hidden="true"></i> Logout</a>
                <span class="sea-avatar"><asp:Literal ID="litInitials" runat="server" Text="A" /></span>
            </nav>
        </header>

        <div class="adm-wrap">

            <%-- Welcome banner: "Welcome, {username}"  +  </sea> logo --%>
            <section class="adm-welcome">
                <h1>Welcome, <span><asp:Literal ID="lblWelcome" runat="server" Text="admin" /></span></h1>
                <div class="adm-logo">SEA<img src="Assets/sea-octopus.svg" alt="octopus" style="height:1.05em;width:auto;vertical-align:middle;margin-left:8px;margin-bottom:-5px;" /></div>
            </section>

            <h2 class="adm-h">Admin Control Panel</h2>
            <p class="adm-sub">Manage users, review quizzes, approve instructor submissions, generate reports, and manage the item shop.</p>

            <section class="adm-grid">
                <a class="adm-feature" href="AdminViewUser.aspx">
                    <i class="ti ti-users" aria-hidden="true"></i>
                    <h3>View User</h3>
                    <p>Browse student, instructor and admin accounts. Add, edit, delete or search users.</p>
                </a>
                <a class="adm-feature" href="AdminViewQuestion.aspx">
                    <i class="ti ti-eye" aria-hidden="true"></i>
                    <h3>View Quiz</h3>
                    <p>See every published quiz by level, with the correct answers revealed.</p>
                </a>
                <a class="adm-feature" href="AdminQuizApproval.aspx">
                    <i class="ti ti-checkup-list" aria-hidden="true"></i>
                    <h3>Quiz Approval</h3>
                    <p>Approve or reject quizzes submitted by instructors before they go live.</p>
                </a>
                <a class="adm-feature" href="AdminReport.aspx">
                    <i class="ti ti-report-analytics" aria-hidden="true"></i>
                    <h3>Generate Report</h3>
                    <p>Generate quiz reports and weekly, monthly or yearly summary reports of student activity.</p>
                </a>
                <a class="adm-feature" href="AdminItemShop.aspx">
                    <i class="ti ti-building-store" aria-hidden="true"></i>
                    <h3>Item Shop</h3>
                    <p>Add new wallpapers and avatar cosmetics to the shop, or remove existing items.</p>
                </a>
            </section>
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
