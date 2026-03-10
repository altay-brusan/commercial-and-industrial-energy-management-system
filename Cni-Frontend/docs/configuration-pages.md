# Configuration Pages

## Overview

The configuration section (`/configuration/*`) manages system settings, user accounts, roles, and permissions. All pages share a common layout with `ConfigurationSidebar` on the left and `AccessTabs` for switching between Users/Roles/Permissions views.

---

## Users Page (`/configuration/users`)

**File:** `app/configuration/users/page.tsx`

Manages user accounts with full CRUD operations.

### Features
- User list table with sortable columns (name, email, department, roles, status, last login)
- Search filter by name, email, or department
- Stats bar showing total, active, and inactive counts
- User detail panel (420px right aside) on row click
- Create/Edit user modal with firstName, lastName, email, department, phone, role assignment
- Delete user confirmation dialog
- Loading spinner while fetching from API
- Mock data fallback when API unavailable

### Data Flow
```
Page load → userService.getUsers() → API call
  ├── Success → display API data
  └── Failure → fallback to mock data (userAccounts from user-mock-data.ts)

Create → userService.createUser(data) → API call
  ├── Success → add returned user to list
  └── Failure → create locally with generated ID

Edit → userService.updateUser(id, data) → API call
  ├── Success → replace user in list
  └── Failure → update locally

Delete → userService.deleteUser(id) → API call (fire-and-forget)
  └── Always → remove from local list
```

### Components Used
- `UserTable` — sortable data table
- `UserDetailPanel` — right aside with user info, roles, actions
- `UserFormModal` — create/edit dialog with role checkboxes
- `UserDeleteDialog` — confirmation alert dialog

---

## Roles Page (`/configuration/roles`)

**File:** `app/configuration/roles/page.tsx`

Manages tenant roles with permission assignment.

### Features
- Role list table with sortable columns
- Search filter by name or description
- Stats bar showing total, active, system, and custom counts
- Role detail panel with assigned users list
- Create/Edit role modal (name, description, active toggle)
- Permission matrix dialog for bulk permission assignment
- Add/remove users from roles
- Delete role confirmation (system roles protected)

### Data Flow
```
Page load → roleService.getRoles() → API call
  ├── Success → display API data
  └── Failure → fallback to mock data

Create → roleService.createRole(data)
Edit → roleService.updateRole(id, data)
Delete → roleService.deleteRole(id)
Permissions → roleService.updateRolePermissions(roleId, permissionIds[])
Add user → roleService.addUserToRole(roleId, userId)
Remove user → roleService.removeUserFromRole(roleId, userId)
```

### Components Used
- `RoleTable` — sortable data table
- `RoleDetailPanel` — right aside with role info, users, actions
- `RoleFormModal` — create/edit dialog
- `RoleDeleteDialog` — confirmation alert dialog
- `PermissionMatrix` — category-grouped permission checkboxes with select-all
- `RoleUsersSection` — assigned users list with add/remove

---

## Permissions Page (`/configuration/permissions`)

**File:** `app/configuration/permissions/page.tsx`

Read-only overview of all features and permissions in the system.

### Features
- Groups permissions by feature category (Core, Energy, Intelligence, Management)
- Shows feature name, description, active/inactive status
- Lists each permission with its key (code format), name, and description
- Loading spinner while fetching
- Displays total permission and feature counts

### Data Flow
```
Page load → permissionService.getPermissions() → API call
  ├── Success → display permission groups from API
  └── Failure → build groups from mock features + permissions
```

### Components Used
- `PermissionsOverview` — category-grouped feature/permission cards (accepts `permissionGroups` prop)

---

## Shared Components

### AccessTabs
**File:** `components/auth/access-tabs.tsx`

Tab navigation between Users, Roles, and Permissions pages. Rendered in the header of all three configuration sub-pages.

### ConfigurationSidebar
**File:** `components/configuration/configuration-sidebar.tsx`

Left sidebar navigation for all configuration pages (Users, Roles, Permissions, Capacity Limits, Modes).
