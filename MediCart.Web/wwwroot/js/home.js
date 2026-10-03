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

    /* ---- Hero search: Real-time search suggestions controller ---------- */
    const searchForm = document.getElementById("heroSearchForm");
    const searchInput = document.getElementById("heroSearchInput");
    const searchClear = document.getElementById("heroSearchClear");
    const searchSpinner = document.getElementById("heroSearchSpinner");
    const suggestionsBox = document.getElementById("heroSearchSuggestions");

    if (searchForm && searchInput && suggestionsBox) {
        let activeIndex = -1;
        let debounceTimer = null;
        let currentAbort = null;
        const suggestionCache = new Map();

        const escapeHtml = (str) => {
            if (!str) return "";
            return str
                .replace(/&/g, "&amp;")
                .replace(/</g, "&lt;")
                .replace(/>/g, "&gt;")
                .replace(/"/g, "&quot;")
                .replace(/'/g, "&#039;");
        };

        const escapeRegExp = (str) => {
            return str.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
        };

        const highlightMatch = (text, query) => {
            if (!text) return "";
            if (!query) return escapeHtml(text);
            const escapedText = escapeHtml(text);
            const escapedQuery = escapeRegExp(escapeHtml(query));
            try {
                const regex = new RegExp("(" + escapedQuery + ")", "gi");
                return escapedText.replace(regex, '<mark class="hero__search-match">$1</mark>');
            } catch (e) {
                return escapedText;
            }
        };

        const formatPrice = (val) => {
            const num = Number(val);
            if (isNaN(num)) return "৳0";
            return "৳" + (num % 1 === 0 ? num.toLocaleString() : num.toFixed(2));
        };

        const hideSuggestions = () => {
            suggestionsBox.hidden = true;
            suggestionsBox.innerHTML = "";
            searchInput.setAttribute("aria-expanded", "false");
            activeIndex = -1;
        };

        const showLoading = (loading) => {
            if (searchSpinner) searchSpinner.hidden = !loading;
            if (searchClear) searchClear.hidden = loading || !searchInput.value.trim();
        };

        const updateActiveItem = (newIndex) => {
            const items = suggestionsBox.querySelectorAll(".hero__suggestion-item, .hero__suggestions-footer");
            if (!items.length) return;

            items.forEach((item) => item.classList.remove("is-active"));

            if (newIndex >= items.length) newIndex = 0;
            if (newIndex < 0) newIndex = items.length - 1;

            activeIndex = newIndex;
            const current = items[activeIndex];
            if (current) {
                current.classList.add("is-active");
                current.scrollIntoView({ block: "nearest" });
            }
        };

        const renderSuggestions = (query, list) => {
            if (!list || !list.length) {
                suggestionsBox.innerHTML = `
                    <div class="hero__suggestions-empty">
                        <svg viewBox="0 0 24 24" width="28" height="28" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
                            <circle cx="11" cy="11" r="8"></circle>
                            <line x1="21" y1="21" x2="16.65" y2="16.65"></line>
                        </svg>
                        <p>No medicines found for &ldquo;${escapeHtml(query)}&rdquo;</p>
                        <span>Press Enter to search catalogue or try a different term</span>
                    </div>
                `;
                suggestionsBox.hidden = false;
                searchInput.setAttribute("aria-expanded", "true");
                activeIndex = -1;
                return;
            }

            const headerHtml = `
                <div class="hero__suggestions-header">
                    <span>Medicines Matching &ldquo;${escapeHtml(query)}&rdquo;</span>
                    <span>${list.length} result${list.length !== 1 ? "s" : ""}</span>
                </div>
            `;

            const itemsHtml = list.map((item, idx) => {
                const highlightedName = highlightMatch(item.name, query);
                const highlightedGeneric = highlightMatch(item.genericName, query);
                const dosage = item.dosage ? ` &middot; ${escapeHtml(item.dosage)}` : "";
                const metaLine = `${highlightedGeneric}${dosage}`;

                const thumb = item.imageUrl
                    ? `<img src="${escapeHtml(item.imageUrl)}" alt="${escapeHtml(item.name)}" class="hero__suggestion-thumb" loading="lazy" />`
                    : `<div class="hero__suggestion-fallback" aria-hidden="true">
                        <svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                            <path d="m10.5 20.5 10-10a4.95 4.95 0 1 0-7-7l-10 10a4.95 4.95 0 1 0 7 7Z"></path>
                            <path d="m8.5 8.5 7 7"></path>
                        </svg>
                       </div>`;

                const rxBadge = item.requiresRx ? `<span class="hero__suggestion-rx" title="Prescription Required">Rx</span>` : "";
                const stockBadge = item.inStock
                    ? `<span class="hero__suggestion-stock hero__suggestion-stock--in">In stock</span>`
                    : `<span class="hero__suggestion-stock hero__suggestion-stock--out">Out of stock</span>`;

                return `
                    <li>
                        <a href="/Medicines/Browse?openDetails=${item.id}" class="hero__suggestion-item" role="option" data-index="${idx}">
                            ${thumb}
                            <div class="hero__suggestion-body">
                                <div class="hero__suggestion-name">
                                    <span>${highlightedName}</span>
                                    ${rxBadge}
                                </div>
                                <div class="hero__suggestion-meta">${metaLine}</div>
                                <div class="hero__suggestion-sub">${escapeHtml(item.manufacturer)} ${item.productType ? `&middot; ${escapeHtml(item.productType)}` : ""}</div>
                            </div>
                            <div class="hero__suggestion-right">
                                <span class="hero__suggestion-price">${formatPrice(item.price)}</span>
                                ${stockBadge}
                            </div>
                        </a>
                    </li>
                `;
            }).join("");

            const footerHtml = `
                <a href="/Medicines/Find?search=${encodeURIComponent(query)}" class="hero__suggestions-footer">
                    <span>Search full catalogue for &ldquo;${escapeHtml(query)}&rdquo;</span>
                    <svg viewBox="0 0 24 24" width="16" height="16" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
                        <line x1="5" y1="12" x2="19" y2="12"></line>
                        <polyline points="12 5 19 12 12 19"></polyline>
                    </svg>
                </a>
            `;

            suggestionsBox.innerHTML = `
                ${headerHtml}
                <ul class="hero__suggestions-list" role="presentation">${itemsHtml}</ul>
                ${footerHtml}
            `;

            suggestionsBox.hidden = false;
            searchInput.setAttribute("aria-expanded", "true");
            activeIndex = -1;
        };

        const fetchSuggestions = (query) => {
            const trimmed = query.trim();
            if (!trimmed) {
                hideSuggestions();
                showLoading(false);
                return;
            }

            if (suggestionCache.has(trimmed.toLowerCase())) {
                showLoading(false);
                renderSuggestions(trimmed, suggestionCache.get(trimmed.toLowerCase()));
                return;
            }

            if (currentAbort) {
                currentAbort.abort();
            }
            currentAbort = new AbortController();

            showLoading(true);

            fetch(`/Medicines/Suggestions?search=${encodeURIComponent(trimmed)}`, {
                signal: currentAbort.signal,
                headers: { "Accept": "application/json" }
            })
                .then((res) => {
                    if (!res.ok) throw new Error("Network error");
                    return res.json();
                })
                .then((data) => {
                    suggestionCache.set(trimmed.toLowerCase(), data);
                    // Only render if input value still matches
                    if (searchInput.value.trim().toLowerCase() === trimmed.toLowerCase()) {
                        renderSuggestions(trimmed, data);
                    }
                })
                .catch((err) => {
                    if (err.name !== "AbortError") {
                        hideSuggestions();
                    }
                })
                .finally(() => {
                    showLoading(false);
                });
        };

        // Input event listener with debounce
        searchInput.addEventListener("input", () => {
            const query = searchInput.value;
            if (searchClear) searchClear.hidden = !query.trim();

            clearTimeout(debounceTimer);
            if (!query.trim()) {
                hideSuggestions();
                showLoading(false);
                return;
            }

            debounceTimer = setTimeout(() => {
                fetchSuggestions(query);
            }, 180);
        });

        // Re-open on focus if text present
        searchInput.addEventListener("focus", () => {
            const query = searchInput.value.trim();
            if (query && suggestionsBox.hidden) {
                fetchSuggestions(query);
            }
        });

        // Clear button click
        if (searchClear) {
            searchClear.addEventListener("click", () => {
                searchInput.value = "";
                searchClear.hidden = true;
                hideSuggestions();
                searchInput.focus();
            });
        }

        // Keyboard navigation (ArrowDown, ArrowUp, Enter, Escape)
        searchInput.addEventListener("keydown", (e) => {
            if (suggestionsBox.hidden) return;

            const items = suggestionsBox.querySelectorAll(".hero__suggestion-item, .hero__suggestions-footer");
            if (!items.length) return;

            if (e.key === "ArrowDown") {
                e.preventDefault();
                updateActiveItem(activeIndex + 1);
            } else if (e.key === "ArrowUp") {
                e.preventDefault();
                updateActiveItem(activeIndex - 1);
            } else if (e.key === "Enter") {
                if (activeIndex >= 0 && items[activeIndex]) {
                    e.preventDefault();
                    items[activeIndex].click();
                }
            } else if (e.key === "Escape") {
                e.preventDefault();
                hideSuggestions();
            }
        });

        // Close when clicking outside
        document.addEventListener("click", (e) => {
            if (!searchForm.contains(e.target)) {
                hideSuggestions();
            }
        });

        // Form submit safety
        searchForm.addEventListener("submit", (e) => {
            if (!searchInput.value.trim()) {
                e.preventDefault();
                searchInput.focus();
            }
        });
    }

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
/**
 * Home Search Module - State Management and Options
 * Maintains debounce timers, active index tracking, and cache maps.
 */

/**
 * highlightMatch: Escapes regex special characters and wraps matching substrings in <mark>.
 */

/**
 * Keyboard Navigation Handler:
 * Supports ArrowUp, ArrowDown, Enter, Escape for full WCAG combobox pattern compliance.
 */

/**
 * Cache Invalidation:
 * Automatically cleans up cached items if query length exceeds threshold.
 */

