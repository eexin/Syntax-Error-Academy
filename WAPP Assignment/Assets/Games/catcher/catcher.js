/* ============================================================
   Byte Catcher - SEA minigame (original code, no libraries)
   Catch the falling code bits with the basket. +1 per catch,
   a missed bit costs a life. 3 lives. Pure client-side: this
   file never talks to the server or the database.
   ============================================================ */
(function () {
    "use strict";

    var canvas = document.getElementById("gameCanvas");
    if (!canvas) return;
    var ctx = canvas.getContext("2d");

    // Fixed logical size; CSS scales the element, DPR keeps it crisp.
    var W = 720, H = 480;
    var DPR = window.devicePixelRatio || 1;
    canvas.width = W * DPR;
    canvas.height = H * DPR;
    ctx.setTransform(DPR, 0, 0, DPR, 0, 0);

    // ---------- theme (re-read every frame so the toggle applies live)
    function pal() {
        var light = document.documentElement.classList.contains("light");
        return light ? {
            ink: "#212121", sub: "#8a877f", line: "rgba(0,0,0,.07)",
            basket: "#40603c", rim: "#2e4a2b",
            items: ["#d8543c", "#40603c", "#2b6f8f", "#7a5fae"],
            flash: "rgba(216,84,60,"
        } : {
            ink: "#f2f2f0", sub: "#7d7d78", line: "rgba(255,255,255,.06)",
            basket: "#ff9485", rim: "#d97b6c",
            items: ["#ff9485", "#8fae7e", "#6fb3d2", "#b39ddb"],
            flash: "rgba(229,83,75,"
        };
    }

    // ---------- tiny generated sound effects (no audio files)
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
        } catch (e) { /* audio unavailable - play silently */ }
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
        document.getElementById("hudLives").textContent = lives;
        document.getElementById("hudBest").textContent = best;
    }

    // ---------- state
    var GLYPHS = ["0", "1", ";", "{ }", "</>", "#", "&&", "=>"];
    var state = "ready";                 // ready | playing | over
    var score = 0, best = 0, lives = 3;  // best is session-only (never saved)
    var items = [], msgs = [];
    var px = W / 2, PW = 96, PH = 22;    // basket position + size
    var spawnT = 0, flash = 0;
    var keys = {};

    function reset() { score = 0; lives = 3; items = []; msgs = []; px = W / 2; spawnT = 0.4; flash = 0; }
    function start() {
        if (state === "playing") return;
        reset(); hud(); state = "playing";
        ov.style.display = "none";
        sfx(660, 0.09, "square", 0.05, 990);
    }
    function gameOver() {
        state = "over";
        best = Math.max(best, score); hud();
        sfx(320, 0.45, "sawtooth", 0.05, 70);
        overlay("Game over", "You caught " + score + " byte" + (score === 1 ? "" : "s") + ". The floor caught the rest.", "Play again");
    }
    btnStart.addEventListener("click", start);

    // ---------- input
    window.addEventListener("keydown", function (e) {
        if (state === "playing" && (e.code === "ArrowLeft" || e.code === "ArrowRight" || e.code === "Space")) e.preventDefault();
        keys[e.code] = true;
        if (state !== "playing" && (e.code === "Space" || e.code === "Enter")) start();
    });
    window.addEventListener("keyup", function (e) { keys[e.code] = false; });

    function toX(e) {
        var r = canvas.getBoundingClientRect();
        var cx = e.touches ? e.touches[0].clientX : e.clientX;
        return (cx - r.left) * (W / r.width);
    }
    canvas.addEventListener("mousemove", function (e) { if (state === "playing") px = toX(e); });
    canvas.addEventListener("touchstart", function (e) { if (state !== "playing") start(); else px = toX(e); }, { passive: true });
    canvas.addEventListener("touchmove", function (e) {
        if (state === "playing") { px = toX(e); e.preventDefault(); }
    }, { passive: false });

    // ---------- game logic
    function spawn() {
        items.push({
            x: 34 + Math.random() * (W - 68),
            y: -22,
            v: 115 + score * 6 + Math.random() * 45,   // faster as you score
            g: GLYPHS[(Math.random() * GLYPHS.length) | 0],
            c: (Math.random() * 4) | 0,
            r: 16,
            a: 0,
            spin: (Math.random() * 2 - 1) * 1.6
        });
    }

    function update(dt) {
        var mv = 360 * dt;
        if (keys.ArrowLeft || keys.KeyA) px -= mv;
        if (keys.ArrowRight || keys.KeyD) px += mv;
        px = Math.max(PW / 2, Math.min(W - PW / 2, px));

        spawnT -= dt;
        if (spawnT <= 0) {
            spawn();
            spawnT = Math.max(0.42, 1.05 - score * 0.02);   // spawn faster as you score
        }

        var basketTop = H - 48;
        for (var i = items.length - 1; i >= 0; i--) {
            var it = items[i];
            it.y += it.v * dt;
            it.a += it.spin * dt;

            // caught: item bottom enters the basket band while over the basket
            if (it.y + it.r >= basketTop && it.y + it.r <= basketTop + 24 &&
                Math.abs(it.x - px) <= PW / 2 + it.r * 0.35) {
                items.splice(i, 1);
                score++;
                best = Math.max(best, score);
                hud();
                msgs.push({ x: it.x, y: basketTop - 16, t: 0.6, s: "+1" });
                sfx(720 + Math.min(score * 9, 420), 0.07, "square", 0.045, 1250);
                continue;
            }
            // missed: fell past the floor
            if (it.y - it.r > H) {
                items.splice(i, 1);
                lives--; hud();
                flash = 0.28;
                sfx(180, 0.25, "sawtooth", 0.05, 85);
                if (lives <= 0) { gameOver(); return; }
            }
        }

        for (var m = msgs.length - 1; m >= 0; m--) {
            msgs[m].t -= dt; msgs[m].y -= 34 * dt;
            if (msgs[m].t <= 0) msgs.splice(m, 1);
        }
        if (flash > 0) flash -= dt;
    }

    // ---------- drawing
    function rr(x, y, w, h, r) {
        ctx.beginPath();
        ctx.moveTo(x + r, y);
        ctx.arcTo(x + w, y, x + w, y + h, r);
        ctx.arcTo(x + w, y + h, x, y + h, r);
        ctx.arcTo(x, y + h, x, y, r);
        ctx.arcTo(x, y, x + w, y, r);
        ctx.closePath();
    }

    function draw() {
        var p = pal();
        ctx.clearRect(0, 0, W, H);

        // faint column grid, like an editor gutter
        ctx.strokeStyle = p.line;
        ctx.lineWidth = 1;
        ctx.beginPath();
        for (var gx = 40; gx < W; gx += 40) { ctx.moveTo(gx, 0); ctx.lineTo(gx, H); }
        ctx.stroke();

        // floor
        ctx.fillStyle = p.line;
        ctx.fillRect(0, H - 12, W, 12);

        // falling code bits
        items.forEach(function (it) {
            ctx.save();
            ctx.translate(it.x, it.y);
            ctx.rotate(it.a * 0.35);
            ctx.fillStyle = p.items[it.c];
            rr(-it.r, -it.r, it.r * 2, it.r * 2, 7);
            ctx.fill();
            ctx.fillStyle = document.documentElement.classList.contains("light") ? "#faf9f6" : "#1d1d1d";
            ctx.font = "bold 13px Consolas, monospace";
            ctx.textAlign = "center";
            ctx.textBaseline = "middle";
            ctx.fillText(it.g, 0, 1);
            ctx.restore();
        });

        // basket: a rounded tray with a rim and { } marks
        var by = H - 44;
        ctx.fillStyle = p.basket;
        rr(px - PW / 2, by, PW, PH, 8);
        ctx.fill();
        ctx.fillStyle = p.rim;
        rr(px - PW / 2, by, PW, 6, 3);
        ctx.fill();
        ctx.fillStyle = document.documentElement.classList.contains("light") ? "#faf9f6" : "#1d1d1d";
        ctx.font = "bold 12px Consolas, monospace";
        ctx.textAlign = "center";
        ctx.textBaseline = "middle";
        ctx.fillText("{      }", px, by + PH / 2 + 2);

        // floating +1 messages
        msgs.forEach(function (m) {
            ctx.globalAlpha = Math.max(0, m.t / 0.6);
            ctx.fillStyle = p.ink;
            ctx.font = "bold 15px Consolas, monospace";
            ctx.textAlign = "center";
            ctx.fillText(m.s, m.x, m.y);
            ctx.globalAlpha = 1;
        });

        // red edge flash on a miss
        if (flash > 0) {
            ctx.fillStyle = p.flash + (flash * 0.45) + ")";
            ctx.fillRect(0, 0, W, H);
        }
    }

    // ---------- main loop
    var last = 0;
    function loop(ts) {
        var dt = Math.min((ts - last) / 1000 || 0, 0.05);
        last = ts;
        if (state === "playing") update(dt);
        draw();
        requestAnimationFrame(loop);
    }
    hud();
    requestAnimationFrame(loop);
})();
