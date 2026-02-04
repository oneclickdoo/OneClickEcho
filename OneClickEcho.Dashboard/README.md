# OneClickEcho Dashboard

Next.js admin dashboard for OneClickEcho application.

## Prerequisites

-   [Node.js](https://nodejs.org/en/download/prebuilt-installer)

## Getting Started

1. **Clone the repository:**

```bash
$ git clone https://gitlab.blackbird.rs/blackbird/oneclickecho
```

2. **Navigate to the project directory:**

```bash
$ cd OneClickEcho/OneClickEcho.Dashboard
```

3. **Install dependencies:**

```bash
$ npm install
```

4. **Run the application:**

```bash
$ npm run dev
```

The application will start, and you should see output indicating that the server is listening.

5. **Open your web browser:**

Navigate to http://localhost:3000/ to access the application.

6. **Build the application**

```bash
$ npm run build
```

7. **Update Google Chrome for DEV**

```chrome://flags/#allow-insecure-localhost```

8. **Manage certificates for development**

cd into certificates folder in OneClickEcho.Dashboard, and run the following command
```dotnet dev-certs https -ep ./certificate.crt --trust --format PEM```
