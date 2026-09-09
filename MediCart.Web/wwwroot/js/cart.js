(function () {
    "use strict";

    var cartItemsBox = document.getElementById("cartItems");
    if (!cartItemsBox) return;

    var cartHeading     = document.getElementById("cartHeading");
    var summarySubtotal = document.getElementById("summarySubtotal");
    var summaryTotal    = document.getElementById("summaryTotal");
    var orderSummary    = document.getElementById("orderSummary");
    var cartEmpty       = document.getElementById("cartEmpty");
    var rxNote          = document.getElementById("rxNote");
    var rxNoteCount     = document.getElementById("rxNoteCount");
    var rxNotePlural    = document.getElementById("rxNotePlural");
    var rxNoteVerb      = document.getElementById("rxNoteVerb");

    function getToken() {
        var input = document.querySelector('input[name="__RequestVerificationToken"]');
        return input ? input.value : "";
    }

    function post(url, data) {
        var params = new URLSearchParams(data);
        params.append("__RequestVerificationToken", getToken());
        return fetch(url, {
            method: "POST",
            headers: { "Content-Type": "application/x-www-form-urlencoded" },
            body: params.toString()
        });
    }

    function getRows() {
        return Array.prototype.slice.call(
            cartItemsBox.querySelectorAll(".cart-item"));
    }

    function recalculate() {
        var rows = getRows();
        var subtotal = 0;
        var rxCount  = 0;

        rows.forEach(function (row) {
            var price = parseFloat(row.dataset.price);
            var qty   = parseInt(row.querySelector(".qty-input").value, 10) || 1;
            row.querySelector(".line-total").textContent =
                "\u09F3" + (price * qty).toFixed(0);
            subtotal += price * qty;
            if (row.dataset.requiresRx === "true") rxCount++;
        });

        summarySubtotal.textContent = "\u09F3" + subtotal.toFixed(0);
        summaryTotal.textContent    = "\u09F3" + subtotal.toFixed(0);
        cartHeading.textContent = "Your cart (" + rows.length +
            " item" + (rows.length === 1 ? "" : "s") + ")";

        if (rxNote) {
            rxNote.hidden            = rxCount === 0;
            rxNoteCount.textContent  = rxCount;
            rxNotePlural.textContent = rxCount === 1 ? "" : "s";
            rxNoteVerb.textContent   = rxCount === 1 ? "s" : "";
        }

        updateBadge(rows.reduce(function (sum, row) {
            return sum + (parseInt(row.querySelector(".qty-input").value, 10) || 0);
        }, 0));

        if (rows.length === 0) {
            if (orderSummary) orderSummary.hidden = true;
            if (cartEmpty)    cartEmpty.hidden    = false;
        }
    }

    function updateBadge(count) {
        var badge = document.getElementById("cartBadge");
        if (!badge) return;
        badge.textContent = count;
        badge.classList.toggle("is-visible", count > 0);
    }

    function showToast(message) {
        var toast = document.getElementById("toast");
        if (!toast) return;
        toast.querySelector(".toast__text").textContent = message;
        toast.classList.add("is-visible");
        clearTimeout(showToast._t);
        showToast._t = setTimeout(function () {
            toast.classList.remove("is-visible");
        }, 2200);
    }

    // ── Click handler (minus / plus / remove) ──────────────────────────────
    cartItemsBox.addEventListener("click", function (e) {
        var row = e.target.closest(".cart-item");
        if (!row) return;

        var cartItemId     = row.dataset.id;
        var qtyInput       = row.querySelector(".qty-input");
        var currentQty     = parseInt(qtyInput.value, 10) || 1;
        var availableStock = parseInt(row.dataset.availableStock, 10) || 0;

        // ── Decrease ──
        if (e.target.classList.contains("qty-minus")) {
            if (currentQty <= 1) return;
            post("/Cart/UpdateQuantity", {
                cartItemId: cartItemId,
                newQuantity: currentQty - 1
            }).then(function (res) {
                return res.json().then(function (d) {
                    if (!res.ok) { showToast(d.error || "Could not update quantity."); return; }
                    qtyInput.value = d.newQuantity;
                    row.dataset.availableStock = d.newStockQuantity;
                    row.querySelector(".qty-plus").disabled  = d.newStockQuantity <= 0;
                    row.querySelector(".qty-minus").disabled = d.newQuantity <= 1;
                    qtyInput.max = d.newQuantity + d.newStockQuantity;
                    recalculate();
                });
            }).catch(function () { showToast("Something went wrong."); });
            return;
        }

        // ── Increase ──
        if (e.target.classList.contains("qty-plus")) {
            if (availableStock <= 0) return;
            post("/Cart/UpdateQuantity", {
                cartItemId: cartItemId,
                newQuantity: currentQty + 1
            }).then(function (res) {
                return res.json().then(function (d) {
                    if (!res.ok) { showToast(d.error || "Could not update quantity."); return; }
                    qtyInput.value = d.newQuantity;
                    row.dataset.availableStock = d.newStockQuantity;
                    row.querySelector(".qty-plus").disabled  = d.newStockQuantity <= 0;
                    row.querySelector(".qty-minus").disabled = d.newQuantity <= 1;
                    qtyInput.max = d.newQuantity + d.newStockQuantity;
                    recalculate();
                });
            }).catch(function () { showToast("Something went wrong."); });
            return;
        }

        // ── Remove ──
        if (e.target.classList.contains("cart-item__remove")) {
            post("/Cart/Remove", { cartItemId: cartItemId })
            .then(function (res) {
                return res.json().then(function (d) {
                    if (!res.ok) { showToast(d.error || "Could not remove item."); return; }
                    row.classList.add("is-removing");
                    setTimeout(function () { row.remove(); recalculate(); }, 180);
                });
            }).catch(function () { showToast("Something went wrong."); });
        }
    });

    // ── Manual qty input ───────────────────────────────────────────────────
    cartItemsBox.addEventListener("change", function (e) {
        if (!e.target.classList.contains("qty-input")) return;
        var input = e.target;
        var row   = input.closest(".cart-item");
        var max   = parseInt(input.max, 10) || 99;
        var val   = parseInt(input.value, 10) || 1;
        if (val < 1) val = 1;
        if (val > max) val = max;
        input.value = val;

        post("/Cart/UpdateQuantity", {
            cartItemId: row.dataset.id,
            newQuantity: val
        }).then(function (res) {
            return res.json().then(function (d) {
                if (!res.ok) { showToast(d.error || "Could not update quantity."); return; }
                input.value = d.newQuantity;
                row.dataset.availableStock = d.newStockQuantity;
                row.querySelector(".qty-plus").disabled  = d.newStockQuantity <= 0;
                row.querySelector(".qty-minus").disabled = d.newQuantity <= 1;
                input.max = d.newQuantity + d.newStockQuantity;
                recalculate();
            });
        }).catch(function () { showToast("Something went wrong."); });
    });

    // Block non-digit keystrokes in the qty input
    cartItemsBox.addEventListener("keydown", function (e) {
        if (!e.target.classList.contains("qty-input")) return;
        var allowed = ["Backspace","Delete","ArrowLeft","ArrowRight","Tab","Enter"];
        if (allowed.indexOf(e.key) === -1 && !/^\d$/.test(e.key)) {
            e.preventDefault();
        }
    });

    recalculate();
})();