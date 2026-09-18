// MediCart — Shared Cart & Authorization Handler
// Handles Add-to-Cart logic, auth checks, and admin-order restrictions uniformly across all entry points.
(function () {
    "use strict";

    function getAuthState() {
        var body = document.body;
        return {
            isAdmin: body.getAttribute("data-is-admin") === "true",
            isCustomer: body.getAttribute("data-is-customer") === "true",
            isAuthenticated: body.getAttribute("data-is-authenticated") === "true",
            loginUrl: body.getAttribute("data-login-url") || "/Identity/Account/Login"
        };
    }

    function getAntiForgeryToken() {
        var input = document.querySelector('input[name="__RequestVerificationToken"]');
        return input ? input.value : "";
    }

    function showToast(message) {
        var toast = document.getElementById("toast");
        if (!toast) return;
        var textEl = toast.querySelector(".toast__text");
        if (textEl) textEl.textContent = message;

        toast.classList.add("is-visible");
        clearTimeout(showToast._timer);
        showToast._timer = setTimeout(function () {
            toast.classList.remove("is-visible");
        }, 2400);
    }

    function updateCartBadge(cartItemCount) {
        var cartButton = document.querySelector(".cart-button");
        var cartBadge = document.getElementById("cartBadge") || document.querySelector(".cart-button__badge");

        if (cartBadge) {
            cartBadge.textContent = String(cartItemCount);
            if (cartItemCount > 0) {
                cartBadge.classList.add("is-visible");
            } else {
                cartBadge.classList.remove("is-visible");
            }
        }

        if (cartButton) {
            cartButton.classList.remove("is-bumped");
            void cartButton.offsetWidth;
            cartButton.classList.add("is-bumped");
        }
    }

    function redirectToLogin(customReturnUrl) {
        var auth = getAuthState();
        var returnUrl = customReturnUrl || (window.location.pathname + window.location.search);
        window.location.href = auth.loginUrl + "?ReturnUrl=" + encodeURIComponent(returnUrl);
    }

    function add(options) {
        options = options || {};
        var auth = getAuthState();
        var medicineId = options.medicineId;
        var quantity = options.quantity || 1;
        var btn = options.button;

        // 1. Role Check: Admin cannot place orders
        if (auth.isAdmin) {
            showToast("Admins cannot place orders");
            if (typeof options.onError === "function") {
                options.onError("Admins cannot place orders");
            }
            return;
        }

        // 2. Auth Check: Guest / not logged in -> redirect to Login page with ReturnUrl
        if (!auth.isAuthenticated || !auth.isCustomer) {
            redirectToLogin(options.returnUrl);
            return;
        }

        // 3. Customer: call POST /Cart/Add
        if (btn) btn.disabled = true;

        var body = new URLSearchParams();
        body.set("medicineId", medicineId);
        body.set("quantity", quantity);
        body.set("__RequestVerificationToken", getAntiForgeryToken());

        fetch("/Cart/Add", {
            method: "POST",
            body: body
        })
        .then(function (res) {
            if (res.redirected || res.status === 401 || res.status === 403) {
                redirectToLogin(options.returnUrl);
                return Promise.reject(null);
            }

            return res.json().then(function (data) {
                if (!res.ok) {
                    return Promise.reject(data.error || "Could not add this to your cart.");
                }
                return data;
            });
        })
        .then(function (data) {
            if (btn) btn.disabled = false;
            updateCartBadge(data.cartItemCount);
            showToast(quantity === 1 ? "Added to cart" : quantity + " items added to cart");
            if (typeof options.onSuccess === "function") {
                options.onSuccess(data);
            }
        })
        .catch(function (err) {
            if (btn) btn.disabled = false;
            if (err) {
                showToast(err);
                if (typeof options.onError === "function") {
                    options.onError(err);
                }
            }
        });
    }

    window.MediCartCart = {
        add: add,
        getAuthState: getAuthState,
        redirectToLogin: redirectToLogin,
        showToast: showToast,
        updateCartBadge: updateCartBadge
    };
})();
