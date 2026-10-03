# Friendly Access Denied (Baymax Experience)

## 1. User Experience Design
Instead of a cold 403 error page, users meet Baymax (MediCart's healthcare companion robot).
Baymax delivers a gentle message explaining that the requested page is restricted.

## 2. Lottie Asset Integration
- Uses `@lottiefiles/lottie-player` with `baymax-robo-medic.json`.
- Automatic graceful fallback to SVG illustration if WebGL / Canvas is unavailable.

