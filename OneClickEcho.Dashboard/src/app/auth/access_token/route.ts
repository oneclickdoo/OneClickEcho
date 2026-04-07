import { NextResponse } from "next/server";

import { getConnectTokenUrl } from "@/lib/serverApiBase";

export async function POST(request: Request) {
    const url = getConnectTokenUrl();

    const formData = await request.formData();
    const params = new URLSearchParams();

    formData.forEach((value, key) => {
        params.append(key, value.toString());
    });

    try {
        const responseBackend = await fetch(url, {
            method: "POST",
            headers: {
                "Content-Type": "application/x-www-form-urlencoded"
            },
            body: params
        });

        if (responseBackend.ok) {
            const data: { access_token: string; refresh_token: string } = await responseBackend.json();

            const response = NextResponse.json({}, { status: 200 });

            response.cookies.set("access_token", data.access_token, {
                httpOnly: true,
                secure: process.env.NODE_ENV === "production",
                path: "/",
                maxAge: 60 * 60 - 10
            });

            response.cookies.set("refresh_token", data.refresh_token, {
                httpOnly: true,
                secure: process.env.NODE_ENV === "production",
                path: "/",
                maxAge: 60 * 60 * 24 * 14 - 10
            });

            return response;
        }

        const data: { error_description?: string } = await responseBackend.json();
        return NextResponse.json(
            { error: data.error_description ?? "Authentication failed." },
            { status: 400 }
        );
    } catch {
        return NextResponse.json(
            { error: "Authentication request failed." },
            { status: 500 }
        );
    }
}