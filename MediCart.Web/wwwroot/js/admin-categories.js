(function () {
    "use strict";

    // ---- Category edit toggle ----
    var categoryForm = document.getElementById("categoryForm");
    var categoryFormId = document.getElementById("categoryFormId");
    var categoryName = document.getElementById("categoryName");
    var categoryDescription = document.getElementById("categoryDescription");
    var categoryParent = document.getElementById("categoryParent");
    var categoryFormTitle = document.getElementById("categoryFormTitle");
    var categoryFormSubmit = document.getElementById("categoryFormSubmit");
    var cancelCategoryEdit = document.getElementById("cancelCategoryEdit");
    var newCategoryBtn = document.getElementById("newCategoryBtn");

    categoryForm.dataset.createAction = categoryForm.getAttribute("action");

    function resetCategoryForm() {
        categoryForm.action = categoryForm.dataset.createAction;
        categoryFormId.value = "";
        categoryName.value = "";
        categoryDescription.value = "";
        categoryParent.value = "";
        categoryFormTitle.textContent = "Add a category";
        categoryFormSubmit.textContent = "Add category";
        cancelCategoryEdit.hidden = true;
    }

    document.querySelectorAll(".edit-category-btn").forEach(function (btn) {
        btn.addEventListener("click", function () {
            categoryFormId.value = btn.dataset.id;
            categoryName.value = btn.dataset.name || "";
            categoryDescription.value = btn.dataset.description || "";
            categoryParent.value = btn.dataset.parentId || "";
            categoryFormTitle.textContent = "Edit category";
            categoryFormSubmit.textContent = "Save changes";
            categoryForm.action = "/Admin/EditCategory";
            cancelCategoryEdit.hidden = false;
            categoryForm.scrollIntoView({ behavior: "smooth", block: "center" });
        });
    });

    cancelCategoryEdit.addEventListener("click", resetCategoryForm);
    newCategoryBtn.addEventListener("click", function () {
        resetCategoryForm();
        categoryName.focus();
    });

    // ---- Product type edit toggle ----
    var productTypeForm = document.getElementById("productTypeForm");
    var productTypeFormId = document.getElementById("productTypeFormId");
    var productTypeName = document.getElementById("productTypeName");
    var productTypeFormTitle = document.getElementById("productTypeFormTitle");
    var productTypeFormSubmit = document.getElementById("productTypeFormSubmit");
    var cancelProductTypeEdit = document.getElementById("cancelProductTypeEdit");
    var newProductTypeBtn = document.getElementById("newProductTypeBtn");

    productTypeForm.dataset.createAction = productTypeForm.getAttribute("action");

    function resetProductTypeForm() {
        productTypeForm.action = productTypeForm.dataset.createAction;
        productTypeFormId.value = "";
        productTypeName.value = "";
        productTypeFormTitle.textContent = "Add a product type";
        productTypeFormSubmit.textContent = "Add type";
        cancelProductTypeEdit.hidden = true;
    }

    document.querySelectorAll(".edit-producttype-btn").forEach(function (btn) {
        btn.addEventListener("click", function () {
            productTypeFormId.value = btn.dataset.id;
            productTypeName.value = btn.dataset.name || "";
            productTypeFormTitle.textContent = "Edit product type";
            productTypeFormSubmit.textContent = "Save changes";
            productTypeForm.action = "/Admin/EditProductType";
            cancelProductTypeEdit.hidden = false;
            productTypeForm.scrollIntoView({ behavior: "smooth", block: "center" });
        });
    });

    cancelProductTypeEdit.addEventListener("click", resetProductTypeForm);
    newProductTypeBtn.addEventListener("click", function () {
        resetProductTypeForm();
        productTypeName.focus();
    });

    // ---- Category tree: expand/collapse subcategories ----
    document.querySelectorAll(".cat-toggle[data-parent-id]").forEach(function (btn) {
        btn.addEventListener("click", function () {
            var parentId = btn.dataset.parentId;
            var isExpanded = btn.getAttribute("aria-expanded") === "true";

            document.querySelectorAll('.cat-row--child[data-parent-id="' + parentId + '"]').forEach(function (row) {
                row.classList.toggle("cat-row--collapsed-hidden", isExpanded);
            });

            btn.setAttribute("aria-expanded", String(!isExpanded));
        });
    });

    // ---- Category search (matches parent or child name; keeps a parent
    // visible if any of its children match, and vice versa) ----
    var categorySearch = document.getElementById("categorySearch");
    var categoryNoMatches = document.getElementById("categoryNoMatches");

    categorySearch?.addEventListener("input", function () {
        var term = categorySearch.value.trim().toLowerCase();
        var visibleCount = 0;

        document.querySelectorAll(".cat-row--parent").forEach(function (parentRow) {
            var parentId = parentRow.dataset.id;
            var childRows = document.querySelectorAll('.cat-row--child[data-parent-id="' + parentId + '"]');

            var parentMatches = term === "" || parentRow.dataset.catName.indexOf(term) !== -1;
            var anyChildMatches = false;
            childRows.forEach(function (childRow) {
                if (childRow.dataset.catName.indexOf(term) !== -1) anyChildMatches = true;
            });

            var showParent = term === "" || parentMatches || anyChildMatches;
            parentRow.classList.toggle("cat-row--search-hidden", !showParent);
            if (showParent) visibleCount++;

            childRows.forEach(function (childRow) {
                var childMatches = term === "" || parentMatches || childRow.dataset.catName.indexOf(term) !== -1;
                var show = showParent && childMatches;
                childRow.classList.toggle("cat-row--search-hidden", !show);
                if (show) visibleCount++;
            });
        });

        if (categoryNoMatches) categoryNoMatches.hidden = visibleCount !== 0 || term === "";
    });

    // ---- Product type search ----
    var productTypeSearch = document.getElementById("productTypeSearch");
    var productTypeNoMatches = document.getElementById("productTypeNoMatches");

    productTypeSearch?.addEventListener("input", function () {
        var term = productTypeSearch.value.trim().toLowerCase();
        var visibleCount = 0;

        document.querySelectorAll(".pt-row").forEach(function (row) {
            var matches = term === "" || row.dataset.ptName.indexOf(term) !== -1;
            row.classList.toggle("cat-row--search-hidden", !matches);
            if (matches) visibleCount++;
        });

        if (productTypeNoMatches) productTypeNoMatches.hidden = visibleCount !== 0 || term === "";
    });

    // ---- Dismissible alert banners ----
    document.querySelectorAll(".admin-alert__dismiss").forEach(function (btn) {
        btn.addEventListener("click", function () {
            btn.closest(".admin-alert")?.remove();
        });
    });
})();
