# Mefisto Theatre Web Application

## Overview

Mefisto Theatre is a web-based platform designed to foster community engagement through discussions, announcements, and insights into theatre performances. Built with .NET 9 and Entity Framework Core, it features user authentication, role-based access, and a structured forum system.

GitHub Repository: [https://github.com/OlehDzhumyk/mefisto-theatre](https://github.com/OlehDzhumyk/mefisto-theatre)

## Features

- **User Roles**: Admin, Staff, Member
- **Forum Categories**: General, Announcements, Performances, Behind the Scenes, Community
- **Content Management**: Create and manage posts and comments
- **User Management**: Role assignments and ban functionality
- **Data Seeding**: Pre-populated users, roles, categories, posts, and comments for testing

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)

### Installation

1. **Clone the repository**:

   ```bash
   git clone https://github.com/OlehDzhumyk/mefisto-theatre.git
   cd mefisto-theatre
   ```

2. **Configure the database**:

   Update the `DefaultConnection` string in `appsettings.json`:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MefistoTheatreDb;Trusted_Connection=True;MultipleActiveResultSets=true"
     },
     "Logging": {
       "LogLevel": {
         "Default": "Information",
         "Microsoft.AspNetCore": "Warning"
       }
     },
     "AllowedHosts": "*",
     "EmailSettings": {
       "FromEmail": "noreply@mefistotheatre.com",
       "SmtpServer": "smtp.example.com",
       "Port": 587,
       "EnableSsl": true
     }
   }
   ```

3. **Run the application**:

   ```bash
   dotnet run
   ```

   Access the application at `https://localhost:5001`.

## Test User Credentials

Use the following credentials to log in and explore different user roles:

| Role   | Email               | Password   |
|--------|---------------------|------------|
| Admin  | admin@example.com   | Admin@123  |
| Staff  | staff1@example.com  | Staff@123  |
| Member | member1@example.com | Member@123 |
| Banned | banned1@example.com | Banned@123 |

> **Note**: The banned user account (`banned1@example.com`) is restricted from accessing certain features.

## License

This project is licensed under the MIT License and is suitable for educational and college-level use.

---

