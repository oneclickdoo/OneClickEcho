import { NextRequest, NextResponse } from "next/server";
import createMiddleware from "next-intl/middleware";
import { routing } from "@/i18n/routing";
import { getApiInternalBase, getConnectTokenUrl } from "@/lib/serverApiBase";

const intlMiddleware = createMiddleware(routing);

export async function middleware(request: NextRequest) {
    // 1) API middleware (tvoj postojeći)
    if (request.nextUrl.pathname.startsWith("/api")) {
        const requestHeaders = new Headers(request.headers);
        const contentType = request.headers.get("content-type") || "";
        const isMultipart = contentType.includes("multipart/form-data");
        const isFileUpload =
            (request.method === "POST" || request.method === "PUT") && isMultipart;

        const accessToken = request.cookies.get("access_token");
        const refreshToken = request.cookies.get("refresh_token");

        if (!accessToken) {
            if (refreshToken) {
                const responseBackend = await fetch(getConnectTokenUrl(), {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/x-www-form-urlencoded"
                    },
                    body: new URLSearchParams({
                        grant_type: "refresh_token",
                        refresh_token: refreshToken.value
                    })
                });

                if (responseBackend.ok) {
                    const data = await responseBackend.json();

                    requestHeaders.set("Authorization", `Bearer ${data.access_token}`);

                    const response = isFileUpload
                        ? NextResponse.next({ request: { headers: requestHeaders } })
                        : NextResponse.rewrite(
                              `${getApiInternalBase()}${request.nextUrl.pathname}${request.nextUrl.search}`,
                              {
                                  request: { headers: requestHeaders }
                              }
                          );

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
            }
        } else {
            requestHeaders.set("Authorization", `Bearer ${accessToken.value}`);
        }

        if (isFileUpload) {
            return NextResponse.next({ request: { headers: requestHeaders } });
        }

        return NextResponse.rewrite(
            `${getApiInternalBase()}${request.nextUrl.pathname}${request.nextUrl.search}`,
            {
                request: { headers: requestHeaders }
            }
        );
    }

    // 2) Login/logout route handlers: do not run next-intl (locale redirects can break POST or cookies).
    if (request.nextUrl.pathname.includes("/auth/")) {
        return NextResponse.next();
    }

    // 3) Sve ostalo (UI) ide kroz next-intl: / -> /en, /sr, itd.
    return intlMiddleware(request);
}

export const config = {
    matcher: ["/((?!_next|.*\\..*).*)"]
};
