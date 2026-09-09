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

    /* ---- "Meet Baymax" Hero Animation Sequence --------------------------- */
    const meetBaymaxBtn = document.getElementById("meetBaymaxBtn");
    const baymaxOverlay = document.getElementById("baymaxHeroOverlay");
    const baymaxWalker = document.getElementById("baymaxHeroWalker");
    const baymaxBubble = document.getElementById("baymaxSpeechBubble");
    const baymaxAnimContainer = document.getElementById("baymaxHeroLottie");

    let heroBaymaxAnim = null;
    let isGreetingRunning = false;

    function initHeroBaymax() {
        if (!meetBaymaxBtn || !baymaxWalker || !baymaxAnimContainer) return;
        if (baymaxOverlay) baymaxOverlay.style.display = "none";
        if (typeof lottie === "undefined") return;

        try {
            heroBaymaxAnim = lottie.loadAnimation({
                container: baymaxAnimContainer,
                renderer: "svg",
                loop: false,
                autoplay: false,
                path: "/animations/loading.json"
            });
            heroBaymaxAnim.setSpeed(1.6);
        } catch (err) {
            console.warn("Could not load hero Baymax animation:", err);
        }

        meetBaymaxBtn.addEventListener("click", function () {
            if (isGreetingRunning) return;
            isGreetingRunning = true;
            meetBaymaxBtn.disabled = true;

            if (baymaxOverlay) {
                baymaxOverlay.style.display = "block";
            }

            const isMobile = window.matchMedia("(max-width: 767px)").matches;
            const prefersReducedMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;

            // Coordinates: desktop lands at x = 22vw; mobile lands at x = 12vw scaled to 55%
            const targetX = isMobile ? "12vw" : "22vw";
            const scaleStr = isMobile ? " scale(0.55)" : "";
            const landedTransform = `translateX(${targetX})${scaleStr}`;
            const offscreenTransform = `translateX(-120%)${scaleStr}`;

            if (heroBaymaxAnim) {
                heroBaymaxAnim.goToAndStop(0, true);
                heroBaymaxAnim.setSpeed(1.6);
            }

            if (prefersReducedMotion) {
                // Reduced motion: skip walk, fade in, hold, fade out
                baymaxWalker.style.transition = "none";
                baymaxWalker.style.transform = landedTransform;
                baymaxWalker.style.opacity = "0";
                baymaxWalker.style.visibility = "visible";

                if (heroBaymaxAnim) {
                    heroBaymaxAnim.play();
                }

                requestAnimationFrame(() => {
                    baymaxWalker.style.transition = "opacity 0.3s ease";
                    baymaxWalker.style.opacity = "1";
                });

                // Speech bubble "Hi!"
                setTimeout(() => {
                    baymaxBubble?.classList.add("is-visible");
                }, 350);

                setTimeout(() => {
                    baymaxBubble?.classList.remove("is-visible");
                }, 1700);

                setTimeout(() => {
                    baymaxWalker.style.transition = "opacity 0.3s ease";
                    baymaxWalker.style.opacity = "0";
                }, 2100);

                setTimeout(() => {
                    baymaxWalker.style.visibility = "hidden";
                    baymaxWalker.style.opacity = "";
                    baymaxWalker.style.transform = offscreenTransform;
                    if (baymaxOverlay) baymaxOverlay.style.display = "none";
                    if (heroBaymaxAnim) {
                        heroBaymaxAnim.goToAndStop(0, true);
                    }
                    meetBaymaxBtn.disabled = false;
                    isGreetingRunning = false;
                }, 2500);

                return;
            }

            // Normal motion sequence:
            // 1. Starts off-screen to the LEFT of the viewport, hidden.
            baymaxWalker.style.transition = "none";
            baymaxWalker.style.transform = offscreenTransform;
            baymaxWalker.style.opacity = "1";
            baymaxWalker.style.visibility = "visible";

            // 2. Slides in from left edge to x = 22% (or 12% on mobile) over ~0.7s, ease-out.
            // Lottie plays during the whole sequence at speed ~1.6.
            if (heroBaymaxAnim) {
                heroBaymaxAnim.play();
            }

            requestAnimationFrame(() => {
                requestAnimationFrame(() => {
                    baymaxWalker.style.transition = "transform 0.7s cubic-bezier(0.16, 1, 0.3, 1)";
                    baymaxWalker.style.transform = landedTransform;
                });
            });

            // 3. Holds at that position for ~1.1s (the "hi" beat). Speech bubble fades+scales in "Hi!"
            setTimeout(() => {
                baymaxBubble?.classList.add("is-visible");
            }, 700);

            // Speech bubble fades out before he leaves (~1.55s)
            setTimeout(() => {
                baymaxBubble?.classList.remove("is-visible");
            }, 1550);

            // 4. Slides back out to the left, off-screen, over ~0.7s, ease-in (~1.8s to ~2.5s)
            setTimeout(() => {
                baymaxWalker.style.transition = "transform 0.7s cubic-bezier(0.7, 0, 0.84, 0)";
                baymaxWalker.style.transform = offscreenTransform;
            }, 1800);

            // Total ≈ 2.5s: unmounts and Lottie resets to frame 0
            setTimeout(() => {
                baymaxWalker.style.visibility = "hidden";
                baymaxWalker.style.transition = "none";
                baymaxWalker.style.transform = offscreenTransform;
                if (baymaxOverlay) baymaxOverlay.style.display = "none";
                if (heroBaymaxAnim) {
                    heroBaymaxAnim.goToAndStop(0, true);
                }
                meetBaymaxBtn.disabled = false;
                isGreetingRunning = false;
            }, 2500);
        });
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", initHeroBaymax);
    } else {
        initHeroBaymax();
    }
})();
