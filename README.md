# OneClickEcho

## **APP**

Welcome to the OneClickEcho. This README provides instructions on how to run the OneClickEcho APP. Please follow the steps below to set up and run the application.

### **Prerequisites**

Before running the application, ensure that you have the .NET 8 SDK installed on your machine. You can download it from the [official .NET website](https://dotnet.microsoft.com/download).

### **Running the APP**

Navigate to the project's root directory in your terminal or command prompt. Use the following commands to run the OneClickEcho APP:

```bash
dotnet run --project OneClickEcho.App
```

### **Migrating the database**

To migrate the database to the latest structure without data-loss run the following command:

```bash
dotnet ef migrations add <MigrationName> --project OneClickEcho.Persistence --startup-project OneClickEcho.Api
```

To update database schema run the following command:

```bash
dotnet ef database update --project OneClickEcho.Persistence --startup-project OneClickEcho.Api
```

### **Setting up the Certificates for Dev**

```bash
mkdir certificates
cd certificates
dotnet dev-certs https -ep ./certificate.crt --trust --format PEM
```
