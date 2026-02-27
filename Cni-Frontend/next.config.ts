import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  output: "standalone",
  async rewrites() {
    return [
      {
        source: "/api/:path*",
        destination: "https://cni-api-dev.oklidis.com.tr/api/:path*",
      },
    ];
  },
};

export default nextConfig;
