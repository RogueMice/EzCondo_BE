# EZCondo - Condo Management Backend
# Introduction
# EZCondo is a modern condominium (condo) management system designed for residents, property managers, and the administrative board. It centralizes key operations and communications in one platform, improving efficiency and organization for multi-unit residential communities. This repository contains the backend API for the EZCondo platform, built with ASP.NET Core (.NET 8). It provides endpoints for core functionalities such as resident management, maintenance requests, and community announcements.
# Features
# Resident Management: Create, view, update, and delete resident profiles and contact information.
# Maintenance Requests: Submit and track maintenance or repair requests from residents, with status updates.
# Community Announcements: Manage community-wide notices and bulletins to keep all residents informed.
# Payment Tracking: Record and track condo fees, dues, and payment history for residents.
# Visitor Management: Log visitor check-ins and approvals for enhanced building security.
# Document Sharing: Upload and share important community documents (e.g., meeting minutes, rules, policies) with residents.
# Technologies
# ASP.NET Core (.NET 8): A modern, high-performance framework for building web APIs.
# SQL Server: A relational database system to store and query application data.
# JWT (JSON Web Tokens): Secure authentication tokens for protecting API endpoints.
# Swagger (OpenAPI): API documentation and testing UI integrated into the project.
Prerequisites
# .NET 8 SDK: Install from the official Microsoft website.
# SQL Server: Ensure a SQL Server instance is available, and note down the connection details.
# ngrok: (Optional) Install ngrok to expose the local server to a public URL for testing.
# Postman (optional): For API testing and collection usage.
# Integrate: PayOS, MailKit, ClosedXML, Cloudinary, Hangfire, Firebase, SignalR.  
# IDE / Code Editor: (e.g., Visual Studio 2022 or Visual Studio Code) for development.
# Setup & Configuration
# Clone the repository:
# bash
# Copy
# Edit
# git clone https://github.com/yourusername/EZCondoBackend.git
# Configure the application:
# Open the appsettings.json file in the project directory.
# Update the ConnectionStrings section (for example, DefaultConnection) with your SQL Server connection string.
# (Optional) Adjust other settings such as the JWT secret key or application URLs if needed.
# Build the project (optional):
# bash
# Copy
# Edit
# cd EZCondoBackend
# dotnet restore
# dotnet build
# Running the Application
# Start the API:
# In the project directory, run:
# bash
# Copy
# Edit
# dotnet run
# This will launch the web API on http://localhost:5000 and https://localhost:5001 by default.
# Expose with ngrok (optional):
# bash
# Copy
# Edit
# ngrok http 7254
# Use the forwarding URL provided by ngrok (e.g., https://1234abcd.ngrok.io) to access the API publicly.
# Access Swagger UI:
# Open a browser and navigate to https://localhost:5001/swagger for the local Swagger interface.
# Or use the ngrok URL followed by /swagger (e.g., https://1234abcd.ngrok.io/swagger) to view it remotely.
# API Testing
# You can test the EZCondo API using Postman or the Swagger UI:
# Swagger UI: Browse the API documentation and execute requests directly in the browser.
# Postman: Import a Postman collection if provided, or manually create requests to the endpoints.
# Use HTTP methods (GET, POST, PUT, DELETE) to interact with the resources.
# For protected routes, first obtain a JWT by calling the appropriate authentication endpoint. Then set the Authorization header in Postman to Bearer <your-jwt-token>.
# Authentication: Remember that many API routes require a valid JWT for access. Always include the token in your requests to test secured endpoints.
# Contributing
# Contributions and feedback are welcome! To contribute:
# Fork this repository and create a new branch from main.
# Commit your changes and open a pull request.
# Feel free to open issues for bugs, features, or documentation improvements.
# Follow the existing code style and conventions.
# For any questions or support, please open an issue or contact the maintainers through the repository.
