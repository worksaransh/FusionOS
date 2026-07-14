# 06_UI_UX_DESIGN_SYSTEM.md — FusionOS UI/UX Standards

## 1. Principles

Enterprise UX, not consumer-app UX stretched over enterprise data: dense, data-forward, keyboard-friendly, and fast — the people using FusionOS all day (warehouse clerks, accountants, planners) are optimizing for throughput, not delight-on-first-use. React 18 + TypeScript, Tailwind CSS, shadcn/ui (Radix primitives) per `02_TECH_STACK.md`.

## 2. Design System Foundations

- **Design tokens** (color, spacing, typography, radii, shadows) defined once in a shared `@fusionos/design-tokens` package consumed by Tailwind config — no module hardcodes hex values or pixel spacing.
- **Component library** (`@fusionos/ui`) wraps shadcn/ui primitives into FusionOS-branded, pre-validated components (DataTable, FormField, StatusBadge, CommandPalette, ApprovalStepper, etc.) — modules compose these, they do not build bespoke buttons/inputs/tables per feature.
- **Iconography**: single icon set (Lucide) across the entire product — no mixing icon libraries per module.

## 3. Dark Mode

Dark mode is a first-class theme, not an inverted-filter afterthought. All design tokens are defined as light/dark pairs; components are tested in both modes before merge; user preference persists per-user (server-stored setting, not just local browser state, since users move devices).

## 4. Responsiveness

Breakpoints defined once and shared: desktop-first (primary enterprise usage), tablet-optimized for warehouse/floor use, mobile-optimized for the dedicated mobile apps (`05_MODULE_ROADMAP.md`) rather than forcing the full desktop data-table experience into a phone viewport. Dense data tables collapse to card/list views below tablet breakpoints.

## 5. Keyboard Shortcuts & Power-User Efficiency

- Global command palette (`Cmd/Ctrl+K`) for navigation and actions across all modules.
- Consistent shortcut conventions platform-wide (e.g., `N` = new record, `/` = focus search, `G then I` = go to Inventory) documented in a single shortcuts reference, not invented per module.
- Grid/table views support keyboard-only row navigation, inline edit, and bulk selection — critical for high-volume data entry (procurement, inventory adjustments, payroll).

## 6. Performance Budgets (binding — coordinate with `09_CODING_STANDARDS.md`)

| Metric | Target |
|---|---|
| Time to Interactive (initial dashboard load) | < 2s on target hardware/network |
| Route transition (module to module) | < 300ms perceived (skeleton/optimistic UI where data fetch exceeds this) |
| List/table with 10k+ rows | Virtualized rendering, no full-DOM render of large grids |
| Search-as-you-type | Results within 1s (aligned to platform search SLA) |

Achieved via code-splitting per module (route-based lazy loading), TanStack Query caching (`02_TECH_STACK.md`), virtualization for large tables (e.g., `@tanstack/react-virtual`), and image/asset optimization.

## 7. Accessibility

WCAG 2.1 AA as the baseline for all new components: keyboard navigability, focus management, ARIA labeling (inherited largely from Radix primitives), and sufficient color contrast in both light and dark themes — verified as part of component library review, not left to individual feature teams to rediscover.

## 8. Permission-Aware UI

UI reflects the same RBAC/permission model enforced server-side (`07_SECURITY.md`): actions the user cannot perform are not just disabled but generally not rendered, to avoid cluttering dense enterprise screens with unusable controls. Permission checks are centralized in a `usePermission()` hook / route guard, never duplicated as ad hoc conditionals scattered through feature code.

## 9. Forms & Data Entry

React Hook Form + Zod schemas per `02_TECH_STACK.md`; validation messages mirror backend FluentValidation rules in wording so users never see a different error client-side vs. server-side. Bulk data entry (e.g., stock adjustments, payroll runs) supports paste-from-spreadsheet and inline grid editing, not one-record-at-a-time modals only.

## 10. Customer Portal & Mobile Considerations

The Sales customer portal and the six mobile applications (Warehouse, Sales, Management, Approvals, Production, Maintenance) reuse the same design tokens and component primitives but are treated as distinct experiences tuned to their context (portal: simplified, customer-facing branding-aware theme; mobile: touch-target sizing, offline-tolerant states, barcode/camera integration for warehouse scanning per `03_SYSTEM_ARCHITECTURE.md` §9).

## 11. Documentation & Governance

Every component in `@fusionos/ui` ships with usage documentation and a visual regression test (Storybook + Chromatic or equivalent). New UI patterns are proposed as additions to the shared library before being used in a feature — a one-off styled component inside a single module's feature code is treated as a design-system gap to close, not a pattern to repeat.
