# 🏢 EZCondo - Condo Management Backend

### 🔰 Introduction

**EZCondo** is a modern condominium (condo) management system designed for residents, property managers, and the administrative board. It centralizes key operations and communications in one platform, improving efficiency and organization for multi-unit residential communities.

This repository contains the **backend API**, built with **ASP.NET Core (.NET 8)**, providing endpoints for core functionalities like resident management, maintenance requests, and announcements.

---

### 🚀 Main Features

- 🔐 **Authentication & Authorization** (JWT)
- 🧾 **CRUD Operations**
- 💸 **Mobile Payment Scanning**
- 📧 **Email Notifications**
- 📁 **Import/Export Excel**
- 🔔 **Real-time Notifications** (SignalR for Web, Firebase for App)
- ☁️ **Image Storage on Cloudinary**

---

### 📦 Technologies Used

- **ASP.NET Core (.NET 8)**
- **SQL Server**
- **JWT (JSON Web Tokens)**
- **Swagger (OpenAPI)**
- **Firebase**
- **SignalR**
- **PayOS**
- **MailKit**
- **ClosedXML**
- **Cloudinary**
- **Hangfire**

---

### 🧩 Features

- **Resident Management** – Create, view, update, and delete resident profiles.
- **Incident Management** – View,create,update incident in the apartment.
- **Community Announcements** – Post and manage notices or bulletins.
- **Payment Tracking** – Record and manage condo fees and payment history.
- **Service Management** – Manage various services such as cleaning, repairs,gym, .... Allows creating, updating, feedback ..
---

### 💻 Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/)
- SQL Server instance running
- [ngrok](https://ngrok.com/) *(optional)*
- Postman *(optional)*
- Visual Studio 2022 / VS Code

---

### ⚙️ Setup & Configuration

```bash
# Clone the repository
git clone https://github.com/RogueMice/EzCondo_BE

# Navigate into project directory
cd EZCondoBackend

# Restore and build
dotnet restore
dotnet build
```

#### 🔧 Configuration

- Open `appsettings.json`.
- Update `ConnectionStrings` with your SQL Server credentials.
- Set JWT secrets and other required values.

---

### ▶️ Running the API

```bash
dotnet run
```

- Default URL:  
  `http://localhost:7254`  

#### 🌐 Optional: Expose with ngrok

```bash
ngrok http 7254
```

- Example:  
  `https://1234abcd.ngrok.io/swagger`

---

### 🧪 API Testing

- **Swagger UI**:  
  `https://localhost:7254/swagger`

- **Postman**:
  - Import collection or manually add requests.
  - Add JWT to headers:  
    ```
    Authorization: Bearer <your-jwt-token>
    ```

---

### 🤝 Contributing

Contributions are welcome!

1. Fork the repo and create a new branch.
2. Make your changes and commit.
3. Open a pull request.

Please follow coding conventions and feel free to open issues for bugs or suggestions.

---

### 📬 Contact

For support or questions, open an issue or contact the maintainers.
