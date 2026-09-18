(function () {
    "use strict";

    var grid = document.getElementById("productGrid");
    if (!grid) return;

    var cards = Array.prototype.slice.call(grid.querySelectorAll(".product-card"));
    var noResults = document.getElementById("noResults");
    var resultsCount = document.getElementById("resultsCount");
    var priceRange = document.getElementById("priceRange");
    var priceRangeValue = document.getElementById("priceRangeValue");
    var sortSelect = document.getElementById("sortSelect");
    var clearFiltersBtn = document.getElementById("clearFilters");
    var browseSearchForm = document.getElementById("browseSearchForm");
    var browseSearchInput = document.getElementById("browseSearchInput");
    var browseSearchClear = document.getElementById("browseSearchClear");
    var medicineData = JSON.parse(document.getElementById("medicineData").textContent);

    function getChecked(groupName) {
        var groups = document.querySelectorAll('[data-filter-group="' + groupName + '"]');
        var values = [];
        groups.forEach(function (group) {
            Array.prototype.slice.call(group.querySelectorAll("input:checked")).forEach(function (i) {
                values.push(i.value);
            });
        });
        return values;
    }

    function applyFilters() {
        var productTypeIds = getChecked("productType");
        var categoryIds = getChecked("category");
        var subCategoryIds = getChecked("subCategory");
        var maxPrice = parseFloat(priceRange.value);
        var searchTerm = (browseSearchInput ? browseSearchInput.value : "").trim().toLowerCase();
        var visibleCount = 0;

        cards.forEach(function (card) {
            var cardName = (card.dataset.name || "").toLowerCase();
            var cardProductTypeId = card.dataset.productTypeId;
            var cardCategoryId = card.dataset.categoryId;
            var cardSubCategoryId = card.dataset.subcategoryId;
            var cardPrice = parseFloat(card.dataset.price);

            var matchesProductType = productTypeIds.length === 0 || productTypeIds.indexOf(cardProductTypeId) !== -1;
            var matchesCategory = categoryIds.length === 0 || categoryIds.indexOf(cardCategoryId) !== -1;
            var matchesSubCategory = subCategoryIds.length === 0 ||
                (cardSubCategoryId !== "" && subCategoryIds.indexOf(cardSubCategoryId) !== -1);
            var matchesPrice = cardPrice <= maxPrice;
            var matchesSearch = !searchTerm || cardName.indexOf(searchTerm) !== -1;

            var visible = matchesProductType && matchesCategory && matchesSubCategory && matchesPrice && matchesSearch;
            card.style.display = visible ? "" : "none";
            if (visible) visibleCount++;
        });

        resultsCount.textContent = visibleCount + (visibleCount === 1 ? " result" : " results");
        noResults.hidden = visibleCount !== 0;
        applySort();
    }

    function applySort() {
        var visibleCards = cards.filter(function (c) { return c.style.display !== "none"; });
        var sortBy = sortSelect.value;

        visibleCards.sort(function (a, b) {
            if (sortBy === "price-asc") return parseFloat(a.dataset.price) - parseFloat(b.dataset.price);
            if (sortBy === "price-desc") return parseFloat(b.dataset.price) - parseFloat(a.dataset.price);
            return a.dataset.name.localeCompare(b.dataset.name); // default: name-asc
        });

        visibleCards.forEach(function (card) { grid.appendChild(card); });
    }

    document.querySelectorAll(".filter-check input").forEach(function (input) {
        input.addEventListener("change", applyFilters);
    });

    priceRange.addEventListener("input", function () {
        priceRangeValue.textContent = "\u09F3" + priceRange.value;
        applyFilters();
    });

    sortSelect.addEventListener("change", applySort);

    function updateSearchUrl(term) {
        var currentParams = new URLSearchParams(window.location.search);
        if (term) {
            currentParams.set("search", term);
        } else {
            currentParams.delete("search");
        }
        currentParams.delete("notFound");
        var newQuery = currentParams.toString();
        var newUrl = window.location.pathname + (newQuery ? "?" + newQuery : "");
        window.history.replaceState(null, "", newUrl);
    }

    if (browseSearchInput) {
        browseSearchInput.addEventListener("input", function () {
            if (browseSearchClear) {
                browseSearchClear.hidden = !browseSearchInput.value.trim();
            }
            applyFilters();
        });

        browseSearchInput.addEventListener("change", function () {
            updateSearchUrl(browseSearchInput.value.trim());
        });

        if (browseSearchForm) {
            browseSearchForm.addEventListener("submit", function (e) {
                e.preventDefault();
                var term = browseSearchInput.value.trim();
                updateSearchUrl(term);
                applyFilters();
            });
        }

        if (browseSearchClear) {
            browseSearchClear.addEventListener("click", function () {
                browseSearchInput.value = "";
                browseSearchClear.hidden = true;
                updateSearchUrl("");
                applyFilters();
                browseSearchInput.focus();
            });
        }
    }

    clearFiltersBtn.addEventListener("click", function () {
        document.querySelectorAll(".filter-check input:checked").forEach(function (i) { i.checked = false; });
        priceRange.value = priceRange.max;
        priceRangeValue.textContent = "\u09F3" + priceRange.max;
        if (browseSearchInput) {
            browseSearchInput.value = "";
            if (browseSearchClear) browseSearchClear.hidden = true;
        }
        var currentParams = new URLSearchParams(window.location.search);
        currentParams.delete("search");
        currentParams.delete("category");
        currentParams.delete("productType");
        currentParams.delete("categoryId");
        currentParams.delete("type");
        currentParams.delete("notFound");
        var newQuery = currentParams.toString();
        var newUrl = window.location.pathname + (newQuery ? "?" + newQuery : "");
        window.history.replaceState(null, "", newUrl);
        applyFilters();
    });

    /* ----- Add to cart from card (real server call) ----- */

    var isCustomer = grid.dataset.isCustomer === "true";
    var isAdmin = grid.dataset.isAdmin === "true";
    var loginUrl = grid.dataset.loginUrl || "/Identity/Account/Login";

    function getAntiForgeryToken() {
        var input = document.querySelector('input[name="__RequestVerificationToken"]');
        return input ? input.value : "";
    }

    function redirectToLogin() {
        var returnUrl = window.location.pathname + window.location.search;
        window.location.href = loginUrl + "?ReturnUrl=" + encodeURIComponent(returnUrl);
    }

    function addToCartOnServer(medicineId, quantity) {
        var body = new URLSearchParams();
        body.set("medicineId", medicineId);
        body.set("quantity", quantity);
        body.set("__RequestVerificationToken", getAntiForgeryToken());

        return fetch("/Cart/Add", {
            method: "POST",
            body: body
        }).then(function (res) {
            if (res.redirected || res.status === 401 || res.status === 403) {
                redirectToLogin();
                return Promise.reject(null);
            }

            return res.json().then(function (data) {
                if (!res.ok) {
                    return Promise.reject(data.error || "Could not add this to your cart.");
                }
                return data;
            });
        });
    }

    function updateCartBadge(cartItemCount) {
        var cartButton = document.querySelector(".cart-button");
        var cartBadge = document.querySelector(".cart-button__badge");

        if (cartBadge) {
            cartBadge.textContent = cartItemCount;
            cartBadge.classList.add("is-visible");
        }

        if (cartButton) {
            cartButton.classList.remove("is-bumped");
            void cartButton.offsetWidth;
            cartButton.classList.add("is-bumped");
        }
    }

    function showToast(message) {
        var toast = document.getElementById("toast");
        if (!toast) return;
        toast.querySelector(".toast__text").textContent = message;
        toast.classList.add("is-visible");
        clearTimeout(showToast._t);
        showToast._t = setTimeout(function () { toast.classList.remove("is-visible"); }, 2200);
    }

    function showSearchNoticeToast(message) {
        var toast = document.getElementById("toast");
        if (!toast) return;
        toast.classList.add("toast--search-notice");
        var icon = toast.querySelector("svg");
        var prevSvg = icon ? icon.innerHTML : "";
        if (icon) {
            icon.innerHTML = '<circle cx="12" cy="12" r="10"></circle><line x1="12" y1="8" x2="12" y2="12"></line><line x1="12" y1="16" x2="12.01" y2="16"></line>';
        }
        toast.querySelector(".toast__text").textContent = message;
        toast.classList.add("is-visible");
        clearTimeout(showSearchNoticeToast._t);
        showSearchNoticeToast._t = setTimeout(function () {
            toast.classList.remove("is-visible");
            setTimeout(function () {
                toast.classList.remove("toast--search-notice");
                if (icon && prevSvg) {
                    icon.innerHTML = prevSvg;
                }
            }, 300);
        }, 3600);
    }

    grid.addEventListener("click", function (e) {
        var addBtn = e.target.closest(".btn-add");
        if (addBtn) {
            var medicineId = addBtn.dataset.id;
            if (!medicineId) return;

            window.MediCartCart.add({
                medicineId: medicineId,
                quantity: 1,
                button: addBtn,
                onSuccess: function () {
                    addBtn.classList.add("added");
                    setTimeout(function () {
                        addBtn.classList.remove("added");
                    }, 350);
                }
            });

            return;
        }

        var detailsBtn = e.target.closest(".view-details-btn");
        if (detailsBtn) {
            openModal(detailsBtn.dataset.id);
        }
    });

    /* ----- Modal ----- */
    var overlay = document.getElementById("modalOverlay");
    var modalClose = document.getElementById("modalClose");
    var qtyInput = document.getElementById("modalQtyInput");
    var qtyMinus = document.getElementById("modalQtyMinus");
    var qtyPlus = document.getElementById("modalQtyPlus");
    var addToCartBtn = document.getElementById("modalAddToCart");
    var currentMedicine = null;

    var NOT_AVAILABLE = "Not available yet";

    function severityClass(severity) {
        var s = (severity || "").toLowerCase();
        if (s === "mild") return "side-effect--mild";
        if (s === "moderate") return "side-effect--moderate";
        if (s === "high" || s === "severe") return "side-effect--high";
        return "side-effect--mild";
    }

    function formatExpiry(dateStr) {
        var parts = dateStr.split("-");
        var d = new Date(Date.UTC(parseInt(parts[0], 10), parseInt(parts[1], 10) - 1, parseInt(parts[2], 10)));
        return d.toLocaleDateString("en-GB", { day: "2-digit", month: "short", year: "numeric", timeZone: "UTC" });
    }

    function openModal(id) {
        var med = medicineData.find(function (m) { return String(m.Id) === String(id); });
        if (!med) return;
        currentMedicine = med;

        document.getElementById("modalTitle").textContent = med.Name;
        document.getElementById("modalComposition").textContent = med.Composition;
        document.getElementById("modalManufacturer").textContent = med.Manufacturer;
        document.getElementById("modalForm").textContent = med.ProductType;
        document.getElementById("modalCategory").textContent = med.Category;

        var subCategoryEl = document.getElementById("modalSubCategory");
        subCategoryEl.textContent = med.SubCategory || "None";
        subCategoryEl.classList.toggle("modal__value--muted", !med.SubCategory);

        var unitEl = document.getElementById("modalUnit");
        unitEl.textContent = med.Unit || NOT_AVAILABLE;
        unitEl.classList.toggle("modal__value--muted", !med.Unit);

        document.getElementById("modalStock").textContent = med.Stock > 0 ? (med.Stock + " units") : "Out of stock";
        document.getElementById("modalPrice").textContent = "\u09F3" + med.Price;

        var expiryEl = document.getElementById("modalExpiry");
        expiryEl.classList.remove("modal__value--muted", "modal__value--warning", "modal__value--danger");
        if (med.ExpiryDate) {
            var expiryDate = new Date(med.ExpiryDate + "T00:00:00Z");
            var today = new Date();
            var daysLeft = Math.floor((expiryDate - today) / (1000 * 60 * 60 * 24));

            expiryEl.textContent = formatExpiry(med.ExpiryDate);
            if (daysLeft < 0) {
                expiryEl.classList.add("modal__value--danger");
            } else if (daysLeft <= 90) {
                expiryEl.classList.add("modal__value--warning");
            }
        } else {
            expiryEl.textContent = NOT_AVAILABLE;
            expiryEl.classList.add("modal__value--muted");
        }

        document.getElementById("modalDescription").textContent = med.Description;

        var dosageEl = document.getElementById("modalDosage");
        dosageEl.textContent = med.Dosage || NOT_AVAILABLE;
        dosageEl.classList.toggle("modal__section-text--muted", !med.Dosage);

        var rxBadge = document.getElementById("modalRxBadge");
        rxBadge.hidden = !med.RequiresRx;
        rxBadge.textContent = med.RequiresRx ? "Rx" : "OTC";

        var sideEffectsBox = document.getElementById("modalSideEffects");
        sideEffectsBox.innerHTML = "";
        (med.SideEffects || []).forEach(function (se) {
            var span = document.createElement("span");
            span.textContent = se.Effect;
            span.className = severityClass(se.Severity);
            sideEffectsBox.appendChild(span);
        });

        if (qtyInput) {
            qtyInput.value = 1;
            qtyInput.max = med.Stock > 0 ? med.Stock : 1;
        }
        if (addToCartBtn) {
            addToCartBtn.textContent = "Add to cart";
            addToCartBtn.classList.remove("added");
            addToCartBtn.disabled = isAdmin || med.Stock <= 0;
        }

        overlay.hidden = false;
        document.body.style.overflow = "hidden";
    }

    function closeModal() {
        overlay.hidden = true;
        document.body.style.overflow = "";
        currentMedicine = null;
    }

    modalClose.addEventListener("click", closeModal);
    overlay.addEventListener("click", function (e) {
        if (e.target === overlay) closeModal();
    });
    document.addEventListener("keydown", function (e) {
        if (e.key === "Escape" && !overlay.hidden) closeModal();
    });

    if (qtyMinus && qtyInput) {
        qtyMinus.addEventListener("click", function () {
            var val = parseInt(qtyInput.value, 10) || 1;
            if (val > 1) qtyInput.value = val - 1;
        });
    }

    if (qtyPlus && qtyInput) {
        qtyPlus.addEventListener("click", function () {
            var val = parseInt(qtyInput.value, 10) || 1;
            var max = parseInt(qtyInput.max, 10) || 99;
            if (val < max) qtyInput.value = val + 1;
        });
    }

    if (qtyInput) {
        qtyInput.addEventListener("change", function () {
            var max = parseInt(qtyInput.max, 10) || 99;
            var val = parseInt(qtyInput.value, 10) || 1;
            if (val < 1) val = 1;
            if (val > max) val = max;
            qtyInput.value = val;
        });
    }

    if (addToCartBtn) {
        addToCartBtn.addEventListener("click", function () {
            if (!currentMedicine) return;

            var qty = qtyInput ? (parseInt(qtyInput.value, 10) || 1) : 1;

            window.MediCartCart.add({
                medicineId: currentMedicine.Id,
                quantity: qty,
                button: addToCartBtn,
                onSuccess: function () {
                    addToCartBtn.textContent = "Added";
                    addToCartBtn.classList.add("added");
                    setTimeout(closeModal, 500);
                }
            });
        });
    }

    var adminModalBtn = document.querySelector(".btn-add-cart--admin");
    if (adminModalBtn) {
        adminModalBtn.addEventListener("click", function () {
            window.MediCartCart.showToast("Admins cannot place orders");
        });
    }

    /* Pre-select filters from URL parameters (?category=..., ?productType=..., ?categoryId=..., ?type=..., ?search=...) */
    var urlParams = new URLSearchParams(window.location.search);
    var categoryParam = (urlParams.get("category") || "").toLowerCase().trim();
    var categoryIdParam = (urlParams.get("categoryId") || "").trim();
    var productTypeParam = (urlParams.get("productType") || urlParams.get("type") || "").toLowerCase().trim();
    var searchParam = (urlParams.get("search") || "").trim();
    var openDetailsParam = (urlParams.get("openDetails") || "").trim();
    var notFoundParam = (urlParams.get("notFound") || "").trim();

    if (categoryParam || categoryIdParam) {
        document.querySelectorAll('[data-filter-group="category"] input, [data-filter-group="subCategory"] input').forEach(function (input) {
            var name = (input.dataset.name || "").toLowerCase().trim();
            var val = (input.value || "").trim();
            if ((categoryParam && name === categoryParam) || (categoryIdParam && val === categoryIdParam)) {
                input.checked = true;
            }
        });
    }

    if (productTypeParam) {
        document.querySelectorAll('[data-filter-group="productType"] input').forEach(function (input) {
            var name = (input.dataset.name || "").toLowerCase().trim();
            var val = (input.value || "").trim();
            if (name === productTypeParam || val === productTypeParam) {
                input.checked = true;
            }
        });
    }

    if (searchParam && browseSearchInput) {
        browseSearchInput.value = searchParam;
        if (browseSearchClear) browseSearchClear.hidden = false;
    }

    /* initial render */
    priceRangeValue.textContent = "\u09F3" + priceRange.value;
    applyFilters();

    /* Check openDetails query param from FIX 2 */
    if (openDetailsParam) {
        openModal(openDetailsParam);
        var cleanParams = new URLSearchParams(window.location.search);
        cleanParams.delete("openDetails");
        var cleanQuery = cleanParams.toString();
        var cleanUrl = window.location.pathname + (cleanQuery ? "?" + cleanQuery : "");
        window.history.replaceState(null, "", cleanUrl);
    }

    /* Check notFound query param from FIX 2 */
    if (notFoundParam) {
        var term = searchParam;
        var msg = term
            ? "No medicine named \u2018" + term + "\u2019 found \u2014 browse the full catalogue below"
            : "No matching medicine found \u2014 browse the full catalogue below";
        showSearchNoticeToast(msg);
        var cleanParams = new URLSearchParams(window.location.search);
        cleanParams.delete("notFound");
        var cleanQuery = cleanParams.toString();
        var cleanUrl = window.location.pathname + (cleanQuery ? "?" + cleanQuery : "");
        window.history.replaceState(null, "", cleanUrl);
    }
})();