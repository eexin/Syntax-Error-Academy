<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AdminQuizApproval.aspx.cs" Inherits="AdminQuizApproval" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Quiz Approval - Admin - SEA</title>
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/@tabler/icons-webfont@3.11.0/dist/tabler-icons.min.css" />
    <link rel="stylesheet" href="Styles/sea-dashboard.css" />
    <link rel="stylesheet" href="Styles/sea-quiz.css" />
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
                    <span class="adm-nlink active"><i class="ti ti-list-check" aria-hidden="true"></i> Quiz Management <i class="ti ti-chevron-down" style="font-size:14px;"></i></span>
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
            <h2 class="adm-h">Quiz Approval</h2>
            <p class="adm-sub">Review quizzes submitted by instructors, then approve or reject them.</p>

            <asp:Panel ID="pnlFlash" runat="server" CssClass="adm-flash" Visible="false">
                <i class="ti ti-circle-check" aria-hidden="true"></i>
                <asp:Literal ID="litFlash" runat="server" />
            </asp:Panel>

            <asp:HiddenField ID="hfTab" runat="server" ClientIDMode="Static" Value="Pending" />
            <asp:HiddenField ID="hfSelectedQuiz" runat="server" ClientIDMode="Static" Value="" />
            <asp:HiddenField ID="hfTargetQuiz" runat="server" ClientIDMode="Static" Value="" />

            <%-- Tabs in the middle --%>
            <div class="adm-toolbar">
                <div class="adm-toolbar-left"></div>
                <div class="adm-toolbar-mid">
                    <asp:LinkButton ID="lnkPending" runat="server" CssClass="adm-tab" CausesValidation="false" OnClick="ShowPending"><i class="ti ti-clock"></i> Pending</asp:LinkButton>
                    <asp:LinkButton ID="lnkApproved" runat="server" CssClass="adm-tab" CausesValidation="false" OnClick="ShowApproved"><i class="ti ti-circle-check"></i> Approved</asp:LinkButton>
                    <asp:LinkButton ID="lnkRejected" runat="server" CssClass="adm-tab" CausesValidation="false" OnClick="ShowRejected"><i class="ti ti-circle-x"></i> Rejected</asp:LinkButton>
                </div>
                <div class="adm-toolbar-right"></div>
            </div>

            <div class="adm-table-wrap">
                <table class="adm-table">
                    <thead>
                        <tr>
                            <th style="width:38px;"></th>
                            <th>Quiz ID</th><th>Quiz Name</th><th>Instructor</th><th>Status</th>
                            <th>Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        <asp:Repeater ID="rptQuizzes" runat="server">
                            <ItemTemplate>
                                <tr data-id='<%# Eval("QuizID") %>'>
                                    <td><input type="radio" name="quizpick" value='<%# Eval("QuizID") %>' onclick="pickQuiz(this)" /></td>
                                    <td>#<%# Eval("QuizID") %></td>
                                    <td><%# Eval("QuizTitle") %></td>
                                    <td><%# Eval("Instructor") %></td>
                                    <td><span class='chip <%# Eval("Status").ToString().ToLower() %>'><%# Eval("Status") %></span></td>
                                    <td><%# IsPending ? RowActions(Eval("QuizID")) : "" %></td>
                                </tr>
                            </ItemTemplate>
                        </asp:Repeater>
                    </tbody>
                </table>
                <asp:Panel ID="pnlEmpty" runat="server" Visible="false" CssClass="adm-empty">No quizzes in this list.</asp:Panel>
            </div>

            <%-- View button, bottom-right --%>
            <div style="display:flex;justify-content:flex-end;margin-top:14px;">
                <button type="button" class="adm-btn ghost" onclick="viewSelected()"><i class="ti ti-eye"></i> View</button>
            </div>
        </div>

        <%-- hidden server triggers --%>
        <asp:Button ID="btnDoApprove" runat="server" ClientIDMode="Static" style="display:none;" OnClick="btnDoApprove_Click" />
        <asp:Button ID="btnDoReject" runat="server" ClientIDMode="Static" style="display:none;" OnClick="btnDoReject_Click" />
        <asp:Button ID="btnDoView" runat="server" ClientIDMode="Static" style="display:none;" OnClick="btnDoView_Click" />

        <%-- ============ VIEW quiz overlay (read-only) ============ --%>
        <div class="adm-overlay" id="ovView">
            <div class="adm-modal wide">
                <div style="display:flex;justify-content:space-between;align-items:center;margin-bottom:6px;">
                    <h2 style="margin:0;"><asp:Literal ID="litViewTitle" runat="server" Text="Quiz" /></h2>
                    <button type="button" class="adm-btn ghost" onclick="closeOverlay('ovView')">Close</button>
                </div>
                <asp:Literal ID="litView" runat="server" />
            </div>
        </div>

        <%-- ============ REJECT reason overlay ============ --%>
        <div class="adm-overlay" id="ovReject">
            <div class="adm-modal">
                <h2>Reject Quiz</h2>
                <p class="modal-sub">Please provide a reason for rejecting this quiz.</p>
                <div class="adm-field">
                    <label>Reason for rejection</label>
                    <asp:TextBox ID="txtRejectReason" runat="server" ClientIDMode="Static" TextMode="MultiLine" />
                    <span class="adm-vmsg" id="rejectMsg" style="display:none;">A reason is required to reject a quiz.</span>
                </div>
                <div class="adm-modal-actions">
                    <button type="button" class="adm-btn ghost" onclick="closeOverlay('ovReject')">Back</button>
                    <button type="button" class="adm-btn danger" onclick="askReject()">Reject</button>
                </div>
            </div>
        </div>

        <%-- ============ generic confirm ============ --%>
        <div class="adm-overlay" id="ovConfirm">
            <div class="adm-modal adm-confirm" id="confirmBox">
                <div class="cf-ic"><i class="ti ti-help-circle" id="confirmIcon"></i></div>
                <p id="confirmMsg">Are you sure?</p>
                <div class="cf-actions">
                    <button type="button" class="adm-btn ghost" id="confirmNo">No</button>
                    <button type="button" class="adm-btn" id="confirmYes">Yes</button>
                </div>
            </div>
        </div>

        <%-- ============ notice ============ --%>
        <div class="adm-overlay" id="ovNotice">
            <div class="adm-modal adm-confirm">
                <div class="cf-ic"><i class="ti ti-alert-circle"></i></div>
                <p id="noticeMsg">Please select a quiz first.</p>
                <div class="cf-actions"><button type="button" class="adm-btn" onclick="closeOverlay('ovNotice')">OK</button></div>
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

        function openOverlay(id) { document.getElementById(id).classList.add('show'); }
        function closeOverlay(id) { document.getElementById(id).classList.remove('show'); }

        var pickedId = '';
        function pickQuiz(radio) {
            var rows = document.querySelectorAll('.adm-table tr.selected');
            rows.forEach(function (r) { r.classList.remove('selected'); });
            radio.closest('tr').classList.add('selected');
            pickedId = radio.value;
            document.getElementById('hfSelectedQuiz').value = pickedId;
        }

        function viewSelected() {
            if (!document.getElementById('hfSelectedQuiz').value) { showNotice('Please select a quiz to view.'); return; }
            document.getElementById('btnDoView').click();
        }

        /* approve / reject per row */
        function openApprove(id) {
            document.getElementById('hfTargetQuiz').value = id;
            openConfirm('Are you sure you want to approve this quiz?', 'ok',
                function () { document.getElementById('btnDoApprove').click(); },
                function () { closeOverlay('ovConfirm'); });
        }
        function openReject(id) {
            document.getElementById('hfTargetQuiz').value = id;
            document.getElementById('txtRejectReason').value = '';
            document.getElementById('rejectMsg').style.display = 'none';
            openOverlay('ovReject');
        }
        function askReject() {
            var reason = document.getElementById('txtRejectReason').value.trim();
            if (!reason) { document.getElementById('rejectMsg').style.display = 'block'; return; }
            openConfirm('Are you sure you want to reject this quiz?', 'danger',
                function () { document.getElementById('btnDoReject').click(); },   // Yes -> reject
                function () { closeOverlay('ovConfirm'); });                        // No -> back to reason
        }

        function openConfirm(message, style, onYes, onNo) {
            document.getElementById('confirmMsg').textContent = message;
            var box = document.getElementById('confirmBox');
            box.classList.remove('ok', 'danger');
            var icon = document.getElementById('confirmIcon');
            if (style === 'danger') { box.classList.add('danger'); icon.className = 'ti ti-alert-triangle'; }
            else if (style === 'ok') { box.classList.add('ok'); icon.className = 'ti ti-circle-check'; }
            else { icon.className = 'ti ti-help-circle'; }
            document.getElementById('confirmYes').onclick = function () { onYes && onYes(); };
            document.getElementById('confirmNo').onclick = function () { onNo && onNo(); };
            openOverlay('ovConfirm');
        }
        function showNotice(msg) { document.getElementById('noticeMsg').textContent = msg; openOverlay('ovNotice'); }
    </script>
    <asp:Literal ID="litStartup" runat="server" EnableViewState="false" />
</body>
</html>
