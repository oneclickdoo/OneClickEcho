import { NextResponse } from "next/server";

import { useSecureSessionCookies } from "@/lib/cookieSecure";
import { getConnectTokenUrl } from "@/lib/serverApiBase";

export async function POST(request: Request) {
    const url = getConnectTokenUrl();

    const rawBody = await request.text();
    const contentType =
        request.headers.get("content-type")?.split(";")[0]?.trim() || "application/x-www-form-urlencoded";

    try {
        const responseBackend = await fetch(url, {
            method: "POST",
            headers: {
                "Content-Type": contentType
            },
            body: rawBody
        });

        if (responseBackend.ok) {
            const data: { access_token: string; refresh_token: string } = await responseBackend.json();

            const response = NextResponse.json({}, { status: 200 });

            const secure = useSecureSessionCookies();

            response.cookies.set("access_token", data.access_token, {
                httpOnly: true,
                secure,
                sameSite: "lax",
                path: "/",
                maxAge: 60 * 60 - 10 // 1 hour and 10 seconds less
            });

            response.cookies.set("refresh_token", data.refresh_token, {
                httpOnly: true,
                secure,
                sameSite: "lax",
                path: "/",
                maxAge: 60 * 60 * 24 * 14 - 10 // 14 days and 10 seconds less
            });

            return response;
        }

        const errorText = await responseBackend.text();
        let errorMessage = "Authentication failed";
        if (errorText) {
            try {
                const errBody = JSON.parse(errorText) as { error_description?: string; error?: string };
                errorMessage =
                    errBody.error_description ?? errBody.error ?? responseBackend.statusText ?? errorMessage;
            } catch {
                errorMessage = errorText.length > 500 ? `${errorText.slice(0, 500)}…` : errorText;
            }
        }

        console.error("[auth/access_token] backend error", responseBackend.status, errorMessage);
        return NextResponse.json({ error: errorMessage }, { status: responseBackend.status || 400 });
    } catch (error) {
        console.error("[auth/access_token]", error);
        const message = error instanceof Error ? error.message : "Authentication failed";
        return NextResponse.json({ error: message }, { status: 500 });
    }
}
