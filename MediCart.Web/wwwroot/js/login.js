// MediCart — Login page interactivity
// Vanilla JS, no build step. Drop into wwwroot/js/login.js.

document.addEventListener("DOMContentLoaded", () => {
  const form = document.getElementById("loginForm")
  if (!form) return

  const emailField = document.getElementById("field-Email")
  const passwordField = document.getElementById("field-Password")
  const submitButton = document.getElementById("loginSubmit")

  const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/

  const wrapperOf = (input) => input.closest(".field")

  const setInvalid = (wrapper, invalid) => {
    wrapper.classList.toggle("field--invalid", invalid)
  }

  const triggerShake = (wrapper) => {
    wrapper.classList.remove("field--shake")
    void wrapper.offsetWidth
    wrapper.classList.add("field--shake")
  }

  // ---------------------------------------------------------------------
  // Password visibility toggle
  // ---------------------------------------------------------------------

  const toggleButton = document.querySelector(".field__toggle")
  if (toggleButton) {
    toggleButton.addEventListener("click", () => {
      const isHidden = passwordField.type === "password"
      passwordField.type = isHidden ? "text" : "password"
      toggleButton.setAttribute("aria-pressed", String(isHidden))
      toggleButton.setAttribute("aria-label", isHidden ? "Hide password" : "Show password")
      toggleButton.innerHTML = isHidden ? eyeOffIcon() : eyeIcon()
    })
  }

  // ---------------------------------------------------------------------
  // Light live validation — server remains the source of truth
  // ---------------------------------------------------------------------

  emailField.addEventListener("input", () => {
    const wrapper = wrapperOf(emailField)
    if (emailField.value.trim().length === 0) {
      setInvalid(wrapper, false)
      return
    }
    setInvalid(wrapper, !emailPattern.test(emailField.value.trim()))
  })

  passwordField.addEventListener("input", () => {
    const wrapper = wrapperOf(passwordField)
    setInvalid(wrapper, false)
  })

  // ---------------------------------------------------------------------
  // Submit — client-side gate before the normal Razor POST happens
  // ---------------------------------------------------------------------

  form.addEventListener("submit", (event) => {
    let firstInvalid = null

    const checks = [
      { input: emailField, valid: emailPattern.test(emailField.value.trim()) },
      { input: passwordField, valid: passwordField.value.length > 0 },
    ]

    checks.forEach(({ input, valid }) => {
      const wrapper = wrapperOf(input)
      setInvalid(wrapper, !valid)
      if (!valid) {
        triggerShake(wrapper)
        if (!firstInvalid) firstInvalid = input
      }
    })

    if (firstInvalid) {
      event.preventDefault()
      firstInvalid.focus()
      return
    }

    submitButton.dataset.loading = "true"
    submitButton.disabled = true
  })

  // ---------------------------------------------------------------------
  // Icons
  // ---------------------------------------------------------------------

  function eyeIcon() {
    return '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8"><path d="M1.5 12S5 5 12 5s10.5 7 10.5 7-3.5 7-10.5 7S1.5 12 1.5 12Z"/><circle cx="12" cy="12" r="3"/></svg>'
  }

  function eyeOffIcon() {
    return '<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8"><path d="M3 3l18 18"/><path d="M10.6 5.2A10.9 10.9 0 0 1 12 5c7 0 10.5 7 10.5 7a13.4 13.4 0 0 1-3.1 3.9M6.6 6.6C3.4 8.6 1.5 12 1.5 12s3.5 7 10.5 7c1.5 0 2.8-.3 4-.8"/><path d="M9.9 9.9a3 3 0 0 0 4.2 4.2"/></svg>'
  }
})
