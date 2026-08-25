// MediCart — Register page interactivity
// Vanilla JS, no build step. Safe to drop into wwwroot/js/register.js
// and reference from the Register.cshtml view.

document.addEventListener("DOMContentLoaded", () => {
  const form = document.getElementById("registerForm")
  if (!form) return

  const fullNameField = document.getElementById("field-FullName")
  const phoneField = document.getElementById("field-Phone")
  const emailField = document.getElementById("field-Email")
  const passwordField = document.getElementById("field-Password")
  const confirmField = document.getElementById("field-ConfirmPassword")
  const termsCheckbox = document.getElementById("field-Terms")
  const submitButton = document.getElementById("registerSubmit")
  const stamp = document.getElementById("registerStamp")

  const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
  const phonePattern = /^[0-9+\-\s]{7,15}$/

  // ---------------------------------------------------------------------
  // Helpers
  // ---------------------------------------------------------------------

  const setFieldState = (wrapper, hintEl, state, message) => {
    wrapper.classList.remove("field--valid", "field--invalid")
    if (state === "valid") wrapper.classList.add("field--valid")
    if (state === "invalid") wrapper.classList.add("field--invalid")
    if (hintEl) hintEl.textContent = message || ""
  }

  const triggerShake = (wrapper) => {
    wrapper.classList.remove("field--shake")
    // Force reflow so the animation can restart on repeated invalid submits
    void wrapper.offsetWidth
    wrapper.classList.add("field--shake")
  }

  const wrapperOf = (input) => input.closest(".field")
  const hintOf = (input) => wrapperOf(input).querySelector(".field__hint")

  // ---------------------------------------------------------------------
  // Password visibility toggles
  // ---------------------------------------------------------------------

  const handleToggleVisibility = (event) => {
    const button = event.currentTarget
    const targetInput = document.getElementById(button.dataset.target)
    if (!targetInput) return

    const isHidden = targetInput.type === "password"
    targetInput.type = isHidden ? "text" : "password"
    button.setAttribute("aria-pressed", String(isHidden))
    button.setAttribute("aria-label", isHidden ? "Hide password" : "Show password")
    button.innerHTML = isHidden ? eyeOffIcon() : eyeIcon()
  }

  document.querySelectorAll(".field__toggle").forEach((button) => {
    button.addEventListener("click", handleToggleVisibility)
  })

  // ---------------------------------------------------------------------
  // Full name — simple presence + minimum length
  // ---------------------------------------------------------------------

  const handleFullNameInput = () => {
    const value = fullNameField.value.trim()
    const wrapper = wrapperOf(fullNameField)
    const hint = hintOf(fullNameField)

    if (value.length === 0) {
      setFieldState(wrapper, hint, "", "")
      return
    }

    if (value.length < 2) {
      setFieldState(wrapper, hint, "invalid", "Enter your full name")
      return
    }

    setFieldState(wrapper, hint, "valid", "Looks good")
  }

  // ---------------------------------------------------------------------
  // Phone — light pattern check, not a full validator (backend owns that)
  // ---------------------------------------------------------------------

  const handlePhoneInput = () => {
    const value = phoneField.value.trim()
    const wrapper = wrapperOf(phoneField)
    const hint = hintOf(phoneField)

    if (value.length === 0) {
      setFieldState(wrapper, hint, "", "")
      return
    }

    if (!phonePattern.test(value)) {
      setFieldState(wrapper, hint, "invalid", "Use digits only, e.g. 01XXXXXXXXX")
      return
    }

    setFieldState(wrapper, hint, "valid", "Looks good")
  }

  // ---------------------------------------------------------------------
  // Email
  // ---------------------------------------------------------------------

  const handleEmailInput = () => {
    const value = emailField.value.trim()
    const wrapper = wrapperOf(emailField)
    const hint = hintOf(emailField)

    if (value.length === 0) {
      setFieldState(wrapper, hint, "", "")
      return
    }

    if (!emailPattern.test(value)) {
      setFieldState(wrapper, hint, "invalid", "Enter a valid email address")
      return
    }

    setFieldState(wrapper, hint, "valid", "Looks good")
  }

  // ---------------------------------------------------------------------
  // Password strength — the "dosage vial" meter
  // ---------------------------------------------------------------------

  const strengthTrack = document.getElementById("passwordStrength")
  const strengthLabel = strengthTrack ? strengthTrack.querySelector(".strength__label") : null

  const scorePassword = (value) => {
    let score = 0
    if (value.length >= 8) score += 1
    if (/[A-Z]/.test(value) && /[a-z]/.test(value)) score += 1
    if (/\d/.test(value)) score += 1
    if (/[^A-Za-z0-9]/.test(value)) score += 1
    return score
  }

  const strengthMeta = {
    0: { level: 0, text: "Enter a password" },
    1: { level: 1, text: "Mild — try adding numbers" },
    2: { level: 2, text: "Moderate — add a symbol" },
    3: { level: 3, text: "Strong" },
    4: { level: 3, text: "Strong" },
  }

  const handlePasswordInput = () => {
    const value = passwordField.value
    const wrapper = wrapperOf(passwordField)
    const hint = hintOf(passwordField)

    if (strengthTrack) {
      const meta = strengthMeta[scorePassword(value)]
      strengthTrack.dataset.level = String(meta.level)
      if (strengthLabel) strengthLabel.textContent = value.length === 0 ? "" : meta.text
    }

    if (value.length === 0) {
      setFieldState(wrapper, hint, "", "")
    } else if (value.length < 8) {
      setFieldState(wrapper, hint, "invalid", "Use at least 8 characters")
    } else {
      setFieldState(wrapper, hint, "valid", "")
    }

    // Re-check confirm field whenever password changes, if it has a value
    if (confirmField.value.length > 0) handleConfirmInput()
  }

  // ---------------------------------------------------------------------
  // Confirm password — live match check
  // ---------------------------------------------------------------------

  const handleConfirmInput = () => {
    const wrapper = wrapperOf(confirmField)
    const hint = hintOf(confirmField)

    if (confirmField.value.length === 0) {
      setFieldState(wrapper, hint, "", "")
      return
    }

    if (confirmField.value !== passwordField.value) {
      setFieldState(wrapper, hint, "invalid", "Passwords don't match yet")
      return
    }

    setFieldState(wrapper, hint, "valid", "Passwords match")
  }

  // ---------------------------------------------------------------------
  // Wire up live listeners
  // ---------------------------------------------------------------------

  fullNameField.addEventListener("input", handleFullNameInput)
  phoneField.addEventListener("input", handlePhoneInput)
  emailField.addEventListener("input", handleEmailInput)
  passwordField.addEventListener("input", handlePasswordInput)
  confirmField.addEventListener("input", handleConfirmInput)

  // ---------------------------------------------------------------------
  // Submit — client-side gate before the normal Razor POST happens.
  // Server-side validation (via [Required], [EmailAddress], etc. on the
  // RegisterViewModel) remains the source of truth; this only improves
  // the moment-to-moment experience.
  // ---------------------------------------------------------------------

  const handleSubmit = (event) => {
    const checks = [
      { input: fullNameField, valid: fullNameField.value.trim().length >= 2, message: "Enter your full name" },
      { input: phoneField, valid: phonePattern.test(phoneField.value.trim()), message: "Enter a valid phone number" },
      { input: emailField, valid: emailPattern.test(emailField.value.trim()), message: "Enter a valid email address" },
      { input: passwordField, valid: passwordField.value.length >= 8, message: "Use at least 8 characters" },
      { input: confirmField, valid: confirmField.value === passwordField.value && confirmField.value.length > 0, message: "Passwords don't match" },
    ]

    let firstInvalid = null

    checks.forEach(({ input, valid, message }) => {
      const wrapper = wrapperOf(input)
      const hint = hintOf(input)
      if (!valid) {
        setFieldState(wrapper, hint, "invalid", message)
        triggerShake(wrapper)
        if (!firstInvalid) firstInvalid = input
      }
    })

    if (!termsCheckbox.checked) {
      triggerShake(termsCheckbox.closest(".register__terms"))
      if (!firstInvalid) firstInvalid = termsCheckbox
    }

    if (firstInvalid) {
      event.preventDefault()
      firstInvalid.focus()
      return
    }

    // All good — let the form POST to the controller normally, but show a
    // loading state on the button so the click feels acknowledged.
    submitButton.dataset.loading = "true"
    submitButton.disabled = true
  }

  form.addEventListener("submit", handleSubmit)

  // ---------------------------------------------------------------------
  // Icons (inline, no icon font dependency)
  // ---------------------------------------------------------------------

  function eyeIcon() {
    return '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8"><path d="M1.5 12S5 5 12 5s10.5 7 10.5 7-3.5 7-10.5 7S1.5 12 1.5 12Z"/><circle cx="12" cy="12" r="3"/></svg>'
  }

  function eyeOffIcon() {
    return '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8"><path d="M3 3l18 18"/><path d="M10.6 5.2A10.9 10.9 0 0 1 12 5c7 0 10.5 7 10.5 7a13.4 13.4 0 0 1-3.1 3.9M6.6 6.6C3.4 8.6 1.5 12 1.5 12s3.5 7 10.5 7c1.5 0 2.8-.3 4-.8"/><path d="M9.9 9.9a3 3 0 0 0 4.2 4.2"/></svg>'
  }
})
