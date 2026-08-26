// MediCart — Home page interactions
(function () {
    "use strict";

    /* ---- Scroll reveal ---------------------------------------------- */
    const revealEls = document.querySelectorAll(".reveal");
    if ("IntersectionObserver" in window && revealEls.length) {
        const observer = new IntersectionObserver(
            (entries) => {
                entries.forEach((entry) => {
                    if (entry.isIntersecting) {
                        entry.target.classList.add("in-view");
                        observer.unobserve(entry.target);
                    }
                });
            },
            { threshold: 0.15 }
        );
        revealEls.forEach((el) => observer.observe(el));
    } else {
        revealEls.forEach((el) => el.classList.add("in-view"));
    }

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

    /* ---- Cart badge (front-end preview only — no backend yet) --------- */
    const cartBadge = document.querySelector(".cart-button__badge");
    const cartButton = document.querySelector(".cart-button");
    let cartCount = 0;

    const bumpCart = () => {
        cartCount += 1;
        if (cartBadge) {
            cartBadge.textContent = String(cartCount);
            cartBadge.classList.add("is-visible");
        }
        cartButton?.classList.remove("is-bumped");
        // restart animation
        void cartButton?.offsetWidth;
        cartButton?.classList.add("is-bumped");
    };

    /* ---- Add-to-cart buttons on product cards -------------------------- */
    document.querySelectorAll(".add-btn").forEach((btn) => {
        btn.addEventListener("click", () => {
            const card = btn.closest(".product-card");
            const name = card?.querySelector(".product-card__name")?.textContent?.trim() || "Item";

            btn.classList.add("is-added");
            btn.setAttribute("aria-label", `${name} added to cart`);
            setTimeout(() => btn.classList.remove("is-added"), 350);

            bumpCart();
            showToast(`${name} added to cart`);
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

    /* ---- Testimonial carousel ---------------------------------------- */
    const testimonialTrack = document.querySelector(".testimonial__track");
    if (testimonialTrack) {
        const slides = Array.from(testimonialTrack.querySelectorAll(".testimonial__slide"));
        const dots = Array.from(document.querySelectorAll(".testimonial__dot"));
        const prevBtn = document.querySelector(".testimonial__arrow--prev");
        const nextBtn = document.querySelector(".testimonial__arrow--next");
        let current = 0;
        let autoTimer = null;

        const goTo = (index) => {
            current = (index + slides.length) % slides.length;
            slides.forEach((slide, i) => slide.classList.toggle("is-active", i === current));
            dots.forEach((dot, i) => dot.classList.toggle("is-active", i === current));
        };

        const startAuto = () => {
            clearInterval(autoTimer);
            autoTimer = setInterval(() => goTo(current + 1), 5500);
        };

        prevBtn?.addEventListener("click", () => { goTo(current - 1); startAuto(); });
        nextBtn?.addEventListener("click", () => { goTo(current + 1); startAuto(); });
        dots.forEach((dot, i) => dot.addEventListener("click", () => { goTo(i); startAuto(); }));

        startAuto();
    }

    /* ---- Subtle mouse-tilt on the hero review card ----------------------- */
    const reviewCard = document.querySelector(".review-card");
    const heroGrid = document.querySelector(".hero__grid");
    if (reviewCard && heroGrid && window.matchMedia("(pointer: fine)").matches) {
        heroGrid.addEventListener("mousemove", (e) => {
            const rect = heroGrid.getBoundingClientRect();
            const x = (e.clientX - rect.left) / rect.width - 0.5;
            const y = (e.clientY - rect.top) / rect.height - 0.5;
            reviewCard.style.transform = `rotateY(${x * 6}deg) rotateX(${y * -6}deg)`;
        });
        heroGrid.addEventListener("mouseleave", () => {
            reviewCard.style.transform = "";
        });
    }
})();
