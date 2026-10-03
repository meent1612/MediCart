# Real-Time Search Suggestions Architecture

## 1. Overview
The search suggestion system provides fast, interactive medicine lookup on the MediCart landing page.
Users receive instant suggestions for brand names, generic names, and manufacturers as they type.

## 2. Debouncing and Query Throttling
- A 180ms debounce window prevents hammering the backend with keystrokes.
- Queries with length < 2 characters are suppressed to avoid high-cardinality result sets.
- Previous pending in-flight requests are automatically aborted via `AbortController`.

## 3. Keyboard Navigation and Interaction Standards
- `ArrowDown`: Moves highlight to next suggestion item.
- `ArrowUp`: Moves highlight to previous suggestion item or back to input.
- `Enter`: Navigates directly to the highlighted medicine details modal or triggers general search.
- `Escape`: Closes the suggestions dropdown and keeps current text.

## 4. EF Core ILike Case-Insensitive Matching
The query uses PostgreSQL `EF.Functions.ILike` on `Name`, `GenericName`, and `Manufacturer`.
Matches starting with the search prefix are weighted first, followed by substring matches.

