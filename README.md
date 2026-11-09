# 🏃 RaceDay – Portfolio of Evidence Part 3 (MVC Web Application)

##  1. Project Overview
RaceDay is a comprehensive, full-stack event management system designed specifically for the South African road running, walking, and cycling community. 

**Part 3** represents the final, user-facing layer of the application. It is a fully functional **ASP.NET Core MVC Web Application** that consumes the RESTful API built in Part 2. It provides a polished, secure, and role-aware interface for both Event Organisers and Participants, complete with session-based authentication, image handling, and Docker containerisation.

---

##  2. CRITICAL: How to Run This Project
**This MVC Web Application relies entirely on the Part 2 API to function.** The website does not connect to a database directly; it sends HTTP requests to the API.

To run this project successfully, you **MUST follow this exact order**:

1. **Start the API First (Part 2):** 
   - Open the `RaceDay.API.Server` project.
   - Press **F5** to run it. 
   - Wait for the console to say: `Now listening on: http://localhost:5000`.
2. **Start the MVC Web App (Part 3):**
   - Once the API is running, open the `RaceDay.Web` project.
   - Press **F5** to run it.
3. **Important:** If you run the Web App without the API running, the website will load, but logging in or fetching events will fail with a "Could not connect to the API" error.

---

##  3. Authentication & Role-Based Access

### Session Management
- When a user logs in, the API returns a secure session cookie (`.RaceDay.Auth`).
- The MVC Web App intercepts this cookie via the `ApiService` and stores it in the browser.
- The `ApiService` automatically attaches this cookie to every subsequent request to the API, ensuring the user stays logged in.

### Roles
**Organiser:**
- Custom navigation menu (Dashboard, Create Event).
- Can create, edit, and delete events.
- Can view all participant enrolments.
- Can capture and publish race results (finish times and positions).

**Participant:**
- Custom navigation menu (Find Events, My Enrolments, My Results).
- Can browse all upcoming events.
- Can enrol in events by selecting specific categories (e.g., 10km, Half Marathon).
- Can view their personal race history and times.

---

##  4. Architecture & Code Structure

The project follows the MVC (Model-View-Controller) architectural pattern and **strictly enforces the separation of concerns**:

- **Controllers:** Handle incoming HTTP requests, manage session state, and return Views.
- **Views (Razor .cshtml):** Handle the UI presentation using Bootstrap 5 for a responsive, mobile-friendly design.
- **Services (`ApiService.cs`):** The "bridge" between the MVC app and the API. It handles serialization, deserialization, and cookie management.
- **No Direct Database Access:** The MVC project contains **zero** Entity Framework code or SQL connections. All data comes from the API.

---

##  5. Key Features Implemented

### Organiser Dashboard
- **Event Management:** Create new events with names, descriptions, dates, locations, distances, and event types (Running, Walking, Cycling).
- **Enrolment Management:** View a detailed list of every participant who has enrolled in an event.
- **Results Capture:** Submit finish times and overall positions for participants after an event concludes.

### Participant Experience
- **Event Browsing:** View a grid of upcoming events with details.
- **Enrolment:** Select a category and enrol instantly.
- **Personal Dashboard:** View personal enrolments (with bib numbers) and personal race results (finish times and positions).
- **Status Tracking:** Color-coded badges indicate if an enrolment is Pending or Confirmed.

---

##  6. Docker Setup & Containerisation

Both the API and the Web Application have been containerised using Docker. 

### Prerequisites
- **Docker Desktop** must be installed and running.
- **Virtual Machine Platform** must be enabled in Windows Features.

### Step 1: Build and Run the API Container
Open PowerShell and run:
```bash
cd RaceDay.API.Server
docker build -t raceday-api .
docker run -d -p 7001:80 --name raceday-api-container raceday-api
Step 2: Build and Run the Web Container
Open a new PowerShell window and run:

bash
cd RaceDay.Web
docker build -t raceday-web .
docker run -d -p 7015:80 --name raceday-web-container raceday-web
Step 3: Verify Containers
Run docker ps to confirm both containers are running successfully (Status: Up).

7. CI/CD Pipeline
The repository is configured with a GitHub Actions workflow located in .github/workflows/dotnet.yml.

What it does: Every time code is pushed to the main or master branch, the workflow automatically provisions a .NET 10 environment, restores NuGet packages, builds both the API and the MVC projects, and runs the unit tests.

Status:
https://github.com/Molly-seemola/Race-dayDB-PROG-1/actions/workflows/dotnet.yml/badge.svg

(Attach your green build screenshot below)
https://docs/ci-cd-screenshot-part3.png

 8. Video Presentation
 Watch the Part 3 Walkthrough on YouTube

The video covers:

Running the API and the MVC App together.

The Participant Interface (browsing, enrolling, viewing results).

The Organiser Interface (creating events, capturing results).

Code explanation (ApiService, Session management, Role-based layout).

Docker build and run demonstration.

 9. Repository Structure
text
Race-dayDB-PROG-1/
│
├── RaceDay.API.Server/       # Part 2 API (Database, Controllers, Models)
│   └── Dockerfile            # API Container configuration
│
├── RaceDay.Tests/            # Part 2 Unit Tests (xUnit)
│
├── RaceDay.Web/              # Part 3 MVC Web Application
│   ├── Controllers/          # MVC Controllers (Auth, Organiser, Participant)
│   ├── Services/             # ApiService (HTTP client)
│   ├── Views/                # Razor Views (HTML/CSS)
│   └── Dockerfile            # Web Container configuration
│
├── docs/                     # ERD, SQL Script, CI/CD Screenshots
├── .github/workflows/        # CI/CD Pipeline
└── README.md                 # Project Documentation
10. Student Information
Student Name: Molly Seemola

Project: RaceDay - Full Stack Event Management System

Repository: https://github.com/Molly-seemola/Race-dayDB-PROG-1

Submission: Portfolio of Evidence (Part 3)


