// MediCart — shared site chrome behavior (navbar + mobile drawer)
(function () {
    "use strict";

    const header = document.querySelector(".site-header");
    const toggleBtn = document.querySelector(".navbar__toggle");
    const closeBtn = document.querySelector(".mobile-nav__close");
    const mobileNav = document.querySelector(".mobile-nav");

    // Shrink / shadow navbar after scrolling past the hero edge
    const onScroll = () => {
        if (!header) return;
        header.classList.toggle("is-scrolled", window.scrollY > 8);
    };
    document.addEventListener("scroll", onScroll, { passive: true });
    onScroll();

    // Mobile nav drawer
    const openMobileNav = () => {
        if (!mobileNav) return;
        mobileNav.classList.add("is-open");
        mobileNav.setAttribute("aria-hidden", "false");
        toggleBtn?.setAttribute("aria-expanded", "true");
        document.body.style.overflow = "hidden";
        mobileNav.querySelector("a, button")?.focus();
    };

    const closeMobileNav = () => {
        if (!mobileNav) return;
        mobileNav.classList.remove("is-open");
        mobileNav.setAttribute("aria-hidden", "true");
        toggleBtn?.setAttribute("aria-expanded", "false");
        document.body.style.overflow = "";
        toggleBtn?.focus();
    };

    toggleBtn?.addEventListener("click", openMobileNav);
    closeBtn?.addEventListener("click", closeMobileNav);

    mobileNav?.querySelectorAll("a").forEach((link) => {
        link.addEventListener("click", closeMobileNav);
    });

    document.addEventListener("keydown", (e) => {
        if (e.key === "Escape" && mobileNav?.classList.contains("is-open")) {
            closeMobileNav();
        }
    });
})();
