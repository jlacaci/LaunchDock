---
name: Fluent Evolution
colors:
  surface: '#131313'
  surface-dim: '#131313'
  surface-bright: '#393939'
  surface-container-lowest: '#0e0e0e'
  surface-container-low: '#1b1b1b'
  surface-container: '#20201f'
  surface-container-high: '#2a2a2a'
  surface-container-highest: '#353535'
  on-surface: '#e5e2e1'
  on-surface-variant: '#c0c7d4'
  inverse-surface: '#e5e2e1'
  inverse-on-surface: '#313030'
  outline: '#8a919e'
  outline-variant: '#404752'
  surface-tint: '#a3c9ff'
  primary: '#a3c9ff'
  on-primary: '#00315c'
  primary-container: '#0078d4'
  on-primary-container: '#ffffff'
  inverse-primary: '#0060ab'
  secondary: '#74d1ff'
  on-secondary: '#003548'
  secondary-container: '#159ccb'
  on-secondary-container: '#002e3f'
  tertiary: '#ffb3ad'
  on-tertiary: '#680009'
  tertiary-container: '#e22d32'
  on-tertiary-container: '#ffffff'
  error: '#ffb4ab'
  on-error: '#690005'
  error-container: '#93000a'
  on-error-container: '#ffdad6'
  primary-fixed: '#d3e3ff'
  primary-fixed-dim: '#a3c9ff'
  on-primary-fixed: '#001c39'
  on-primary-fixed-variant: '#004883'
  secondary-fixed: '#c1e8ff'
  secondary-fixed-dim: '#74d1ff'
  on-secondary-fixed: '#001e2b'
  on-secondary-fixed-variant: '#004d67'
  tertiary-fixed: '#ffdad6'
  tertiary-fixed-dim: '#ffb3ad'
  on-tertiary-fixed: '#410003'
  on-tertiary-fixed-variant: '#930012'
  background: '#131313'
  on-background: '#e5e2e1'
  surface-variant: '#353535'
typography:
  display-sm:
    fontFamily: Inter
    fontSize: 24px
    fontWeight: '600'
    lineHeight: 32px
    letterSpacing: -0.01em
  body-md:
    fontFamily: Inter
    fontSize: 14px
    fontWeight: '400'
    lineHeight: 20px
  body-sm:
    fontFamily: Inter
    fontSize: 13px
    fontWeight: '400'
    lineHeight: 18px
  label-lg:
    fontFamily: Inter
    fontSize: 12px
    fontWeight: '500'
    lineHeight: 16px
    letterSpacing: 0.02em
  label-md:
    fontFamily: Inter
    fontSize: 11px
    fontWeight: '500'
    lineHeight: 14px
rounded:
  sm: 0.25rem
  DEFAULT: 0.5rem
  md: 0.75rem
  lg: 1rem
  xl: 1.5rem
  full: 9999px
spacing:
  container-padding: 12px
  item-gap: 8px
  section-margin: 24px
  dock-height: 48px
  max-width-desktop: 1200px
---

## Brand & Style

The design system evolves the utilitarian, flat aesthetic of legacy dock interfaces into the sophisticated **Fluent Design** language. The personality is professional yet ethereal, characterized by depth, translucency, and intentional motion. It targets power users who value organizational clarity within a high-performance, modern OS environment.

The visual style is a hybrid of **Minimalism** and **Glassmorphism**, leveraging the "Mica" material effect. Surfaces are not merely colored but are tinted windows into the desktop background, creating a sense of place and hierarchy. The emotional response is one of calm productivity and "lightness," where the UI feels like it floats effortlessly above the workspace.

## Colors

The palette is rooted in a **Dark Mode** foundation to reduce eye strain and provide a high-contrast backdrop for vibrant application icons. 

- **Primary:** Windows Blue (#0078D4) for active states and critical interactions.
- **Secondary:** Sky Blue (#60CDFF) for subtle highlights and secondary indicators.
- **Tertiary:** Accent Red (#FF4343), derived from the original dock’s indicators, used sparingly for urgent notifications or destructive actions.
- **Neutral:** A deep slate gray (#1C1C1C) used as the base for translucent materials, ensuring legibility of white text while maintaining the depth of the Mica effect.

## Typography

The design system utilizes **Inter** as a functional alternative to Segoe UI Variable, maintaining the high legibility and variable-weight benefits essential for modern desktop interfaces. 

The type scale is deliberately compact to suit the "Dock" context, where space is at a premium. **Label-lg** is the primary choice for category names (e.g., "PROGRAMACIÓN"), using uppercase styling and subtle tracking to mirror the legacy design while improving modern readability. Standard body sizes are reserved for tooltips and expanded menu items.

## Layout & Spacing

The layout follows a **Fixed Grid** philosophy for the central dock container, which floats with a consistent margin from the screen edges (typically 12px-24px from the bottom). 

Internal spacing is governed by a strict 4px rhythm. The dock uses a horizontal layout with logical groupings separated by subtle vertical dividers. On smaller screens, the dock dynamically collapses labels into icon-only representations, while on desktop, it maintains full category titles with an 8px gap between interactive elements.

## Elevation & Depth

Hierarchy is established through **Glassmorphism** and multi-layered shadows. 

1.  **Base Layer (Mica):** The main dock background uses a 70% opacity neutral tint with a 30px backdrop blur and a 1px inner translucent stroke (white at 10% opacity) to define the edge.
2.  **Interactive Layer:** Hovered items receive a subtle tonal lift—a light gray overlay at 5% opacity.
3.  **Shadow:** The entire dock casts a "Soft Ambient Shadow" (0px 8px 32px rgba(0,0,0, 0.4)) to separate it from the desktop wallpaper and open windows.

## Shapes

The shape language is defined by **Soft/Rounded** geometry. 

- **Main Container:** Uses a 24px corner radius (`rounded-xl` equivalent) to create a friendly, modern "pill" silhouette.
- **Interactive States:** Hover backgrounds for individual items use a 6px corner radius to ensure they feel contained within the larger dock structure without looking too sharp.
- **Buttons/Icons:** Circular shapes are used for specialized system actions (Settings, Edit) to differentiate them from category-based navigation.

## Components

### Dock Items
Category links consist of a text label and a chevron. On hover, a background plate fades in. Active categories are marked with a 2px thick primary blue underline, centered under the text.

### Icons
System icons (Grid, Pencil, Gear) use thin-stroke "Fluent" iconography. They are monochromatic white by default but transition to the primary blue accent when active or hovered.

### Menus (Flyouts)
When a category is clicked, a menu appears above the dock. These menus inherit the Mica translucency and 12px rounded corners, featuring a 4px gap from the main dock body to maintain the "floating" aesthetic.

### Buttons
Action buttons at the dock's ends (Settings, Edit) are treated as ghost buttons that gain a translucent circular background on hover, ensuring the interface remains minimal when not in active use.