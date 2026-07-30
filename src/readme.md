# Engineering Calculator

A built-in engineering calculator for ENCY 3. It adds a **Calculator** button to the utilities
menu; clicking it opens a compact, ENCY-styled calculator window that stays open while you keep
working in ENCY (non-modal).

## Features

- **Arithmetic:** `+  −  ×  ÷`, parentheses, percent, factorial (`n!`)
- **Powers & roots:** `x^y`, `x²`, square root, cube root
- **Trigonometry:** `sin cos tan` and inverse `asin acos atan`, switchable **DEG / RAD**
- **Logarithms & exp:** natural `ln`, base-10 `log`, `exp`
- **Constants:** `π`, `e`
- **Free-form entry:** type a whole expression such as `2*(3+4)^2 - sqrt(81)` and press `=` / Enter

## How to use

1. Open the utilities menu in ENCY and click **Calculator**.
2. Enter an expression with the on-screen keys or straight from the keyboard.
3. Press `=` or **Enter** to evaluate. `Esc` or **C** clears; `⌫` deletes the last character.
4. Toggle **DEG / RAD** to choose how angles are interpreted in trig functions.

## Supported functions

`sin cos tan asin acos atan sqrt cbrt ln log exp abs` — plus operators `+ - * / ^ %` and `!`.

## Notes

- The calculator is self-contained and does not modify your project or geometry.
- Window is non-modal, so it can stay open next to your work.
