# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build and Development Commands

```bash
yarn dev          # Start development server
yarn build        # Build for production
yarn start        # Start production server
yarn lint         # Run ESLint
yarn format       # Format code with Prettier
```

## Architecture Overview

This is a **Next.js 13 (App Router)** DERMS (Distributed Energy Resource Management System) frontend built with:
- **PrimeReact** for UI components
- **PrimeFlex** for CSS utilities
- **next-intl** for internationalization (en, tr, de)
- **SignalR** for real-time WebSocket data
- **React Hook Form + Yup** for form handling
- **@xyflow/react** for energy system topology visualization

### Directory Structure

- `app/(main)/` - Authenticated pages with sidebar layout
- `app/(full-page)/` - Full-page layouts (login, error pages)
- `layout/` - Core layout components (AppMenu, AppTopbar, AppSidebar)
- `src/components/nodes/` - React Flow node components for topology (BessNode, GridNode, TransformerNode, etc.)
- `src/services/` - API services (`httpClient` singleton) and WebSocket handling
- `src/context/` - React contexts (Loading, Tenant, Toaster)
- `src/messages/` - i18n translation files (en.json, tr.json, de.json)
- `src/types/` - TypeScript type definitions
- `src/hooks/` - Custom hooks (e.g., `useUser` for auth state)
- `src/utils/` - Utilities including `tryCatch` for async error handling

### Key Patterns

**HTTP Client**: `src/services/http.ts` exports a singleton `httpClient` with automatic token refresh. Services in `src/services/` use this for API calls.

**Error Handling**: Use `tryCatch(promise)` utility which returns `[error, result]` tuple for cleaner async error handling.

**Menu Configuration**: Navigation is defined in `layout/AppMenu.tsx` as a hierarchical `AppMenuItem[]` array. Menu items require i18n keys in `src/messages/*.json` under the `menu` namespace. Some items have `visible` conditions based on user roles.

**Page Structure**: Pages under `app/(main)/` are client components (`'use client'`). They typically use:
- `useTranslations()` for i18n (can scope to namespace like `useTranslations('BESS')`)
- `useTenant()` for facility context and date selection
- `useLoading()` for loading state (`showLoading()`/`hideLoading()`)
- `useUser()` for current user info and roles

**Real-time Data**: `WebSocketService` connects to SignalR hub at `liveDataHub`. Pattern:
1. Connect with `webSocketService.connect(url)`
2. Subscribe with `webSocketService.subscribe('EventName', callback)`
3. Invoke server methods with `webSocketService.invoke('MethodName', args)`
4. Cleanup subscription in useEffect return

**i18n**: Translation keys are namespaced (e.g., `common.*`, `menu.*`, `BESS.*`, `validation.*`). When adding new pages, add corresponding keys to all three language files.

**Dynamic Routes**: Pages with `[...mode]` support create/edit modes (e.g., `/user-identification/edit/123`).

## Commit Messages

Do not mention Claude or AI in commit messages.
