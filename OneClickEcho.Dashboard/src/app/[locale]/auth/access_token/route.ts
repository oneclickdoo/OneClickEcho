import { NextResponse } from "next/server";

export async function POST(request: Request) {
    const url = `${process.env.NEXT_PUBLIC_API_URL}/connect/token`;
    
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
                maxAge: 60 * 60 - 10 // 1 hour and 10 seconds less
            });

            response.cookies.set("refresh_token", data.refresh_token, {
                httpOnly: true,
                secure: process.env.NODE_ENV === "production",
                path: "/",
                maxAge: 60 * 60 * 24 * 14 - 10 // 14 days and 10 seconds less
            });

            return response;
        }

        let errorMessage = "Authentication failed";
        try {
            const errBody = (await responseBackend.json()) as { error_description?: string; error?: string };
            errorMessage =
                errBody.error_description ?? errBody.error ?? responseBackend.statusText ?? errorMessage;
        } catch {
            // non-JSON error body
        }
        return NextResponse.json({ error: errorMessage }, { status: responseBackend.status || 400 });
    } catch (error) {
        console.error("[auth/access_token]", error);
        const message = error instanceof Error ? error.message : "Authentication failed";
        return NextResponse.json({ error: message }, { status: 500 });
    }
}
