<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="InstructorViewStudentAttempt.aspx.cs" Inherits="InstructorViewStudentAttempt" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Student Attempt - Instructor - SEA</title>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/@tabler/icons-webfont@3.11.0/dist/tabler-icons.min.css" />
    <link rel="stylesheet" href="Styles/sea-dashboard.css?v=7" />
    <link rel="stylesheet" href="Styles/sea-quiz.css?v=4" />
    <link rel="stylesheet" href="Styles/sea-admin.css?v=1" />
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
                <a href="InstructorProfile.aspx" class="sea-avatar"><asp:Literal ID="litNavAvatar" runat="server" Text="I" /></a>
            </nav>
        </header>

        <div class="quiz-wrap">
            <a class="quiz-back" id="lnkBack" runat="server" href="#"><i class="ti ti-arrow-left" aria-hidden="true"></i> Back to Quiz</a>

            <h1 class="quiz-title"><asp:Literal ID="lblQuizTitle" runat="server" /></h1>
            <p class="quiz-desc"><asp:Literal ID="lblSub" runat="server" /></p>

            <asp:Literal ID="litQuestions" runat="server" />
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
