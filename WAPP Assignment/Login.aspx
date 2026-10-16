<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="Login" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>Log in - Syntax Error Academy</title>
    <link rel="stylesheet" href="Styles/sea-auth.css?v=3" />
    <style>
        /* ---- Internal CSS: animated lamp (login page only) ---- */
        :root {
            --on: 0; --shade-hue: 320; --glow-color: hsl(320,60%,60%);
            --cord: hsl(210,0%,calc((40 + (var(--on,0)*50))*1%));
            --opening: hsl(50,calc((10 + (var(--on,0)*80))*1%),calc((20 + (var(--on,0)*70))*1%));
            --feature: #0a0a0a; --tongue: #e06952;
            --base-top: hsl(210,0%,calc((40 + (var(--on,0)*40))*1%));
            --base-side: hsl(210,0%,calc((20 + (var(--on,0)*40))*1%));
            --post: hsl(210,0%,calc((20 + (var(--on,0)*40))*1%));
            --b-1: hsla(45,0%,calc((50 + (var(--on,0)*50))*1%),.85);
            --b-2: hsla(45,0%,calc((20 + (var(--on,0)*30))*1%),.25);
            --b-3: hsla(45,0%,calc((20 + (var(--on,0)*30))*1%),.5);
            --b-4: hsla(45,0%,calc((20 + (var(--on,0)*30))*1%),.25);
            --l-1: hsla(45,calc((var(--on,0)*20)*1%),calc((50 + (var(--on,0)*50))*1%),.85);
            --l-2: hsla(45,calc((var(--on,0)*20)*1%),calc((50 + (var(--on,0)*50))*1%),.85);
            --t-1: hsl(var(--shade-hue),calc((var(--on,0)*20)*1%),calc((30 + (var(--on,0)*60))*1%));
            --t-2: hsl(var(--shade-hue),calc((var(--on,0)*20)*1%),calc((20 + (var(--on,0)*35))*1%));
            --t-3: hsl(var(--shade-hue),calc((var(--on,0)*20)*1%),calc((10 + (var(--on,0)*20))*1%));
        }
        html.lamp-on { --on: 1; }

        .lamp-row { display: flex; align-items: center; justify-content: center; gap: 14px; margin-bottom: 6px; }
        .lamp { display: block; height: 96px; overflow: visible !important; cursor: pointer; }
        .cord { stroke: var(--cord); }
        .cord--rig { display: none; }
        .lamp__tongue { fill: var(--tongue); }
        .lamp__hit { cursor: pointer; opacity: 0; }
        .lamp__feature { fill: var(--feature); }
        .lamp__stroke { stroke: var(--feature); }
        .lamp__mouth, .lamp__light { opacity: var(--on,0); transition: opacity .4s ease; }
        .shade__opening { fill: var(--opening); }
        .shade__opening-shade { opacity: calc(1 - var(--on,0)); transition: opacity .4s ease; }
        .post__body { fill: var(--post); }
        .base__top { fill: var(--base-top); }
        .base__side { fill: var(--base-side); }
        .top__body { fill: var(--t-3); }
        .lamp__eye { transform-box: fill-box; transform-origin: center; transform: rotate(180deg); transition: transform .35s ease; }
        html.lamp-on .lamp__eye { transform: rotate(0deg); }

        .lamp-hint {
            position: relative; max-width: 130px; line-height: 1.35; font-size: 12px; color: #d6d6d2;
            background: rgba(255,255,255,.07); border: 1px solid rgba(255,255,255,.14);
            padding: 7px 11px; border-radius: 9px; animation: hintNudge 1.6s ease-in-out infinite;
        }
        .lamp-hint::before {
            content: ""; position: absolute; left: -7px; top: 50%; transform: translateY(-50%);
            border: 7px solid transparent; border-right-color: rgba(255,255,255,.14);
        }
        @keyframes hintNudge { 0%,100% { transform: translateX(0); } 50% { transform: translateX(4px); } }

        .auth-card { transition: box-shadow .4s ease; }
        .auth-card.lit { box-shadow: 0 10px 40px rgba(0,0,0,.45), 0 0 30px var(--glow-color); }
    </style>
