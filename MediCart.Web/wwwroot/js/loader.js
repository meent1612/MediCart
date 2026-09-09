/**
 * MediCart — Baymax Healthcare Companion Loading System
 * Handles both the full-screen website loading screen and the contextual action modal.
 */
(function (window, document) {
    "use strict";

    var introAnimation = null;
    var modalAnimation = null;

    // --------------------------------------------------------------------------
    // 1. Full-Screen Website Loading Screen (Lands on Homepage when finished)
    // --------------------------------------------------------------------------
    var MediCartLoadingScreen = {
        element: null,
        animContainer: null,
        progressBar: null,
        isCompleted: false,
        timeoutId: null,

        init: function () {
            this.element = document.getElementById("siteLoadingScreen");
            if (!this.element) return;

            this.animContainer = document.getElementById("baymaxIntroAnimation");
            this.progressBar = document.getElementById("siteLoadingProgress");

            var currentPath = window.location.pathname.toLowerCase().replace(/\/$/, "");
            var isHomePage = currentPath === "" || currentPath === "/home" || currentPath === "/home/index";
            var isLoadingPage = currentPath === "/loading" || currentPath === "/home/loading";
            var urlParams = new URLSearchParams(window.location.search);
            var forceIntro = urlParams.get("intro") === "1" || urlParams.get("replay") === "1";

            // Check if we should display the intro
            if (isHomePage || isLoadingPage || forceIntro) {
                this.start({
                    redirectToHome: isLoadingPage
                });
            } else {
                this.element.classList.add("is-hidden");
            }
        },

        start: function (options) {
            var opts = options || {};
            if (!this.element || !this.animContainer) return;

            this.isCompleted = false;
            if (this.timeoutId) {
                clearTimeout(this.timeoutId);
                this.timeoutId = null;
            }

            this.element.classList.remove("is-hidden", "is-dismissed");
            document.body.style.overflow = "hidden";

            // Reset progress bar
            if (this.progressBar) {
                this.progressBar.style.transition = "none";
                this.progressBar.style.width = "0%";
            }

            var self = this;
            var durationMs = 2800; // 2.8s total duration

            // Drive progress bar smoothly from 0% to 100% over 2.8s with ease-out
            requestAnimationFrame(function () {
                requestAnimationFrame(function () {
                    if (self.progressBar && !self.isCompleted) {
                        self.progressBar.style.transition = "width 2.8s cubic-bezier(0.25, 0.1, 0.25, 1)";
                        self.progressBar.style.width = "100%";
                    }
                });
            });

            // Hard timeout guarantee: never block longer than 2.8s even if assets are still loading
            this.timeoutId = setTimeout(function () {
                self.finish(opts.redirectToHome);
            }, durationMs);

            if (typeof lottie === "undefined") {
                return;
            }

            // Clean up previous instance if any
            if (introAnimation) {
                introAnimation.destroy();
                introAnimation = null;
            }

            try {
                introAnimation = lottie.loadAnimation({
                    container: self.animContainer,
                    renderer: "svg",
                    loop: false,
                    autoplay: true,
                    path: "/animations/loading.json"
                });

                // Set Lottie playback speed to ~2.7 (7.5s / 2.8s) so full animation plays start-to-finish faster
                introAnimation.setSpeed(2.7);

                introAnimation.addEventListener("complete", function () {
                    self.finish(opts.redirectToHome);
                });

                introAnimation.addEventListener("error", function (err) {
                    console.warn("Could not load Baymax intro animation:", err);
                });

            } catch (err) {
                console.warn("Could not start Baymax intro animation:", err);
            }
        },

        finish: function (redirectToHome) {
            if (this.isCompleted) return;
            this.isCompleted = true;

            if (this.timeoutId) {
                clearTimeout(this.timeoutId);
                this.timeoutId = null;
            }

            if (this.progressBar) {
                this.progressBar.style.width = "100%";
            }

            var self = this;
            if (this.element) {
                // Fade out over ~300ms then unmount
                this.element.classList.add("is-dismissed");
                document.body.style.overflow = "";

                setTimeout(function () {
                    self.element.classList.add("is-hidden");
                    if (redirectToHome) {
                        window.location.href = "/";
                    }
                }, 300);
            } else if (redirectToHome) {
                window.location.href = "/";
            }
        }
    };

    // --------------------------------------------------------------------------
    // 2. Contextual Action Modal (Orders, Login, Register, Admin tasks)
    // --------------------------------------------------------------------------
    var modalOverlayEl = null;
    var modalStatusEl = null;
    var modalAnimContainer = null;
    var defaultModalStatus = "Your personal healthcare companion is preparing your request…";

    function initActionModal() {
        modalOverlayEl = document.getElementById("medicartLoader");
        if (!modalOverlayEl) return;

        modalStatusEl = document.getElementById("medicartLoaderStatus");
        modalAnimContainer = document.getElementById("baymaxLoadingAnimation");

        if (modalAnimContainer && typeof lottie !== "undefined" && !modalAnimation) {
            try {
                modalAnimation = lottie.loadAnimation({
                    container: modalAnimContainer,
                    renderer: "svg",
                    loop: true,
                    autoplay: true,
                    path: "/animations/loading.json"
                });
            } catch (err) {
                console.warn("Could not load Baymax modal animation:", err);
            }
        }
    }

    var MediCartLoader = {
        show: function (message) {
            if (!modalOverlayEl) initActionModal();
            if (!modalOverlayEl) return;

            if (modalStatusEl) {
                modalStatusEl.textContent = message || defaultModalStatus;
            }

            modalOverlayEl.classList.add("is-active");
            modalOverlayEl.setAttribute("aria-hidden", "false");

            if (modalAnimation && modalAnimation.isPaused) {
                modalAnimation.play();
            }
        },

        hide: function () {
            if (!modalOverlayEl) return;
            modalOverlayEl.classList.remove("is-active");
            modalOverlayEl.setAttribute("aria-hidden", "true");
        },

        setText: function (message) {
            if (modalStatusEl) {
                modalStatusEl.textContent = message || defaultModalStatus;
            }
        },

        isVisible: function () {
            return modalOverlayEl ? modalOverlayEl.classList.contains("is-active") : false;
        }
    };

    // Global APIs
    window.MediCartLoadingScreen = MediCartLoadingScreen;
    window.MediCartLoader = MediCartLoader;
    window.showLoading = MediCartLoader.show.bind(MediCartLoader);
    window.hideLoading = MediCartLoader.hide.bind(MediCartLoader);
    window.replayIntro = function () {
        MediCartLoadingScreen.start({ redirectToHome: false });
    };

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", function () {
            MediCartLoadingScreen.init();
            initActionModal();
        });
    } else {
        MediCartLoadingScreen.init();
        initActionModal();
    }
})(window, document);
