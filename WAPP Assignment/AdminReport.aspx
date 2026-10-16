<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AdminReport.aspx.cs" Inherits="AdminReport" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Generate Report - Admin - SEA</title>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/@tabler/icons-webfont@3.11.0/dist/tabler-icons.min.css" />
    <link rel="stylesheet" href="Styles/sea-dashboard.css" />
    <link rel="stylesheet" href="Styles/sea-admin.css" />
    <script>
        if (localStorage.getItem('sea-theme') === 'light') { document.documentElement.classList.add('light'); }
    </script>
</head>
<body>
    <form id="form1" runat="server">
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
                    <span class="adm-nlink active"><i class="ti ti-report-analytics" aria-hidden="true"></i> Generate Report <i class="ti ti-chevron-down" style="font-size:14px;"></i></span>
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
            <h2 class="adm-h">Generate Report</h2>
            <p class="adm-sub">Choose a report to generate.</p>

            <section class="adm-grid">
                <a class="adm-feature" href="AdminQuizReport.aspx">
                    <i class="ti ti-chart-bar" aria-hidden="true"></i>
                    <h3>Quiz Report</h3>
                    <p>Analyse a single quiz: who answered it today and over the last 7 days, correct vs wrong rate, and a per-question breakdown.</p>
                </a>
                <a class="adm-feature" href="AdminSummaryReport.aspx">
                    <i class="ti ti-file-analytics" aria-hidden="true"></i>
                    <h3>Summary Report</h3>
                    <p>Aggregated performance across all quizzes for a chosen week, month, or year.</p>
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
