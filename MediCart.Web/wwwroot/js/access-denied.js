/**
 * MediCart — Access Denied Page Interactivity
 * - Lottie animation player with autoplay & loop
 * - Framing and graceful fallback handling
 * - Interactive speedup on hover / click with speech bubble bounce
 * - Typewriter effect for "Access denied" heading
 * - Staggered entrance of subtext & action buttons
 * - Full prefers-reduced-motion support
 */

(function () {
    'use strict';

    document.addEventListener('DOMContentLoaded', function () {
        var lottieContainer = document.getElementById('baymaxLottie');
        var characterWrapper = document.getElementById('baymaxCharacterWrapper');
        var speechBubble = document.getElementById('speechBubble');
        var heading = document.getElementById('bubbleHeading');
        var messageSection = document.getElementById('messageSection');
        var actionSection = document.getElementById('actionSection');

        // Check user motion preferences
        var prefersReducedMotion = window.matchMedia &&
            window.matchMedia('(prefers-reduced-motion: reduce)').matches;

        // Retrieve animation JSON path
        var animPath = '';
        if (lottieContainer) {
            animPath = lottieContainer.getAttribute('data-lottie-path');
        }
        if (!animPath) {
            var scriptTag = document.querySelector('script[data-animation-path]');
            if (scriptTag) {
                animPath = scriptTag.getAttribute('data-animation-path');
            }
        }
        if (!animPath) {
            animPath = '/animations/baymax-robo-medic.json';
        }

        // -------------------------------------------------------------
        // 1. Fallback Renderer
        // -------------------------------------------------------------
        function renderFallback() {
            if (!lottieContainer) return;
            lottieContainer.innerHTML = [
                '<div class="access-denied__fallback">',
                '    <svg viewBox="0 0 200 200" fill="none" class="access-denied__fallback-svg" aria-label="Baymax care companion">',
                '        <circle cx="100" cy="100" r="76" fill="#FFFFFF" stroke="#859B77" stroke-width="5" />',
                '        <ellipse cx="100" cy="88" rx="44" ry="28" fill="#FFFFFF" stroke="#152E19" stroke-width="3.5" />',
                '        <circle cx="82" cy="88" r="5" fill="#152E19" />',
                '        <circle cx="118" cy="88" r="5" fill="#152E19" />',
                '        <line x1="82" y1="88" x2="118" y2="88" stroke="#152E19" stroke-width="2.5" />',
                '        <circle cx="132" cy="132" r="11" fill="#ECCBD9" stroke="#152E19" stroke-width="2" />',
                '        <path d="M127 132h10M132 127v10" stroke="#152E19" stroke-width="2" stroke-linecap="round" />',
                '    </svg>',
                '</div>'
            ].join('\n');
        }

        // -------------------------------------------------------------
        // 2. Initialize Lottie Animation
        // -------------------------------------------------------------
        var anim = null;
        var isSpeeding = false;

        if (typeof lottie !== 'undefined' && lottieContainer) {
            try {
                anim = lottie.loadAnimation({
                    container: lottieContainer,
                    renderer: 'svg',
                    loop: true,
                    autoplay: !prefersReducedMotion,
                    path: animPath,
                    rendererSettings: {
                        preserveAspectRatio: 'xMidYMid meet',
                        progressiveLoad: true,
                        hideOnTransparent: false
                    }
                });

                if (prefersReducedMotion) {
                    anim.addEventListener('DOMLoaded', function () {
                        try {
                            anim.goToAndStop(0, true);
                        } catch (e) { }
                    });
                }

                anim.addEventListener('data_failed', function () {
                    renderFallback();
                });

                anim.addEventListener('error', function () {
                    renderFallback();
                });
            } catch (err) {
                renderFallback();
            }
        } else {
            renderFallback();
        }

        // -------------------------------------------------------------
        // 3. Hover / Click Reaction (Speedup + Speech Bubble Bounce)
        // -------------------------------------------------------------
        function triggerBaymaxReaction() {
            if (prefersReducedMotion) return;

            // Speed up Lottie animation temporarily
            if (anim && !isSpeeding) {
                isSpeeding = true;
                anim.setSpeed(1.6);
                setTimeout(function () {
                    if (anim) {
                        anim.setSpeed(1.0);
                    }
                    isSpeeding = false;
                }, 850);
            }

            // Bounce the speech bubble
            if (speechBubble) {
                speechBubble.classList.remove('is-bouncing');
                // Trigger reflow to restart animation smoothly
                void speechBubble.offsetWidth;
                speechBubble.classList.add('is-bouncing');
            }
        }

        if (characterWrapper) {
            characterWrapper.addEventListener('mouseenter', triggerBaymaxReaction);
            characterWrapper.addEventListener('click', triggerBaymaxReaction);

            // Keyboard accessibility (Space or Enter on Baymax)
            characterWrapper.addEventListener('keydown', function (e) {
                if (e.key === 'Enter' || e.key === ' ') {
                    e.preventDefault();
                    triggerBaymaxReaction();
                }
            });
        }

        // -------------------------------------------------------------
        // 4. Staggered Reveal of Subtext and Action Buttons
        // -------------------------------------------------------------
        function revealSubtextAndActions() {
            if (messageSection) {
                messageSection.classList.add('is-visible');
            }
            setTimeout(function () {
                if (actionSection) {
                    actionSection.classList.add('is-visible');
                }
            }, 140);
        }

        // -------------------------------------------------------------
        // 5. Typewriter Effect for Heading
        // -------------------------------------------------------------
        if (heading) {
            var fullText = (heading.textContent || 'Access denied').trim();

            if (prefersReducedMotion) {
                heading.textContent = fullText;
                revealSubtextAndActions();
            } else {
                // Keep the original text in aria-label for accessibility & screen readers
                heading.setAttribute('aria-label', fullText);
                heading.textContent = '';

                // Create blinking cursor element
                var cursor = document.createElement('span');
                cursor.className = 'access-denied__cursor';
                cursor.textContent = '|';
                cursor.setAttribute('aria-hidden', 'true');
                if (heading.parentNode) {
                    heading.parentNode.insertBefore(cursor, heading.nextSibling);
                }

                var charIndex = 0;
                var typingSpeed = 60; // 60ms per character as specified

                var typingInterval = setInterval(function () {
                    if (charIndex < fullText.length) {
                        heading.textContent += fullText.charAt(charIndex);
                        charIndex++;
                    } else {
                        clearInterval(typingInterval);
                        // Remove typing cursor after a brief pause
                        setTimeout(function () {
                            if (cursor && cursor.parentNode) {
                                cursor.parentNode.removeChild(cursor);
                            }
                            revealSubtextAndActions();
                        }, 220);
                    }
                }, typingSpeed);
            }
        } else {
            revealSubtextAndActions();
        }
    });
})();
