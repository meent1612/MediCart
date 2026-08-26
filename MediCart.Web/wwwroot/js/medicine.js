(function () {
    "use strict";

    const dataEl = document.getElementById("medicines-data");
    const medicines = dataEl ? JSON.parse(dataEl.textContent) : [];
    const byId = (id) => medicines.find(m => m.id === id || m.Id === id);

    // ---------- FILTER STATE ----------
    const state = { type: new Set(), category: new Set(), tag: new Set(), maxPrice: 500, sort: "popular" };

    const grid = document.getElementById("medicineGrid");
    const cards = Array.from(document.querySelectorAll(".medicine-card"));
    const emptyState = document.getElementById("emptyState");
    const resultsCount = document.getElementById("resultsCount");

    document.querySelectorAll(".filter-input").forEach(input => {
        input.addEventListener("change", () => {
            const bucket = state[input.dataset.filter];
            input.checked ? bucket.add(input.value) : bucket.delete(input.value);
            applyFilters();
        });
    });

    const priceRange = document.getElementById("priceRange");
    const priceRangeValue = document.getElementById("priceRangeValue");
    priceRange.addEventListener("input", () => {
        state.maxPrice = Number(priceRange.value);
        priceRangeValue.textContent = "৳" + state.maxPrice;
        applyFilters();
    });

    document.getElementById("clearFilters").addEventListener("click", () => {
        document.querySelectorAll(".filter-input").forEach(i => i.checked = false);
        state.type.clear(); state.category.clear(); state.tag.clear();
        state.maxPrice = 500; priceRange.value = 500; priceRangeValue.textContent = "৳500";
        applyFilters();
    });

    document.getElementById("sortSelect").addEventListener("change", (e) => {
        state.sort = e.target.value;
        applyFilters();
    });

    function applyFilters() {
        let visible = cards.filter(card => {
            const type = card.dataset.type, category = card.dataset.category;
            const tags = card.dataset.tags.split(",").filter(Boolean);
            const price = Number(card.dataset.price);

            if (state.type.size && !state.type.has(type)) return false;
            if (state.category.size && !state.category.has(category)) return false;
            if (state.tag.size && !tags.some(t => state.tag.has(t))) return false;
            if (price > state.maxPrice) return false;
            return true;
        });

        // sort
        const sorters = {
            "price-asc": (a, b) => a.dataset.price - b.dataset.price,
            "price-desc": (a, b) => b.dataset.price - a.dataset.price,
            "name-asc": (a, b) => a.dataset.name.localeCompare(b.dataset.name),
            "popular": () => 0
        };
        visible.sort(sorters[state.sort]);

        cards.forEach(c => c.style.display = "none");
        visible.forEach(c => { c.style.display = ""; grid.appendChild(c); });

        resultsCount.textContent = `(${visible.length} result${visible.length === 1 ? "" : "s"})`;
        emptyState.hidden = visible.length !== 0;
    }

    // ---------- QUICK VIEW MODAL ----------
    const backdrop = document.getElementById("modalBackdrop");
    const modal = document.getElementById("medicineModal");
    let currentMed = null;
    let qty = 1;

    function openModal(id) {
        const med = byId(id) || medicines.find(m => (m.Id ?? m.id) === id);
        if (!med) return;
        currentMed = med;
        qty = 1;
        document.getElementById("qtyValue").textContent = qty;

        document.getElementById("modalTitle").textContent = med.Name ?? med.name;
        document.getElementById("modalGeneric").textContent = `${med.GenericName ?? med.genericName} · ${med.Manufacturer ?? med.manufacturer}`;
        document.getElementById("modalDesc").textContent = med.Description ?? med.description;
        document.getElementById("modalDosage").textContent = med.DosageInstructions ?? med.dosageInstructions;
        document.getElementById("modalIcon").textContent = med.IconGlyph ?? med.iconGlyph ?? "💊";
        document.getElementById("modalPrice").textContent = "৳ " + (med.Price ?? med.price);

        const badges = document.getElementById("modalBadges");
        const requiresRx = med.RequiresPrescription ?? med.requiresPrescription;
        const stockLabel = med.StockLabel ?? med.stockLabel;
        const stockCss = med.StockCss ?? med.stockCss;
        const intensity = med.Intensity ?? med.intensity;
        badges.innerHTML = "";
        if (requiresRx) badges.innerHTML += `<span class="badge badge-rx">Rx required</span>`;
        else badges.innerHTML += `<span class="badge ${stockCss}">${stockLabel}</span>`;
        badges.innerHTML += `<span class="badge badge-intensity">${intensity}</span>`;

        const sideEffects = med.SideEffects ?? med.sideEffects ?? [];
        const list = document.getElementById("modalSideEffects");
        list.innerHTML = sideEffects.map(se => `<li>${se}</li>`).join("");

        backdrop.hidden = false;
        document.body.style.overflow = "hidden";
        document.getElementById("modalClose").focus();
    }

    function closeModal() {
        backdrop.hidden = true;
        document.body.style.overflow = "";
        currentMed = null;
    }

    cards.forEach(card => {
        const id = Number(card.dataset.id);
        card.addEventListener("click", () => openModal(id));
        card.addEventListener("keydown", (e) => {
            if (e.key === "Enter" || e.key === " ") { e.preventDefault(); openModal(id); }
        });
    });

    document.getElementById("modalClose").addEventListener("click", closeModal);
    backdrop.addEventListener("click", (e) => { if (e.target === backdrop) closeModal(); });
    document.addEventListener("keydown", (e) => { if (e.key === "Escape" && !backdrop.hidden) closeModal(); });

    document.getElementById("qtyMinus").addEventListener("click", () => {
        qty = Math.max(1, qty - 1);
        document.getElementById("qtyValue").textContent = qty;
    });
    document.getElementById("qtyPlus").addEventListener("click", () => {
        qty = Math.min(20, qty + 1);
        document.getElementById("qtyValue").textContent = qty;
    });

    // ---------- ADD TO CART ----------
    function showToast(message) {
        const toast = document.getElementById("cartToast");
        toast.textContent = message;
        toast.hidden = false;
        requestAnimationFrame(() => toast.classList.add("show"));
        clearTimeout(showToast._t);
        showToast._t = setTimeout(() => {
            toast.classList.remove("show");
            setTimeout(() => { toast.hidden = true; }, 200);
        }, 1800);
    }

    // Placeholder cart hook — backend teammates can replace this with a real
    // POST to /Cart/Add and update the header cart badge from the response.
    document.querySelectorAll(".add-btn").forEach(btn => {
        btn.addEventListener("click", () => {
            const card = btn.closest(".medicine-card");
            showToast(`Added ${card.dataset.name} to cart`);
        });
    });

    document.getElementById("modalAddBtn").addEventListener("click", () => {
        if (!currentMed) return;
        const name = currentMed.Name ?? currentMed.name;
        showToast(`Added ${qty} × ${name} to cart`);
        closeModal();
    });

    applyFilters();
})();
