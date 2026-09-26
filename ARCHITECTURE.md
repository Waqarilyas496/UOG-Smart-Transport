# UOG Smart Transport Management & Intelligence System
## Architecture Documentation

---

## 1. User Roles

| Role | Responsibilities |
|---|---|
| **SUPER_ADMIN** | Full system control. Creates/removes admin accounts, manages system-wide settings, has access to everything. |
| **TRANSPORT_ADMIN** | Manages daily transport operations — buses, routes, drivers, trip assignments, complaint handling. |
| **DRIVER** | Manages assigned bus, starts/ends trips, shares GPS location during active trips, checks students in/out, reports emergencies. |
| **STUDENT** | Views assigned route/bus, live tracking, digital transport pass, files complaints/emergencies. |
| **MAINTENANCE_STAFF** | Views and updates vehicle maintenance records and service schedules only. |

**Design principle:** Roles follow least-privilege access — each role only sees/does what it needs. SUPER_ADMIN and TRANSPORT_ADMIN are kept separate so system-level control is isolated from daily operations.

---

## 2. System Modules

| Module | Included Features |
|---|---|
| User & Auth | Login, registration, roles, JWT authentication |
| Fleet Management | Buses, routes, stops |
| Trip Management | Trip start/end, real-time GPS tracking |
| Transport Pass & Passenger | Student registration, digital pass, QR check-in/out, occupancy monitoring |
| Fuel Management | Fuel entries, consumption/cost calculation |
| Maintenance | Service history, maintenance alerts, schedules |
| Complaints & Emergency | Student complaints, SOS/breakdown reports, status tracking |
| Notifications | Alerts to students, drivers, and admins |
| Analytics & Reports | Charts, downloadable reports |
| AI/Intelligence (future) | Anomaly detection, demand/predictive analytics (Phase 13) |

---

## 3. Database Entities (High-Level)

Detailed schema (columns, keys, relationships) will be designed in Phase 2.

| # | Entity | Purpose |
|---|---|---|
| 1 | Users | Login credentials, role, shared base info for all roles |
| 2 | Students | Student-specific info (roll number, department, semester) — linked to Users |
| 3 | Drivers | Driver-specific info (license number, contact) — linked to Users |
| 4 | Buses | Bus number, capacity, status |
| 5 | Routes | Route name, start/end points |
| 6 | Stops | Stops per route (name, order, location) |
| 7 | Trips | Trip record — bus, driver, start/end time |
| 8 | GPSLocations | GPS points per trip (lat, long, speed, timestamp) |
| 9 | PassengerRecords | QR check-in/check-out logs (student, trip) |
| 10 | TransportPasses | Student's digital pass (validity, assigned route) |
| 11 | FuelRecords | Fuel entries (litres, price, odometer) |
| 12 | MaintenanceRecords | Service history (oil change, repairs, dates) |
| 13 | Complaints | Student complaints (type, status) |
| 14 | EmergencyReports | SOS/breakdown reports |
| 15 | Notifications | Alerts sent to users |
| 16 | AuditLogs | Change tracking for security/accountability |

**Design principle:** `Users` holds fields common to all roles (name, email, password hash, role). `Students` and `Drivers` hold only their role-specific extra fields, linked to `Users` via foreign key — avoids data duplication.

---

## 4. High-Level System Architecture

### Overall Data Flow

```
Driver's Phone (GPS)
        |
        v  (HTTP / SignalR)
Backend API (ASP.NET Core)
        |
        v
PostgreSQL Database (persisted)
        |
        v
SignalR Broadcast (real-time push)
        |            |
        v            v
Admin Dashboard   Student App
  (Live Map)       (Live Map)
```

### Key Points

1. **Single backend** (ASP.NET Core Web API) serves all three clients: Admin Web, Student App, Driver App.
2. **Two communication types:**
   - **REST API** — standard requests (login, bus list, complaints, etc.)
   - **SignalR (WebSocket)** — real-time updates only (live GPS location)
3. **GPS Flow:**
   - Driver starts a trip → phone sends GPS coordinates every few seconds (e.g. 5-10s)
   - Backend saves to `GPSLocations` table and immediately broadcasts via SignalR to Admin/Student clients
   - GPS transmission stops when the trip ends
4. **Security:** All requests authenticated via JWT with role-based authorization (e.g. only DRIVER role can submit GPS for their own trip; STUDENT is read-only).
5. **Scalability considerations (Phase 17):** Continuous GPS data from 60 buses will grow the database quickly — periodic archiving/cleanup of old GPS records is planned.

---

## Status

- [x] Phase 0 — Environment Setup
- [x] Phase 1 — Project Architecture (this document)
- [ ] Phase 2 — Database Design (EF Core models & migrations)
- [ ] Phase 3 — Backend API
- [ ] ... (see project roadmap)
