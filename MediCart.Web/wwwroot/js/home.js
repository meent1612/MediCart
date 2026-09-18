// MediCart — Home page interactions
(function () {
    "use strict";

    /* ---- Toast helper -------------------------------------------------- */
    const toast = document.getElementById("toast");
    let toastTimer = null;
    const showToast = (message) => {
        if (!toast) return;
        toast.querySelector(".toast__text").textContent = message;
        toast.classList.add("is-visible");
        clearTimeout(toastTimer);
        toastTimer = setTimeout(() => toast.classList.remove("is-visible"), 2200);
    };

    /* ---- Add-to-cart buttons on product cards (shared cart handler) --- */
    document.querySelectorAll(".add-btn").forEach((btn) => {
        btn.addEventListener("click", () => {
            const medId = btn.dataset.id;
            if (!medId) return;

            window.MediCartCart.add({
                medicineId: medId,
                quantity: 1,
                button: btn,
                onSuccess: () => {
                    btn.classList.add("is-added");
                    setTimeout(() => btn.classList.remove("is-added"), 350);
                }
            });
        });
    });

    /* ---- Category cards keyboard support -------------------------------- */
    document.querySelectorAll(".category-card").forEach((card) => {
        card.addEventListener("keydown", (e) => {
            if (e.key === "Enter" || e.key === " ") {
                e.preventDefault();
                card.click();
            }
        });
    });

    /* ---- Card grids pop-in on scroll (product type, category, how-it-works, stats, testimonials) ---- */
    const popGrids = document.querySelectorAll(".category-grid, .shop-category-grid, .steps, .stats-strip, .testimonials-grid");
    if (popGrids.length) {
        const prefersReducedMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;

        popGrids.forEach((grid) => {
            const cards = grid.querySelectorAll(".category-card, .shop-category-card, .step, .stats-strip__item, .testimonial-card");
            cards.forEach((card, index) => {
                card.style.setProperty("--pop-delay", index);
                if (prefersReducedMotion || !("IntersectionObserver" in window)) {
                    card.classList.add("is-visible");
                }
            });
        });

        if (!prefersReducedMotion && "IntersectionObserver" in window) {
            const popObserver = new IntersectionObserver(
                (entries, observer) => {
                    entries.forEach((entry) => {
                        if (entry.isIntersecting) {
                            entry.target.classList.add("is-visible");
                            observer.unobserve(entry.target);
                        }
                    });
                },
                {
                    rootMargin: "0px 0px -10% 0px",
                    threshold: 0.15
                }
            );

            popGrids.forEach((grid) => {
                const cards = grid.querySelectorAll(".category-card, .shop-category-card, .step, .stats-strip__item, .testimonial-card");
                cards.forEach((card) => popObserver.observe(card));
            });
        }
    }

    /* ---- Hero search: friendly no-op guard until Browse page exists ----- */
    const searchForm = document.getElementById("heroSearchForm");
    searchForm?.addEventListener("submit", (e) => {
        const input = document.getElementById("heroSearchInput");
        if (!input?.value.trim()) {
            e.preventDefault();
            input?.focus();
        }
        // otherwise lets the GET submit through to the Browse route
    });

    /* ---- Stats strip: count up when scrolled into view ------------------- */
    const statEls = document.querySelectorAll("[data-count-to]");
    if (statEls.length) {
        const animateCount = (el) => {
            const target = parseInt(el.getAttribute("data-count-to"), 10) || 0;
            const suffix = el.getAttribute("data-suffix") || "";
            const duration = 1200;
            const start = performance.now();

            const tick = (now) => {
                const progress = Math.min((now - start) / duration, 1);
                const eased = 1 - Math.pow(1 - progress, 3); // ease-out cubic
                const value = Math.round(target * eased);
                el.textContent = value.toLocaleString() + suffix;
                if (progress < 1) requestAnimationFrame(tick);
            };
            requestAnimationFrame(tick);
        };

        if ("IntersectionObserver" in window) {
            const statObserver = new IntersectionObserver(
                (entries) => {
                    entries.forEach((entry) => {
                        if (entry.isIntersecting) {
                            animateCount(entry.target);
                            statObserver.unobserve(entry.target);
                        }
                    });
                },
                { threshold: 0.4 }
            );
            statEls.forEach((el) => statObserver.observe(el));
        } else {
            statEls.forEach((el) => animateCount(el));
        }
    }

    /* ---- Hero Baymax Visual Anchor & Reaction --------------------------- */
    const meetBaymaxBtn = document.getElementById("meetBaymaxBtn");
    const baymaxAnchor = document.getElementById("heroBaymaxAnchor");
    const baymaxBubble = document.getElementById("heroBaymaxBubble");
    const baymaxBubbleText = document.getElementById("heroBaymaxBubbleText");
    const baymaxAnimContainer = document.getElementById("baymaxHeroLottie");
    const howItWorksSection = document.getElementById("howItWorksSection");

    let heroBaymaxAnim = null;
    let isReactionRunning = false;

    function initHeroBaymax() {
        if (!baymaxAnimContainer) return;
        if (typeof lottie === "undefined") return;

        try {
            heroBaymaxAnim = lottie.loadAnimation({
                container: baymaxAnimContainer,
                renderer: "svg",
                loop: false,
                autoplay: false,
                path: "/animations/loading.json"
            });

            heroBaymaxAnim.addEventListener("DOMLoaded", () => {
                // Hold on a calm standing frame as the visual anchor
                heroBaymaxAnim.goToAndStop(0, true);
            });
        } catch (err) {
            console.warn("Could not load hero Baymax animation:", err);
        }

        const triggerBaymaxReaction = () => {
            if (isReactionRunning) return;
            isReactionRunning = true;
            if (meetBaymaxBtn) meetBaymaxBtn.disabled = true;

            const prefersReducedMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;

            // 1. Play reaction animation (wave / greeting)
            if (heroBaymaxAnim && !prefersReducedMotion) {
                heroBaymaxAnim.setSpeed(1.4);
                heroBaymaxAnim.goToAndPlay(0, true);
            }

            // 2. React with speech bubble
            if (baymaxBubble) {
                baymaxBubble.classList.add("is-reacting");
            }
            if (baymaxBubbleText) {
                baymaxBubbleText.textContent = "I will care for your order!";
            }

            // 3. Scroll to "How it works" section where pharmacist & order review is explained
            setTimeout(() => {
                if (howItWorksSection) {
                    howItWorksSection.scrollIntoView({
                        behavior: prefersReducedMotion ? "auto" : "smooth",
                        block: "start"
                    });
                }
            }, prefersReducedMotion ? 100 : 700);

            // 4. Reset speech bubble and enable button
            setTimeout(() => {
                if (baymaxBubble) {
                    baymaxBubble.classList.remove("is-reacting");
                }
                if (baymaxBubbleText) {
                    baymaxBubbleText.textContent = "Hello! I am Baymax.";
                }
                if (heroBaymaxAnim) {
                    heroBaymaxAnim.goToAndStop(0, true);
                }
                if (meetBaymaxBtn) meetBaymaxBtn.disabled = false;
                isReactionRunning = false;
            }, 2600);
        };

        meetBaymaxBtn?.addEventListener("click", triggerBaymaxReaction);
        baymaxAnchor?.addEventListener("click", triggerBaymaxReaction);
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", initHeroBaymax);
    } else {
        initHeroBaymax();
    }
})();
