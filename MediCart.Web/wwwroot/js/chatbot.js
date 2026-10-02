/**
 * MediCart — Baymax Floating Chatbot Widget
 * Role-aware assistant for customer-facing and admin pages.
 */

(function (window, document) {
    "use strict";

    var widgetEl = null;
    var launcherBtn = null;
    var launcherAnimEl = null;
    var unreadBadgeEl = null;
    var panelEl = null;
    var closeBtn = null;
    var messagesEl = null;
    var typingEl = null;
    var chipsEl = null;
    var formEl = null;
    var inputEl = null;

    var lottieAnim = null;
    var userRole = "Guest";
    var conversationHistory = [];
    var isOpen = false;

    // Role-specific UI definitions.
    // These are only greetings and quick actions.
    // Catalogue facts must NEVER be hard-coded here.
    var roleConfigs = {
        Admin: {
            greeting: "Hi, need a hand with the admin panel?",
            chips: [
                "How many orders are flagged?",
                "Show me low-stock items",
                "How do I approve an order?"
            ]
        },

        Customer: {
            greeting: "Hi, I'm Baymax — ask me about medicines, your order, or how MediCart works.",
            chips: [
                "Track my order",
                "Is Paracetamol in stock?",
                "How does pharmacist review work?"
            ]
        },

        Guest: {
            greeting: "Hi, I'm Baymax — ask me about medicines, your order, or how MediCart works.",
            chips: [
                "Track my order",
                "Is Paracetamol in stock?",
                "How does pharmacist review work?"
            ]
        }
    };

    function initWidget() {
        widgetEl = document.getElementById("baymaxChatWidget");
        if (!widgetEl) return;

        launcherBtn = document.getElementById("baymaxLauncher");
        launcherAnimEl = document.getElementById("baymaxLauncherAnim");
        unreadBadgeEl = document.getElementById("baymaxUnreadBadge");
        panelEl = document.getElementById("baymaxChatPanel");
        closeBtn = document.getElementById("baymaxCloseBtn");
        messagesEl = document.getElementById("baymaxMessages");
        typingEl = document.getElementById("baymaxTyping");
        chipsEl = document.getElementById("baymaxChips");
        formEl = document.getElementById("baymaxForm");
        inputEl = document.getElementById("baymaxInput");

        userRole = widgetEl.getAttribute("data-role") || "Guest";

        if (!roleConfigs[userRole]) {
            userRole = "Guest";
        }

        initLottieLauncher();
        initGreetingAndChips();
        bindEvents();
        initProactiveBadge();
    }

    // --------------------------------------------------------------------------
    // PART 1 — Launcher Animation & Interaction
    // --------------------------------------------------------------------------

    function applyFaceCrop() {
        if (!launcherAnimEl) return;

        var svg = launcherAnimEl.querySelector("svg");

        if (svg) {
            svg.setAttribute("viewBox", "974 258 150 150");
            svg.setAttribute("preserveAspectRatio", "xMidYMid meet");
            svg.style.width = "100%";
            svg.style.height = "100%";
        }
    }

    function initLottieLauncher() {
        if (!launcherAnimEl || typeof lottie === "undefined") {
            return;
        }

        try {
            lottieAnim = lottie.loadAnimation({
                container: launcherAnimEl,
                renderer: "svg",
                loop: false,
                autoplay: false,
                path: "/animations/Baymax_Robo_Medic.json"
            });

            lottieAnim.addEventListener("DOMLoaded", function () {
                applyFaceCrop();

                lottieAnim.goToAndStop(0, true);

                applyFaceCrop();

                var svg = launcherAnimEl.querySelector("svg");

                if (svg && window.MutationObserver) {
                    var observer = new MutationObserver(function () {
                        if (svg.getAttribute("viewBox") !== "974 258 150 150") {
                            svg.setAttribute("viewBox", "974 258 150 150");
                        }
                    });

                    observer.observe(svg, {
                        attributes: true,
                        attributeFilter: ["viewBox"]
                    });
                }
            });

            lottieAnim.addEventListener("data_ready", applyFaceCrop);

            launcherBtn.addEventListener("mouseenter", function () {
                if (!isOpen && lottieAnim) {
                    lottieAnim.playSegments([15, 35], true);
                    applyFaceCrop();
                }
            });

            launcherBtn.addEventListener("mouseleave", function () {
                if (!isOpen && lottieAnim) {
                    lottieAnim.goToAndStop(15, true);
                    applyFaceCrop();
                }
            });

        } catch (err) {
            console.warn(
                "Could not initialize Baymax Lottie launcher:",
                err
            );
        }
    }

    function initProactiveBadge() {
        if (!unreadBadgeEl) return;

        var hasSeen = sessionStorage.getItem("medicart_baymax_seen");

        if (hasSeen === "true") return;

        setTimeout(function () {
            if (!isOpen && unreadBadgeEl) {
                unreadBadgeEl.style.display = "block";
            }
        }, 3500);
    }

    // --------------------------------------------------------------------------
    // PART 2 & 3 — Panel State & Role-Aware Greeting
    // --------------------------------------------------------------------------

    function initGreetingAndChips() {
        var cfg = roleConfigs[userRole] || roleConfigs.Guest;

        if (
            messagesEl &&
            messagesEl.querySelectorAll(".baymax-bubble").length === 0
        ) {
            appendMessage("bot", cfg.greeting);
        }

        renderChips(cfg.chips);
    }

    function renderChips(chips) {
        if (!chipsEl) return;

        chipsEl.innerHTML = "";

        chips.forEach(function (chipText) {
            var chipBtn = document.createElement("button");

            chipBtn.type = "button";
            chipBtn.className = "baymax-chip";
            chipBtn.textContent = chipText;

            chipBtn.addEventListener("click", function () {
                handleUserSend(chipText);
            });

            chipsEl.appendChild(chipBtn);
        });
    }

    function openPanel() {
        if (isOpen) return;

        isOpen = true;

        widgetEl.classList.add("is-open");
        launcherBtn.setAttribute("aria-expanded", "true");
        panelEl.setAttribute("aria-hidden", "false");

        if (unreadBadgeEl) {
            unreadBadgeEl.style.display = "none";
        }

        sessionStorage.setItem("medicart_baymax_seen", "true");

        setTimeout(function () {
            if (inputEl) {
                inputEl.focus();
            }

            scrollToBottom();
        }, 150);
    }

    function closePanel() {
        if (!isOpen) return;

        isOpen = false;

        widgetEl.classList.remove("is-open");
        launcherBtn.setAttribute("aria-expanded", "false");
        panelEl.setAttribute("aria-hidden", "true");

        if (launcherBtn) {
            launcherBtn.focus();
        }
    }

    function bindEvents() {
        launcherBtn.addEventListener("click", function (e) {
            e.preventDefault();
            openPanel();
        });

        closeBtn.addEventListener("click", function (e) {
            e.preventDefault();
            closePanel();
        });

        document.addEventListener("keydown", function (e) {
            if (e.key === "Escape" && isOpen) {
                closePanel();
            }
        });

        formEl.addEventListener("submit", function (e) {
            e.preventDefault();

            var text = inputEl
                ? inputEl.value.trim()
                : "";

            if (text) {
                handleUserSend(text);
            }
        });
    }

    // --------------------------------------------------------------------------
    // Messaging
    // --------------------------------------------------------------------------

    function appendMessage(sender, text) {
        if (!messagesEl) return;

        var bubble = document.createElement("div");

        bubble.className =
            "baymax-bubble " +
            (sender === "user"
                ? "baymax-bubble--user"
                : "baymax-bubble--bot");

        bubble.textContent = text;

        if (typingEl && typingEl.parentNode === messagesEl) {
            messagesEl.insertBefore(bubble, typingEl);
        } else {
            messagesEl.appendChild(bubble);
        }

        conversationHistory.push({
            sender: sender,
            text: text,
            timestamp: new Date().toISOString()
        });

        scrollToBottom();
    }

    function showTyping() {
        if (typingEl) {
            typingEl.style.display = "flex";

            if (messagesEl) {
                messagesEl.appendChild(typingEl);
            }

            scrollToBottom();
        }
    }

    function hideTyping() {
        if (typingEl) {
            typingEl.style.display = "none";
        }
    }

    function scrollToBottom() {
        if (messagesEl) {
            messagesEl.scrollTop = messagesEl.scrollHeight;
        }
    }

    // --------------------------------------------------------------------------
    // Backend Integration
    //
    // IMPORTANT:
    // There is intentionally NO simulated catalogue fallback.
    //
    // If the backend fails, we show an error instead of inventing medicine,
    // price, stock, order, expiry, or other MediCart data.
    // --------------------------------------------------------------------------

    function handleUserSend(text) {
        if (!text) return;

        appendMessage("user", text);

        if (inputEl) {
            inputEl.value = "";
        }

        showTyping();

        var payload = {
            message: text,
            role: userRole,
            history: conversationHistory
        };

        fetch("/api/chat/message", {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "X-Requested-With": "XMLHttpRequest"
            },
            body: JSON.stringify(payload)
        })
            .then(function (res) {
                if (!res.ok) {
                    return res.text().then(function (body) {
                        var errorMessage =
                            "Baymax backend request failed with HTTP " +
                            res.status +
                            ".";

                        if (body) {
                            console.error(
                                "Baymax backend error:",
                                res.status,
                                body
                            );
                        }

                        throw new Error(errorMessage);
                    });
                }

                return res.json();
            })
            .then(function (data) {
                hideTyping();

                var reply =
                    data.reply ||
                    data.message;

                if (!reply) {
                    console.error(
                        "Baymax returned an unexpected response:",
                        data
                    );

                    appendMessage(
                        "bot",
                        "I received an unexpected response from the MediCart assistant. Please try again."
                    );

                    return;
                }

                appendMessage("bot", reply);
            })
            .catch(function (error) {
                hideTyping();

                console.error(
                    "Baymax chat request failed:",
                    error
                );

                appendMessage(
                    "bot",
                    "I couldn't connect to the MediCart assistant right now. I won't guess about medicine stock or catalogue information. Please try again."
                );
            });
    }

    // Initialize once DOM is ready
    if (document.readyState === "loading") {
        document.addEventListener(
            "DOMContentLoaded",
            initWidget
        );
    } else {
        initWidget();
    }

    // Expose global controller for testing/verification
    window.MediCartBaymax = {
        open: openPanel,

        close: closePanel,

        toggle: function () {
            if (isOpen) {
                closePanel();
            } else {
                openPanel();
            }
        },

        sendMessage: handleUserSend,

        getRole: function () {
            return userRole;
        }
    };

})(window, document);