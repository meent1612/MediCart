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

    /* ----- Add to cart from card ----- */
    var cartCount = 0;

    function bumpCart(qty) {
        cartCount += qty;
        var cartButton = document.querySelector(".cart-button");
        var cartBadge = document.querySelector(".cart-button__badge");

        if (cartBadge) {
            cartBadge.textContent = cartCount;
            cartBadge.classList.add("is-visible");
        }

        if (cartButton) {
            cartButton.classList.remove("is-bumped");
            // force reflow so the animation can restart on repeated clicks
            void cartButton.offsetWidth;
            cartButton.classList.add("is-bumped");
        }

        showToast(qty === 1 ? "Added to cart" : qty + " items added to cart");
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
            bumpCart(1);
            addBtn.classList.add("added");
            setTimeout(function () { addBtn.classList.remove("added"); }, 350);
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

        document.getElementById("modalTitle").textContent = med.Name;
        document.getElementById("modalComposition").textContent = med.Composition + " " + med.Strength;
        document.getElementById("modalManufacturer").textContent = med.Manufacturer;
        document.getElementById("modalStrength").textContent = med.Strength;
        document.getElementById("modalForm").textContent = med.ProductType;
        document.getElementById("modalCategory").textContent = med.Category;
        document.getElementById("modalStock").textContent = med.Stock > 0 ? (med.Stock + " units") : "Out of stock";
        document.getElementById("modalPrice").textContent = "\u09F3" + med.Price;
        document.getElementById("modalAbout").textContent = med.About;
        document.getElementById("modalDosage").textContent = med.Dosage;

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
        var qty = parseInt(qtyInput.value, 10) || 1;
        bumpCart(qty);
        addToCartBtn.textContent = "Added";
        addToCartBtn.classList.add("added");
        setTimeout(closeModal, 500);
    });

    /* initial render */
    priceRangeValue.textContent = "\u09F3" + priceRange.value;
    applyFilters();
})();