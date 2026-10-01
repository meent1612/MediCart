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

    // Role-specific definitions
    var roleConfigs = {
        Admin: {
            greeting: "Hi, need a hand with the admin panel?",
            chips: [
                "How many orders are flagged?",
                "Show me low-stock items",
                "How do I approve an order?"
            ],
            simulatedReplies: {
                "how many orders are flagged?": "You can inspect all flagged orders under Orders → Flagged Orders (/AdminOrders/FlaggedOrders). Orders are automatically flagged by our safety algorithms when high-risk drugs or unusual quantities are detected.",
                "show me low-stock items": "You can monitor inventory levels under Inventory → Stock & Expiry (/Admin/StockExpiry). Medicines reaching critical thresholds or nearing expiration dates are highlighted for rapid replenishment.",
                "how do i approve an order?": "Navigate to Incoming Orders (/AdminOrders/IncomingOrders) or open any Order Detail view. Review the patient information, verify any prescription documents, and click 'Verify & Approve Order' to dispatch."
            }
        },
        Customer: {
            greeting: "Hi, I'm Baymax — ask me about medicines, your order, or how MediCart works.",
            chips: [
                "Track my order",
                "Is Paracetamol in stock?",
                "How does pharmacist review work?"
            ],
            simulatedReplies: {
                "track my order": "You can review all your orders and live fulfillment status in your Account Orders page. Every order is reviewed by a licensed pharmacist before it ships!",
                "is paracetamol in stock?": "Yes, Paracetamol 500mg tablets are currently in stock with high availability! You can browse and add them to your cart from our Medicines catalogue.",
                "how does pharmacist review work?": "At MediCart, every single order is personally reviewed and verified by a licensed pharmacist before shipping — not just prescription-only drugs. We verify dosages and ensure safe medicine combinations."
            }
        },
        Guest: {
            greeting: "Hi, I'm Baymax — ask me about medicines, your order, or how MediCart works.",
            chips: [
                "Track my order",
                "Is Paracetamol in stock?",
                "How does pharmacist review work?"
            ],
            simulatedReplies: {
                "track my order": "To track an existing order, please log in to your MediCart account. If you don't have an account yet, registration only takes a minute with no prescription needed to sign up!",
                "is paracetamol in stock?": "Yes, Paracetamol 500mg is currently in stock! You can browse our catalogue and place an order anytime.",
                "how does pharmacist review work?": "MediCart is an admin-verified online pharmacy. Every order is carefully reviewed by a licensed pharmacist before dispatch to ensure your health and safety."
            }
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
    // PART 1 — Launcher Animation & Interaction (Baymax Face Close-Up)
    // --------------------------------------------------------------------------
    function applyFaceCrop() {
        if (!launcherAnimEl) return;
        var svg = launcherAnimEl.querySelector("svg");
        if (svg) {
            // Zoom and crop into Baymax's face close-up
            // Canvas: 1920x1080. Face center is at X: 1049, Y: 333
            // Box [974, 258, 150, 150] tightly centers the face and eyes close-up
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
                // Pause at upright standing frame (frame 0) for idle state
                lottieAnim.goToAndStop(0, true);
                applyFaceCrop();

                // Lock the viewBox so any animation frame preserves the face close-up
                var svg = launcherAnimEl.querySelector("svg");
                if (svg && window.MutationObserver) {
                    var observer = new MutationObserver(function () {
                        if (svg.getAttribute("viewBox") !== "974 258 150 150") {
                            svg.setAttribute("viewBox", "974 258 150 150");
                        }
                    });
                    observer.observe(svg, { attributes: true, attributeFilter: ["viewBox"] });
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
            console.warn("Could not initialize Baymax Lottie launcher:", err);
        }
    }

    function initProactiveBadge() {
        if (!unreadBadgeEl) return;
        var hasSeen = sessionStorage.getItem("medicart_baymax_seen");
        if (hasSeen === "true") return;

        // Subtle proactive greeting badge after 3.5 seconds
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

        // Render initial opening message if empty
        if (messagesEl && messagesEl.querySelectorAll(".baymax-bubble").length === 0) {
            appendMessage("bot", cfg.greeting);
        }

        // Render quick-reply chips
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

        // Focus input after panel open transition
        setTimeout(function () {
            if (inputEl) inputEl.focus();
            scrollToBottom();
        }, 150);
    }

    function closePanel() {
        if (!isOpen) return;
        isOpen = false;
        widgetEl.classList.remove("is-open");
        launcherBtn.setAttribute("aria-expanded", "false");
        panelEl.setAttribute("aria-hidden", "true");

        // Return focus to launcher
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

        // Close on Escape key
        document.addEventListener("keydown", function (e) {
            if (e.key === "Escape" && isOpen) {
                closePanel();
            }
        });

        // Send on form submit
        formEl.addEventListener("submit", function (e) {
            e.preventDefault();
            var text = inputEl ? inputEl.value.trim() : "";
            if (text) {
                handleUserSend(text);
            }
        });
    }

    // --------------------------------------------------------------------------
    // Messaging & Backend Fallback
    // --------------------------------------------------------------------------
    function appendMessage(sender, text) {
        if (!messagesEl) return;

        var bubble = document.createElement("div");
        bubble.className = "baymax-bubble " + (sender === "user" ? "baymax-bubble--user" : "baymax-bubble--bot");
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

    function handleUserSend(text) {
        if (!text) return;
        appendMessage("user", text);

        if (inputEl) {
            inputEl.value = "";
        }

        showTyping();

        // ----------------------------------------------------------------------
        // PART 4 — Backend Integration Contract
        // Attempt POST /api/chat/message; if unavailable or failing, fallback to
        // responsive persona simulation.
        // ----------------------------------------------------------------------
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
                throw new Error("Backend chat endpoint not implemented (" + res.status + ")");
            }
            return res.json();
        })
        .then(function (data) {
            hideTyping();
            var reply = data.reply || data.message || "I am here to help you.";
            appendMessage("bot", reply);
        })
        .catch(function () {
            // Simulated response with realistic thinking latency
            setTimeout(function () {
                hideTyping();
                var simulatedReply = generateSimulatedReply(text);
                appendMessage("bot", simulatedReply);
            }, 600);
        });
    }

    function generateSimulatedReply(userText) {
        var lower = userText.toLowerCase().trim();
        var cfg = roleConfigs[userRole] || roleConfigs.Guest;

        // Check configured quick-reply responses
        if (cfg.simulatedReplies && cfg.simulatedReplies[lower]) {
            return cfg.simulatedReplies[lower];
        }

        // Fuzzy matching
        if (lower.indexOf("paracetamol") !== -1 || lower.indexOf("stock") !== -1) {
            return "Paracetamol and essential pain relief medicines are in stock in our catalogue. You can view specifications, pack sizes, and batch details on the product page!";
        }
        if (lower.indexOf("order") !== -1 || lower.indexOf("track") !== -1) {
            if (userRole === "Admin") {
                return "You can view, search, and audit all orders across MediCart in the Incoming and Flagged Orders panels.";
            } else {
                return "Your orders are monitored from placement to delivery. Visit the Orders page to review live updates on your pharmacist verification.";
            }
        }
        if (lower.indexOf("pharmacist") !== -1 || lower.indexOf("review") !== -1 || lower.indexOf("prescription") !== -1) {
            return "Our pharmacist review process inspects clinical safety, correct dosing, and doctor authorization for every patient order before dispatch.";
        }
        if (lower.indexOf("flag") !== -1) {
            return "Flagged orders require manual review by an admin pharmacist due to high dosage or high potency categories.";
        }
        if (lower.indexOf("hello") !== -1 || lower.indexOf("hi") !== -1 || lower.indexOf("hey") !== -1) {
            return "Hello. I am Baymax, your personal healthcare companion. How may I assist you with your health and medicine needs today?";
        }

        // Default companion fallback
        return "I am Baymax, your personal healthcare companion. I am here to help answer questions about MediCart's medicines, order verification, and pharmacy services!";
    }

    // Initialize once DOM is ready
    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", initWidget);
    } else {
        initWidget();
    }

    // Expose global controller for testing/verification
    window.MediCartBaymax = {
        open: openPanel,
        close: closePanel,
        toggle: function () {
            if (isOpen) closePanel();
            else openPanel();
        },
        sendMessage: handleUserSend,
        getRole: function () { return userRole; }
    };
})(window, document);
