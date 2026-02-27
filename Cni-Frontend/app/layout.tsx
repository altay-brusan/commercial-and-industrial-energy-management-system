import type { Metadata } from "next";
import { Geist, Geist_Mono } from "next/font/google";
import localFont from "next/font/local";
import { ThemeProvider } from "@/components/theme-provider";
import { AuthProvider } from "@/lib/auth/auth-context";
import { FacilityProvider } from "@/lib/facilities/facility-context";
import { Toaster } from "@/components/ui/sonner";
import "./globals.css";

const geistSans = Geist({
  variable: "--font-geist-sans",
  subsets: ["latin"],
});

const geistMono = Geist_Mono({
  variable: "--font-geist-mono",
  subsets: ["latin"],
});

const cocon = localFont({
  src: "./fonts/cocon_regular.otf",
  variable: "--font-cocon",
  display: "swap",
});

export const metadata: Metadata = {
  title: "Energy Dashboard",
  description: "Monitor and manage your energy consumption",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en" suppressHydrationWarning>
      <body
        className={`${geistSans.variable} ${geistMono.variable} ${cocon.variable} antialiased`}
      >
        <ThemeProvider>
 <AuthProvider>
              <FacilityProvider>{children}</FacilityProvider>                                  
              <Toaster />
            </AuthProvider> 
        </ThemeProvider>
      </body>
    </html>
  );
}
