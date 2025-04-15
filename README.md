# GeoStream

GeoStream is a modular, real-time monitoring platform built with .NET 8 for visualizing and processing live data streams from distributed Scanners that detect nearby Emitters installed on moving Assets. Developed using Clean Architecture principles, the system supports scalable service orchestration, message-based communication, and interactive geospatial visualization via a Blazor Server frontend with Leaflet maps.

---

## 🚀 Features

### 🌐 Real-Time Data Visualization
- **Assets** transmit live signals through onboard **Emitters**.
- Nearby **Scanners** detect these signals and forward them through RabbitMQ.
- Events are visualized in real-time on interactive Leaflet maps.
- The UI displays live signals, connection statuses, and rule-based **incidents**.

### 🧱 Scalable Clean Architecture (CQRS + SOLID)
- Fully testable and maintainable project structure:
  - **Domain** – Core business rules and entity models.
  - **Application** – Use cases, commands, validation.
  - **Infrastructure** – Messaging, logging, persistence, RabbitMQ.
  - **API/UI** – Blazor Server frontend and ASP.NET Web API.

### ⚙️ Background Services
- **TelemetryService** reads live data from **Emitters** and publishes them to RabbitMQ.
- **MessageProcessorService** listens to messages and processes them (e.g., logs, incidents).

### 📡 Messaging with RabbitMQ
- Event-driven microservices using publish/subscribe architecture.
- Realtime synchronization between services and UI components.

### 🧭 Interactive Dashboard & Maps
- Live location and signal updates for all **Assets**.
- Configurable overlays for routes using KMZ files.
- Map-based overlays for **Hubs**, scanned signals, and incident triggers.

### 🔄 Component-Level Pub/Sub (UI)
- Internal `EventAggregator` handles component communication inside Blazor (e.g. updates, map redraws, alerts).

### 📋 Incident Detection & Logging
- Rule-based conditions such as:
  - Unauthorized access (e.g., no **Special Access**).
  - **Asset** idle too long or missing scans.
- Logs stored in MongoDB for audit and analysis.

### 🔐 Security & Access Management
- Role-based access control using ASP.NET Identity.
- Secure login, logout, and role assignment.
- Token authentication for Web API access.

### 📊 Analytics & Reporting
- Realtime heatmaps, incident summaries, and connection graphs.
- Historical event and signal logs with filtering.

---

## 🧩 Project Structure

```
GeoStream/
├── GeoStream.UI/                  # Blazor Server frontend (Dashboard, Maps)
│   ├── Components/
│   ├── Pages/
│   ├── Maps/
│   ├── UIEvents/
│   └── Services/
│
├── GeoStream.Api/                 # ASP.NET Core Web API
│   ├── Controllers/
│   ├── Application/
│   ├── Domain/
│   ├── Infrastructure/
│   └── Middlewares/
│
├── MessageProcessorService/       # Handles domain processing of scanned events
│   ├── Application/
│   ├── Core/
│   ├── Domain/
│   └── Infrastructure/
│
├── TelemetryService/              # Reads live data from connected Scanners
│   ├── Application/
│   ├── Core/
│   ├── Domain/
│   └── Infrastructure/
│
└── TelemetryService.Tests/        # Unit tests for telemetry processing
```

---

## 🧰 Technologies Used

### Backend
- .NET 8
- Entity Framework Core (SQL Server)
- MongoDB.Driver
- RabbitMQ.Client
- MediatR (CQRS)
- FluentValidation

### Frontend
- Blazor Server
- MudBlazor / MudExtensions
- Leaflet.js via JSInterop
- KMZ parser for route overlays

### Messaging & Background Services
- RabbitMQ for distributed messaging
- Microsoft.Extensions.Hosting for background services

### Security & Localization
- ASP.NET Identity (cookie authentication)
- Role-based authorization
- Globalized formatting and culture-aware UI

### Testing
- NUnit for unit testing
- Extensible for integration and background service testing

---

## ⚙️ Configuration Steps

### Prerequisites
- .NET 8 SDK
- Running RabbitMQ instance
- SQL Server and MongoDB with valid credentials

### Configuration
- Configure `appsettings.json` with valid data

---

## License
This project is licensed under the [MIT License](LICENSE).