<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="InstructorDashboard.aspx.cs" Inherits="InstructorDashboard" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Instructor Dashboard - SEA</title>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/@tabler/icons-webfont@3.11.0/dist/tabler-icons.min.css" />
    <link rel="stylesheet" href="Styles/sea-dashboard.css?v=7" />
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

        <div class="sea-body">
            <aside class="sea-side">
                <div class="sea-side-label">LEVELS</div>
                <asp:Repeater ID="rptLevels" runat="server">
                    <ItemTemplate>
                        <a href='<%# "InstructorDashboard.aspx?level=" + Eval("Level") %>' class='lvl <%# Eval("Active") %>'>
                            <i class="ti ti-stack-2" aria-hidden="true"></i>
                            <span>Level <%# Eval("Level") %></span>
                        </a>
                    </ItemTemplate>
                </asp:Repeater>
            </aside>

            <main class="sea-main">
                <h1 class="sea-h">Level <asp:Literal ID="lblLevel" runat="server" Text="1" /> quizzes</h1>
                <p class="sea-sub">Quizzes you created in this level.</p>

                <section class="ins-grid">
                    <asp:Repeater ID="rptQuizzes" runat="server">
                        <ItemTemplate>
                            <div class="ins-card">
                                <h3 class="ins-card-title"><%# Eval("QuizTitle") %></h3>
                                <p class="ins-card-desc"><%# Eval("QuizDescription") %></p>
                                <div class="ins-card-foot">
                                    <span class='ins-badge <%# StatusClass(Eval("QuizStatus")) %>'><%# StatusText(Eval("QuizStatus")) %></span>
                                    <a class="ins-badge view" href='<%# "InstructorViewQuiz.aspx?id=" + Eval("QuizID") %>'>View</a>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </section>

                <asp:Panel ID="pnlEmpty" runat="server" Visible="false" CssClass="ins-panel" Style="text-align:center;color:var(--text-2);">
                    You have not created any quizzes in this level yet.
                    <div style="margin-top:14px;"><a class="ins-btn" href="InstructorCreateQuiz.aspx"><i class="ti ti-plus"></i> Create a quiz</a></div>
                </asp:Panel>
            </main>
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
