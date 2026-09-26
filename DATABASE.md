# UOG Smart Transport Management & Intelligence System
## Database Design (Phase 2)

Primary key strategy: **UUID** for all tables (chosen for security — non-guessable IDs — and to support future scaling beyond 60 buses / thousands of students without ID collision risk).

---

## 1. Users
Base account table — shared fields for every role.

| Column | Type | Notes |
|---|---|---|
| Id | UUID (PK) | |
| FullName | varchar | |
| Email | varchar (unique) | Login |
| PasswordHash | varchar | Never store plain password |
| Role | enum/varchar | SUPER_ADMIN, TRANSPORT_ADMIN, DRIVER, STUDENT, MAINTENANCE_STAFF |
| PhoneNumber | varchar | |
| IsActive | boolean | Disable instead of delete |
| CreatedAt | timestamp | |

## 2. Students
Extends Users with student-specific fields.

| Column | Type | Notes |
|---|---|---|
| Id | UUID (PK) | |
| UserId | UUID (FK → Users) | |
| RollNumber | varchar (unique) | |
| Batch | varchar | e.g. "2023-2027" |
| Department | varchar | |
| Semester | int | |
| RouteId | UUID (FK → Routes, nullable) | Assigned route |

## 3. Drivers
Extends Users with driver-specific fields.

| Column | Type | Notes |
|---|---|---|
| Id | UUID (PK) | |
| UserId | UUID (FK → Users) | |
| LicenseNumber | varchar (unique) | |
| AssignedBusId | UUID (FK → Buses, nullable) | |

## 4. Buses

| Column | Type | Notes |
|---|---|---|
| Id | UUID (PK) | |
| BusNumber | varchar (unique) | e.g. "UOG-01" |
| Capacity | int | |
| Status | enum/varchar | ACTIVE, MAINTENANCE, OUT_OF_SERVICE |
| RouteId | UUID (FK → Routes, nullable) | |

## 5. Routes

| Column | Type | Notes |
|---|---|---|
| Id | UUID (PK) | |
| RouteName | varchar | |
| StartPoint | varchar | |
| EndPoint | varchar | |
| IsActive | boolean | |

## 6. Stops

| Column | Type | Notes |
|---|---|---|
| Id | UUID (PK) | |
| RouteId | UUID (FK → Routes) | |
| StopName | varchar | |
| StopOrder | int | Sequence along the route (used later for ETA) |
| Latitude | decimal | |
| Longitude | decimal | |

## 7. Trips

| Column | Type | Notes |
|---|---|---|
| Id | UUID (PK) | |
| BusId | UUID (FK → Buses) | |
| DriverId | UUID (FK → Drivers) | |
| RouteId | UUID (FK → Routes) | |
| StartTime | timestamp | |
| EndTime | timestamp (nullable) | Null while trip is active |
| Status | enum/varchar | SCHEDULED, ACTIVE, COMPLETED, CANCELLED |

## 8. GPSLocations

| Column | Type | Notes |
|---|---|---|
| Id | UUID (PK) | |
| TripId | UUID (FK → Trips) | |
| Latitude | decimal | |
| Longitude | decimal | |
| Speed | decimal (nullable) | km/h if provided by phone |
| Heading | decimal (nullable) | 0-360°, if available |
| RecordedAt | timestamp | Time on the phone when captured |
| ReceivedAt | timestamp | Time backend received it (may lag on poor network) |

## 9. PassengerRecords

| Column | Type | Notes |
|---|---|---|
| Id | UUID (PK) | |
| TripId | UUID (FK → Trips) | |
| StudentId | UUID (FK → Students) | |
| Type | enum/varchar | BOARDING, ALIGHTING |
| ScannedAt | timestamp | |
| ScannedByDriverId | UUID (FK → Drivers) | |

**Duplicate-scan prevention** (application logic, not a DB constraint): before inserting a new record, the backend checks the student's latest record for that trip — a BOARDING can only follow an ALIGHTING (or no prior record), and vice versa. Implemented in Phase 8.

