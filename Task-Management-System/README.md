# 🚀 Task Management System – Backend

Enterprise-grade **Task Management System** built with the **.NET ecosystem**.  
Designed to manage tasks, teams, and performance with a strong focus on
**Clean Architecture**, **real-time communication**, and **scalability**.

This project is based on real-world corporate requirements and follows
modern backend development best practices.

---

## 🛠 Tech Stack

### 🔹 Backend
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=csharp&logoColor=white)
![.NET](https://img.shields.io/badge/.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core%20Web%20API-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)

---

### 🔹 Architecture & Patterns
![Clean Architecture](https://img.shields.io/badge/Clean%20Architecture-000000?style=for-the-badge)
![Onion Architecture](https://img.shields.io/badge/Onion%20Architecture-5A2D82?style=for-the-badge)
![Repository Pattern](https://img.shields.io/badge/Repository%20Pattern-4B0082?style=for-the-badge)
![Unit of Work](https://img.shields.io/badge/Unit%20of%20Work-4B0082?style=for-the-badge)
![SOLID](https://img.shields.io/badge/SOLID-Principles-blue?style=for-the-badge)

---

### 🔹 Databases & Storage
![SQL Server](https://img.shields.io/badge/SQL%20Server-CC2927?style=for-the-badge&logo=microsoftsqlserver&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-316192?style=for-the-badge&logo=postgresql&logoColor=white)
![MinIO](https://img.shields.io/badge/MinIO-C72E49?style=for-the-badge&logo=minio&logoColor=white)

---

### 🔹 Real-time & Background Jobs
![SignalR](https://img.shields.io/badge/SignalR-512BD4?style=for-the-badge)
![Hangfire](https://img.shields.io/badge/Hangfire-000000?style=for-the-badge)
![Quartz.NET](https://img.shields.io/badge/Quartz.NET-2F4F4F?style=for-the-badge)

---

### 🔹 Automation & DevOps
![n8n](https://img.shields.io/badge/n8n-EA4B71?style=for-the-badge&logo=n8n&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white)
![Docker Compose](https://img.shields.io/badge/Docker%20Compose-2496ED?style=for-the-badge)

---

### 🔹 Security & Tools
![JWT](https://img.shields.io/badge/JWT-000000?style=for-the-badge)
![ASP.NET Identity](https://img.shields.io/badge/ASP.NET%20Identity-512BD4?style=for-the-badge)
![Serilog](https://img.shields.io/badge/Serilog-000000?style=for-the-badge)
![AutoMapper](https://img.shields.io/badge/AutoMapper-6E4AFF?style=for-the-badge)
![FluentValidation](https://img.shields.io/badge/FluentValidation-4CAF50?style=for-the-badge)
![Swagger](https://img.shields.io/badge/Swagger-85EA2D?style=for-the-badge&logo=swagger&logoColor=black)

---

## 📐 Architecture Overview

The project follows **Clean Architecture**, ensuring separation of concerns
and long-term maintainability.

Domain
├─ Entities
├─ Enums
└─ Interfaces

Application
├─ DTOs
├─ Services
├─ Business Rules
└─ Validation

Persistence
├─ DbContext
├─ Repositories
├─ Unit of Work
└─ Migrations

Presentation
├─ API Controllers
├─ SignalR Hubs
└─ Configuration


---

## ⚡ Core Features

### ✅ Task Management
- Create, update, delete tasks
- Assign, finish, reject tasks
- Difficulty-based task structure

### ✅ Work Groups
- Team creation
- Add / remove users from teams

### ✅ Performance Points System
Automatic scoring based on task difficulty:
- Easy → 10 points  
- Medium → 20 points  
- Hard → 30 points  

### ✅ Leaderboard
- Real-time performance ranking using SignalR

### ✅ Comment System
- Task comments
- @mention support

### ✅ Real-time Notifications
- Task assignment
- Comment notifications
- Status changes

### ✅ File Management
- Upload and download files via MinIO (S3 compatible)

### ✅ Password Reset
- OTP-based password recovery mechanism

---

## 🔐 Security

- Role-based authorization (Admin, Manager, Employee)
- JWT Bearer authentication
- Secure password hashing
- Token validation and expiration handling

---

## 🐳 Docker Support

The system is fully containerized using **Docker** and **Docker Compose**,
ensuring consistent behavior across development and production environments.

---

## 🚧 Project Status

🟢 Actively under development.

Planned improvements:
- Advanced reporting
- Audit logging
- Performance optimizations
- Microservice-ready refactor
- Caching mechanisms

---

## 📌 Purpose

This project was built to demonstrate:
- Real-world backend system design
- Clean Architecture in practice
- Real-time communication with SignalR
- Secure and scalable API development

---
