// MediCart Admin — shared shell behavior (mobile sidebar)
(function () {
    "use strict";
    const sidebar = document.getElementById("adminSidebar");
    const toggleBtn = document.getElementById("sidebarToggle");

    toggleBtn?.addEventListener("click", () => {
        const isOpen = sidebar.classList.toggle("is-open");
        toggleBtn.setAttribute("aria-expanded", String(isOpen));
    });

    document.addEventListener("click", (e) => {
        if (!sidebar?.classList.contains("is-open")) return;
        if (sidebar.contains(e.target) || toggleBtn.contains(e.target)) return;
        sidebar.classList.remove("is-open");
        toggleBtn.setAttribute("aria-expanded", "false");
    });

    // Toast helper reused across admin pages
    window.showToast = function (message) {
        const toast = document.getElementById("toast");
        if (!toast) return;
        toast.querySelector(".toast__text").textContent = message;
        toast.classList.add("is-visible");
        clearTimeout(window.__toastTimer);
        window.__toastTimer = setTimeout(() => toast.classList.remove("is-visible"), 2600);
    };
})();
