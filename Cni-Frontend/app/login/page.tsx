"use client"

import { useState } from "react"
import { Eye, EyeOff, Loader2 } from "lucide-react"
import Link from "next/link"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { toast } from "sonner"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Checkbox } from "@/components/ui/checkbox"
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form"
import { useAuth } from "@/lib/auth/auth-context"
import { loginSchema, type LoginFormValues } from "@/lib/auth/schemas"
import { ApiError } from "@/lib/auth/api-client"

export default function LoginPage() {
  const [showPassword, setShowPassword] = useState(false)
  const { login } = useAuth()

  const form = useForm<LoginFormValues>({
    resolver: zodResolver(loginSchema),
    defaultValues: {
      email: "",
      password: "",
      rememberMe: false,
    },
  })

  const emailValue = form.watch("email")

  async function onSubmit(data: LoginFormValues) {
    try {
      await login(data)
    } catch (error) {
      if (error instanceof ApiError) {
        if (error.status === 401) {
          toast.error("Invalid email or password")
        } else {
          toast.error(error.message)
        }
      } else {
        toast.error("Unable to connect to the server")
      }
    }
  }

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

      {/* Right — Login Form */}
      <div className="flex w-full lg:w-1/2 items-center justify-center p-6 sm:p-12">
        <div className="w-full max-w-sm space-y-8">
          {/* Mobile logo */}
          <div className="lg:hidden">
            <span className="text-2xl tracking-tight" style={{ fontFamily: "'Cocon', sans-serif" }}>
              N2N Energy
            </span>
          </div>

          <div className="space-y-2">
            <h2 className="text-2xl font-semibold tracking-tight">Welcome back</h2>
            <p className="text-sm text-muted-foreground">
              Sign in to your account to continue
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

              <FormField
                control={form.control}
                name="password"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Password</FormLabel>
                    <FormControl>
                      <div className="relative">
                        <Input
                          type={showPassword ? "text" : "password"}
                          placeholder="Enter your password"
                          autoComplete="current-password"
                          className="pr-10"
                          {...field}
                        />
                        <button
                          type="button"
                          onClick={() => setShowPassword(!showPassword)}
                          className="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground hover:text-foreground transition-colors"
                          tabIndex={-1}
                        >
                          {showPassword ? (
                            <EyeOff className="h-4 w-4" />
                          ) : (
                            <Eye className="h-4 w-4" />
                          )}
                        </button>
                      </div>
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <div className="flex items-center justify-between">
                <FormField
                  control={form.control}
                  name="rememberMe"
                  render={({ field }) => (
                    <FormItem className="flex items-center gap-2 space-y-0">
                      <FormControl>
                        <Checkbox
                          checked={field.value}
                          onCheckedChange={field.onChange}
                        />
                      </FormControl>
                      <FormLabel className="text-sm font-normal cursor-pointer">
                        Remember me
                      </FormLabel>
                    </FormItem>
                  )}
                />
                <Link
                  href={emailValue ? `/forgot-password?email=${encodeURIComponent(emailValue)}` : "/forgot-password"}
                  className="text-sm text-primary hover:underline underline-offset-4"
                >
                  Forgot password?
                </Link>
              </div>

              <Button
                type="submit"
                className="w-full"
                disabled={form.formState.isSubmitting}
              >
                {form.formState.isSubmitting && (
                  <Loader2 className="h-4 w-4 animate-spin" />
                )}
                Sign in
              </Button>
            </form>
          </Form>

          <p className="text-center text-xs text-muted-foreground">
            Don&apos;t have an account?{" "}
            <a href="#" className="text-primary hover:underline underline-offset-4">
              Contact your administrator
            </a>
          </p>
        </div>
      </div>
    </div>
  )
}
