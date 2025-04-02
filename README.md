# 📃 **Expenses Tracker**

A simple and user-friendly web application for managing and tracking personal expenses.

### ✅ **Features:**

- 📧 **User Authentication:** Secure login and registration using ASP.NET Core Identity.
- 💰 **Expense Management:** Add, edit, and delete expenses.
- 📊 **Reports:** Download expenses records as PDF / CSV.
- 🔍 **Pagination & Sorting:** Easily browse and sort records.
- 🌐 **Responsive UI:** Fully responsive design using Bootstrap 5.
- 📄 **Logging:** Integrated Serilog for structured logging.

---

## ⚙️ **Tech Stack:**

- **Frontend:** HTML5, CSS3, Bootstrap, Razor Pages
- **Backend:** C#, ASP.NET Core
- **Database:** MS SQL Server (with EntityFramework Core)
- **Authentication:** ASP.NET Core Identity
- **Logging:** Serilog
- **Packages:** Rotativa (PDF download), CSVHelper (CSV download)
- **Testing:** xUnit, Moq, AutoFixture, FluentAssertions
- **Version Control:** Git
- **Tools:** Visual Studio Community 2022, Visual Studio Code

---

## 💻 **Installation and Setup:**

### 1. Clone the Repository

```bash
git clone https://github.com/glenvillethedev/asp-net-core-app.git
cd ExpensesTracker
```

### 2. Install Dependencies

Make sure you have the latest .NET SDK installed.\
Install the necessary NuGet packages:

```bash
dotnet restore
```

(Rotativa) Download and save the wkhtmltopdf.exe under `wwwroot\Rotativa\`\
https://wkhtmltopdf.org/downloads.html

### 3. Configure the Database

- Update the `appsettings.json` file with your SQL Server connection string.

```json
"ConnectionStrings": {
  "DefaultConnection": "your_db_connection_string_here"
}
```

- Apply EF Core Migrations:

```bash
dotnet ef database update
```

### 4. Run the Application

```bash
dotnet run
```

Open your browser and navigate to\
`https://localhost:5001` 
(Default) 

---

## 💻 **Configuration:**

### AppSettings

Configure connection strings, logging, and authentication settings in `appsettings.json`.

### LaunchSettings

Configure launch settings (e.g. applicationUrl, environmentVariables, server) in `launchSettings.json`.

### Logging

Serilog configuration in `appsettings.json`:

```json
"Serilog": {
  "MinimumLevel": "Information",
  "Using": [
    "Serilog.Sinks.Console",
    "Serilog.Sinks.File"
  ],
  "Enrich": [ "FromLogContext" ],
  "WriteTo": [
    {
      "Name": "Console"
    },
    {
      "Name": "File",
      "Args": {
        "path": "logs/general/log-.txt",
        "rollingInterval": "Day",
        "fileSizeLimitBytes": 1048576,
        "rollOnFileSizeLimit": true,
        "shared": true,
        "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message}CorrelationID: {CorrelationID}{NewLine}{Exception}",
        "restrictedToMinimumLevel": "Information",
        "restrictedToMaximumLevel": "Warning"
      }
    },
    {
      "Name": "File",
      "Args": {
        "path": "logs/error/err-log-.txt",
        "rollingInterval": "Day",
        "fileSizeLimitBytes": 1048576,
        "rollOnFileSizeLimit": true,
        "shared": true,
        "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message}CorrelationID: {CorrelationID}{NewLine}{Exception}",
        "restrictedToMinimumLevel": "Error"
      }
    }
  ]
}
```

---

## 🌐 **Usage:**

### 1. Register

- Go to the "SignIn" page or click on the "Register" link. (/Account/Register)
- Fill in the required details.
- Click the "Register" button.
- If success, new user will be logged in.

### 2. SignIn

- Go to the "SignIn" page or click on the "SignIn" Link. (/Account/SignIn)
- Enter registered email and password.
- If success, user will be logged in.

### 3. Expenses Management

[Create]
- Click on the "Add Entry" button on the navigation. (/Entry/Add)
- Fill in the required details.
- Click on the "Add" button.
- If success, New Entry should be added on the table.

[Retrieve]
- Use the search fields to filter the results. (/Entry)
- Use the pagination to browse pages.
- Use the pagination dropdown to select number of records per page.
- Click on the table headers to sort the table.

[Update]
- Click on the "Update" button on the table. (/Entry/Update)
- Update the fields you want to change.
- Click on the "Update" button.
- If success, Updated Entry should be reflected on the table.

[Delete]
- Click on the "Delete" button on the table. (/Entry/Delete)
- Click on the "Delete" button to delete the entry.
- If success, Deleted Entry should be removed on the table.

---

## 📂 **Folder Structure:**

```
📁 ExpensesTracker (WebApp)
 ├── 📁 Properties -> contains launchSettings.json file
 ├── 📁 wwwroot → Static files (css, img, Rotativa)
     ├── 📁 css
     ├── 📁 img
     ├── 📁 Rotativa
 ├── 📁 Areas → Separate MVC for Areas (e.g. Admin Area)
     ├── 📁 Area
         ├── 📁 Controllers
         ├── 📁 Models
         ├── 📁 Views
 ├── 📁 Controllers → Controller Endpoints
 ├── 📁 Extensions → Extension Methods
     ├── 📝 StartupExtensionMethods.cs -> Configure app services & middleware here
 ├── 📁 logs → Serilog file logs
 ├── 📁 Middlewares → custom middlewares here
 ├── 📁 Views → RazorPages for frontend
 ├── 📝 appsettings.json → Configuration settings
 ├── 📝 entries.json -> Entry Table seed data
 ├── 🛠️ Program.cs → App Entry Point. (use StartupExtensionMethods.cs to configure services & middlewares)

📁 ExpensesTracker.Models (Models/Entities/DTOs)
 ├── 📁 DTOs → Data Transfer Objects used in the App
 ├── 📁 Entities → Database Models
 ├── 📁 Enums → Enums used in the Application
 ├── 📁 IdentityEntities → Models used for ASP.NET Core Identity
 ├── 📁 Migrations → EF Core migration scripts  
 ├── 📝 ApplicationDbContext.cs → DBContext file

📁 ExpensesTracker.Repositories (Repository Layer)
 ├── 📁 Interfaces
     ├── 📝 IRepository.cs -> Repository Interface
 ├── 📝 Repository.cs → Repository Implementation

📁 ExpensesTracker.Services (Service Layer)
 ├── 📁 Interfaces
     ├── 📝 IService.cs -> Service Interface
 ├── 📝 Service.cs → Service Implementation

📁 ExpensesTracker.Tests (Test Project)
 ├── 📝 EntryControllerTest.cs →  EntryController Unit Test File
 ├── 📝 EntryServiceTest.cs → EntryService Unit Test File
```

---

## 📃 **Future Updates:**

- 🎯 Email Verification
- 🎯 Password Recovery
- 🎯 User Settings
- 🎯 Admin Settings

---

## 📌 **Contact:**

- 🌐 GitHub: https://github.com/glenvillethedev
- 📧 Email: glenville.work@gmail.com
- 🛠️ LinkedIn: https://www.linkedin.com/in/glenville-maturan

