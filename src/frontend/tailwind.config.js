/**
 * MyMusic — Tailwind CSS theme (v3)
 * --------------------------------------------------------------------------
 * Maps the design-system tokens onto Tailwind's theme so the whole system is
 * usable as utility classes in Angular templates (utility-first, per the wiki).
 *
 * Brand palette (`brand-*`) is static hex → supports opacity modifiers
 * (e.g. `bg-brand-500/20`). Semantic tokens (`accent`, `fg`, `surface`, …)
 * reference the CSS variables from `colors_and_type.css`, so they switch
 * automatically with `[data-theme="dark"]` — no `dark:` variants required.
 *
 * Setup:
 *   1. Import the tokens once (global styles.css):  @import "../colors_and_type.css";
 *      (or move the :root / [data-theme] blocks into your global stylesheet)
 *   2. Point `content` at your templates.
 *   3. Build with the utility recipes in tailwind/README.md.
 */
module.exports = {
  // Toggle dark mode by setting <html data-theme="dark">.
  darkMode: ["selector", '[data-theme="dark"]'],

  content: ["./src/**/*.{html,ts}"],

  theme: {
    extend: {
      colors: {
        /* --- Brand scale (static → opacity modifiers work) --- */
        brand: {
          50: "#ecfdf5", 100: "#d1fae5", 200: "#a7f3d0", 300: "#6ee7b7",
          400: "#34d399", 500: "#10b981", 600: "#059669", 700: "#047857",
          800: "#065f46", 900: "#064e3b", 950: "#022c22",
        },

        /* --- Semantic, theme-aware (switch with [data-theme]) --- */
        accent: "var(--accent)",                       // brand mark, active states, rings
        "accent-solid": "var(--accent-solid)",         // primary button fill
        "accent-solid-hover": "var(--accent-solid-hover)",
        "accent-text": "var(--accent-text)",           // links / accent text
        "accent-subtle": "var(--accent-subtle-bg)",    // subtle accent surface
        "on-accent": "var(--fg-on-accent)",            // text on accent fill

        page: "var(--bg-page)",
        surface: "var(--bg-surface)",
        "surface-2": "var(--bg-surface-2)",
        inset: "var(--bg-inset)",
        hover: "var(--bg-hover)",

        fg: "var(--fg)",
        "fg-muted": "var(--fg-muted)",
        "fg-subtle": "var(--fg-subtle)",

        line: "var(--border)",                         // hairline borders
        "line-strong": "var(--border-strong)",         // input borders

        /* --- Status --- */
        success: "var(--success-fg)", "success-bg": "var(--success-bg)", "success-line": "var(--success-bd)",
        warning: "var(--warning-fg)", "warning-bg": "var(--warning-bg)", "warning-line": "var(--warning-bd)",
        danger: "var(--danger-fg)", "danger-bg": "var(--danger-bg)", "danger-line": "var(--danger-bd)",
        "danger-solid": "var(--danger-solid)", "danger-solid-hover": "var(--danger-solid-hover)",
        info: "var(--info-fg)", "info-bg": "var(--info-bg)", "info-line": "var(--info-bd)",
      },

      fontFamily: {
        sans: ['Inter', 'system-ui', '-apple-system', '"Segoe UI"', 'Roboto', 'sans-serif'],
        mono: ['"JetBrains Mono"', 'ui-monospace', '"SF Mono"', 'Menlo', 'monospace'],
      },

      /* Compact, data-first scale. Note: `base` = 14px (overrides default 16px). */
      fontSize: {
        "2xs": ["11px", { lineHeight: "1.4" }],
        xs:    ["12px", { lineHeight: "1.4" }],
        sm:    ["13px", { lineHeight: "1.5" }],
        base:  ["14px", { lineHeight: "1.55" }],
        md:    ["16px", { lineHeight: "1.5" }],
        lg:    ["18px", { lineHeight: "1.4" }],
        xl:    ["20px", { lineHeight: "1.3", letterSpacing: "-0.011em" }],
        "2xl": ["24px", { lineHeight: "1.25", letterSpacing: "-0.011em" }],
        "3xl": ["30px", { lineHeight: "1.2", letterSpacing: "-0.02em" }],
      },

      letterSpacing: { tightish: "-0.011em", label: "0.06em" },

      /* Medium radius feel. `rounded` (DEFAULT) = 8px → buttons/inputs. */
      borderRadius: {
        xs: "4px", sm: "6px", DEFAULT: "8px", md: "8px", lg: "10px", xl: "14px",
      },

      /* Theme-aware elevation (deepens automatically in dark). */
      boxShadow: {
        xs: "var(--shadow-xs)",
        sm: "var(--shadow-sm)",
        md: "var(--shadow-md)",
        lg: "var(--shadow-lg)",
      },

      ringColor: { DEFAULT: "var(--accent)" },
    },
  },

  plugins: [],
};