## 10. TransportPasses

| Column | Type | Notes |
|---|---|---|
| Id | UUID (PK) | |
| StudentId | UUID (FK → Students) | |
| RouteId | UUID (FK → Routes) | |
| PassNumber | varchar (unique) | Encoded into the student's QR code |
| ValidFrom | date | |
| ValidUntil | date | |
| Status | enum/varchar | ACTIVE, EXPIRED, SUSPENDED |
| IssuedAt | timestamp | |

## 11. FuelRecords

| Column | Type | Notes |
|---|---|---|
| Id | UUID (PK) | |
| BusId | UUID (FK → Buses) | |
| DriverId | UUID (FK → Drivers, nullable) | |
| Litres | decimal | |
| PricePerLitre | decimal | |
| TotalCost | decimal | |
| Odometer | int | Used to compute km/litre between fills |
| FuelStation | varchar (nullable) | |
| RecordedAt | timestamp | |
| Notes | varchar (nullable) | |

## 12. MaintenanceRecords

| Column | Type | Notes |
|---|---|---|
| Id | UUID (PK) | |
| BusId | UUID (FK → Buses) | |
| ServiceType | varchar | e.g. "Oil Change" |
| ServiceDate | date | |
| Cost | decimal (nullable) | |
| NextServiceDue | date (nullable) | Drives the maintenance-alerts dashboard |
| PerformedBy | varchar (nullable) | |
| Notes | varchar (nullable) | |

## 13. Complaints

| Column | Type | Notes |
|---|---|---|
| Id | UUID (PK) | |
| StudentId | UUID (FK → Students) | |
| TripId | UUID (FK → Trips, nullable) | |
| Type | enum/varchar | BUS_LATE, OVERCROWDING, AC_ISSUE, DRIVER_ISSUE, ROUTE_ISSUE, OTHER |
| Description | text | |
| Status | enum/varchar | PENDING, ASSIGNED, IN_PROGRESS, RESOLVED, CLOSED |
| AssignedToUserId | UUID (FK → Users, nullable) | |
| CreatedAt | timestamp | |
| ResolvedAt | timestamp (nullable) | |

## 14. EmergencyReports
Kept separate from Complaints — emergencies need immediate, SOS-style priority handling, unlike the normal complaint workflow.

| Column | Type | Notes |
|---|---|---|
| Id | UUID (PK) | |
| TripId | UUID (FK → Trips, nullable) | |
| ReportedByUserId | UUID (FK → Users) | |
| Type | enum/varchar | BREAKDOWN, ACCIDENT, MEDICAL, OTHER |
| Description | text | |
| Latitude | decimal (nullable) | |
| Longitude | decimal (nullable) | |
| Status | enum/varchar | PENDING, ASSIGNED, IN_PROGRESS, RESOLVED, CLOSED |
| CreatedAt | timestamp | |

## 15. Notifications

| Column | Type | Notes |
|---|---|---|
| Id | UUID (PK) | |
| UserId | UUID (FK → Users) | |
| Title | varchar | |
| Message | text | |
| Type | enum/varchar | INFO, WARNING, ALERT |
| IsRead | boolean | |
| CreatedAt | timestamp | |

## 16. AuditLogs

| Column | Type | Notes |
|---|---|---|
| Id | UUID (PK) | |
| UserId | UUID (FK → Users) | Who performed the action |
| Action | varchar | e.g. "UPDATED_BUS_STATUS" |
| EntityType | varchar | e.g. "Bus", "Complaint" |
| EntityId | UUID | The affected record's ID |
| Timestamp | timestamp | |

---

## Status

- [x] Phase 0 — Environment Setup
- [x] Phase 1 — Project Architecture
- [x] Phase 2 — Database Design (this document)
- [ ] Phase 2 (continued) — EF Core models & migrations
- [ ] Phase 3 — Backend API
