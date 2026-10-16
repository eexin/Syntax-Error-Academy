<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="InstructorViewQuiz.aspx.cs" Inherits="InstructorViewQuiz" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>View Quiz - Instructor - SEA</title>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/@tabler/icons-webfont@3.11.0/dist/tabler-icons.min.css" />
    <link rel="stylesheet" href="Styles/sea-dashboard.css?v=7" />
    <link rel="stylesheet" href="Styles/sea-quiz.css?v=4" />
    <link rel="stylesheet" href="Styles/sea-admin.css?v=1" />
    <link rel="stylesheet" href="Styles/sea-instructor.css?v=1" />
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
            <div style="display:flex;align-items:center;gap:12px;flex-wrap:wrap;margin-bottom:14px;">
                <a class="quiz-back" href="InstructorDashboard.aspx" style="margin:0;"><i class="ti ti-arrow-left" aria-hidden="true"></i> Back to dashboard</a>
                <div style="flex:1;display:flex;justify-content:center;">
                    <div class="ins-tabgroup">
                        <asp:LinkButton ID="lnkQuestions" runat="server" CssClass="ins-tab active" CausesValidation="false"
                            OnClick="ShowQuestions"><i class="ti ti-list-check"></i> Questions</asp:LinkButton>
                        <asp:LinkButton ID="lnkAnswers" runat="server" CssClass="ins-tab" CausesValidation="false"
                            OnClick="ShowAnswers"><i class="ti ti-users"></i> Student Answers</asp:LinkButton>
                    </div>
                </div>
                <span style="width:150px;"></span>
            </div>

            <asp:Panel ID="pnlReject" runat="server" Visible="false"
                Style="margin:18px 0 22px;padding:16px 18px;border:1px solid #e0573f;border-radius:12px;background:rgba(224,87,63,.12);">
                <div style="display:flex;align-items:center;gap:8px;font-weight:700;color:#e0573f;font-size:13.5px;margin-bottom:6px;">
                    <i class="ti ti-alert-circle"></i> This quiz was rejected
                </div>
                <div style="font-size:13.5px;color:var(--text);line-height:1.6;">
                    Reason from admin: <asp:Literal ID="litRejectReason" runat="server" />
                </div>
            </asp:Panel>

            <h1 class="quiz-title"><asp:Literal ID="lblQuizTitle" runat="server" /></h1>
            <p class="quiz-desc"><asp:Literal ID="lblQuizDesc" runat="server" /></p>

            <asp:Panel ID="pnlQuestions" runat="server">
                <asp:Literal ID="litQuestions" runat="server" />
            </asp:Panel>

            <asp:Panel ID="pnlAnswers" runat="server" Visible="false">
                <asp:Literal ID="litAnswers" runat="server" />
            </asp:Panel>

            <div class="ins-btnrow">
                <button type="button" class="ins-btn danger" onclick="openConfirm()"><i class="ti ti-trash"></i> Delete Quiz</button>
            </div>
        </div>

        <asp:Button ID="btnDelete" runat="server" ClientIDMode="Static" style="display:none;" OnClick="btnDelete_Click" />

        <div class="ins-overlay" id="ovConfirm">
            <div class="ins-modal">
                <div class="m-ic"><i class="ti ti-alert-triangle"></i></div>
                <p>You will not be able to recover this quiz. Do you want to delete this quiz?</p>
                <div class="m-actions">
                    <button type="button" class="ins-btn ghost" onclick="closeConfirm()">No</button>
                    <button type="button" class="ins-btn danger" onclick="document.getElementById('btnDelete').click();">Yes</button>
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
        function openConfirm() { document.getElementById('ovConfirm').classList.add('show'); }
        function closeConfirm() { document.getElementById('ovConfirm').classList.remove('show'); }
    </script>
</body>
</html>
