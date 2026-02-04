import type { Metadata } from "next";
import { ThemeProvider } from "next-themes";
import { Inter } from "next/font/google";

import "./globals.css";
import "react-datepicker/dist/react-datepicker.css";

import { siteConfig } from "./siteConfig";

const inter = Inter({
    subsets: ["latin"],
    display: "swap",
    variable: "--font-inter"
});

export const metadata: Metadata = {
    metadataBase: new URL("https://oneclick.rs"),
    title: siteConfig.name,
    description: siteConfig.description,
    keywords: [],
    authors: [
        {
            name: "Oneclick Solutions d.o.o.",
            url: "https://oneclick.rs"
        }
    ],
    creator: "Oneclick",
    openGraph: {
        type: "website",
        locale: "en_US",
        url: siteConfig.url,
        title: siteConfig.name,
        description: siteConfig.description,
        siteName: siteConfig.name
    },
    icons: {
        icon: "/favicon.ico"
    }
};

export default function RootLayout({
    children
}: Readonly<{
    children: React.ReactNode;
}>) {
    return (
        <html lang="en" suppressHydrationWarning>
            <body
                className={`${inter.className} overflow-y-scroll antialiased dark:subpixel-antialiased scroll-auto selection:bg-indigo-100 selection:text-indigo-700 dark:bg-gray-950`}
                suppressHydrationWarning
            >
                <ThemeProvider defaultTheme="system" attribute="class">
                    <div className="mx-auto max-w-screen-2xl">{children}</div>
                </ThemeProvider>
            </body>
        </html>
    );
}
