"use client";
import { AuthProvider } from "@/context/AuthContext";

export default function RootLayout({ children }: { children: JSX.Element }) {
    return <AuthProvider>{children}</AuthProvider>;
}
