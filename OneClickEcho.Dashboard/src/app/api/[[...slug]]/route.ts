import { NextRequest, NextResponse } from "next/server";
import { getApiInternalBase } from "@/lib/serverApiBase";

export const runtime = "nodejs";
export const dynamic = "force-dynamic";

function buildUpstreamPath(slug: string[] | undefined): string {
    if (!slug?.length) {
        return "/api";
    }
    return `/api/${slug.join("/")}`;
}

function bearerFromRequest(req: NextRequest): string | null {
    const header = req.headers.get("authorization");
    if (header?.startsWith("Bearer ")) {
        return header;
    }
    const cookieToken = req.cookies.get("access_token")?.value;
    if (cookieToken) {
        return `Bearer ${cookieToken}`;
    }
    return null;
}

async function proxyMultipartToApi(req: NextRequest, slug: string[] | undefined) {
    const contentType = req.headers.get("content-type") || "";
    if (!contentType.includes("multipart/form-data")) {
        return NextResponse.json({ error: "Expected multipart/form-data" }, { status: 400 });
    }

    const auth = bearerFromRequest(req);
    if (!auth) {
        return NextResponse.json({ error: "Unauthorized" }, { status: 401 });
    }

    const base = getApiInternalBase().replace(/\/$/, "");
    if (!base) {
        return NextResponse.json({ error: "API base URL not configured" }, { status: 500 });
    }

    const path = buildUpstreamPath(slug);
    const search = new URL(req.url).search;
    const url = `${base}${path}${search}`;

    const body = await req.arrayBuffer();

    const upstream = await fetch(url, {
        method: req.method,
        headers: {
            Authorization: auth,
            "Content-Type": contentType
        },
        body
    });

    const outBody = await upstream.arrayBuffer();
    const res = new NextResponse(outBody, { status: upstream.status });
    const ctOut = upstream.headers.get("content-type");
    if (ctOut) {
        res.headers.set("content-type", ctOut);
    }
    return res;
}

export async function POST(req: NextRequest, context: { params: { slug?: string[] } }) {
    return proxyMultipartToApi(req, context.params.slug);
}

export async function PUT(req: NextRequest, context: { params: { slug?: string[] } }) {
    return proxyMultipartToApi(req, context.params.slug);
}
