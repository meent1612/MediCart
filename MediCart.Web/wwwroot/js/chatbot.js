/**
 * MediCart — Baymax Floating Chatbot Widget
 * Role-aware assistant. Every answer comes from POST /api/chat/message.
 * If the backend fails, an honest error is shown — never canned answers.
 */
(function (window, document) {
    "use strict";

    var widgetEl = null;
    var launcherBtn = null;
    var unreadBadgeEl = null;
    var panelEl = null;
    var closeBtn = null;
    var messagesEl = null;
    var typingEl = null;
    var chipsEl = null;
    var formEl = null;
    var inputEl = null;
    var sendBtn = null;

    var userRole = "Guest";
    var conversationHistory = [];
    var isOpen = false;
    var isSending = false;

    var MAX_HISTORY = 20;
    var REQUEST_TIMEOUT_MS = 45000;

    // Greetings and quick-reply chips per role.
    // Chips only ask for things the server-side tools for that role can answer.
    var roleConfigs = {
        Admin: {
            greeting: "Hi, need a hand with the admin panel?",
            chips: [
                "What needs attention today?",
                "How many orders are flagged?",
                "Which medicines are low on stock?"
            ]
        },
        Customer: {
            greeting: "Hi, I'm Baymax — ask me about medicines, your order, or how MediCart works.",
            chips: [
                "Where is my latest order?",
                "Find medicines under ৳100 in stock",
                "Show my recent orders"
            ]
        },
        Guest: {
            greeting: "Hi, I'm Baymax — I can help you register, understand how MediCart works, or write a Contact Us message.",
            chips: [
                "How do I register?",
                "How does MediCart work?",
                "Help me write a Contact Us message"
            ]
        }
    };

    function initWidget() {
        widgetEl = document.getElementById("baymaxChatWidget");
        if (!widgetEl) return;

        launcherBtn = document.getElementById("baymaxLauncher");
        unreadBadgeEl = document.getElementById("baymaxUnreadBadge");
        panelEl = document.getElementById("baymaxChatPanel");
        closeBtn = document.getElementById("baymaxCloseBtn");
        messagesEl = document.getElementById("baymaxMessages");
        typingEl = document.getElementById("baymaxTyping");
        chipsEl = document.getElementById("baymaxChips");
        formEl = document.getElementById("baymaxForm");
        inputEl = document.getElementById("baymaxInput");
        sendBtn = document.getElementById("baymaxSendBtn");

        userRole = widgetEl.getAttribute("data-role") || "Guest";
        if (!roleConfigs[userRole]) {
            userRole = "Guest";
        }

        initGreetingAndChips();
        bindEvents();
        initProactiveBadge();
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
    // Greeting, chips, panel state
    // --------------------------------------------------------------------------
    function initGreetingAndChips() {
        var cfg = roleConfigs[userRole] || roleConfigs.Guest;

        if (messagesEl && messagesEl.querySelectorAll(".baymax-bubble").length === 0) {
            appendMessage("bot", cfg.greeting, { skipHistory: true });
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
            var text = inputEl ? inputEl.value.trim() : "";
            if (text) {
                handleUserSend(text);
            }
        });
    }

    // --------------------------------------------------------------------------
    // Rendering bot replies (bold + separate cards for list items)
    // Uses DOM text nodes only — never innerHTML — so reply text can't inject HTML.
    // --------------------------------------------------------------------------

    // Turns "**bold**" into <strong>; any stray "**" is dropped.
    function appendInline(parent, text) {
        var parts = String(text).split(/(\*\*[^*]+\*\*)/g);

        parts.forEach(function (part) {
            if (!part) return;

            var boldMatch = part.match(/^\*\*([^*]+)\*\*$/);
            if (boldMatch) {
                var strong = document.createElement("strong");
                strong.textContent = boldMatch[1];
                parent.appendChild(strong);
            } else {
                parent.appendChild(document.createTextNode(part.replace(/\*\*/g, "")));
            }
        });
    }

    // Splits a reply into paragraphs and list items.
    function parseBotText(rawText) {
        var text = String(rawText || "").replace(/\r\n/g, "\n").trim();

        // Some replies put a whole numbered list on a single line: "... 1. A 2. B 3. C"
        if (/(^|\s)1\.\s/.test(text) && /\s2\.\s/.test(text)) {
            text = text.replace(/\s+(?=\d{1,2}\.\s)/g, "\n");
        }

        var blocks = [];

        text.split("\n").forEach(function (line) {
            var trimmed = line.trim();
            if (!trimmed) return;

            var numbered = trimmed.match(/^(\d{1,2})[.)]\s+(.+)$/);
            if (numbered) {
                blocks.push({ type: "item", number: numbered[1], text: numbered[2] });
                return;
            }

            var bullet = trimmed.match(/^[-•]\s+(.+)$/);
            if (bullet) {
                blocks.push({ type: "item", number: "", text: bullet[1] });
                return;
            }

            blocks.push({ type: "para", text: trimmed });
        });

        return blocks;
    }

    // One list item = one card: number badge, bold title, details underneath.
    // "**Name** – ৳5.00 – 1 strip – 70 in stock" => title "Name", details "৳5.00 · 1 strip · 70 in stock"
    function buildItem(block) {
        var item = document.createElement("div");
        item.className = "baymax-item";

        if (block.number) {
            var num = document.createElement("span");
            num.className = "baymax-item__num";
            num.textContent = block.number;
            item.appendChild(num);
        }

        var body = document.createElement("div");
        body.className = "baymax-item__body";

        var parts = block.text.split(/\s+[–—-]\s+/);

        var title = document.createElement("div");
        title.className = "baymax-item__title";
        appendInline(title, parts[0]);
        body.appendChild(title);

        if (parts.length > 1) {
            var meta = document.createElement("div");
            meta.className = "baymax-item__meta";
            appendInline(meta, parts.slice(1).join(" · "));
            body.appendChild(meta);
        }

        item.appendChild(body);
        return item;
    }

    function renderBotContent(container, rawText) {
        var blocks = parseBotText(rawText);

        if (blocks.length === 0) {
            container.textContent = String(rawText || "");
            return;
        }

        blocks.forEach(function (block) {
            if (block.type === "item") {
                container.appendChild(buildItem(block));
                return;
            }

            var p = document.createElement("p");
            p.className = "baymax-para";
            appendInline(p, block.text);
            container.appendChild(p);
        });
    }

    // --------------------------------------------------------------------------
    // Messaging
    // --------------------------------------------------------------------------
    function appendMessage(sender, text, options) {
        options = options || {};
        if (!messagesEl) return;

        var bubble = document.createElement("div");
        bubble.className = "baymax-bubble " +
            (sender === "user" ? "baymax-bubble--user" : "baymax-bubble--bot");

        if (options.isError) {
            bubble.className += " baymax-bubble--error";
        }

        if (sender === "user" || options.isError) {
            bubble.textContent = text;
        } else {
            renderBotContent(bubble, text);
        }

        if (typingEl && typingEl.parentNode === messagesEl) {
            messagesEl.insertBefore(bubble, typingEl);
        } else {
            messagesEl.appendChild(bubble);
        }

        // Greetings and error messages are not part of the conversation the AI should see.
        if (!options.skipHistory) {
            conversationHistory.push({ sender: sender, text: text });
            if (conversationHistory.length > MAX_HISTORY) {
                conversationHistory = conversationHistory.slice(-MAX_HISTORY);
            }
        }

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

    function setSending(flag) {
        isSending = flag;
        if (sendBtn) sendBtn.disabled = flag;
    }

    function friendlyError(err) {
        if (err && err.name === "AbortError") {
            return "That took too long. Please try again.";
        }
        if (err && err.status === 429) {
            return "You're sending messages too quickly. Please wait a moment and try again.";
        }
        if (err && err.status === 403) {
            return "This assistant isn't available for your account.";
        }
        if (err && err.status >= 500) {
            return err.message || "The assistant is unavailable right now. Please try again in a moment.";
        }
        return "I couldn't reach the assistant. Check your connection and try again.";
    }

    function handleUserSend(text) {
        text = (text || "").trim();
        if (!text || isSending) return;

        appendMessage("user", text);

        if (inputEl) {
            inputEl.value = "";
        }

        setSending(true);
        showTyping();

        // The server decides the user's role from the login cookie; none is sent here.
        var payload = {
            message: text,
            history: conversationHistory.map(function (h) {
                return { sender: h.sender, text: h.text };
            })
        };

        var controller = typeof AbortController !== "undefined" ? new AbortController() : null;
        var timer = controller
            ? setTimeout(function () { controller.abort(); }, REQUEST_TIMEOUT_MS)
            : null;

        fetch("/api/chat/message", {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "X-Requested-With": "XMLHttpRequest"
            },
            body: JSON.stringify(payload),
            signal: controller ? controller.signal : undefined
        })
        .then(function (res) {
            return res.json().catch(function () { return {}; }).then(function (data) {
                if (!res.ok) {
                    throw { status: res.status, message: data && data.error };
                }
                return data;
            });
        })
        .then(function (data) {
            var reply = data && data.reply;
            if (!reply) {
                throw { status: 502, message: null };
            }
            hideTyping();
            appendMessage("bot", reply);
        })
        .catch(function (err) {
            hideTyping();
            appendMessage("bot", friendlyError(err), { isError: true, skipHistory: true });
        })
        .then(function () {
            if (timer) clearTimeout(timer);
            hideTyping();
            setSending(false);
        });
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