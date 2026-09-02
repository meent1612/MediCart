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

    /* ---- Form submissions (frontend-only feedback) ------------------------ */
    document.getElementById("discardProfile")?.addEventListener("click", () => {
        document.getElementById("profileForm")?.reset();
        window.showToast?.("Changes discarded");
    });
})();
