import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  // Necessário para a imagem Docker (copia .next/standalone + server.js).
  output: "standalone",
  reactStrictMode: true,
  poweredByHeader: false,
};

export default nextConfig;
