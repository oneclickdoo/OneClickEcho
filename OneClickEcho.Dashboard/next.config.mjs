/** @type {import('next').NextConfig} */
import createNextIntlPlugin from "next-intl/plugin";

const withNextIntl = createNextIntlPlugin();

const nextConfig = {
    redirects: async () => {
        return [
            {
                source: "/",
                destination: "/en/login",
                permanent: true
            }
        ];
    },
    images: {
        remotePatterns: [
            {
                protocol: "https",
                hostname: "localhost"
            }
        ]
    },
    experimental: {
        proxyTimeout: 1000 * 600 // 10 min
    },
    webpack: (config, { dev }) => {
        if (dev) config.cache = false;
        return config;
    }
};

export default withNextIntl(nextConfig);
