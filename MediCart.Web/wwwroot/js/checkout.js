(function () {
    "use strict";

    var checkoutData = document.getElementById("checkoutData");
    if (!checkoutData) return;

    var data = JSON.parse(checkoutData.textContent);
    var subtotal = data.subtotal;

    var areaMap = {
        "Dhaka": ["Mirpur", "Dhanmondi", "Gulshan", "Uttara", "Mohammadpur"],
        "Chattogram": ["Pahartali", "Kotwali", "Panchlaish", "Halishahor"],
        "Khulna": ["Sonadanga", "Khalishpur", "Daulatpur"],
        "Rajshahi": ["Boalia", "Motihar", "Rajpara"],
        "Barishal": ["Kotwali", "Band Road"],
        "Sylhet": ["Zindabazar", "Ambarkhana"],
        "Rangpur": ["Kotwali", "Mahiganj"],
        "Mymensingh": ["Kotwali", "Trishal"]
    };

    var deliveryMap = {
        "Dhaka": 60, "Chattogram": 100, "Khulna": 110, "Rajshahi": 120,
        "Barishal": 130, "Sylhet": 120, "Rangpur": 130, "Mymensingh": 110
    };

    var divisionSelect = document.getElementById("division");
    var citySelect = document.getElementById("city");
    var deliveryDivisionLabel = document.getElementById("deliveryDivisionLabel");
    var summaryDelivery = document.getElementById("summaryDelivery");
    var summaryTotal = document.getElementById("summaryTotal");

    function populateCities(division) {
        citySelect.innerHTML = "";
        (areaMap[division] || []).forEach(function (area) {
            var opt = document.createElement("option");
            opt.value = area;
            opt.textContent = area;
            citySelect.appendChild(opt);
        });
    }

    function updateDelivery() {
        var division = divisionSelect.value;
        var delivery = deliveryMap[division] || 100;
        deliveryDivisionLabel.textContent = division;
        summaryDelivery.textContent = "\u09F3" + delivery;
        summaryTotal.textContent = "\u09F3" + (subtotal + delivery).toFixed(0);
    }

    divisionSelect.addEventListener("change", function () {
        populateCities(divisionSelect.value);
        updateDelivery();
    });

    populateCities(divisionSelect.value);
    updateDelivery();

    /* ----- Payment method ----- */
    var paymentOptions = document.getElementById("paymentOptions");
    var bkashFields = document.getElementById("bkashFields");
    var cardFields = document.getElementById("cardFields");
    var selectedMethod = "Cash on delivery";

    function showPaymentFields(method) {
        bkashFields.hidden = method !== "bKash";
        cardFields.hidden = method !== "Card";
    }

    paymentOptions.addEventListener("click", function (e) {
        var btn = e.target.closest(".payment-option");
        if (!btn) return;
        paymentOptions.querySelectorAll(".payment-option").forEach(function (b) { b.classList.remove("is-selected"); });
        btn.classList.add("is-selected");
        selectedMethod = btn.dataset.method;
        showPaymentFields(selectedMethod);
    });

    /* ----- bKash field formatting ----- */
    var bkashNumber = document.getElementById("bkashNumber");
    var bkashTxnId = document.getElementById("bkashTxnId");
    var bkashError = document.getElementById("bkashError");

    bkashNumber.addEventListener("input", function () {
        bkashNumber.value = bkashNumber.value.replace(/\D/g, "").slice(0, 11);
    });

    /* ----- Card field formatting ----- */
    var cardName = document.getElementById("cardName");
    var cardNumber = document.getElementById("cardNumber");
    var cardExpiry = document.getElementById("cardExpiry");
    var cardCvv = document.getElementById("cardCvv");
    var cardError = document.getElementById("cardError");

    cardNumber.addEventListener("input", function () {
        var digits = cardNumber.value.replace(/\D/g, "").slice(0, 16);
        cardNumber.value = digits.replace(/(.{4})/g, "$1 ").trim();
    });

    cardExpiry.addEventListener("input", function () {
        var digits = cardExpiry.value.replace(/\D/g, "").slice(0, 4);
        cardExpiry.value = digits.length > 2 ? digits.slice(0, 2) + "/" + digits.slice(2) : digits;
    });

    cardCvv.addEventListener("input", function () {
        cardCvv.value = cardCvv.value.replace(/\D/g, "").slice(0, 4);
    });

    /* ----- Prescription upload ----- */
    var dropzone = document.getElementById("dropzone");
    var prescriptionInput = document.getElementById("prescriptionInput");
    var dropzoneText = document.getElementById("dropzoneText");
    var prescriptionError = document.getElementById("prescriptionError");

    if (dropzone && prescriptionInput) {
        ["dragover", "dragenter"].forEach(function (evt) {
            dropzone.addEventListener(evt, function (e) {
                e.preventDefault();
                dropzone.classList.add("is-dragover");
            });
        });

        ["dragleave", "drop"].forEach(function (evt) {
            dropzone.addEventListener(evt, function () {
                dropzone.classList.remove("is-dragover");
            });
        });

        dropzone.addEventListener("drop", function (e) {
            e.preventDefault();
            if (e.dataTransfer.files.length) {
                prescriptionInput.files = e.dataTransfer.files;
                handleFileSelected(e.dataTransfer.files[0]);
            }
        });

        prescriptionInput.addEventListener("change", function () {
            if (prescriptionInput.files.length) handleFileSelected(prescriptionInput.files[0]);
        });

        function handleFileSelected(file) {
            dropzone.classList.add("has-file");
            dropzoneText.innerHTML = "Attached: <strong>" + file.name + "</strong><br />Click to replace";
            prescriptionError.hidden = true;
        }
    }

    /* ----- Place order ----- */
    var placeOrderBtn = document.getElementById("placeOrderBtn");
    var addressError = document.getElementById("addressError");
    var fullAddress = document.getElementById("fullAddress");
    var phoneNumber = document.getElementById("phoneNumber");
    var phoneError = document.getElementById("phoneError");

    phoneNumber.addEventListener("input", function () {
        phoneNumber.value = phoneNumber.value.replace(/\D/g, "").slice(0, 11);
        if (phoneNumber.value.length === 11) phoneError.hidden = true;
    });

    placeOrderBtn.addEventListener("click", function () {
        var valid = true;

        if (!fullAddress.value.trim()) {
            addressError.hidden = false;
            valid = false;
        } else {
            addressError.hidden = true;
        }

        if (phoneNumber.value.length !== 11) {
            phoneError.hidden = false;
            valid = false;
        } else {
            phoneError.hidden = true;
        }

        if (selectedMethod === "bKash") {
            var bkashOk = bkashNumber.value.length === 11 && bkashTxnId.value.trim().length > 0;
            bkashError.hidden = bkashOk;
            if (!bkashOk) valid = false;
        }

        if (selectedMethod === "Card") {
            var digitsOnly = cardNumber.value.replace(/\D/g, "");
            var expiryOk = /^\d{2}\/\d{2}$/.test(cardExpiry.value);
            var cardOk = cardName.value.trim().length > 0 && digitsOnly.length === 16 && expiryOk && cardCvv.value.length >= 3;
            cardError.hidden = cardOk;
            if (!cardOk) valid = false;
        }

        if (data.requiresPrescription && prescriptionInput && !prescriptionInput.files.length) {
            prescriptionError.hidden = false;
            valid = false;
        }

        if (!valid) return;

        placeOrderBtn.textContent = "Order placed";
        placeOrderBtn.disabled = true;
        showToast("Your order has been placed");
        setTimeout(function () {
            window.location.href = "/Confirmation";
        }, 900);
    });

    function showToast(message) {
        var toast = document.getElementById("toast");
        if (!toast) return;
        toast.querySelector(".toast__text").textContent = message;
        toast.classList.add("is-visible");
        setTimeout(function () { toast.classList.remove("is-visible"); }, 2500);
    }
})();
