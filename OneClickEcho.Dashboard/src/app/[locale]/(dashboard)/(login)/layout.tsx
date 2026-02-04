"use client";

import { useEffect, ReactNode } from "react";
import { useRouter, useParams } from "next/navigation";
import { useAuth } from "@/context/AuthContext";

export default function RootLayout({ children }: { children: ReactNode }) {
    const router = useRouter();
    const params = useParams<{ locale: string }>();
    const locale = (params?.locale as string) || "en";

    const { authenticated } = useAuth();

    useEffect(() => {
        if (authenticated) {
            router.push(`/${locale}/overview`);
        }
    }, [authenticated, locale, router]);

    return <main className="min-h-screen dark:bg-gray-950">{children}</main>;
}
