// MediCart Admin — Profile page interactivity
(function () {
    "use strict";

    /* ---- Tabs ------------------------------------------------------------ */
    const tabBtns = document.querySelectorAll(".tabs__btn");
    const panels = document.querySelectorAll(".tabs__panel");

    tabBtns.forEach((btn) => {
        btn.addEventListener("click", () => {
            tabBtns.forEach((b) => { b.classList.remove("is-active"); b.setAttribute("aria-selected", "false"); });
            panels.forEach((p) => { p.classList.remove("is-active"); p.hidden = true; });

            btn.classList.add("is-active");
            btn.setAttribute("aria-selected", "true");
            const target = document.querySelector(`.tabs__panel[data-panel="${btn.dataset.tab}"]`);
            if (target) { target.hidden = false; target.classList.add("is-active"); }
        });
    });

    /* ---- Avatar upload / preview ------------------------------------------ */
    const avatarInput = document.getElementById("avatarInput");
    const avatarEditBtn = document.getElementById("avatarEditBtn");
    const avatarPreview = document.getElementById("avatarPreview");

    avatarEditBtn?.addEventListener("click", () => avatarInput?.click());

    avatarInput?.addEventListener("change", () => {
        const file = avatarInput.files?.[0];
        if (!file) return;
        const reader = new FileReader();
        reader.onload = (e) => {
            avatarPreview.style.backgroundImage = `url(${e.target.result})`;
            avatarPreview.style.backgroundSize = "cover";
            avatarPreview.style.backgroundPosition = "center";
            avatarPreview.textContent = "";
        };
        reader.readAsDataURL(file);
        window.showToast?.("Photo updated — remember to save changes");
    });

    /* ---- Password visibility toggles -------------------------------------- */
    document.querySelectorAll(".input-with-icon__toggle").forEach((btn) => {
        btn.addEventListener("click", () => {
            const input = document.getElementById(btn.dataset.toggleFor);
            if (!input) return;
            input.type = input.type === "password" ? "text" : "password";
        });
    });

    /* ---- Password strength meter ------------------------------------------ */
    const newPassword = document.getElementById("newPassword");
    const pwBar = document.querySelector("#pwStrength span");
    const pwLabel = document.getElementById("pwStrengthLabel");

    newPassword?.addEventListener("input", () => {
        const val = newPassword.value;
        let score = 0;
        if (val.length >= 8) score++;
        if (/[A-Z]/.test(val)) score++;
        if (/[0-9]/.test(val)) score++;
        if (/[^A-Za-z0-9]/.test(val)) score++;

        const levels = [
            { width: "0%", color: "var(--danger-700)", label: "Use 8+ characters with a number and symbol" },
            { width: "25%", color: "var(--danger-700)", label: "Weak password" },
            { width: "55%", color: "var(--amber-600)", label: "Getting better" },
            { width: "80%", color: "var(--amber-500)", label: "Good password" },
            { width: "100%", color: "var(--success-700)", label: "Strong password" },
        ];
        const level = levels[val.length === 0 ? 0 : score];
        pwBar.style.width = level.width;
        pwBar.style.background = level.color;
        pwLabel.textContent = level.label;
    });

    /* ---- Form submissions (frontend-only feedback) ------------------------ */
    document.getElementById("profileForm")?.addEventListener("submit", (e) => {
        e.preventDefault();
        window.showToast?.("Profile changes saved");
    });

    document.getElementById("discardProfile")?.addEventListener("click", () => {
        document.getElementById("profileForm")?.reset();
        window.showToast?.("Changes discarded");
    });

    document.getElementById("passwordForm")?.addEventListener("submit", (e) => {
        e.preventDefault();
        const newPw = document.getElementById("newPassword").value;
        const confirmPw = document.getElementById("confirmPassword").value;
        if (newPw !== confirmPw) {
            window.showToast?.("Passwords don't match");
            return;
        }
        window.showToast?.("Password updated");
        e.target.reset();
        pwBar.style.width = "0%";
        pwLabel.textContent = "Use 8+ characters with a number and symbol";
    });
})();
