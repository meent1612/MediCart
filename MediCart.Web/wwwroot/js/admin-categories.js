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
})();