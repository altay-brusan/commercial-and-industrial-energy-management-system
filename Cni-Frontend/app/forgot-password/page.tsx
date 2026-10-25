"use client"

import { Suspense, useState } from "react"
import { useSearchParams } from "next/navigation"
import { ArrowLeft, Loader2, Mail } from "lucide-react"
import Link from "next/link"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { toast } from "sonner"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form"
import { forgotPasswordSchema, type ForgotPasswordFormValues } from "@/lib/auth/schemas"
import * as authService from "@/lib/auth/auth-service"
import { ApiError } from "@/lib/auth/api-client"

function ForgotPasswordForm() {
  const searchParams = useSearchParams()
  const [submitted, setSubmitted] = useState(false)

  const form = useForm<ForgotPasswordFormValues>({
    resolver: zodResolver(forgotPasswordSchema),
    defaultValues: {
      email: searchParams.get("email") ?? "",
    },
  })

  async function onSubmit(data: ForgotPasswordFormValues) {
    try {
      await authService.forgotPassword(data)
      setSubmitted(true)
    } catch (error) {
      if (error instanceof ApiError) {
        toast.error(error.message)
      } else {
        toast.error("Unable to connect to the server")
      }
    }
  }

  return (
    <div className="w-full max-w-sm space-y-8">
      {/* Mobile logo */}
      <div className="lg:hidden">
        <span className="text-2xl tracking-tight" style={{ fontFamily: "'Cocon', sans-serif" }}>
          N2N Energy
        </span>
      </div>

      {submitted ? (
        /* Success State */
        <div className="space-y-6">
          <div className="flex items-center justify-center w-12 h-12 rounded-full bg-energy-solar/10">
            <Mail className="w-6 h-6 text-energy-solar" />
          </div>
          <div className="space-y-2">
            <h2 className="text-2xl font-semibold tracking-tight">Check your email</h2>
            <p className="text-sm text-muted-foreground">
              We&apos;ve sent a password reset link to your email address. Please check your inbox and follow the instructions.
            </p>
          </div>
          <Button variant="outline" className="w-full" asChild>
            <Link href="/login">
              <ArrowLeft className="h-4 w-4" />
              Back to sign in
            </Link>
          </Button>
        </div>
      ) : (
        /* Form State */
        <>
          <div className="space-y-2">
            <h2 className="text-2xl font-semibold tracking-tight">Forgot password?</h2>
            <p className="text-sm text-muted-foreground">
              Enter your email address and we&apos;ll send you a link to reset your password.
            </p>
          </div>

          <Form {...form}>
            <form className="space-y-5" onSubmit={form.handleSubmit(onSubmit)}>
              <FormField
                control={form.control}
                name="email"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Email</FormLabel>
                    <FormControl>
                      <Input
                        type="email"
                        placeholder="name@company.com"
                        autoComplete="email"
                        {...field}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <Button
                type="submit"
                className="w-full"
                disabled={form.formState.isSubmitting}
              >
                {form.formState.isSubmitting && (
                  <Loader2 className="h-4 w-4 animate-spin" />
                )}
                Send reset link
              </Button>
            </form>
          </Form>

          <div className="text-center">
            <Link
              href="/login"
              className="inline-flex items-center gap-1.5 text-sm text-muted-foreground hover:text-foreground transition-colors"
            >
              <ArrowLeft className="h-3.5 w-3.5" />
              Back to sign in
            </Link>
          </div>
        </>
      )}
    </div>
  )
}

export default function ForgotPasswordPage() {
  return (
    <div className="flex min-h-svh">
      {/* Left — Branding Panel */}
      <div className="hidden lg:flex lg:w-1/2 relative bg-primary items-center justify-center overflow-hidden">
        {/* Decorative grid */}
        <div className="absolute inset-0 opacity-10">
          <div className="absolute inset-0" style={{
            backgroundImage: `radial-gradient(circle at 1px 1px, rgba(255,255,255,0.3) 1px, transparent 0)`,
            backgroundSize: '40px 40px'
          }} />
        </div>

        {/* Decorative circles */}
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

      {/* Right — Forgot Password Form */}
      <div className="flex w-full lg:w-1/2 items-center justify-center p-6 sm:p-12">
        <Suspense fallback={<div className="w-full max-w-sm" />}>
          <ForgotPasswordForm />
        </Suspense>
      </div>
    </div>
  )
}
