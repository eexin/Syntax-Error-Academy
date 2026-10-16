<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AdminViewUser.aspx.cs" Inherits="AdminViewUser" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>View User - Admin - SEA</title>
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
                <a class="adm-nlink active" href="AdminViewUser.aspx"><i class="ti ti-users" aria-hidden="true"></i> View User</a>
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
            <h2 class="adm-h">View User</h2>
            <p class="adm-sub">Browse and manage every registered account.</p>

            <asp:Panel ID="pnlFlash" runat="server" CssClass="adm-flash" Visible="false">
                <i class="ti ti-circle-check" aria-hidden="true"></i>
                <asp:Literal ID="litFlash" runat="server" />
            </asp:Panel>

            <asp:HiddenField ID="hfActiveView" runat="server" ClientIDMode="Static" Value="student" />
            <asp:HiddenField ID="hfSelectedId" runat="server" ClientIDMode="Static" Value="" />

            <%-- ===== Toolbar A: switch between the two tables (middle) ===== --%>
            <div class="adm-toolbar">
                <div class="adm-toolbar-left"></div>
                <div class="adm-toolbar-mid">
                    <asp:LinkButton ID="lnkStudents" runat="server" CssClass="adm-tab" OnClick="ShowStudents"
                        CausesValidation="false"><i class="ti ti-school"></i> Students</asp:LinkButton>
                    <asp:LinkButton ID="lnkInstructors" runat="server" CssClass="adm-tab" OnClick="ShowInstructors"
                        CausesValidation="false"><i class="ti ti-chalkboard"></i> Instructors</asp:LinkButton>
                    <asp:LinkButton ID="lnkAdmins" runat="server" CssClass="adm-tab" OnClick="ShowAdmins"
                        CausesValidation="false"><i class="ti ti-shield-lock"></i> Admins</asp:LinkButton>
                </div>
                <div class="adm-toolbar-right"></div>
            </div>

            <%-- ===== Toolbar B: the four functions ===== --%>
            <div class="adm-toolbar">
                <div class="adm-toolbar-left">
                    <button type="button" class="adm-btn" onclick="openAdd()"><i class="ti ti-user-plus"></i> Add User</button>
                    <asp:TextBox ID="txtSearch" runat="server" CssClass="adm-search-input"
                        placeholder="Search by ID or name" Style="background:var(--bg);border:1px solid var(--border);border-radius:9px;color:var(--text);padding:9px 12px;font-size:13px;min-width:200px;" />
                    <asp:Button ID="btnSearch" runat="server" CssClass="adm-btn ghost" Text="Search"
                        OnClick="btnSearch_Click" CausesValidation="false" />
                </div>
                <div class="adm-toolbar-right">
                    <button type="button" class="adm-btn ghost" onclick="editSelected()"><i class="ti ti-edit"></i> Edit User</button>
                    <button type="button" class="adm-btn danger" onclick="deleteSelected()"><i class="ti ti-trash"></i> Delete User</button>
                </div>
            </div>

            <%-- ===== Student table ===== --%>
            <asp:Panel ID="pnlStudentTable" runat="server">
                <div class="adm-table-wrap">
                    <table class="adm-table">
                        <thead>
                            <tr>
                                <th style="width:38px;"></th>
                                <th>ID</th><th>Name</th><th>DOB</th><th>Email</th><th>Password (hashed)</th>
                                <th>Total Score</th><th>Total Point</th><th>Streak</th><th>Last Active</th>
                            </tr>
                        </thead>
                        <tbody>
                            <asp:Repeater ID="rptStudents" runat="server">
                                <ItemTemplate>
                                    <tr data-id='<%# Eval("StudentID") %>'
                                        data-name='<%# System.Web.HttpUtility.HtmlEncode(Convert.ToString(Eval("StudentName"))) %>'
                                        data-dob='<%# Eval("StudentDOB","{0:yyyy-MM-dd}") %>'
                                        data-email='<%# System.Web.HttpUtility.HtmlEncode(Convert.ToString(Eval("StudentEmail"))) %>'
                                        data-score='<%# Eval("TotalScore") %>'
                                        data-point='<%# Eval("TotalPoint") %>'>
                                        <td><input type="radio" name="userpick" value='<%# Eval("StudentID") %>' onclick="pickUser(this)" /></td>
                                        <td><%# Eval("StudentID") %></td>
                                        <td><%# Eval("StudentName") %></td>
                                        <td><%# Eval("StudentDOB","{0:yyyy-MM-dd}") %></td>
                                        <td><%# Eval("StudentEmail") %></td>
                                        <td class="pwd-cell" title='<%# Eval("StudentPassword") %>'><%# Eval("StudentPassword") %></td>
                                        <td><%# Eval("TotalScore") %></td>
                                        <td><%# Eval("TotalPoint") %></td>
                                        <td><%# Eval("Streak") %></td>
                                        <td><%# Eval("LastActiveDate","{0:yyyy-MM-dd}") %></td>
                                    </tr>
                                </ItemTemplate>
                            </asp:Repeater>
                        </tbody>
                    </table>
                    <asp:Panel ID="pnlStudentEmpty" runat="server" Visible="false" CssClass="adm-empty">No student records found.</asp:Panel>
                </div>
            </asp:Panel>

            <%-- ===== Instructor table (data is NULL for now) ===== --%>
            <asp:Panel ID="pnlInstructorTable" runat="server" Visible="false">
                <div class="adm-table-wrap">
                    <table class="adm-table">
                        <thead>
                            <tr>
                                <th style="width:38px;"></th>
                                <th>ID</th><th>Name</th><th>DOB</th><th>Email</th><th>Password (hashed)</th>
                            </tr>
                        </thead>
                        <tbody>
                            <asp:Repeater ID="rptInstructors" runat="server">
                                <ItemTemplate>
                                    <tr data-id='<%# Eval("InstructorID") %>'
                                        data-name='<%# System.Web.HttpUtility.HtmlEncode(Convert.ToString(Eval("InstructorName"))) %>'
                                        data-dob='<%# Eval("InstructorDOB","{0:yyyy-MM-dd}") %>'
                                        data-email='<%# System.Web.HttpUtility.HtmlEncode(Convert.ToString(Eval("InstructorEmail"))) %>'
                                        data-score='' data-point=''>
                                        <td><input type="radio" name="userpick" value='<%# Eval("InstructorID") %>' onclick="pickUser(this)" /></td>
                                        <td><%# Eval("InstructorID") %></td>
                                        <td><%# Eval("InstructorName") %></td>
                                        <td><%# Eval("InstructorDOB","{0:yyyy-MM-dd}") %></td>
                                        <td><%# Eval("InstructorEmail") %></td>
                                        <td class="pwd-cell" title='<%# Eval("InstructorPassword") %>'><%# Eval("InstructorPassword") %></td>
                                    </tr>
                                </ItemTemplate>
                            </asp:Repeater>
                        </tbody>
                    </table>
                    <asp:Panel ID="pnlInstructorEmpty" runat="server" Visible="false" CssClass="adm-empty">No instructor records found.</asp:Panel>
                </div>
            </asp:Panel>

            <%-- ===== Admin table ===== --%>
            <asp:Panel ID="pnlAdminTable" runat="server" Visible="false">
                <div class="adm-table-wrap">
                    <table class="adm-table">
                        <thead>
                            <tr>
                                <th style="width:38px;"></th>
                                <th>ID</th><th>Name</th><th>DOB</th><th>Email</th><th>Password (hashed)</th>
                            </tr>
                        </thead>
                        <tbody>
                            <asp:Repeater ID="rptAdmins" runat="server">
                                <ItemTemplate>
                                    <tr data-id='<%# Eval("AdminID") %>'
                                        data-name='<%# System.Web.HttpUtility.HtmlEncode(Convert.ToString(Eval("AdminName"))) %>'
                                        data-dob='<%# Eval("AdminDOB","{0:yyyy-MM-dd}") %>'
                                        data-email='<%# System.Web.HttpUtility.HtmlEncode(Convert.ToString(Eval("AdminEmail"))) %>'
                                        data-score='' data-point=''>
                                        <td><input type="radio" name="userpick" value='<%# Eval("AdminID") %>' onclick="pickUser(this)" /></td>
                                        <td><%# Eval("AdminID") %></td>
                                        <td><%# Eval("AdminName") %></td>
                                        <td><%# Eval("AdminDOB","{0:yyyy-MM-dd}") %></td>
                                        <td><%# Eval("AdminEmail") %></td>
                                        <td class="pwd-cell" title='<%# Eval("AdminPassword") %>'><%# Eval("AdminPassword") %></td>
                                    </tr>
                                </ItemTemplate>
                            </asp:Repeater>
                        </tbody>
                    </table>
                    <asp:Panel ID="pnlAdminEmpty" runat="server" Visible="false" CssClass="adm-empty">No admin records found.</asp:Panel>
                </div>
            </asp:Panel>
        </div>

        <%-- ================= ADD USER overlay ================= --%>
        <div class="adm-overlay" id="ovAdd">
            <div class="adm-modal">
                <h2>Add User</h2>
                <p class="modal-sub">Create a new student or instructor account.</p>

                <div class="adm-field">
                    <label>Role</label>
                    <asp:DropDownList ID="ddlAddRole" runat="server" ClientIDMode="Static">
                        <asp:ListItem Value="Student" Text="Student" />
                        <asp:ListItem Value="Instructor" Text="Instructor" />
                        <asp:ListItem Value="Admin" Text="Admin" />
                    </asp:DropDownList>
                </div>
                <div class="adm-field">
                    <label>Name</label>
                    <asp:TextBox ID="txtAddName" runat="server" ClientIDMode="Static" />
                </div>
                <div class="adm-field">
                    <label>Date of Birth</label>
                    <asp:TextBox ID="txtAddDob" runat="server" ClientIDMode="Static" TextMode="Date" />
                </div>
                <div class="adm-field">
                    <label>Email</label>
                    <asp:TextBox ID="txtAddEmail" runat="server" ClientIDMode="Static" TextMode="Email" />
                </div>
                <div class="adm-field">
                    <label>Password</label>
                    <asp:TextBox ID="txtAddPassword" runat="server" ClientIDMode="Static" TextMode="Password" />
                    <span class="adm-vmsg" id="addMsg" style="display:none;"></span>
                </div>

                <div class="adm-modal-actions">
                    <button type="button" class="adm-btn ghost" onclick="closeOverlay('ovAdd')">Cancel</button>
                    <asp:Button ID="btnAddUser" runat="server" ClientIDMode="Static" CssClass="adm-btn"
                        Text="Add User" OnClientClick="return validateAdd();" OnClick="btnAddUser_Click" />
                </div>
            </div>
        </div>

        <%-- ================= EDIT USER overlay ================= --%>
        <div class="adm-overlay" id="ovEdit">
            <div class="adm-modal">
                <h2>Edit User</h2>
                <p class="modal-sub">Update the selected account's details.</p>

                <asp:HiddenField ID="hfEditId" runat="server" ClientIDMode="Static" />
                <asp:HiddenField ID="hfEditRole" runat="server" ClientIDMode="Static" />

                <div class="adm-field">
                    <label>ID (read-only)</label>
                    <asp:TextBox ID="txtEditId" runat="server" ClientIDMode="Static" ReadOnly="true" />
                </div>
                <div class="adm-field">
                    <label>Username</label>
                    <asp:TextBox ID="txtEditName" runat="server" ClientIDMode="Static" />
                </div>
                <div class="adm-field">
                    <label>Date of Birth</label>
                    <asp:TextBox ID="txtEditDob" runat="server" ClientIDMode="Static" TextMode="Date" />
                </div>
                <div class="adm-field">
                    <label>Email</label>
                    <asp:TextBox ID="txtEditEmail" runat="server" ClientIDMode="Static" TextMode="Email" />
                </div>
                <div class="adm-field" id="editScoreWrap">
                    <label>Total Score</label>
                    <asp:TextBox ID="txtEditScore" runat="server" ClientIDMode="Static" TextMode="Number" />
                </div>
                <div class="adm-field" id="editPointWrap">
                    <label>Total Point</label>
                    <asp:TextBox ID="txtEditPoint" runat="server" ClientIDMode="Static" TextMode="Number" />
                </div>

                <div class="adm-modal-actions">
                    <button type="button" class="adm-btn ghost" onclick="cancelEdit()">Cancel</button>
                    <button type="button" class="adm-btn" onclick="askSaveEdit()">Save</button>
                </div>

                <%-- hidden real submit, fired only after the confirm dialog --%>
                <asp:Button ID="btnSaveEdit" runat="server" ClientIDMode="Static" style="display:none;"
                    OnClick="btnSaveEdit_Click" />
            </div>
        </div>

        <%-- hidden real delete submit --%>
        <asp:Button ID="btnDoDelete" runat="server" ClientIDMode="Static" style="display:none;" OnClick="btnDoDelete_Click" />

        <%-- ================= Generic confirm dialog ================= --%>
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

        <%-- ================= "Please select a user" notice ================= --%>
        <div class="adm-overlay" id="ovNotice">
            <div class="adm-modal adm-confirm">
                <div class="cf-ic"><i class="ti ti-alert-circle"></i></div>
                <p id="noticeMsg">Please select a user first.</p>
                <div class="cf-actions">
                    <button type="button" class="adm-btn" onclick="closeOverlay('ovNotice')">OK</button>
                </div>
            </div>
        </div>

        <button type="button" id="seaToggle" class="sea-toggle" aria-label="Toggle dark and light mode">
            <i class="ti ti-sun" aria-hidden="true"></i>
        </button>
    </form>

    <script>
        /* ---------- theme toggle ---------- */
        (function () {
            var btn = document.getElementById('seaToggle');
            function paint() { btn.innerHTML = document.documentElement.classList.contains('light') ? '<i class="ti ti-moon"></i>' : '<i class="ti ti-sun"></i>'; }
            paint();
            btn.addEventListener('click', function () {
                var light = document.documentElement.classList.toggle('light');
                localStorage.setItem('sea-theme', light ? 'light' : 'dark'); paint();
            });
        })();

        /* ---------- overlay helpers ---------- */
        function openOverlay(id) { document.getElementById(id).classList.add('show'); }
        function closeOverlay(id) { document.getElementById(id).classList.remove('show'); }

        /* ---------- selection ---------- */
        var picked = null;               // the selected <tr>
        function pickUser(radio) {
            var tr = radio.closest('tr');
            var tbl = document.querySelectorAll('.adm-table tr.selected');
            tbl.forEach(function (r) { r.classList.remove('selected'); });
            tr.classList.add('selected');
            picked = tr;
            document.getElementById('hfSelectedId').value = tr.getAttribute('data-id');
        }

        /* ---------- ADD ---------- */
        function openAdd() {
            document.getElementById('txtAddName').value = '';
            document.getElementById('txtAddDob').value = '';
            document.getElementById('txtAddEmail').value = '';
            document.getElementById('txtAddPassword').value = '';
            document.getElementById('addMsg').style.display = 'none';
            openOverlay('ovAdd');
        }
        function validateAdd() {
            var n = document.getElementById('txtAddName').value.trim();
            var e = document.getElementById('txtAddEmail').value.trim();
            var p = document.getElementById('txtAddPassword').value;
            var msg = document.getElementById('addMsg');
            if (!n || !e || !p) { msg.textContent = 'Name, email and password are required.'; msg.style.display = 'block'; return false; }
            return true;   // allow the server postback (btnAddUser_Click)
        }

        /* ---------- EDIT ---------- */
        var editOriginal = {};
        function editSelected() {
            var id = document.getElementById('hfSelectedId').value;
            if (!id || !picked) { showNotice('Please select a user before editing.'); return; }

            var role = document.getElementById('hfActiveView').value;   // student | instructor
            document.getElementById('hfEditRole').value = role;
            document.getElementById('hfEditId').value = id;
            document.getElementById('txtEditId').value = id;
            document.getElementById('txtEditName').value = picked.getAttribute('data-name');
            document.getElementById('txtEditDob').value = picked.getAttribute('data-dob');
            document.getElementById('txtEditEmail').value = picked.getAttribute('data-email');
            document.getElementById('txtEditScore').value = picked.getAttribute('data-score') || '0';
            document.getElementById('txtEditPoint').value = picked.getAttribute('data-point') || '0';

            // instructors have no score / point
            var isStudent = (role === 'student');
            document.getElementById('editScoreWrap').style.display = isStudent ? '' : 'none';
            document.getElementById('editPointWrap').style.display = isStudent ? '' : 'none';

            editOriginal = snapshotEdit();
            openOverlay('ovEdit');
        }
        function snapshotEdit() {
            return {
                name: document.getElementById('txtEditName').value,
                dob: document.getElementById('txtEditDob').value,
                email: document.getElementById('txtEditEmail').value,
                score: document.getElementById('txtEditScore').value,
                point: document.getElementById('txtEditPoint').value
            };
        }
        function editChanged() {
            var c = snapshotEdit();
            return c.name !== editOriginal.name || c.dob !== editOriginal.dob ||
                   c.email !== editOriginal.email || c.score !== editOriginal.score ||
                   c.point !== editOriginal.point;
        }
        function doSaveEdit() { document.getElementById('btnSaveEdit').click(); }

        function askSaveEdit() {
            openConfirm('Are you sure you want to save these changes?', 'question',
                function () { doSaveEdit(); },              // Yes -> save
                function () { closeOverlay('ovConfirm'); }  // No  -> back to edit overlay
            );
        }
        function cancelEdit() {
            if (!editChanged()) { closeOverlay('ovEdit'); return; }   // no changes -> straight back
            openConfirm('You have unsaved changes. Do you want to save them?', 'question',
                function () { doSaveEdit(); },                                    // Yes -> save
                function () { closeOverlay('ovConfirm'); closeOverlay('ovEdit'); } // No  -> discard
            );
        }

        /* ---------- DELETE ---------- */
        function deleteSelected() {
            var id = document.getElementById('hfSelectedId').value;
            if (!id) { showNotice('Please select a user before deleting.'); return; }
            openConfirm('Are you sure you want to delete this user? This cannot be undone.', 'danger',
                function () { document.getElementById('btnDoDelete').click(); },   // Yes -> delete
                function () { closeOverlay('ovConfirm'); }                          // No  -> back to table
            );
        }

        /* ---------- generic confirm ---------- */
        function openConfirm(message, style, onYes, onNo) {
            document.getElementById('confirmMsg').textContent = message;
            var box = document.getElementById('confirmBox');
            box.classList.remove('ok', 'danger');
            var icon = document.getElementById('confirmIcon');
            if (style === 'danger') { box.classList.add('danger'); icon.className = 'ti ti-alert-triangle'; }
            else { icon.className = 'ti ti-help-circle'; }
            var yes = document.getElementById('confirmYes');
            var no = document.getElementById('confirmNo');
            yes.onclick = function () { onYes && onYes(); };
            no.onclick = function () { onNo && onNo(); };
            openOverlay('ovConfirm');
        }
        function showNotice(msg) { document.getElementById('noticeMsg').textContent = msg; openOverlay('ovNotice'); }
    </script>
</body>
</html>
