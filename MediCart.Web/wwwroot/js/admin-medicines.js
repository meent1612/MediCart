(function () {
    "use strict";

    /* ---------------------------------------------------------------------
       Elements
       ------------------------------------------------------------------ */
    var form = document.getElementById("medicineFilterForm");
    var searchInput = document.getElementById("medicineSearchInput");
    var filterCategory = document.getElementById("filterCategory");
    var filterProductType = document.getElementById("filterProductType");
    var resultsCount = document.getElementById("resultsCount");
    var tableWrap = document.getElementById("medicineTableWrap");
    var table = document.getElementById("medicineTable");
    var noMatchesRow = document.getElementById("clientNoMatchesRow");
    var clearFiltersBtn = document.getElementById("clientClearFiltersBtn");

    if (!form || !table) return;

    var tbody = table.querySelector("tbody");
    var rows = Array.prototype.slice.call(tbody.querySelectorAll("tr.med-row"));
    var totalRowCount = rows.length;

    /* ---------------------------------------------------------------------
       Debounce helper
       ------------------------------------------------------------------ */
    function debounce(fn, wait) {
        var timer;
        return function () {
            var args = arguments;
            clearTimeout(timer);
            timer = setTimeout(function () { fn.apply(null, args); }, wait);
        };
    }

    /* ---------------------------------------------------------------------
       Instant client-side filtering (search + category + product type)
       ------------------------------------------------------------------ */
    function selectedOptionText(select) {
        if (!select || select.selectedIndex < 0) return "";
        return select.options[select.selectedIndex].text.trim();
    }

    function applyFilters() {
        if (totalRowCount === 0) return;

        var term = (searchInput?.value || "").trim().toLowerCase();
        var categoryActive = !!(filterCategory && filterCategory.value !== "");
        var typeActive = !!(filterProductType && filterProductType.value !== "");
        var categoryText = selectedOptionText(filterCategory);
        var typeText = selectedOptionText(filterProductType);

        var visibleCount = 0;

        rows.forEach(function (row) {
            var matchesTerm = !term || row.dataset.name.toLowerCase().indexOf(term) !== -1;
            var matchesCategory = !categoryActive || row.dataset.category === categoryText;
            var matchesType = !typeActive || row.dataset.productType === typeText;
            var isMatch = matchesTerm && matchesCategory && matchesType;

            row.hidden = !isMatch;
            if (isMatch) visibleCount++;
        });

        if (noMatchesRow) {
            noMatchesRow.hidden = visibleCount !== 0;
        }

        if (resultsCount) {
            resultsCount.textContent = visibleCount === totalRowCount
                ? totalRowCount + " medicine" + (totalRowCount === 1 ? "" : "s") + " found"
                : "Showing " + visibleCount + " of " + totalRowCount + " medicines";
        }
    }

    var debouncedFilter = debounce(applyFilters, 120);

    searchInput?.addEventListener("input", debouncedFilter);
    filterCategory?.addEventListener("change", applyFilters);
    filterProductType?.addEventListener("change", applyFilters);

    clearFiltersBtn?.addEventListener("click", function () {
        if (searchInput) searchInput.value = "";
        if (filterCategory) filterCategory.selectedIndex = 0;
        if (filterProductType) filterProductType.selectedIndex = 0;
        applyFilters();
        searchInput?.focus();
    });

    /* ---------------------------------------------------------------------
       Sortable columns
       ------------------------------------------------------------------ */
    var sortHeaders = Array.prototype.slice.call(table.querySelectorAll(".sort-th"));

    function parseSortValue(row, key, type) {
        var raw = row.dataset[key] !== undefined ? row.dataset[key] : "";
        if (type === "number") return parseFloat(raw) || 0;
        return raw.toLowerCase();
    }

    function sortRows(key, type, direction) {
        var sorted = rows.slice().sort(function (a, b) {
            var va = parseSortValue(a, key, type);
            var vb = parseSortValue(b, key, type);
            if (va < vb) return direction === "ascending" ? -1 : 1;
            if (va > vb) return direction === "ascending" ? 1 : -1;
            return 0;
        });
        sorted.forEach(function (row) { tbody.appendChild(row); });
        rows = sorted;
        if (noMatchesRow) tbody.appendChild(noMatchesRow);
    }

    function handleSortActivate(header) {
        var key = header.dataset.sortKey;
        // camelCase the dataset key for keys like "productType" — our keys are single words, so this is a no-op safeguard.
        var type = header.dataset.sortType || "text";
        var current = header.getAttribute("aria-sort");
        var next = current === "ascending" ? "descending" : "ascending";

        sortHeaders.forEach(function (h) { h.setAttribute("aria-sort", "none"); });
        header.setAttribute("aria-sort", next);

        sortRows(key, type, next);
    }

    sortHeaders.forEach(function (header) {
        header.addEventListener("click", function () { handleSortActivate(header); });
        header.addEventListener("keydown", function (e) {
            if (e.key === "Enter" || e.key === " ") {
                e.preventDefault();
                handleSortActivate(header);
            }
        });
    });

    /* ---------------------------------------------------------------------
       Delete confirmation modal
       ------------------------------------------------------------------ */
    var deleteOverlay = document.getElementById("deleteModalOverlay");
    var deleteNameEl = document.getElementById("deleteModalName");
    var deleteCancelBtn = document.getElementById("deleteModalCancel");
    var deleteConfirmBtn = document.getElementById("deleteModalConfirm");
    var pendingForm = null;
    var lastTrigger = null;

    function openDeleteModal(trigger) {
        var formId = trigger.dataset.formId;
        pendingForm = formId ? document.getElementById(formId) : null;
        if (!pendingForm || !deleteOverlay) return;

        lastTrigger = trigger;
        if (deleteNameEl) deleteNameEl.textContent = trigger.dataset.medicineName || "this medicine";

        deleteOverlay.classList.add("is-open");
        deleteOverlay.setAttribute("aria-hidden", "false");
        deleteCancelBtn?.focus();
        document.addEventListener("keydown", onDeleteModalKeydown);
    }

    function closeDeleteModal() {
        if (!deleteOverlay) return;
        deleteOverlay.classList.remove("is-open");
        deleteOverlay.setAttribute("aria-hidden", "true");
        document.removeEventListener("keydown", onDeleteModalKeydown);
        pendingForm = null;
        lastTrigger?.focus();
        lastTrigger = null;
    }

    function onDeleteModalKeydown(e) {
        if (e.key === "Escape") {
            closeDeleteModal();
            return;
        }
        if (e.key === "Tab") {
            // Simple two-item focus trap between Cancel and Delete.
            var focusables = [deleteCancelBtn, deleteConfirmBtn];
            var currentIndex = focusables.indexOf(document.activeElement);
            e.preventDefault();
            var nextIndex = e.shiftKey
                ? (currentIndex <= 0 ? focusables.length - 1 : currentIndex - 1)
                : (currentIndex === focusables.length - 1 ? 0 : currentIndex + 1);
            focusables[nextIndex]?.focus();
        }
    }

    document.querySelectorAll(".js-delete-trigger").forEach(function (trigger) {
        trigger.addEventListener("click", function (e) {
            e.preventDefault();
            openDeleteModal(trigger);
        });
    });

    deleteCancelBtn?.addEventListener("click", closeDeleteModal);
    deleteOverlay?.addEventListener("click", function (e) {
        if (e.target === deleteOverlay) closeDeleteModal();
    });
    deleteConfirmBtn?.addEventListener("click", function () {
        // form.submit() bypasses the native onsubmit confirm() fallback,
        // so the user isn't asked twice.
        pendingForm?.submit();
    });

    /* ---------------------------------------------------------------------
       Dismissible / auto-dismissing alerts
       ------------------------------------------------------------------ */
    document.querySelectorAll("[data-alert]").forEach(function (alert) {
        function dismiss() {
            alert.style.transition = "opacity .2s ease, transform .2s ease";
            alert.style.opacity = "0";
            alert.style.transform = "translateY(-6px)";
            setTimeout(function () { alert.remove(); }, 200);
        }

        alert.querySelector(".admin-alert__dismiss")?.addEventListener("click", dismiss);

        if (alert.dataset.autoDismiss === "true") {
            setTimeout(dismiss, 5000);
        }
    });
})();
