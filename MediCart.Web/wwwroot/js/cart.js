(function () {
    "use strict";

    var cartItemsBox = document.getElementById("cartItems");
    if (!cartItemsBox) return;

    var cartHeading = document.getElementById("cartHeading");
    var summarySubtotal = document.getElementById("summarySubtotal");
    var summaryTotal = document.getElementById("summaryTotal");
    var orderSummary = document.getElementById("orderSummary");
    var cartEmpty = document.getElementById("cartEmpty");
    var rxNote = document.getElementById("rxNote");
    var rxNoteCount = document.getElementById("rxNoteCount");
    var rxNotePlural = document.getElementById("rxNotePlural");
    var rxNoteVerb = document.getElementById("rxNoteVerb");

    function getRows() {
        return Array.prototype.slice.call(cartItemsBox.querySelectorAll(".cart-item"));
    }

    function recalculate() {
        var rows = getRows();
        var subtotal = 0;
        var rxCount = 0;

        rows.forEach(function (row) {
            var price = parseFloat(row.dataset.price);
            var qty = parseInt(row.querySelector(".qty-input").value, 10) || 1;
            var lineTotal = price * qty;

            row.querySelector(".line-total").textContent = "\u09F3" + lineTotal.toFixed(0);
            subtotal += lineTotal;

            if (row.dataset.requiresRx === "true") rxCount++;
        });

        summarySubtotal.textContent = "\u09F3" + subtotal.toFixed(0);
        summaryTotal.textContent = "\u09F3" + subtotal.toFixed(0);

        cartHeading.textContent = "Your cart (" + rows.length + " item" + (rows.length === 1 ? "" : "s") + ")";

        if (rxNote) {
            rxNote.hidden = rxCount === 0;
            rxNoteCount.textContent = rxCount;
            rxNotePlural.textContent = rxCount === 1 ? "" : "s";
            rxNoteVerb.textContent = rxCount === 1 ? "s" : "";
        }

        if (rows.length === 0) {
            orderSummary.hidden = true;
            cartEmpty.hidden = false;
        }
    }

    cartItemsBox.addEventListener("click", function (e) {
        var row = e.target.closest(".cart-item");
        if (!row) return;
        var qtyInput = row.querySelector(".qty-input");

        if (e.target.classList.contains("qty-minus")) {
            var val = parseInt(qtyInput.value, 10) || 1;
            if (val > 1) {
                qtyInput.value = val - 1;
                recalculate();
            }
            return;
        }

        if (e.target.classList.contains("qty-plus")) {
            var max = parseInt(qtyInput.max, 10) || 99;
            var current = parseInt(qtyInput.value, 10) || 1;
            if (current < max) {
                qtyInput.value = current + 1;
                recalculate();
            }
            return;
        }

        if (e.target.classList.contains("cart-item__remove")) {
            row.classList.add("is-removing");
            setTimeout(function () {
                row.remove();
                recalculate();
            }, 180);
        }
    });

    cartItemsBox.addEventListener("change", function (e) {
        if (!e.target.classList.contains("qty-input")) return;
        var input = e.target;
        var max = parseInt(input.max, 10) || 99;
        var val = parseInt(input.value, 10) || 1;
        if (val < 1) val = 1;
        if (val > max) val = max;
        input.value = val;
        recalculate();
    });

    recalculate();
})();
