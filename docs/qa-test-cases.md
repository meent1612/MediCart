# QA Test Cases - Real-Time Search Suggestions

## Test Matrix Overview
This document outlines manual and automated verification procedures for the search feature.

## TC-01: Partial Query Matching
- **Action**: Type 'nap' in `#heroSearchInput`.
- **Expected**: Suggestions contain 'Napa', 'Napa Extra', 'Napa Extend'.
- **Status**: PASSED.

## TC-02: Keyboard Navigation & Enter Selection
- **Action**: Press `ArrowDown` once, verify first result highlighted. Press `Enter`.
- **Expected**: URL navigates to `/Medicines/Browse?openDetails={id}`.
- **Status**: PASSED.

## TC-03: Clear Button & Click Outside Dismissal
- **Action**: Click `✕` clear button.
- **Expected**: Input clears, suggestions close, focus remains on input.
- **Status**: PASSED.

