(function () {
    "use strict";

    var checkoutDataEl = document.getElementById("checkoutData");
    var divisionsDataEl = document.getElementById("divisionsData");
    if (!checkoutDataEl || !divisionsDataEl) return;

    var checkoutData = JSON.parse(checkoutDataEl.textContent);
    var divisions    = JSON.parse(divisionsDataEl.textContent);
    var subtotal     = checkoutData.subtotal;

    // ── Antiforgery token ────────────────────────────────────────────────────
    function getToken() {
        var input = document.querySelector('input[name="__RequestVerificationToken"]');
        return input ? input.value : "";
    }

    function postForm(url, data) {
        var params = new URLSearchParams(data);
        params.append("__RequestVerificationToken", getToken());
        return fetch(url, {
            method: "POST",
            headers: { "Content-Type": "application/x-www-form-urlencoded" },
            body: params.toString()
        });
    }

    function postJson(url, body) {
        return fetch(url, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "RequestVerificationToken": getToken()
            },
            body: JSON.stringify(body)
        });
    }

    // ── 10-minute checkout timer ─────────────────────────────────────────────
    var timerDisplay  = document.getElementById("timerDisplay");
    var timerBanner   = document.getElementById("checkoutTimer");
    var placeOrderBtn = document.getElementById("placeOrderBtn");
    var secondsLeft   = 600;   // 10 minutes
    var timerExpired  = false;

    function formatTime(s) {
        var m = Math.floor(s / 60);
        var sec = s % 60;
        return (m < 10 ? "0" : "") + m + ":" + (sec < 10 ? "0" : "") + sec;
    }

    var timerInterval = setInterval(function () {
        secondsLeft--;
        if (timerDisplay) timerDisplay.textContent = formatTime(secondsLeft);

        if (secondsLeft <= 60 && timerBanner) {
            timerBanner.classList.add("is-warning");
        }

        if (secondsLeft <= 0) {
            clearInterval(timerInterval);
            timerExpired = true;
            if (timerDisplay) timerDisplay.textContent = "00:00";
            showTimerExpiredModal();
        }
    }, 1000);

    function showTimerExpiredModal() {
        var modal = document.getElementById("timerExpiredModal");
        if (modal) modal.hidden = false;
        if (placeOrderBtn) placeOrderBtn.disabled = true;
    }

    var goToCartBtn = document.getElementById("goToCartBtn");
    if (goToCartBtn) {
        goToCartBtn.addEventListener("click", function () {
            window.location.href = "/Cart";
        });
    }

    // ── Division / City dropdowns (from DB data) ─────────────────────────────
    var divisionSelect         = document.getElementById("division");
    var citySelect             = document.getElementById("city");
    var deliveryDivisionLabel  = document.getElementById("deliveryDivisionLabel");
    var summaryDelivery        = document.getElementById("summaryDelivery");
    var summaryTotal           = document.getElementById("summaryTotal");

    var selectedDivisionId = 0;
    var selectedCityId     = 0;

    function populateDivisions() {
        divisionSelect.innerHTML = "";
        divisions.forEach(function (div) {
            var opt = document.createElement("option");
            opt.value = div.id;
            opt.textContent = div.name;
            divisionSelect.appendChild(opt);
        });
    }

    function populateCities(divisionId) {
        citySelect.innerHTML = "";
        var div = divisions.find(function (d) { return d.id === divisionId; });
        if (!div) return;
        div.cities.forEach(function (city) {
            var opt = document.createElement("option");
            opt.value = city.id;
            opt.textContent = city.name;
            citySelect.appendChild(opt);
        });
        selectedCityId = div.cities.length > 0 ? div.cities[0].id : 0;
    }

    function updateDelivery() {
        var divId = parseInt(divisionSelect.value, 10);
        var div = divisions.find(function (d) { return d.id === divId; });
        if (!div) return;
        selectedDivisionId = div.id;
        if (deliveryDivisionLabel) deliveryDivisionLabel.textContent = div.name;
        summaryDelivery.textContent = "\u09F3" + div.deliveryCharge.toFixed(0);
        summaryTotal.textContent    = "\u09F3" + (subtotal + div.deliveryCharge).toFixed(0);
    }

    divisionSelect.addEventListener("change", function () {
        var divId = parseInt(divisionSelect.value, 10);
        populateCities(divId);
        updateDelivery();
    });

    citySelect.addEventListener("change", function () {
        selectedCityId = parseInt(citySelect.value, 10);
    });

    // Initial population
    populateDivisions();
    if (divisions.length > 0) {
        populateCities(divisions[0].id);
        updateDelivery();
    }

    // ── Phone number — digits only ───────────────────────────────────────────
    var phoneNumber = document.getElementById("phoneNumber");
    var phoneError  = document.getElementById("phoneError");

    phoneNumber.addEventListener("input", function () {
        phoneNumber.value = phoneNumber.value.replace(/\D/g, "").slice(0, 11);
        if (phoneNumber.value.length === 11) phoneError.hidden = true;
    });

    phoneNumber.addEventListener("keydown", function (e) {
        var allowed = ["Backspace","Delete","ArrowLeft","ArrowRight","Tab"];
        if (allowed.indexOf(e.key) === -1 && !/^\d$/.test(e.key)) e.preventDefault();
    });

    // ── Payment method tabs ──────────────────────────────────────────────────
    var paymentOptions = document.getElementById("paymentOptions");
    var bkashFields    = document.getElementById("bkashFields");
    var cardFields     = document.getElementById("cardFields");
    var selectedMethod = "Cash on delivery";

    paymentOptions.addEventListener("click", function (e) {
        var btn = e.target.closest(".payment-option");
        if (!btn) return;
        paymentOptions.querySelectorAll(".payment-option")
            .forEach(function (b) { b.classList.remove("is-selected"); });
        btn.classList.add("is-selected");
        selectedMethod = btn.dataset.method;
        bkashFields.hidden = selectedMethod !== "bKash";
        cardFields.hidden  = selectedMethod !== "Card";
    });

    // ── bKash number — digits only ───────────────────────────────────────────
    var bkashNumber      = document.getElementById("bkashNumber");
    var bkashNumberError = document.getElementById("bkashNumberError");

    if (bkashNumber) {
        bkashNumber.addEventListener("input", function () {
            bkashNumber.value = bkashNumber.value.replace(/\D/g, "").slice(0, 11);
            if (bkashNumber.value.length === 11) bkashNumberError.hidden = true;
        });
        bkashNumber.addEventListener("keydown", function (e) {
            var allowed = ["Backspace","Delete","ArrowLeft","ArrowRight","Tab"];
            if (allowed.indexOf(e.key) === -1 && !/^\d$/.test(e.key)) e.preventDefault();
        });
    }

    // ── OTP flow ─────────────────────────────────────────────────────────────
    var sendOtpBtn       = document.getElementById("sendOtpBtn");
    var otpVerifySection = document.getElementById("otpVerifySection");
    var pinSection       = document.getElementById("pinSection");
    var otpInput         = document.getElementById("otpInput");
    var otpError         = document.getElementById("otpError");
    var verifyOtpBtn     = document.getElementById("verifyOtpBtn");
    var resendOtpBtn     = document.getElementById("resendOtpBtn");
    var otpTimerDisplay  = document.getElementById("otpTimerDisplay");
    var bkashPin         = document.getElementById("bkashPin");
    var pinError         = document.getElementById("pinError");
    var bkashVerified    = document.getElementById("bkashVerified");
    var bkashError       = document.getElementById("bkashError");

    var otpVerified  = false;
    var otpCountdown = null;

    // OTP input — digits only
    if (otpInput) {
        otpInput.addEventListener("input", function () {
            otpInput.value = otpInput.value.replace(/\D/g, "").slice(0, 5);
        });
        otpInput.addEventListener("keydown", function (e) {
            var allowed = ["Backspace","Delete","ArrowLeft","ArrowRight","Tab"];
            if (allowed.indexOf(e.key) === -1 && !/^\d$/.test(e.key)) e.preventDefault();
        });
    }

    // PIN input — digits only
    if (bkashPin) {
        bkashPin.addEventListener("input", function () {
            bkashPin.value = bkashPin.value.replace(/\D/g, "").slice(0, 6);
            if (bkashPin.value.length === 6) pinError.hidden = true;
        });
        bkashPin.addEventListener("keydown", function (e) {
            var allowed = ["Backspace","Delete","ArrowLeft","ArrowRight","Tab"];
            if (allowed.indexOf(e.key) === -1 && !/^\d$/.test(e.key)) e.preventDefault();
        });
    }

    function startOtpCountdown() {
        var seconds = 30;
        if (otpTimerDisplay) otpTimerDisplay.textContent = "(" + seconds + "s)";
        if (resendOtpBtn) resendOtpBtn.disabled = true;

        clearInterval(otpCountdown);
        otpCountdown = setInterval(function () {
            seconds--;
            if (otpTimerDisplay) otpTimerDisplay.textContent = "(" + seconds + "s)";
            if (seconds <= 0) {
                clearInterval(otpCountdown);
                if (otpTimerDisplay) otpTimerDisplay.textContent = "(expired)";
                if (resendOtpBtn)    resendOtpBtn.disabled = false;
            }
        }, 1000);
    }

    if (sendOtpBtn) {
        sendOtpBtn.addEventListener("click", function () {
            if (!bkashNumber || bkashNumber.value.length !== 11) {
                bkashNumberError.hidden = false;
                return;
            }
            bkashNumberError.hidden = true;
            sendOtpBtn.disabled = true;
            sendOtpBtn.textContent = "Sending…";

            postForm("/Checkout/SendOtp", {})
            .then(function (res) {
                sendOtpBtn.textContent = "Send OTP";
                sendOtpBtn.disabled = false;
                if (!res.ok) {
                    return res.json().then(function (d) {
                        showToast(d.error || "Could not send OTP.");
                    });
                }
                otpVerifySection.hidden = false;
                otpInput.value = "";
                otpError.hidden = true;
                startOtpCountdown();
                showToast("OTP sent to your email.");
            }).catch(function () {
                sendOtpBtn.textContent = "Send OTP";
                sendOtpBtn.disabled = false;
                showToast("Could not send OTP. Please try again.");
            });
        });
    }

    if (resendOtpBtn) {
        resendOtpBtn.addEventListener("click", function () {
            postForm("/Checkout/SendOtp", {})
            .then(function (res) {
                if (!res.ok) {
                    return res.json().then(function (d) {
                        showToast(d.error || "Could not resend OTP.");
                    });
                }
                otpInput.value = "";
                otpError.hidden = true;
                startOtpCountdown();
                showToast("New OTP sent to your email.");
            }).catch(function () {
                showToast("Could not resend OTP. Please try again.");
            });
        });
    }

    if (verifyOtpBtn) {
        verifyOtpBtn.addEventListener("click", function () {
            var code = otpInput ? otpInput.value.trim() : "";
            if (code.length !== 5) {
                otpError.hidden = false;
                return;
            }
            verifyOtpBtn.disabled = true;
            verifyOtpBtn.textContent = "Verifying…";

            postJson("/Checkout/VerifyOtp", { code: code })
            .then(function (res) {
                verifyOtpBtn.textContent = "Verify OTP";
                verifyOtpBtn.disabled = false;
                if (!res.ok) {
                    otpError.hidden = false;
                    return;
                }
                otpError.hidden = true;
                otpVerified = true;
                clearInterval(otpCountdown);
                otpVerifySection.hidden = true;
                pinSection.hidden = false;
                if (bkashVerified) bkashVerified.hidden = false;
            }).catch(function () {
                verifyOtpBtn.textContent = "Verify OTP";
                verifyOtpBtn.disabled = false;
                showToast("Verification failed. Please try again.");
            });
        });
    }

    // ── Card fields — digits only ────────────────────────────────────────────
    var cardName   = document.getElementById("cardName");
    var cardNumber = document.getElementById("cardNumber");
    var cardExpiry = document.getElementById("cardExpiry");
    var cardCvv    = document.getElementById("cardCvv");
    var cardError  = document.getElementById("cardError");

    if (cardNumber) {
        cardNumber.addEventListener("input", function () {
            var digits = cardNumber.value.replace(/\D/g, "").slice(0, 16);
            cardNumber.value = digits.replace(/(.{4})/g, "$1 ").trim();
        });
        cardNumber.addEventListener("keydown", function (e) {
            var allowed = ["Backspace","Delete","ArrowLeft","ArrowRight","Tab"," "];
            if (allowed.indexOf(e.key) === -1 && !/^\d$/.test(e.key)) e.preventDefault();
        });
    }

    if (cardExpiry) {
        cardExpiry.addEventListener("input", function () {
            var digits = cardExpiry.value.replace(/\D/g, "").slice(0, 4);
            cardExpiry.value = digits.length > 2
                ? digits.slice(0, 2) + "/" + digits.slice(2)
                : digits;
        });
        cardExpiry.addEventListener("keydown", function (e) {
            var allowed = ["Backspace","Delete","ArrowLeft","ArrowRight","Tab","/"];
            if (allowed.indexOf(e.key) === -1 && !/^\d$/.test(e.key)) e.preventDefault();
        });
    }

    if (cardCvv) {
        cardCvv.addEventListener("input", function () {
            cardCvv.value = cardCvv.value.replace(/\D/g, "").slice(0, 3);
        });
        cardCvv.addEventListener("keydown", function (e) {
            var allowed = ["Backspace","Delete","ArrowLeft","ArrowRight","Tab"];
            if (allowed.indexOf(e.key) === -1 && !/^\d$/.test(e.key)) e.preventDefault();
        });
    }

    // ── Prescription upload ──────────────────────────────────────────────────
    var dropzone          = document.getElementById("dropzone");
    var prescriptionInput = document.getElementById("prescriptionInput");
    var dropzoneText      = document.getElementById("dropzoneText");
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
            if (prescriptionInput.files.length)
                handleFileSelected(prescriptionInput.files[0]);
        });
        function handleFileSelected(file) {
            dropzone.classList.add("has-file");
            dropzoneText.innerHTML =
                "Attached: <strong>" + file.name + "</strong><br />Click to replace";
            if (prescriptionError) prescriptionError.hidden = true;
        }
    }

    // ── Place Order ──────────────────────────────────────────────────────────
    var addressError = document.getElementById("addressError");
    var fullAddress  = document.getElementById("fullAddress");

    if (placeOrderBtn) {
        placeOrderBtn.addEventListener("click", function () {
            if (timerExpired) return;

            var valid = true;

            // Address
            if (!fullAddress.value.trim()) {
                addressError.hidden = false;
                valid = false;
            } else {
                addressError.hidden = true;
            }

            // Phone
            if (phoneNumber.value.length !== 11) {
                phoneError.hidden = false;
                valid = false;
            } else {
                phoneError.hidden = true;
            }

            // bKash
            if (selectedMethod === "bKash") {
                if (bkashNumber.value.length !== 11) {
                    bkashNumberError.hidden = false;
                    valid = false;
                }
                if (!otpVerified) {
                    bkashError.hidden = false;
                    valid = false;
                } else {
                    bkashError.hidden = true;
                }
                if (bkashPin && bkashPin.value.length !== 6) {
                    pinError.hidden = false;
                    valid = false;
                } else if (pinError) {
                    pinError.hidden = true;
                }
            }

            // Card
            if (selectedMethod === "Card") {
                var digitsOnly = cardNumber ? cardNumber.value.replace(/\D/g, "") : "";
                var expiryOk   = cardExpiry && /^\d{2}\/\d{2}$/.test(cardExpiry.value);
                var cardOk     = cardName && cardName.value.trim().length > 0
                              && digitsOnly.length === 16
                              && expiryOk
                              && cardCvv && cardCvv.value.length === 3;
                if (cardError) cardError.hidden = !!cardOk;
                if (!cardOk) valid = false;
            }

            // Prescription
            if (checkoutData.requiresPrescription
                    && prescriptionInput
                    && !prescriptionInput.files.length) {
                if (prescriptionError) prescriptionError.hidden = false;
                valid = false;
            }

            if (!valid) return;

            // Build multipart form for prescription file upload
            placeOrderBtn.disabled = true;
            placeOrderBtn.textContent = "Placing order…";
            if (window.showLoading) {
                window.showLoading("Placing your order & notifying our pharmacist…");
            }

            var divisionId = parseInt(divisionSelect.value, 10);
            var cityId     = parseInt(citySelect.value, 10);

            var formData = new FormData();
            formData.append("divisionId",     divisionId);
            formData.append("cityId",         cityId);
            formData.append("addressLine",    fullAddress.value.trim());
            formData.append("phone",          phoneNumber.value);
            formData.append("paymentMethod",  selectedMethod);
            formData.append("__RequestVerificationToken", getToken());

            if (prescriptionInput && prescriptionInput.files.length) {
                formData.append("prescriptionFile", prescriptionInput.files[0]);
            }

            fetch("/Checkout/PlaceOrder", {
                method: "POST",
                body: formData
            }).then(function (res) {
                return res.json().then(function (d) {
                    if (!res.ok) {
                        if (window.hideLoading) window.hideLoading();
                        placeOrderBtn.disabled = false;
                        placeOrderBtn.textContent = "Place order";
                        showToast(d.error || "Could not place order. Please try again.");
                        return;
                    }
                    clearInterval(timerInterval);
                    window.location.href = "/Confirmation/" + d.orderId;
                });
            }).catch(function () {
                if (window.hideLoading) window.hideLoading();
                placeOrderBtn.disabled = false;
                placeOrderBtn.textContent = "Place order";
                showToast("Something went wrong. Please try again.");
            });
        });
    }

    function showToast(message) {
        var toast = document.getElementById("toast");
        if (!toast) return;
        toast.querySelector(".toast__text").textContent = message;
        toast.classList.add("is-visible");
        clearTimeout(showToast._t);
        showToast._t = setTimeout(function () {
            toast.classList.remove("is-visible");
        }, 2500);
    }
})();