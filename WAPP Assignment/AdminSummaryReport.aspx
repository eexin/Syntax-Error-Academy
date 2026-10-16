<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AdminSummaryReport.aspx.cs" Inherits="AdminSummaryReport" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Summary Report - Admin - SEA</title>
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
            <h2 class="adm-h">Summary Report</h2>
            <p class="adm-sub">Aggregated performance across every quiz for a chosen period.</p>

            <asp:HiddenField ID="hfType" runat="server" ClientIDMode="Static" />
            <asp:HiddenField ID="hfYear" runat="server" ClientIDMode="Static" />
            <asp:HiddenField ID="hfMonth" runat="server" ClientIDMode="Static" />
            <asp:HiddenField ID="hfStart" runat="server" ClientIDMode="Static" />
            <asp:HiddenField ID="hfEnd" runat="server" ClientIDMode="Static" />
            <asp:HiddenField ID="hfLabel" runat="server" ClientIDMode="Static" />

            <%-- selection nav bar --%>
            <div class="adm-toolbar sum-bar">
                <div class="adm-toolbar-left" style="align-items:flex-end;">
                    <div class="sum-field show">
                        <label>Report type</label>
                        <select id="selType">
                            <option value="">Select report type</option>
                            <option value="Weekly">Weekly summary report</option>
                            <option value="Monthly">Monthly summary report</option>
                            <option value="Yearly">Yearly summary report</option>
                        </select>
                    </div>
                    <div class="sum-field" id="wrapYear">
                        <label>Year</label>
                        <select id="selYear"><option value="">Select year</option></select>
                    </div>
                    <div class="sum-field" id="wrapMonth">
                        <label>Month</label>
                        <select id="selMonth"><option value="">Select month</option></select>
                    </div>
                    <div class="sum-field" id="wrapWeek">
                        <label>Week</label>
                        <select id="selWeek"><option value="">Select week</option></select>
                    </div>
                </div>
                <div class="adm-toolbar-right">
                    <span id="wrapGen">
                        <asp:Button ID="btnGenerate" runat="server" ClientIDMode="Static" CssClass="adm-btn"
                            Text="Generate" OnClientClick="return prepGenerate();" OnClick="btnGenerate_Click" />
                    </span>
                </div>
            </div>

            <asp:Literal ID="litReport" runat="server" />
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

        var MON = ['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec'];
        function el(id){ return document.getElementById(id); }
        function show(id){ el(id).classList.add('show'); }
        function hide(id){ el(id).classList.remove('show'); }
        function pad(n){ return (n<10?'0':'')+n; }
        function iso(d){ return d.getFullYear()+'-'+pad(d.getMonth()+1)+'-'+pad(d.getDate()); }
        function fmt(d){ return d.getDate()+' '+MON[d.getMonth()]; }

        var selType=el('selType'), selYear=el('selYear'), selMonth=el('selMonth'), selWeek=el('selWeek');

        function fillYears(){
            if (selYear.options.length>1) return;
            var y=new Date().getFullYear();
            for(var i=y;i>=y-4;i--){ var o=document.createElement('option'); o.value=i; o.textContent=i; selYear.appendChild(o); }
        }
        function fillMonths(){
            if (selMonth.options.length>1) return;
            for(var m=1;m<=12;m++){ var o=document.createElement('option'); o.value=m; o.textContent=MON[m-1]; selMonth.appendChild(o); }
        }
        // Monday-start weeks; week 1 = first Monday of the month.
        function weeksOf(year, month){
            var weeks=[], d=new Date(year, month-1, 1), day=d.getDay();
            var add=(day===1)?0:((8-day)%7);
            var cur=new Date(year, month-1, 1+add), wk=1;
            while(cur.getMonth()===month-1){
                var s=new Date(cur), e=new Date(cur); e.setDate(e.getDate()+6);
                weeks.push({wk:wk,s:s,e:e}); cur=new Date(cur); cur.setDate(cur.getDate()+7); wk++;
            }
            return weeks;
        }
        function fillWeeks(year, month){
            selWeek.innerHTML='<option value="">Select week</option>';
            weeksOf(year,month).forEach(function(w){
                var o=document.createElement('option');
                o.value=iso(w.s)+'|'+iso(w.e);
                o.textContent='Week '+w.wk+' — '+fmt(w.s)+' to '+fmt(w.e);
                selWeek.appendChild(o);
            });
        }

        selType.onchange=function(){
            hide('wrapYear'); hide('wrapMonth'); hide('wrapWeek'); hide('wrapGen');
            selYear.value=''; selMonth.value=''; selWeek.value='';
            if(selType.value){ fillYears(); show('wrapYear'); }
        };
        selYear.onchange=function(){
            hide('wrapMonth'); hide('wrapWeek'); hide('wrapGen'); selMonth.value=''; selWeek.value='';
            if(!selYear.value) return;
            if(selType.value==='Yearly'){ show('wrapGen'); }
            else { fillMonths(); show('wrapMonth'); }
        };
        selMonth.onchange=function(){
            hide('wrapWeek'); hide('wrapGen'); selWeek.value='';
            if(!selMonth.value) return;
            if(selType.value==='Monthly'){ show('wrapGen'); }
            else if(selType.value==='Weekly'){ fillWeeks(+selYear.value,+selMonth.value); show('wrapWeek'); }
        };
        selWeek.onchange=function(){ if(selWeek.value) show('wrapGen'); else hide('wrapGen'); };

        // build the hidden fields the server reads, right before postback
        function prepGenerate(){
            var t=selType.value, y=+selYear.value;
            if(!t || !y){ return false; }
            var start, end, label;
            if(t==='Yearly'){
                start=new Date(y,0,1); end=new Date(y,11,31); label=''+y;
            } else if(t==='Monthly'){
                if(!selMonth.value) return false;
                var m=+selMonth.value; start=new Date(y,m-1,1); end=new Date(y,m,0);
                label=MON[m-1]+' '+y;
            } else { // Weekly
                if(!selWeek.value) return false;
                var parts=selWeek.value.split('|');
                start=new Date(parts[0]+'T00:00:00'); end=new Date(parts[1]+'T00:00:00');
                label='Week of '+fmt(start)+' – '+fmt(end)+' '+end.getFullYear();
            }
            el('hfType').value=t; el('hfYear').value=y; el('hfMonth').value=(selMonth.value||'');
            el('hfStart').value=iso(start); el('hfEnd').value=iso(end); el('hfLabel').value=label;
            return true;
        }

        // after a Generate postback, rebuild the selections so they stay visible
        (function restore(){
            var t=el('hfType').value; if(!t) return;
            selType.value=t; fillYears(); show('wrapYear');
            selYear.value=el('hfYear').value;
            if(t==='Yearly'){ show('wrapGen'); return; }
            fillMonths(); show('wrapMonth'); selMonth.value=el('hfMonth').value;
            if(t==='Monthly'){ show('wrapGen'); return; }
            if(t==='Weekly'){ fillWeeks(+el('hfYear').value,+el('hfMonth').value); show('wrapWeek');
                selWeek.value=el('hfStart').value+'|'+el('hfEnd').value; show('wrapGen'); }
        })();
    </script>
</body>
</html>
