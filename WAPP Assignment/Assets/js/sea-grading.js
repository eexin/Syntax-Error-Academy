/* ============================================================
   Syntax Error Academy - submit guard + grading overlay
   Shared by Quiz.aspx and DailyChallenge.aspx.

   Exposes window.seaSubmit(btn), used from a button's
   OnClientClick. It:
     1. runs the matching-question completeness guard,
     2. runs the standard ASP.NET client-side validators,
     3. blocks a second click (the AI grade takes a few seconds),
     4. shows an animated overlay until the page posts back.

   The overlay markup is injected here so the pages stay clean
   and there is only one copy to maintain.
   ============================================================ */
(function () {
    "use strict";

    var OCTOPUS =
        '<svg viewBox="0 0 100 100" xmlns="http://www.w3.org/2000/svg" aria-hidden="true">' +
        '<ellipse class="gv-t gv-t1" cx="17" cy="60" rx="10" ry="11" fill="#F582AE"/>' +
        '<ellipse class="gv-t gv-t2" cx="83" cy="60" rx="10" ry="11" fill="#F582AE"/>' +
        '<ellipse class="gv-t gv-t3" cx="30" cy="71" rx="11" ry="12" fill="#F26FA6"/>' +
        '<ellipse class="gv-t gv-t4" cx="70" cy="71" rx="11" ry="12" fill="#F26FA6"/>' +
        '<circle cx="50" cy="40" r="29" fill="#F26FA6"/>' +
        '<ellipse cx="40" cy="29" rx="12" ry="8" fill="#F89AC4" opacity="0.65"/>' +
        '<ellipse class="gv-t gv-t5" cx="50" cy="72" rx="12" ry="13" fill="#FBA8CF"/>' +
        '<ellipse class="gv-t gv-t3" cx="34" cy="69" rx="10" ry="11" fill="#FBA8CF"/>' +
        '<ellipse class="gv-t gv-t4" cx="66" cy="69" rx="10" ry="11" fill="#FBA8CF"/>' +
        '<circle cx="40" cy="44" r="8" fill="#ffffff"/>' +
        '<circle cx="60" cy="44" r="8" fill="#ffffff"/>' +
        '<circle class="gv-eye" cx="40.5" cy="45" r="4.6" fill="#41463B"/>' +
        '<circle class="gv-eye" cx="60.5" cy="45" r="4.6" fill="#41463B"/>' +
        '</svg>';

    // Longer, friendlier sequence when an essay is being marked by the AI.
    var AI_STEPS = [
        "Reading what you wrote\u2026",
        "Comparing it with the model answer\u2026",
        "Deciding on your feedback\u2026",
        "Almost done\u2026"
    ];
    var QUICK_STEPS = ["Checking your answers\u2026"];

    var veil, msgEl, titleEl, timer;

    function build() {
        if (veil) return;
        veil = document.createElement("div");
        veil.className = "grade-veil";
        veil.setAttribute("role", "status");
        veil.setAttribute("aria-live", "polite");
        veil.innerHTML =
            '<div class="grade-card">' +
                '<div class="grade-oct"><div class="grade-ring"></div>' + OCTOPUS + '</div>' +
                '<h2 class="grade-title">Marking your work</h2>' +
                '<p class="grade-msg"></p>' +
                '<div class="grade-dots"><span></span><span></span><span></span></div>' +
            '</div>';
        document.body.appendChild(veil);
        titleEl = veil.querySelector(".grade-title");
        msgEl = veil.querySelector(".grade-msg");
    }

    // An essay on the page means the AI webhook will be called, which
    // takes a few seconds - so show the longer message sequence.
    function hasEssay() {
        return !!document.querySelector("textarea.essay, textarea[id*='txtEssay']");
    }

    function show() {
        build();
        var ai = hasEssay();
        var steps = ai ? AI_STEPS : QUICK_STEPS;

        titleEl.textContent = ai ? "Your tutor is marking this" : "Marking your work";
        veil.classList.add("show");

        var i = 0;
        msgEl.textContent = steps[0];
        if (steps.length > 1) {
            timer = setInterval(function () {
                i = (i + 1) % steps.length;
                msgEl.style.animation = "none";
                void msgEl.offsetWidth;          // restart the fade
                msgEl.style.animation = "";
                msgEl.textContent = steps[i];
            }, 1800);
        }
    }

    function hide() {
        if (!veil) return;
        veil.classList.remove("show");
        clearInterval(timer);
    }

    // Called from the Submit button's OnClientClick.
    window.seaSubmit = function (btn) {
        // 1. every matching question must be fully matched (Quiz.aspx only)
        if (typeof window.seaMatchReady === "function" && !window.seaMatchReady()) {
            return false;
        }

        // 2. let the ASP.NET validators have their say before we cover the page
        if (typeof Page_ClientValidate === "function" && !Page_ClientValidate("")) {
            return false;
        }

        // 3. block a double submit while the AI is thinking
        if (btn) {
            if (btn.dataset && btn.dataset.busy === "1") return false;
            if (btn.dataset) btn.dataset.busy = "1";
            btn.classList.add("is-busy");
            // Do NOT set disabled here: a disabled submit button is not
            // posted back by the browser, which would break the postback.
            setTimeout(function () { btn.style.pointerEvents = "none"; }, 0);
        }

        show();
        return true;
    };

    // If the user comes back via the browser's Back button the page may be
    // restored from cache with the overlay still up - clear it.
    window.addEventListener("pageshow", hide);
})();
