<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="InstructorCreateQuiz.aspx.cs" Inherits="InstructorCreateQuiz" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Create Quiz - Instructor - SEA</title>
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
                <a href="javascript:void(0)" class="lvl active" data-level="1" onclick="pickLevel(this)"><i class="ti ti-stack-2"></i><span>Level 1</span></a>
                <a href="javascript:void(0)" class="lvl" data-level="2" onclick="pickLevel(this)"><i class="ti ti-stack-2"></i><span>Level 2</span></a>
                <a href="javascript:void(0)" class="lvl" data-level="3" onclick="pickLevel(this)"><i class="ti ti-stack-2"></i><span>Level 3</span></a>
                <a href="javascript:void(0)" class="lvl" data-level="4" onclick="pickLevel(this)"><i class="ti ti-stack-2"></i><span>Level 4</span></a>
                <a href="javascript:void(0)" class="lvl" data-level="5" onclick="pickLevel(this)"><i class="ti ti-stack-2"></i><span>Level 5</span></a>
            </aside>

            <main class="sea-main">
                <h1 class="sea-h">Create Quiz</h1>
                <p class="sea-sub">Building a quiz for <b>Level <span id="lvlLabel">1</span></b>.</p>

                <asp:HiddenField ID="hfLevel" runat="server" ClientIDMode="Static" Value="1" />
                <asp:HiddenField ID="hfName" runat="server" ClientIDMode="Static" />
                <asp:HiddenField ID="hfDesc" runat="server" ClientIDMode="Static" />
                <asp:HiddenField ID="hfQuestions" runat="server" ClientIDMode="Static" />

                <%-- STEP 1: name + description --%>
                <div id="step1">
                    <div class="ins-panel">
                        <div class="ins-field">
                            <label>Quiz name</label>
                            <input type="text" id="txtName1" placeholder="e.g. C# Loops Basics" />
                            <span class="ins-mini" id="nameErr" style="display:none;color:#e0573f;">Quiz name is required.</span>
                        </div>
                        <div class="ins-field">
                            <label>Quiz description</label>
                            <textarea id="txtDesc1" placeholder="Short description of what this quiz covers"></textarea>
                        </div>
                        <div class="ins-btnrow">
                            <button type="button" class="ins-btn ghost" onclick="location.href='InstructorDashboard.aspx'">Back</button>
                            <button type="button" class="ins-btn" onclick="goStep2()">Next</button>
                        </div>
                    </div>
                </div>

                <%-- STEP 2: drag-and-drop builder --%>
                <div id="step2" style="display:none;">
                    <div class="ins-build">
                        <div class="ins-palette">
                            <div class="ins-pal-label">Question types</div>
                            <div class="ins-pal-item" draggable="true" data-type="MCQ" ondragstart="dragStart(event)"><i class="ti ti-list-check"></i> MCQ</div>
                            <div class="ins-pal-item" draggable="true" data-type="Matching" ondragstart="dragStart(event)"><i class="ti ti-arrows-left-right"></i> Matching</div>
                            <div class="ins-pal-item" draggable="true" data-type="Puzzle" ondragstart="dragStart(event)"><i class="ti ti-puzzle"></i> Puzzle</div>
                            <div class="ins-pal-item" draggable="true" data-type="Essay" ondragstart="dragStart(event)"><i class="ti ti-writing"></i> Essay</div>
                        </div>

                        <div class="ins-canvas">
                            <div class="ins-quizhead">
                                <div class="ins-field" style="margin-bottom:10px;">
                                    <label>Quiz name</label>
                                    <input type="text" id="txtName2" />
                                </div>
                                <div class="ins-field" style="margin-bottom:0;">
                                    <label>Quiz description</label>
                                    <textarea id="txtDesc2" style="min-height:60px;"></textarea>
                                </div>
                            </div>

                            <div id="questionsWrap"></div>

                            <div class="ins-adder">
                                <div class="ins-dropzone" id="dropAdd"
                                     ondragover="dragOver(event)" ondragleave="dragLeave(event)" ondrop="dropQ(event)">
                                    <span id="dropHint">Drag and drop a question type to start</span>
                                </div>
                            </div>

                            <div class="ins-btnrow">
                                <button type="button" class="ins-btn ghost" onclick="openLeave()">Back</button>
                                <button type="button" class="ins-btn" onclick="createQuiz()"><i class="ti ti-check"></i> Create Quiz</button>
                            </div>
                            <p class="ins-mini" id="createErr" style="display:none;text-align:center;color:#e0573f;"></p>
                        </div>
                    </div>
                </div>
            </main>
        </div>

        <asp:Button ID="btnCreate" runat="server" ClientIDMode="Static" style="display:none;" OnClick="btnCreate_Click" />

        <%-- leave confirm --%>
        <div class="ins-overlay" id="ovLeave">
            <div class="ins-modal">
                <div class="m-ic"><i class="ti ti-alert-triangle"></i></div>
                <p>All changes will be lost. Do you want to leave?</p>
                <div class="m-actions">
                    <button type="button" class="ins-btn ghost" onclick="closeLeave()">No</button>
                    <button type="button" class="ins-btn danger" onclick="location.href='InstructorDashboard.aspx'">Yes</button>
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

        /* ----- level selection ----- */
        function pickLevel(a) {
            document.querySelectorAll('.sea-side .lvl').forEach(function (x) { x.classList.remove('active'); });
            a.classList.add('active');
            var lv = a.getAttribute('data-level');
            document.getElementById('hfLevel').value = lv;
            document.getElementById('lvlLabel').textContent = lv;
        }

        /* ----- step 1 -> step 2 ----- */
        function goStep2() {
            var name = document.getElementById('txtName1').value.trim();
            if (!name) { document.getElementById('nameErr').style.display = 'block'; return; }
            document.getElementById('nameErr').style.display = 'none';
            document.getElementById('txtName2').value = name;
            document.getElementById('txtDesc2').value = document.getElementById('txtDesc1').value;
            document.getElementById('step1').style.display = 'none';
            document.getElementById('step2').style.display = 'block';
        }

        /* ----- drag & drop ----- */
        var dragType = null;
        function dragStart(ev) { dragType = ev.target.getAttribute('data-type'); ev.dataTransfer.setData('text/plain', dragType); }
        function dragOver(ev) { ev.preventDefault(); ev.currentTarget.classList.add('over'); }
        function dragLeave(ev) { ev.currentTarget.classList.remove('over'); }
        function dropQ(ev) {
            ev.preventDefault();
            ev.currentTarget.classList.remove('over');
            var type = ev.dataTransfer.getData('text/plain') || dragType;
            if (type) addQuestion(type);
        }

        /* ----- build a question block ----- */
        function esc(s) { return (s || '').replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;'); }

        function addQuestion(type) {
            var wrap = document.getElementById('questionsWrap');
            var n = wrap.children.length + 1;
            var block = document.createElement('div');
            block.className = 'ins-qblock';
            block.setAttribute('data-type', type);

            var body = '';
            if (type === 'MCQ') {
                body =
                    '<div class="ins-field"><label>Question</label><input type="text" class="q-text-in" placeholder="Enter the question"></div>' +
                    '<div class="ins-mini">Enter up to 4 options and tick the correct one.</div>' +
                    optRow(0) + optRow(1) + optRow(2) + optRow(3);
            } else if (type === 'Matching') {
                body =
                    '<div class="ins-field"><label>Question</label><input type="text" class="q-text-in" placeholder="e.g. Match the keyword to its meaning"></div>' +
                    '<div class="ins-mini">Left item &rarr; matching right item.</div>' +
                    pairRow() + pairRow() + pairRow();
            } else if (type === 'Puzzle') {
                body =
                    '<div class="ins-field"><label>Question</label><input type="text" class="q-text-in" placeholder="e.g. Arrange the tokens in the correct order"></div>' +
                    '<div class="ins-mini">Enter tokens in their CORRECT order (top = first).</div>' +
                    tokRow() + tokRow() + tokRow() + tokRow();
            } else { // Essay - question only, no answer needed
                body =
                    '<div class="ins-field"><label>Question</label><input type="text" class="q-text-in" placeholder="Enter the essay question"></div>' +
                    '<div class="ins-mini">Students write a free-text response. No answer is required here.</div>';
            }

            block.innerHTML =
                '<div class="ins-qblock-head"><span class="ins-qtag"><i class="ti ti-point"></i> ' + esc(type) + ' &middot; Q' + n + '</span>' +
                '<button type="button" class="ins-qremove" onclick="removeQ(this)" title="Remove">&times;</button></div>' + body;

            wrap.appendChild(block);
            document.getElementById('dropHint').textContent = 'Drag and drop a question type to add more questions';
            renumber();
        }
        function optRow(i) {
            return '<div class="ins-opt-row"><span class="lbl">Correct</span>' +
                   '<input type="radio" name="__corr_' + Date.now() + Math.random().toString(36).slice(2) + '" class="q-corr" ' + (i === 0 ? 'checked' : '') + '>' +
                   '<input type="text" class="q-opt" placeholder="Option ' + (i + 1) + '"></div>';
        }
        function pairRow() {
            return '<div class="ins-opt-row"><input type="text" class="q-left" placeholder="Left"><span class="lbl">&rarr;</span><input type="text" class="q-right" placeholder="Right"></div>';
        }
        function tokRow() {
            return '<div class="ins-opt-row"><input type="text" class="q-tok" placeholder="Token"></div>';
        }
        function removeQ(btn) { btn.closest('.ins-qblock').remove(); renumber(); }
        function renumber() {
            var blocks = document.querySelectorAll('#questionsWrap .ins-qblock');
            blocks.forEach(function (b, i) {
                var tag = b.querySelector('.ins-qtag');
                tag.innerHTML = '<i class="ti ti-point"></i> ' + b.getAttribute('data-type') + ' &middot; Q' + (i + 1);
            });
            // fix radio grouping per block so each MCQ has independent "correct"
            document.querySelectorAll('#questionsWrap .ins-qblock').forEach(function (b, i) {
                b.querySelectorAll('.q-corr').forEach(function (r) { r.name = '__corr_block_' + i; });
            });
            if (blocks.length === 0) document.getElementById('dropHint').textContent = 'Drag and drop a question type to start';
        }

        /* ----- create ----- */
        function createQuiz() {
            var name = document.getElementById('txtName2').value.trim();
            var err = document.getElementById('createErr');
            if (!name) { err.textContent = 'Quiz name is required.'; err.style.display = 'block'; return; }

            var blocks = document.querySelectorAll('#questionsWrap .ins-qblock');
            if (blocks.length === 0) { err.textContent = 'Add at least one question.'; err.style.display = 'block'; return; }

            var questions = [];
            for (var i = 0; i < blocks.length; i++) {
                var b = blocks[i];
                var type = b.getAttribute('data-type');
                var text = (b.querySelector('.q-text-in') || {}).value || '';
                if (!text.trim()) { err.textContent = 'Question ' + (i + 1) + ' needs a question text.'; err.style.display = 'block'; return; }
                var q = { type: type, text: text.trim() };

                if (type === 'MCQ') {
                    var opts = [], correct = 0, radios = b.querySelectorAll('.q-corr'), texts = b.querySelectorAll('.q-opt');
                    for (var k = 0; k < texts.length; k++) {
                        if (texts[k].value.trim() !== '') {
                            if (radios[k].checked) correct = opts.length;
                            opts.push(texts[k].value.trim());
                        }
                    }
                    if (opts.length < 2) { err.textContent = 'Question ' + (i + 1) + ' (MCQ) needs at least 2 options.'; err.style.display = 'block'; return; }
                    q.options = opts; q.correct = correct;
                } else if (type === 'Matching') {
                    var pairs = [], L = b.querySelectorAll('.q-left'), R = b.querySelectorAll('.q-right');
                    for (var p = 0; p < L.length; p++) if (L[p].value.trim() && R[p].value.trim()) pairs.push([L[p].value.trim(), R[p].value.trim()]);
                    if (pairs.length < 1) { err.textContent = 'Question ' + (i + 1) + ' (Matching) needs at least 1 pair.'; err.style.display = 'block'; return; }
                    q.pairs = pairs;
                } else if (type === 'Puzzle') {
                    var toks = [];
                    b.querySelectorAll('.q-tok').forEach(function (t) { if (t.value.trim()) toks.push(t.value.trim()); });
                    if (toks.length < 2) { err.textContent = 'Question ' + (i + 1) + ' (Puzzle) needs at least 2 tokens.'; err.style.display = 'block'; return; }
                    q.tokens = toks;
                }
                // Essay needs nothing beyond the question text
                questions.push(q);
            }

            document.getElementById('hfName').value = name;
            document.getElementById('hfDesc').value = document.getElementById('txtDesc2').value;
            document.getElementById('hfQuestions').value = JSON.stringify(questions);
            document.getElementById('btnCreate').click();
        }

        function openLeave() { document.getElementById('ovLeave').classList.add('show'); }
        function closeLeave() { document.getElementById('ovLeave').classList.remove('show'); }
    </script>
</body>
</html>
