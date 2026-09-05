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
        var group = document.querySelector('[data-filter-group="' + groupName + '"]');
        if (!group) return [];
        return Array.prototype.slice.call(group.querySelectorAll("input:checked")).map(function (i) { return i.value; });
    }

    function applyFilters() {
        var types = getChecked("productType");
        var categories = getChecked("category");
        var tags = getChecked("useTag");
        var maxPrice = parseFloat(priceRange.value);
        var visibleCount = 0;

        cards.forEach(function (card) {
            var cardType = card.dataset.productType;
            var cardCategory = card.dataset.category;
            var cardTags = card.dataset.useTags ? card.dataset.useTags.split(",") : [];
            var cardPrice = parseFloat(card.dataset.price);

            var matchesType = types.length === 0 || types.indexOf(cardType) !== -1;
            var matchesCategory = categories.length === 0 || categories.indexOf(cardCategory) !== -1;
            var matchesTag = tags.length === 0 || tags.some(function (t) { return cardTags.indexOf(t) !== -1; });
            var matchesPrice = cardPrice <= maxPrice;

            var visible = matchesType && matchesCategory && matchesTag && matchesPrice;
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
            if (sortBy === "name-asc") return a.dataset.name.localeCompare(b.dataset.name);
            return parseInt(b.dataset.popularity, 10) - parseInt(a.dataset.popularity, 10);
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

    // Whether this visitor is logged in as a Customer, and where to send
    // them to log in if not. Set as data-* attributes on #productGrid
    // by Views/Medicines/Index.cshtml.
    var isCustomer = grid.dataset.isCustomer === "true";
    var loginUrl = grid.dataset.loginUrl || "/Identity/Account/Login";

    // The anti-forgery token CartController.Add requires.
    // Emitted on the page by @Html.AntiForgeryToken().
    function getAntiForgeryToken() {
        var input = document.querySelector('input[name="__RequestVerificationToken"]');
        return input ? input.value : "";
    }

    // Sends the visitor to the login page and brings them straight back
    // to this page (with their filters/scroll lost, but at the right URL)
    // after they log in.
    function redirectToLogin() {
        var returnUrl = window.location.pathname + window.location.search;
        window.location.href = loginUrl + "?ReturnUrl=" + encodeURIComponent(returnUrl);
    }

    // Calls POST /Cart/Add for real. Returns a Promise that resolves with
    // the server's JSON ({ newQuantity, newStockQuantity, cartItemCount })
    // or rejects with a plain error message string.
    function addToCartOnServer(medicineId, quantity) {
        var body = new URLSearchParams();
        body.set("medicineId", medicineId);
        body.set("quantity", quantity);
        body.set("__RequestVerificationToken", getAntiForgeryToken());

        return fetch("/Cart/Add", {
            method: "POST",
            body: body
        }).then(function (res) {
            // If the session expired mid-browse, ASP.NET Identity redirects
            // an unauthenticated request to the login page. fetch() follows
            // that redirect automatically, so we detect it here as a
            // fallback safety net (the isCustomer check above should
            // normally catch this before we ever get here).
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
            // force reflow so the animation can restart on repeated clicks
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
            // Guest clicking "+" — send to login, never touch the cart.
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

    function openModal(id) {
        var med = medicineData.find(function (m) { return String(m.Id) === String(id); });
        if (!med) return;
        currentMedicine = med;

        var NOT_AVAILABLE = "Not available yet";

        document.getElementById("modalTitle").textContent = med.Name;
        document.getElementById("modalComposition").textContent = med.Strength
            ? med.Composition + " " + med.Strength
            : med.Composition;
        document.getElementById("modalManufacturer").textContent = med.Manufacturer;

        var strengthEl = document.getElementById("modalStrength");
        strengthEl.textContent = med.Strength || NOT_AVAILABLE;
        strengthEl.classList.toggle("modal__value--muted", !med.Strength);

        document.getElementById("modalForm").textContent = med.ProductType;
        document.getElementById("modalCategory").textContent = med.Category;
        document.getElementById("modalStock").textContent = med.Stock > 0 ? (med.Stock + " units") : "Out of stock";
        document.getElementById("modalPrice").textContent = "\u09F3" + med.Price;
        document.getElementById("modalAbout").textContent = med.About;

        var dosageEl = document.getElementById("modalDosage");
        dosageEl.textContent = med.Dosage || NOT_AVAILABLE;
        dosageEl.classList.toggle("modal__section-text--muted", !med.Dosage);

        var rxBadge = document.getElementById("modalRxBadge");
        rxBadge.hidden = !med.RequiresRx;
        rxBadge.textContent = med.RequiresRx ? "Rx" : "OTC";

        var sideEffectsBox = document.getElementById("modalSideEffects");
        sideEffectsBox.innerHTML = "";
        (med.SideEffects || []).forEach(function (effect) {
            var span = document.createElement("span");
            span.textContent = effect;
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

        // Guest clicking "Add to cart" inside the details modal — send to
        // login, never touch the cart.
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