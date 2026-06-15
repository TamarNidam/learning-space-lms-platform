# Learning Space - LMS Platform

[View Project Documentation]([./docs/](https://docs.google.com/document/d/19uwhi3ficPKjMJZMLHEocLuHUy4IVlY5dKD3XcsgZVA/edit?usp=sharing))

A robust, enterprise-grade Learning Management System (LMS) built with C# and ASP.NET MVC. The platform is engineered to facilitate seamless remote and hybrid education environments, ensuring distributed learning continuity through centralized course management and real-time synchronization.

## 🏗 System Architecture & Security

The core architectural focus of this platform is a strict implementation of **Role-Based Access Control (RBAC)**. The system enforces rigid boundaries between user tiers, ensuring data privacy, operational security, and tenant isolation across courses. 

The application utilizes the MVC (Model-View-Controller) design pattern to maintain a clean separation of concerns, decoupling the business logic and database interactions from the user interface.

## 🔐 Role-Based Access Control (RBAC) Matrix

The system dictates permissions strictly based on three distinct hierarchical roles. There is zero horizontal data leakage between unauthorized peers.

| Feature / Domain | Student | Teacher | Administrator |
| :--- | :--- | :--- | :--- |
| **Course Access** | Enrolled courses only | Assigned courses only | Global read-only access |
| **Course Management** | ❌ None | ✔️ Edit, Upload materials, Schedule | ❌ None (Oversight only) |
| **User Management** | ❌ None | ❌ None | ✔️ Full (Add/Provision users) |
| **Attendance & Grades** | View personal records | ✔️ Mark attendance, Grade students | View global analytics |
| **Communication** | Direct messaging, Zoom access | Broadcast to class, Direct messaging | Broadcast to teachers globally |
| **Calendar & Events** | Personal & Course events | Course events & Admin notices | Global institutional calendar |
| **Personal Tools** | Digital notebook creation | ❌ N/A | ❌ N/A |

## ✨ Core Technical Features

* **Multi-Tier Authorization:** Controllers and endpoints are protected by granular role-validation logic, preventing privilege escalation.
* **Synchronous & Asynchronous Integration:** Facilitates real-time sessions (Zoom integration) alongside static document retrieval and offline assignment submissions.
* **Centralized Dashboarding:** Dedicated dynamic views rendered via Razor pages, tailored to the specific state and role of the authenticated user.
* **State Management:** Efficient tracking of user progression, attendance metadata, and notification delivery mechanisms.

## 🛠 Tech Stack

* **Language:** C#
* **Framework:** ASP.NET MVC
* **Architecture:** Monolithic MVC, RBAC Security Model
* **Frontend:** Server-Rendered Views (Razor)

