(function () {
    "use strict";

    var table = document.getElementById("medicineTable");
    if (!table) return;

    var tbody = table.querySelector("tbody");

    /* ---------------------------------------------------------------------
       Sortable columns (client-side re-sort of the current page only —
       does not change which medicines are shown, only their order)
       ------------------------------------------------------------------ */
    var sortHeaders = Array.prototype.slice.call(table.querySelectorAll(".sort-th"));
    var rows = Array.prototype.slice.call(tbody.querySelectorAll("tr.med-row"));

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
    }

    function handleSortActivate(header) {
        var key = header.dataset.sortKey;
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
       Dependent SubCategory dropdown in the filter bar.
       Filtering itself only happens when "Filter" is clicked (normal GET
       submit) — this only repopulates which subcategory options are valid
       for the currently selected category.
       ------------------------------------------------------------------ */
    var filterCategory = document.getElementById("filterCategory");
    var filterSubCategory = document.getElementById("filterSubCategory");

    function loadFilterSubCategories(categoryId, preselectId) {
        filterSubCategory.innerHTML = '<option value="">All subcategories</option>';
        if (!categoryId) return;

        fetch("/Admin/GetSubCategories?categoryId=" + encodeURIComponent(categoryId))
            .then(function (res) {
                if (!res.ok) throw new Error("Failed to load subcategories");
                return res.json();
            })
            .then(function (subCategories) {
                subCategories.forEach(function (sc) {
                    var opt = document.createElement("option");
                    opt.value = sc.id;
                    opt.textContent = sc.label;
                    if (preselectId && String(sc.id) === String(preselectId)) {
                        opt.selected = true;
                    }
                    filterSubCategory.appendChild(opt);
                });
            })
            .catch(function () {});
    }

    if (filterCategory && filterSubCategory) {
        var initialSelectedSubCategory = filterSubCategory.dataset.selected || "";

        // On category change: reload the subcategory list, but reset the
        // selection since the previous subcategory may not belong to the
        // newly chosen category. Does NOT submit the form.
        filterCategory.addEventListener("change", function () {
            loadFilterSubCategories(filterCategory.value, null);
        });

        // On initial page load: if a category (and possibly subcategory)
        // was already applied via the URL, restore that state.
        if (filterCategory.value) {
            loadFilterSubCategories(filterCategory.value, initialSelectedSubCategory);
        }
    }

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