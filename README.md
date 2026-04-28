# OneClickEcho

Source repository: [github.com/oneclickdoo/OneClickEcho](https://github.com/oneclickdoo/OneClickEcho)

**Operativna dokumentacija** (Docker, Next.js, nginx, Git, server): [docs/PROJECT-HANDBOOK.md](docs/PROJECT-HANDBOOK.md)

```bash
git clone https://github.com/oneclickdoo/OneClickEcho.git
cd OneClickEcho
```

The first push from this machine (after the repo exists on GitHub and you are logged in):

```bash
git push -u origin main
```

CI: `.gitlab-ci.yml` is for GitLab runners. If you only use GitHub, add GitHub Actions workflows or remove the file.

Secrets (OpenAI, SMS, Viber, database) are not committed. For local API development use [.NET User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets) or environment variables matching `OneClickEcho.Api` configuration keys.

If OpenAI calls must go through a **Blackbird** (or other) OpenAI-compatible gateway instead of `https://api.openai.com/`, set **`OpenAi:BaseUrl`** to that gateway’s base URL (the app still calls `/v1/chat/completions` on that host). Use **`OpenAi__BaseUrl`** in Docker/env.

## **APP**

Welcome to the OneClickEcho. This README provides instructions on how to run the OneClickEcho APP. Please follow the steps below to set up and run the application.

### **Prerequisites**

Before running the application, ensure that you have the .NET 8 SDK installed on your machine. You can download it from the [official .NET website](https://dotnet.microsoft.com/download).

### **Running the APP**

Navigate to the project's root directory in your terminal or command prompt. Use the following commands to run the OneClickEcho APP:

```bash
dotnet run --project OneClickEcho.Api
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
