"use client";
import { useState } from "react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";

import { ProtectedRoute } from "@/context/AuthContext";
import { ActionNavigationLoadingProvider } from "@/context/ActionNavigationLoadingContext";
import { ShortcutsProvider } from "@/context/ShortcutsContext";

import { Sidebar } from "@/components/navigation/sidebar";
import { Toaster } from "@/components/tremor/Toaster";

export default function Layout({
    children
}: Readonly<{
    children: React.ReactNode;
}>) {
    // One client per layout mount — never `new QueryClient()` on every render (resets cache / races after setCurrentCompany).
    const [queryClient] = useState(
        () =>
            new QueryClient({
                defaultOptions: {
                    queries: {
                        staleTime: 30_000,
                        retry: 1
                    }
                }
            })
    );

    return (
        <ProtectedRoute>
            <QueryClientProvider client={queryClient}>
                <ShortcutsProvider>
                    <ActionNavigationLoadingProvider>
                        <Sidebar />
                        <Toaster />
                        <main className="lg:pl-72 min-h-screen dark:bg-gray-950">
                            <div className="relative">
                                <div className="p-4 sm:px-6 sm:pb-10 sm:pt-10 lg:px-10 lg:pt-7">{children}</div>
                            </div>
                        </main>
                    </ActionNavigationLoadingProvider>
                </ShortcutsProvider>
            </QueryClientProvider>
        </ProtectedRoute>
    );
}
