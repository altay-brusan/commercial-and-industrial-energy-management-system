# Project Overview

## About

CNI Frontend is the web interface for a **DERMS (Distributed Energy Resource Management System)**. It provides real-time monitoring, alarm management, maintenance tracking, financial analysis, and configuration interfaces for distributed energy assets.

## Tech Stack

| Technology | Version | Purpose |
|------------|---------|---------|
| Next.js | 16.1.6 | App Router framework |
| React | 19.2.3 | UI library |
| TypeScript | 5.x | Type safety (strict mode) |
| Tailwind CSS | 4.x | Utility-first CSS |
| shadcn/ui | new-york style | 57+ base UI components |
| lucide-react | 0.563.0 | Primary icon library |
| react-icons | 5.5.0 | Supplementary icons |
| recharts | 2.15.4 | Chart/data visualization |
| react-hook-form | 7.71.1 | Form state management |
| zod | 4.3.6 | Schema validation |
| date-fns | 4.1.0 | Date utilities |
| sonner | 2.0.7 | Toast notifications |

## Commands

```bash
npm run dev      # Start development server (http://localhost:3000)
npm run build    # Production build (standalone output)
npm run dev      # Start development server
npm run start    # Start production server
npm run lint     # Run ESLint
```

## Environment Variables

| Variable | Description | Example |
|----------|-------------|---------|
| `NEXT_PUBLIC_API_BASE_URL` | Backend API base URL prefix | `/api/v1` |
| `FUSIONSOLAR_BASE_URL` | FusionSolar API URL (server-side) | `https://intl.fusionsolar.huawei.com` |
| `FUSIONSOLAR_USERNAME` | FusionSolar username (server-side) | — |
| `FUSIONSOLAR_SYSTEM_CODE` | FusionSolar system code (server-side) | — |

## API Proxy

Development API calls are proxied via Next.js rewrites in `next.config.ts`:

```
/api/:path* → https://cni-api-dev.oklidis.com.tr/api/:path*
```

The `NEXT_PUBLIC_API_BASE_URL` is set to `/api/v1` in `.env.local`. The `apiClient` prepends this to all endpoint paths, resulting in URLs like `/api/v1/Auth/Login`, which are then proxied to the actual backend.

## Build Output

The project uses `output: "standalone"` for containerized deployments (Docker).
