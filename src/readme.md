# Taper Calculator

A shop-floor geometry helper for ENCY 3. It adds a **Taper Calculator** button to the utilities
menu; clicking it opens a compact, ENCY-styled window that stays open while you keep working in
ENCY (non-modal).

## Features

- **Taper / Cone:** enter any three of {big diameter D, small diameter d, length L, half angle α}
  and it solves the fourth, plus the included angle and the taper ratio `1 : X`.
- **Right triangle:** enter two sides, or one side and angle A, to get every remaining side and
  angle — handy for chamfers, slopes and angular allowances.
- **Metric thread (ISO):** enter the nominal diameter and pitch (leave the pitch empty to use the
  standard coarse pitch) to get the fundamental height H, pitch diameter d2, minor diameter d1,
  thread depth h3 and an approximate tap-drill diameter.

## How to use

1. Open the utilities menu in ENCY and click **Taper Calculator**.
2. Pick a tab, fill in the known values (both `.` and `,` work as the decimal mark).
3. Press **Solve** to compute, or **Clear** to reset the tab for the next part.

## Notes

- Self-contained: it does not modify your project or geometry.
- Non-modal window, so it can stay open next to your work.
- Angles are in degrees; diameters and lengths share whatever unit you type.
