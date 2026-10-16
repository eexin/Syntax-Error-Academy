/* ============================================================
   Target Shot - SEA minigame (original code, no libraries)
   Click-drag back from the bow to aim + set power, release to
   fire. Projectile physics with gravity and (later) wind.
   10 arrows per round; rings score 10 / 8 / 5 / 3.
   Pure client-side: nothing is saved anywhere.
   ============================================================ */
(function () {
    "use strict";

    var canvas = document.getElementById("gameCanvas");
    if (!canvas) return;
    var ctx = canvas.getContext("2d");

    var W = 760, H = 430;
    var DPR = window.devicePixelRatio || 1;
    canvas.width = W * DPR;
    canvas.height = H * DPR;
    ctx.setTransform(DPR, 0, 0, DPR, 0, 0);

    var GROUND = H - 26;
    var A = { x: 95, y: H - 150 };      // bow anchor
    var G = 560;                        // gravity px/s^2

    // ---------- theme
    function isLight() { return document.documentElement.classList.contains("light"); }
    function pal() {
        return isLight() ? {
            ink: "#212121", sub: "#8a877f", line: "rgba(0,0,0,.07)",
            ground: "rgba(0,0,0,.12)", bow: "#40603c", arrow: "#5a4632",
            rings: ["#e9e2d6", "#8fae7e", "#6fb3d2", "#d8543c"], bull: "#ffd166"
        } : {
            ink: "#f2f2f0", sub: "#7d7d78", line: "rgba(255,255,255,.06)",
            ground: "rgba(255,255,255,.10)", bow: "#8fae7e", arrow: "#c9a86a",
            rings: ["#4a4a48", "#8fae7e", "#6fb3d2", "#ff9485"], bull: "#ffd166"
        };
    }

    // ---------- sound
    var AC = null, muted = false;
    function sfx(freq, dur, type, vol, slideTo) {
        if (muted) return;
        try {
            AC = AC || new (window.AudioContext || window.webkitAudioContext)();
            if (AC.state === "suspended") AC.resume();
            var o = AC.createOscillator(), g = AC.createGain();
            o.type = type || "square";
            o.frequency.setValueAtTime(freq, AC.currentTime);
            if (slideTo) o.frequency.exponentialRampToValueAtTime(slideTo, AC.currentTime + dur);
            g.gain.setValueAtTime(vol || 0.05, AC.currentTime);
            g.gain.exponentialRampToValueAtTime(0.0001, AC.currentTime + dur);
            o.connect(g); g.connect(AC.destination);
            o.start(); o.stop(AC.currentTime + dur);
        } catch (e) { }
    }
    var mBtn = document.getElementById("btnMute");
    if (mBtn) mBtn.addEventListener("click", function () {
        muted = !muted;
        mBtn.innerHTML = muted ? '<i class="ti ti-volume-off"></i>' : '<i class="ti ti-volume"></i>';
    });

    // ---------- overlay + hud
    var ov = document.getElementById("gameOverlay"),
        ovTitle = document.getElementById("ovTitle"),
        ovMsg = document.getElementById("ovMsg"),
        btnStart = document.getElementById("btnStart");
    function overlay(title, msg, btn) {
        ovTitle.textContent = title; ovMsg.textContent = msg;
        btnStart.textContent = btn; ov.style.display = "flex";
    }
    function hud() {
        document.getElementById("hudScore").textContent = score;
        document.getElementById("hudArrows").textContent = arrows;
        document.getElementById("hudBest").textContent = best;
    }

    // ---------- state
    var state = "ready";        // ready | idle | aiming | flight | between | over
    var score = 0, best = 0, arrows = 10, level = 1, wind = 0;
    var target = null, arrow = null, aim = null;
    var msgs = [], betweenT = 0;

    function newTarget() {
        target = {
            x: 430 + Math.min(230, level * 24) + Math.random() * 40,
            cy: GROUND - 80 - Math.random() * 95,
            r: 50,
            stuck: []
        };
        wind = level >= 3 ? (Math.random() - 0.5) * (24 + level * 7) : 0;
    }
    function start() {
        if (state !== "ready" && state !== "over") return;
        score = 0; arrows = 10; level = 1;
        msgs = []; arrow = null; aim = null;
        newTarget(); hud();
        state = "idle";
        ov.style.display = "none";
        sfx(660, 0.09, "square", 0.05, 990);
    }
    function gameOver() {
        state = "over";
        best = Math.max(best, score); hud();
        sfx(300, 0.5, "sawtooth", 0.05, 70);
        overlay("Round over", "You scored " + score + " with your ten arrows." + (score >= 70 ? " Sharp shooting!" : ""), "Play again");
    }
    btnStart.addEventListener("click", start);
    window.addEventListener("keydown", function (e) {
        if ((state === "ready" || state === "over") && (e.code === "Space" || e.code === "Enter")) start();
    });

    // ---------- input (drag anywhere on the canvas; release fires)
    function toXY(e) {
        var r = canvas.getBoundingClientRect();
        var t = e.touches ? e.touches[0] : (e.changedTouches ? e.changedTouches[0] : e);
        return { x: (t.clientX - r.left) * (W / r.width), y: (t.clientY - r.top) * (H / r.height) };
    }
    function aimStart(e) {
        if (state !== "idle") return;
        state = "aiming";
        aim = toXY(e);
        sfx(140, 0.12, "triangle", 0.03, 190);
    }
    function aimMove(e) {
        if (state !== "aiming") return;
        aim = toXY(e);
        if (e.cancelable) e.preventDefault();
    }
    function aimEnd(e) {
        if (state !== "aiming") return;
        var p = toXY(e);
        var dx = A.x - p.x, dy = A.y - p.y;      // pull back = fire forward
        var len = Math.sqrt(dx * dx + dy * dy);
        if (len < 20) { state = "idle"; aim = null; return; }   // too weak: cancel
        var power = Math.min(640, len * 3);
        arrow = { x: A.x, y: A.y, vx: dx / len * power, vy: dy / len * power };
        arrows--; hud();
        aim = null;
        state = "flight";
        sfx(190, 0.16, "sawtooth", 0.05, 55);
    }
    canvas.addEventListener("mousedown", aimStart);
    window.addEventListener("mousemove", aimMove);
    window.addEventListener("mouseup", aimEnd);
    canvas.addEventListener("touchstart", function (e) { aimStart(e); e.preventDefault(); }, { passive: false });
    window.addEventListener("touchmove", aimMove, { passive: false });
    window.addEventListener("touchend", aimEnd);

    // ---------- physics helpers
    function windAx() { return wind * 3; }

    function resolveShot(ring) {
        if (ring > 0) {
            score += ring;
            best = Math.max(best, score);
            msgs.push({ x: target.x, y: target.cy - target.r - 14, t: 0.9, s: "+" + ring });
            sfx(420 + ring * 55, 0.12, "square", 0.05, 700 + ring * 60);
            if (ring === 10) sfx(1180, 0.16, "square", 0.045, 1500);
        } else {
            sfx(120, 0.18, "triangle", 0.05, 60);
        }
        hud();
        betweenT = ring > 0 ? 0.8 : 0.55;
        state = "between";
    }

    function update(dt) {
        if (state === "flight" && arrow) {
            arrow.vx += windAx() * dt;
            arrow.vy += G * dt;
            arrow.x += arrow.vx * dt;
            arrow.y += arrow.vy * dt;

            // hit the target face?
            var ddx = arrow.x - target.x, ddy = arrow.y - target.cy;
            var d = Math.sqrt(ddx * ddx + ddy * ddy);
            if (d <= target.r) {
                var rel = d / target.r;
                var ring = rel < 0.18 ? 10 : rel < 0.45 ? 8 : rel < 0.72 ? 5 : 3;
                target.stuck.push({ x: arrow.x, y: arrow.y, rot: Math.atan2(arrow.vy, arrow.vx) });
                arrow = null;
                resolveShot(ring);
                return;
            }
            // ground / out of range
            if (arrow.y >= GROUND || arrow.x > W + 50) {
                arrow = null;
                resolveShot(0);
                return;
            }
        }
        if (state === "between") {
            betweenT -= dt;
            if (betweenT <= 0) {
                if (arrows <= 0) { gameOver(); return; }
                level++;
                newTarget();          // fresh target after every arrow
                state = "idle";
            }
        }
        for (var m = msgs.length - 1; m >= 0; m--) {
            msgs[m].t -= dt; msgs[m].y -= 26 * dt;
            if (msgs[m].t <= 0) msgs.splice(m, 1);
        }
    }

    // ---------- drawing
    function drawArrowShape(x, y, rot, p) {
        ctx.save();
        ctx.translate(x, y);
        ctx.rotate(rot);
        ctx.strokeStyle = p.arrow;
        ctx.lineWidth = 3;
        ctx.beginPath();
        ctx.moveTo(-26, 0); ctx.lineTo(0, 0);
        ctx.stroke();
        ctx.fillStyle = p.arrow;
        ctx.beginPath();
        ctx.moveTo(6, 0); ctx.lineTo(-3, -4); ctx.lineTo(-3, 4);
        ctx.closePath(); ctx.fill();
        ctx.strokeStyle = p.ink;
        ctx.lineWidth = 2;
        ctx.beginPath();
        ctx.moveTo(-26, 0); ctx.lineTo(-31, -4);
        ctx.moveTo(-26, 0); ctx.lineTo(-31, 4);
        ctx.stroke();
        ctx.restore();
    }

    function draw() {
        var p = pal();
        ctx.clearRect(0, 0, W, H);

        // backdrop dots + ground
        ctx.fillStyle = p.line;
        for (var gy = 20; gy < GROUND; gy += 40)
            for (var gx = 20; gx < W; gx += 40)
                ctx.fillRect(gx, gy, 2, 2);
        ctx.fillStyle = p.ground;
        ctx.fillRect(0, GROUND, W, H - GROUND);
        ctx.strokeStyle = p.line;
        ctx.beginPath();
        for (var t = 30; t < W; t += 46) { ctx.moveTo(t, GROUND); ctx.lineTo(t + 7, GROUND - 7); }
        ctx.stroke();

        // wind indicator
        if (wind !== 0 && state !== "ready") {
            var n = Math.min(4, Math.ceil(Math.abs(wind) / 12));
            var chr = wind > 0 ? "\u203A" : "\u2039";  // › or ‹
            ctx.fillStyle = p.sub;
            ctx.font = "bold 15px Consolas, monospace";
            ctx.textAlign = "center";
            ctx.fillText("wind " + new Array(n + 1).join(chr), W / 2, 26);
        }

        if (target) {
            // stand
            ctx.strokeStyle = p.sub;
            ctx.lineWidth = 3;
            ctx.beginPath();
            ctx.moveTo(target.x - 16, GROUND); ctx.lineTo(target.x, target.cy + target.r - 6);
            ctx.moveTo(target.x + 16, GROUND); ctx.lineTo(target.x, target.cy + target.r - 6);
            ctx.stroke();
            // rings (outer -> inner) + gold bull
            var rr = [1, 0.72, 0.45, 0.18];
            for (var i = 0; i < rr.length; i++) {
                ctx.fillStyle = p.rings[i];
                ctx.beginPath();
                ctx.arc(target.x, target.cy, target.r * rr[i], 0, Math.PI * 2);
                ctx.fill();
            }
            ctx.fillStyle = p.bull;
            ctx.beginPath();
            ctx.arc(target.x, target.cy, target.r * 0.07, 0, Math.PI * 2);
            ctx.fill();
            // arrows stuck in the board
            target.stuck.forEach(function (s) { drawArrowShape(s.x, s.y, s.rot, p); });
        }

        // aim data
        var pull = 0, ang = 0;
        if (state === "aiming" && aim) {
            var dx = A.x - aim.x, dy = A.y - aim.y;
            var len = Math.sqrt(dx * dx + dy * dy);
            ang = Math.atan2(dy, dx);
            pull = Math.min(26, len / 640 * 3 * 26);

            // dotted trajectory preview
            var power = Math.min(640, len * 3);
            var sx = A.x, sy = A.y, vx = dx / len * power, vy = dy / len * power;
            ctx.fillStyle = p.sub;
            for (var st = 0; st < 20; st++) {
                vx += windAx() * 0.055;
                vy += G * 0.055;
                sx += vx * 0.055;
                sy += vy * 0.055;
                if (sy > GROUND) break;
                ctx.globalAlpha = 1 - st / 22;
                ctx.beginPath();
                ctx.arc(sx, sy, 2.2, 0, Math.PI * 2);
                ctx.fill();
            }
            ctx.globalAlpha = 1;
        }

        // bow (rotates toward the aim)
        ctx.save();
        ctx.translate(A.x, A.y);
        ctx.rotate(state === "aiming" ? ang : -0.35);
        ctx.strokeStyle = p.bow;
        ctx.lineWidth = 5;
        ctx.lineCap = "round";
        ctx.beginPath();
        ctx.arc(0, 0, 34, -1.15, 1.15);
        ctx.stroke();
        var tipX = Math.cos(1.15) * 34, tipY = Math.sin(1.15) * 34;
        ctx.strokeStyle = p.sub;
        ctx.lineWidth = 1.5;
        ctx.beginPath();
        ctx.moveTo(tipX, -tipY);
        ctx.lineTo(-pull, 0);
        ctx.lineTo(tipX, tipY);
        ctx.stroke();
        if (state === "idle" || state === "aiming") {
            // nocked arrow
            ctx.strokeStyle = p.arrow;
            ctx.lineWidth = 3;
            ctx.beginPath();
            ctx.moveTo(-pull, 0); ctx.lineTo(36, 0);
            ctx.stroke();
        }
        ctx.restore();

        // flying arrow
        if (arrow) drawArrowShape(arrow.x, arrow.y, Math.atan2(arrow.vy, arrow.vx), p);

        // floating score messages
        msgs.forEach(function (m) {
            ctx.globalAlpha = Math.max(0, m.t / 0.9);
            ctx.fillStyle = p.ink;
            ctx.font = "bold 17px Consolas, monospace";
            ctx.textAlign = "center";
            ctx.fillText(m.s, m.x, m.y);
            ctx.globalAlpha = 1;
        });
    }

    // ---------- loop
    var last = 0;
    function loop(ts) {
        var dt = Math.min((ts - last) / 1000 || 0, 0.05);
        last = ts;
        if (state !== "ready" && state !== "over") update(dt);
        draw();
        requestAnimationFrame(loop);
    }
    hud();
    requestAnimationFrame(loop);
})();
