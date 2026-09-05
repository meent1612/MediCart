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
        var visibleCount = 0;

        cards.forEach(function (card) {
            var cardProductTypeId = card.dataset.productTypeId;
            var cardCategoryId = card.dataset.categoryId;
            var cardSubCategoryId = card.dataset.subcategoryId;
            var cardPrice = parseFloat(card.dataset.price);

            var matchesProductType = productTypeIds.length === 0 || productTypeIds.indexOf(cardProductTypeId) !== -1;
            var matchesCategory = categoryIds.length === 0 || categoryIds.indexOf(cardCategoryId) !== -1;
            var matchesSubCategory = subCategoryIds.length === 0 ||
                (cardSubCategoryId !== "" && subCategoryIds.indexOf(cardSubCategoryId) !== -1);
            var matchesPrice = cardPrice <= maxPrice;

            var visible = matchesProductType && matchesCategory && matchesSubCategory && matchesPrice;
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

    clearFiltersBtn.addEventListener("click", function () {
        document.querySelectorAll(".filter-check input:checked").forEach(function (i) { i.checked = false; });
        priceRange.value = priceRange.max;
        priceRangeValue.textContent = "\u09F3" + priceRange.max;
        applyFilters();
    });

    /* ----- Add to cart from card (real server call) ----- */

    var isCustomer = grid.dataset.isCustomer === "true";
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

    grid.addEventListener("click", function (e) {
        var addBtn = e.target.closest(".btn-add");
        if (addBtn && !addBtn.disabled) {
            if (!isCustomer) {
                redirectToLogin();
                return;
            }

            var medicineId = addBtn.dataset.id;
            addBtn.disabled = true;

            addToCartOnServer(medicineId, 1).then(function (data) {
                updateCartBadge(data.cartItemCount);
                addBtn.classList.add("added");
                showToast("Added to cart");
                setTimeout(function () {
                    addBtn.classList.remove("added");
                    addBtn.disabled = false;
                }, 350);
            }).catch(function (errorMessage) {
                addBtn.disabled = false;
                if (errorMessage) showToast(errorMessage);
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
        // dateStr comes from a DateOnly, serialized as "yyyy-MM-dd"
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

        qtyInput.value = 1;
        qtyInput.max = med.Stock > 0 ? med.Stock : 1;
        addToCartBtn.textContent = "Add to cart";
        addToCartBtn.classList.remove("added");
        addToCartBtn.disabled = med.Stock <= 0;

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

    qtyMinus.addEventListener("click", function () {
        var val = parseInt(qtyInput.value, 10) || 1;
        if (val > 1) qtyInput.value = val - 1;
    });

    qtyPlus.addEventListener("click", function () {
        var val = parseInt(qtyInput.value, 10) || 1;
        var max = parseInt(qtyInput.max, 10) || 99;
        if (val < max) qtyInput.value = val + 1;
    });

    qtyInput.addEventListener("change", function () {
        var max = parseInt(qtyInput.max, 10) || 99;
        var val = parseInt(qtyInput.value, 10) || 1;
        if (val < 1) val = 1;
        if (val > max) val = max;
        qtyInput.value = val;
    });

    addToCartBtn.addEventListener("click", function () {
        if (!currentMedicine || addToCartBtn.disabled) return;

        if (!isCustomer) {
            redirectToLogin();
            return;
        }

        var qty = parseInt(qtyInput.value, 10) || 1;
        addToCartBtn.disabled = true;

        addToCartOnServer(currentMedicine.Id, qty).then(function (data) {
            updateCartBadge(data.cartItemCount);
            addToCartBtn.textContent = "Added";
            addToCartBtn.classList.add("added");
            showToast(qty === 1 ? "Added to cart" : qty + " items added to cart");
            setTimeout(closeModal, 500);
        }).catch(function (errorMessage) {
            addToCartBtn.disabled = false;
            if (errorMessage) showToast(errorMessage);
        });
    });

    /* initial render */
    priceRangeValue.textContent = "\u09F3" + priceRange.value;
    applyFilters();
})();