</head>
<body class="auth-page">
    <form id="form1" runat="server">

        <!-- Animated lamp: pull the cord to light it up -->
            <div class="lamp-row">
            <svg
                            class="lamp"
                            viewBox="0 0 333 484"
                            fill="none"
                            xmlns="http://www.w3.org/2000/svg"
                        >
                            <g class="lamp__shade shade">
                                <ellipse
                                    class="shade__opening"
                                    cx="165"
                                    cy="220"
                                    rx="130"
                                    ry="20"
                                />
                                <ellipse
                                    class="shade__opening-shade"
                                    cx="165"
                                    cy="220"
                                    rx="130"
                                    ry="20"
                                    fill="url(#opening-shade)"
                                />
                            </g>
                            <g class="lamp__base base">
                                <path
                                    class="base__side"
                                    d="M165 464c44.183 0 80-8.954 80-20v-14h-22.869c-14.519-3.703-34.752-6-57.131-6-22.379 0-42.612 2.297-57.131 6H85v14c0 11.046 35.817 20 80 20z"
                                />
                                <path
                                    d="M165 464c44.183 0 80-8.954 80-20v-14h-22.869c-14.519-3.703-34.752-6-57.131-6-22.379 0-42.612 2.297-57.131 6H85v14c0 11.046 35.817 20 80 20z"
                                    fill="url(#side-shading)"
                                />
                                <ellipse
                                    class="base__top"
                                    cx="165"
                                    cy="430"
                                    rx="80"
                                    ry="20"
                                />
                                <ellipse
                                    cx="165"
                                    cy="430"
                                    rx="80"
                                    ry="20"
                                    fill="url(#base-shading)"
                                />
                            </g>
                            <g class="lamp__post post">
                                <path
                                    class="post__body"
                                    d="M180 142h-30v286c0 3.866 6.716 7 15 7 8.284 0 15-3.134 15-7V142z"
                                />
                                <path
                                    d="M180 142h-30v286c0 3.866 6.716 7 15 7 8.284 0 15-3.134 15-7V142z"
                                    fill="url(#post-shading)"
                                />
                            </g>
                            <g class="lamp__cords cords">
                                <path
                                    class="cord cord--rig"
                                    d="M124 187.033V347"
                                    stroke-width="6"
                                    stroke-linecap="round"
                                />
                                <path
                                    class="cord cord--rig"
                                    d="M124 187.023s17.007 21.921 17.007 34.846c0 12.925-11.338 23.231-17.007 34.846-5.669 11.615-17.007 21.921-17.007 34.846 0 12.925 17.007 34.846 17.007 34.846"
                                    stroke-width="6"
                                    stroke-linecap="round"
                                />
                                <path
                                    class="cord cord--rig"
                                    d="M124 187.017s-21.259 17.932-21.259 30.26c0 12.327 14.173 20.173 21.259 30.26 7.086 10.086 21.259 17.933 21.259 30.26 0 12.327-21.259 30.26-21.259 30.26"
                                    stroke-width="6"
                                    stroke-linecap="round"
                                />
                                <path
                                    class="cord cord--rig"
                                    d="M124 187s29.763 8.644 29.763 20.735-19.842 13.823-29.763 20.734c-9.921 6.912-29.763 8.644-29.763 20.735S124 269.939 124 269.939"
                                    stroke-width="6"
                                    stroke-linecap="round"
                                />
                                <path
                                    class="cord cord--rig"
                                    d="M124 187.029s-10.63 26.199-10.63 39.992c0 13.794 7.087 26.661 10.63 39.992 3.543 13.331 10.63 26.198 10.63 39.992 0 13.793-10.63 39.992-10.63 39.992"
                                    stroke-width="6"
                                    stroke-linecap="round"
                                />
                                <path
                                    class="cord cord--rig"
                                    d="M124 187.033V347"
                                    stroke-width="6"
                                    stroke-linecap="round"
                                />
                                <line
                                    class="cord cord--dummy"
                                    x1="124"
                                    y2="348"
                                    x2="124"
                                    y1="190"
                                    stroke-width="6"
                                    stroke-linecap="round"
                                />
                            </g>
                            <path
                                class="lamp__light"
                                d="M290.5 193H39L0 463.5c0 11.046 75.478 20 165.5 20s167-11.954 167-23l-42-267.5z"
                                fill="url(#light)"
                            />
                            <g class="lamp__top top">
                                <path
                                    class="top__body"
                                    fill-rule="evenodd"
                                    clip-rule="evenodd"
                                    d="M164.859 0c55.229 0 100 8.954 100 20l29.859 199.06C291.529 208.451 234.609 200 164.859 200S38.189 208.451 35 219.06L64.859 20c0-11.046 44.772-20 100-20z"
                                />
                                <path
                                    class="top__shading"
                                    fill-rule="evenodd"
                                    clip-rule="evenodd"
                                    d="M164.859 0c55.229 0 100 8.954 100 20l29.859 199.06C291.529 208.451 234.609 200 164.859 200S38.189 208.451 35 219.06L64.859 20c0-11.046 44.772-20 100-20z"
                                    fill="url(#top-shading)"
                                />
                            </g>
                            <g class="lamp__face face">
                                <g class="lamp__mouth">
                                    <path
                                        d="M165 178c19.882 0 36-16.118 36-36h-72c0 19.882 16.118 36 36 36z"
                                        fill="#141414"
                                    />
                                    <clipPath
                                        class="lamp__feature"
                                        id="mouth"
                                        x="129"
                                        y="142"
                                        width="72"
                                        height="36"
                                    >
                                        <path
                                            d="M165 178c19.882 0 36-16.118 36-36h-72c0 19.882 16.118 36 36 36z"
                                            fill="#141414"
                                        />
                                    </clipPath>
                                    <g clip-path="url(#mouth)">
                                        <circle
                                            class="lamp__tongue"
                                            cx="179.4"
                                            cy="172.6"
                                            r="18"
                                        />
                                    </g>
                                </g>
                                <g class="lamp__eyes">
                                    <path
                                        class="lamp__eye lamp__stroke"
                                        d="M115 135c0-5.523-5.82-10-13-10s-13 4.477-13 10"
                                        stroke-width="4"
                                        stroke-linecap="round"
                                        stroke-linejoin="round"
                                    />
                                    <path
                                        class="lamp__eye lamp__stroke"
                                        d="M241 135c0-5.523-5.82-10-13-10s-13 4.477-13 10"
                                        stroke-width="4"
                                        stroke-linecap="round"
                                        stroke-linejoin="round"
                                    />
                                </g>
                            </g>
                            <defs>
                                <linearGradient
                                    id="opening-shade"
                                    x1="35"
                                    y1="220"
                                    x2="295"
                                    y2="220"
                                    gradientUnits="userSpaceOnUse"
                                >
                                    <stop />
                                    <stop
                                        offset="1"
                                        stop-color="var(--shade)"
                                        stop-opacity="0"
                                    />
                                </linearGradient>
                                <linearGradient
                                    id="base-shading"
                                    x1="85"
                                    y1="444"
                                    x2="245"
                                    y2="444"
                                    gradientUnits="userSpaceOnUse"
                                >
                                    <stop stop-color="var(--b-1)" />
                                    <stop
                                        offset="0.8"
                                        stop-color="var(--b-2)"
                                        stop-opacity="0"
                                    />
                                </linearGradient>
                                <linearGradient
                                    id="side-shading"
                                    x1="119"
                                    y1="430"
                                    x2="245"
                                    y2="430"
                                    gradientUnits="userSpaceOnUse"
                                >
                                    <stop stop-color="var(--b-3)" />
                                    <stop
                                        offset="1"
                                        stop-color="var(--b-4)"
                                        stop-opacity="0"
                                    />
                                </linearGradient>
                                <linearGradient
                                    id="post-shading"
                                    x1="150"
                                    y1="288"
                                    x2="180"
                                    y2="288"
                                    gradientUnits="userSpaceOnUse"
                                >
                                    <stop stop-color="var(--b-1)" />
                                    <stop
                                        offset="1"
                                        stop-color="var(--b-2)"
                                        stop-opacity="0"
                                    />
                                </linearGradient>
                                <linearGradient
                                    id="light"
                                    x1="165.5"
                                    y1="218.5"
                                    x2="165.5"
                                    y2="483.5"
                                    gradientUnits="userSpaceOnUse"
                                >
                                    <stop stop-color="var(--l-1)" stop-opacity=".2" />
                                    <stop
                                        offset="1"
                                        stop-color="var(--l-2)"
                                        stop-opacity="0"
                                    />
                                </linearGradient>
                                <linearGradient
                                    id="top-shading"
                                    x1="56"
                                    y1="110"
                                    x2="295"
                                    y2="110"
                                    gradientUnits="userSpaceOnUse"
                                >
                                    <stop stop-color="var(--t-1)" stop-opacity=".8" />
                                    <stop
                                        offset="1"
                                        stop-color="var(--t-2)"
                                        stop-opacity="0"
                                    />
                                </linearGradient>
                            </defs>
                            <circle
                                class="lamp__hit"
                                cx="124"
                                cy="347"
                                r="66"
                                fill="#C4C4C4"
                                fill-opacity=".1"
                            />
                        </svg>
            <span class="lamp-hint">Click the lamp to turn it on</span>
        </div>

        <div class="auth-brand">SEA<img src="Assets/sea-octopus.svg" alt="octopus" style="height:1.8em;width:auto;vertical-align:middle;margin-left:3px;margin-bottom:-2px;" /></div>
        <div class="auth-tag">Click the lamp, Get surprise!!</div>

        <div class="auth-card" id="authCard">
            <h1>Log in</h1>

            <asp:Label ID="lblError" runat="server" CssClass="auth-error" Visible="false" />

            <div class="field">
                <label>Email or username</label>
                <asp:TextBox ID="txtLoginId" runat="server" />
                <asp:RequiredFieldValidator ID="rfvLoginId" runat="server"
                    ControlToValidate="txtLoginId" Display="Dynamic" CssClass="vmsg"
                    ErrorMessage="Enter your email or username." />
            </div>

            <div class="field">
                <label>Password</label>
                <asp:TextBox ID="txtLoginPwd" runat="server" TextMode="Password" />
                <asp:RequiredFieldValidator ID="rfvLoginPwd" runat="server"
                    ControlToValidate="txtLoginPwd" Display="Dynamic" CssClass="vmsg"
                    ErrorMessage="Enter your password." />
            </div>

            <asp:Button ID="btnLogin" runat="server" Text="Log in"
                CssClass="btn-primary" OnClick="btnLogin_Click" />

            <div class="auth-alt">New here? <a href="SignUp.aspx">Create an account</a></div>
        </div>
    </form>

    <!-- Self-contained lamp: CLICK to turn the light on/off (plain JS + CSS, offline) -->
    <script>
        (function () {
            var svg = document.querySelector(".lamp");
            if (!svg) return;
            var cord = svg.querySelector(".cord--dummy");
            var card = document.getElementById("authCard");
            var hint = document.querySelector(".lamp-hint");
            var ORIGY = cord ? parseFloat(cord.getAttribute("y2")) : 0;
            var raf = 0;

            function bob() {                                  // quick cord wiggle for feedback
                if (!cord) return;
                cancelAnimationFrame(raf);
                var t0 = performance.now(), dur = 420, amp = 70;
                (function step(t) {
                    var k = Math.min(1, (t - t0) / dur);
                    cord.setAttribute("y2", ORIGY + Math.sin(k * Math.PI) * amp);
                    if (k < 1) raf = requestAnimationFrame(step);
                    else cord.setAttribute("y2", ORIGY);
                })(t0);
            }

            function toggle() {
                var on = document.documentElement.classList.toggle("lamp-on");
                if (on) {
                    var hue = Math.floor(Math.random() * 360);
                    document.documentElement.style.setProperty("--shade-hue", hue);
                    document.documentElement.style.setProperty("--glow-color", "hsl(" + hue + ",60%,60%)");
                }
                if (card) card.classList.toggle("lit", on);
                if (hint) hint.style.display = "none";         // hide hint after first click
            }

            var row = document.querySelector(".lamp-row") || svg;
            row.style.cursor = "pointer";
            row.addEventListener("click", function () { bob(); toggle(); });
        })();
    </script>
</body>
</html>
