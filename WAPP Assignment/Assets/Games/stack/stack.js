/* ============================================================
   Stack Tower - SEA minigame (original code, no libraries)
   A block slides above the tower; drop it with click / tap /
   Space. Only the overlap survives - the overhang is sliced
   off and falls away. Score = tower height. Pure client-side.
   ============================================================ */
(function () {
    "use strict";

    var canvas = document.getElementById("gameCanvas");
    if (!canvas) return;
    var ctx = canvas.getContext("2d");

    var W = 480, H = 560;
    var DPR = window.devicePixelRatio || 1;
    canvas.width = W * DPR;
    canvas.height = H * DPR;
    ctx.setTransform(DPR, 0, 0, DPR, 0, 0);

    var BH = 26;                 // block height
    var GROUND = H - 36;         // top of the ground band
    var BASE_W = 150;            // width of the starting block

    // ---------- theme
    function isLight() { return document.documentElement.classList.contains("light"); }
    function pal() {
        return isLight() ? {
            ink: "#212121", sub: "#8a877f", line: "rgba(0,0,0,.07)", ground: "rgba(0,0,0,.10)"
        } : {
            ink: "#f2f2f0", sub: "#7d7d78", line: "rgba(255,255,255,.06)", ground: "rgba(255,255,255,.09)"
        };
    }
    // block colours cycle through the hue wheel, starting near SEA coral
    function blockColor(i, alpha) {
        var h = (8 + i * 14) % 360;
        var s = isLight() ? 62 : 68;
        var l = isLight() ? 52 : 58;
        return "hsla(" + h + "," + s + "%," + l + "%," + (alpha == null ? 1 : alpha) + ")";
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
        document.getElementById("hudBest").textContent = best;
    }

    // ---------- state
    var state = "ready";               // ready | playing | dying | over
    var blocks = [], debris = [], sparks = [];
    var active = null;
    var score = 0, best = 0, combo = 0;

    // world y of the TOP of tower slot i (0 = first placed block)
    function slotY(i) { return GROUND - (i + 1) * BH; }
    // camera: keep the active row comfortably on screen
    function cam() {
        var topY = slotY(blocks.length);            // where the active block sits
        return topY < 230 ? 230 - topY : 0;
    }

    function reset() {
        blocks = [{ x: W / 2 - BASE_W / 2, w: BASE_W }];
        debris = []; sparks = [];
        score = 0; combo = 0;
        newActive();
    }
    function newActive() {
        var top = blocks[blocks.length - 1];
        var fromLeft = blocks.length % 2 === 1;     // alternate entry side
        active = {
            w: top.w,
            x: fromLeft ? -top.w : W,
            dir: fromLeft ? 1 : -1,
            speed: Math.min(330, 150 + blocks.length * 8)
        };
    }
    function start() {
        if (state === "playing") return;
        reset(); hud(); state = "playing";
        ov.style.display = "none";
        sfx(660, 0.09, "square", 0.05, 990);
    }
    function gameOver() {
        state = "over";
        best = Math.max(best, score); hud();
        sfx(320, 0.5, "sawtooth", 0.05, 60);
        overlay("Tower down!", "You stacked " + score + " block" + (score === 1 ? "" : "s") + " high.", "Play again");
    }
    btnStart.addEventListener("click", start);

    // ---------- input
    function drop() {
        if (state !== "playing") { if (state !== "dying") start(); return; }

        var top = blocks[blocks.length - 1];
        var ox = Math.max(active.x, top.x);
        var oe = Math.min(active.x + active.w, top.x + top.w);
        var ov_ = oe - ox;

        if (ov_ <= 2) {
            // complete miss: the whole block tumbles, then it's game over
            debris.push({ x: active.x, w: active.w, y: slotY(blocks.length), vy: -60, rot: 0, vr: active.dir * 2.4, c: blocks.length });
            state = "dying";
            sfx(240, 0.2, "triangle", 0.05, 120);
            setTimeout(gameOver, 750);
            return;
        }

        // perfect drop: snap to the full width of the block below
        if (top.w - ov_ < 6) {
            ox = top.x; ov_ = top.w;
            combo++;
            sfx(880 + combo * 60, 0.1, "square", 0.05, 1400);
            for (var s = 0; s < 10; s++) {
                sparks.push({
                    x: ox + Math.random() * ov_, y: slotY(blocks.length) + BH / 2,
                    vx: (Math.random() * 2 - 1) * 130, vy: -60 - Math.random() * 130, t: 0.55
                });
            }
        } else {
            combo = 0;
            // slice off the overhang and let it fall
            if (active.x < ox) debris.push({ x: active.x, w: ox - active.x, y: slotY(blocks.length), vy: 0, rot: 0, vr: -1.8, c: blocks.length });
            if (active.x + active.w > oe) debris.push({ x: oe, w: active.x + active.w - oe, y: slotY(blocks.length), vy: 0, rot: 0, vr: 1.8, c: blocks.length });
            sfx(430, 0.08, "square", 0.045, 300);
        }

        blocks.push({ x: ox, w: ov_ });
        score = blocks.length - 1;
        best = Math.max(best, score);
        hud();
        newActive();
    }
    canvas.addEventListener("mousedown", drop);
    canvas.addEventListener("touchstart", function (e) { drop(); e.preventDefault(); }, { passive: false });
    window.addEventListener("keydown", function (e) {
        if (e.code === "Space") {
            if (state === "playing") e.preventDefault();
            drop();
        } else if (state !== "playing" && e.code === "Enter") start();
    });

    // ---------- update
    function update(dt) {
        if (state === "playing" && active) {
            active.x += active.dir * active.speed * dt;
            if (active.x < -46) { active.x = -46; active.dir = 1; }
            if (active.x + active.w > W + 46) { active.x = W + 46 - active.w; active.dir = -1; }
        }
        for (var i = debris.length - 1; i >= 0; i--) {
            var d = debris[i];
            d.vy += 1050 * dt;
            d.y += d.vy * dt;
            d.rot += d.vr * dt;
            if (d.y > H + cam() + 80) debris.splice(i, 1);
        }
        for (var s = sparks.length - 1; s >= 0; s--) {
            var sp = sparks[s];
            sp.vy += 700 * dt;
            sp.x += sp.vx * dt; sp.y += sp.vy * dt; sp.t -= dt;
            if (sp.t <= 0) sparks.splice(s, 1);
        }
    }

    // ---------- draw
    function drawBlock(x, y, w, color, stroke) {
        ctx.fillStyle = color;
        ctx.fillRect(x, y, w, BH - 2);
        ctx.fillStyle = stroke;
        ctx.fillRect(x, y + BH - 6, w, 4);   // darker base edge for depth
    }

    function draw() {
        var p = pal();
        var cy = cam();
        ctx.clearRect(0, 0, W, H);

        // dotted backdrop
        ctx.fillStyle = p.line;
        for (var gy = 20; gy < H; gy += 40)
            for (var gx = 20; gx < W; gx += 40)
                ctx.fillRect(gx, gy, 2, 2);

        ctx.save();
        ctx.translate(0, cy);

        // ground
        ctx.fillStyle = p.ground;
        ctx.fillRect(0, GROUND, W, H);

        // tower (only the visible top slice for performance)
        var from = Math.max(0, blocks.length - 24);
        for (var i = from; i < blocks.length; i++) {
            drawBlock(blocks[i].x, slotY(i), blocks[i].w, blockColor(i), "rgba(0,0,0,.22)");
        }

        // sliding block
        if (active && (state === "playing")) {
            drawBlock(active.x, slotY(blocks.length), active.w, blockColor(blocks.length), "rgba(0,0,0,.22)");
        }

        // falling offcuts
        debris.forEach(function (d) {
            ctx.save();
            ctx.translate(d.x + d.w / 2, d.y + BH / 2);
            ctx.rotate(d.rot);
            ctx.fillStyle = blockColor(d.c, 0.9);
            ctx.fillRect(-d.w / 2, -BH / 2, d.w, BH - 2);
            ctx.restore();
        });

        // perfect-drop sparks
        sparks.forEach(function (sp) {
            ctx.globalAlpha = Math.max(0, sp.t / 0.55);
            ctx.fillStyle = "#ffd166";
            ctx.fillRect(sp.x, sp.y, 3, 3);
            ctx.globalAlpha = 1;
        });

        ctx.restore();

        // combo badge
        if (combo > 1 && state === "playing") {
            ctx.fillStyle = p.ink;
            ctx.font = "bold 15px Consolas, monospace";
            ctx.textAlign = "center";
            ctx.fillText("PERFECT x" + combo, W / 2, 34);
        }
    }

    // ---------- loop
    var last = 0;
    function loop(ts) {
        var dt = Math.min((ts - last) / 1000 || 0, 0.05);
        last = ts;
        if (state === "playing" || state === "dying") update(dt);
        draw();
        requestAnimationFrame(loop);
    }
    hud();
    requestAnimationFrame(loop);
})();
