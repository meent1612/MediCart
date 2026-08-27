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
    paymentOptions.addEventListener("click", function (e) {
        var btn = e.target.closest(".payment-option");
        if (!btn) return;
        paymentOptions.querySelectorAll(".payment-option").forEach(function (b) { b.classList.remove("is-selected"); });
        btn.classList.add("is-selected");
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

    placeOrderBtn.addEventListener("click", function () {
        var valid = true;

        if (!fullAddress.value.trim() || !phoneNumber.value.trim()) {
            addressError.hidden = false;
            valid = false;
        } else {
            addressError.hidden = true;
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
