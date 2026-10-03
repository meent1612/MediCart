# Real-Time Search Suggestions Architecture

## 1. Overview
The search suggestion system provides fast, interactive medicine lookup on the MediCart landing page.
Users receive instant suggestions for brand names, generic names, and manufacturers as they type.

## 2. Debouncing and Query Throttling
- A 180ms debounce window prevents hammering the backend with keystrokes.
- Queries with length < 2 characters are suppressed to avoid high-cardinality result sets.
- Previous pending in-flight requests are automatically aborted via `AbortController`.

