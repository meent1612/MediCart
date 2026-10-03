# Accessibility Audit (WCAG 2.1 AA)

## 1. Executive Summary
The search suggestion component follows the W3C WAI-ARIA Combobox design pattern.

## 2. ARIA Roles & State Management
- Input has `role="combobox"`, `aria-autocomplete="list"`, `aria-expanded="true/false"`.
- Suggestions container has `role="listbox"`.
- Each suggestion item has `role="option"` with dynamic `aria-selected="true"`.

