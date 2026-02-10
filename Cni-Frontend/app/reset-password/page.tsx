"use client"

import { useState } from "react"
import { Eye, EyeOff, CheckCircle2 } from "lucide-react"
import Link from "next/link"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"

export default function ResetPasswordPage() {
  const [showPassword, setShowPassword] = useState(false)
  const [showConfirm, setShowConfirm] = useState(false)
  const [submitted, setSubmitted] = useState(false)

  return (
    <div className="flex min-h-svh">
      {/* Left — Branding Panel */}
      <div className="hidden lg:flex lg:w-1/2 relative bg-primary items-center justify-center overflow-hidden">
        <div className="absolute inset-0 opacity-10">
          <div className="absolute inset-0" style={{
            backgroundImage: `radial-gradient(circle at 1px 1px, rgba(255,255,255,0.3) 1px, transparent 0)`,
            backgroundSize: '40px 40px'
          }} />
        </div>

        <div className="absolute -top-24 -left-24 w-96 h-96 rounded-full bg-energy-solar/20 blur-3xl" />
        <div className="absolute -bottom-32 -right-32 w-[500px] h-[500px] rounded-full bg-energy-battery/20 blur-3xl" />
        <div className="absolute top-1/3 right-1/4 w-64 h-64 rounded-full bg-energy-consumption/15 blur-3xl" />

        <div className="relative z-10 max-w-md px-12 text-primary-foreground">
          <div className="mb-8">
            <span className="text-3xl tracking-tight" style={{ fontFamily: "'Cocon', sans-serif" }}>
              N2N Energy
            </span>
          </div>
          <h1 className="text-4xl font-bold leading-tight mb-4">
            Distributed Energy Resource Management
          </h1>
          <p className="text-lg text-primary-foreground/70 leading-relaxed">
            Monitor, control, and optimize your distributed energy assets from a single unified platform.
          </p>
        </div>
      </div>

      {/* Right — Reset Password Form */}
      <div className="flex w-full lg:w-1/2 items-center justify-center p-6 sm:p-12">
        <div className="w-full max-w-sm space-y-8">
          {/* Mobile logo */}
          <div className="lg:hidden">
            <span className="text-2xl tracking-tight" style={{ fontFamily: "'Cocon', sans-serif" }}>
              N2N Energy
            </span>
          </div>

          {submitted ? (
            <div className="space-y-6">
              <div className="flex items-center justify-center w-12 h-12 rounded-full bg-energy-solar/10">
                <CheckCircle2 className="w-6 h-6 text-energy-solar" />
              </div>
              <div className="space-y-2">
                <h2 className="text-2xl font-semibold tracking-tight">Password reset successful</h2>
                <p className="text-sm text-muted-foreground">
                  Your password has been updated. You can now sign in with your new password.
                </p>
              </div>
              <Button className="w-full" asChild>
                <Link href="/login">
                  Sign in
                </Link>
              </Button>
            </div>
          ) : (
            <>
              <div className="space-y-2">
                <h2 className="text-2xl font-semibold tracking-tight">Reset your password</h2>
                <p className="text-sm text-muted-foreground">
                  Enter a new password for your account.
                </p>
              </div>

              <form className="space-y-5" onSubmit={(e) => { e.preventDefault(); setSubmitted(true) }}>
                <div className="space-y-2">
                  <Label htmlFor="password">New password</Label>
                  <div className="relative">
                    <Input
                      id="password"
                      type={showPassword ? "text" : "password"}
                      placeholder="Enter new password"
                      autoComplete="new-password"
                      className="pr-10"
                    />
                    <button
                      type="button"
                      onClick={() => setShowPassword(!showPassword)}
                      className="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-foreground transition-colors"
                      tabIndex={-1}
                    >
                      {showPassword ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
                    </button>
                  </div>
                </div>

                <div className="space-y-2">
                  <Label htmlFor="confirm-password">Confirm password</Label>
                  <div className="relative">
                    <Input
                      id="confirm-password"
                      type={showConfirm ? "text" : "password"}
                      placeholder="Confirm new password"
                      autoComplete="new-password"
                      className="pr-10"
                    />
                    <button
                      type="button"
                      onClick={() => setShowConfirm(!showConfirm)}
                      className="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-foreground transition-colors"
                      tabIndex={-1}
                    >
                      {showConfirm ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
                    </button>
                  </div>
                </div>

                <Button type="submit" className="w-full">
                  Reset password
                </Button>
              </form>
            </>
          )}
        </div>
      </div>
    </div>
  )
}
