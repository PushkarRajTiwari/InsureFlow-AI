"use client";

import { useRouter } from "next/navigation";
import { useGoogleLogin } from "@react-oauth/google";
import { Mail, ShieldCheck } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { apiFetch, setSession } from "@/lib/api";

export default function LoginPage() {
  const googleClientId = process.env.NEXT_PUBLIC_GOOGLE_CLIENT_ID;

  if (!googleClientId) {
    return (
      <main className="flex min-h-screen items-center justify-center bg-background px-4">
        <LoginCard>
          <Button className="w-full" disabled>
            <Mail className="h-4 w-4" />
            Google credentials missing
          </Button>
          <p className="mt-3 text-sm text-muted-foreground">Add GOOGLE_CLIENT_ID and GOOGLE_CLIENT_SECRET to .env when you are ready to test sign-in.</p>
        </LoginCard>
      </main>
    );
  }

  return <GoogleLoginExperience />;
}

function GoogleLoginExperience() {
  const router = useRouter();

  const login = useGoogleLogin({
    flow: "auth-code",
    scope: "openid email profile",
    onSuccess: async (codeResponse) => {
      const response = await apiFetch<{ token: string; user: { id: string; email: string; name: string } }>("/api/auth/google", {
        method: "POST",
        body: JSON.stringify({ code: codeResponse.code })
      });
      setSession(response.token, response.user);
      router.push("/dashboard");
    }
  });

  return (
    <main className="flex min-h-screen items-center justify-center bg-background px-4">
      <LoginCard>
        <Button className="w-full" onClick={() => login()}>
          <Mail className="h-4 w-4" />
          Sign in with Google
        </Button>
      </LoginCard>
    </main>
  );
}

function LoginCard({ children }: { children: React.ReactNode }) {
  return (
    <Card className="w-full max-w-md">
      <CardHeader>
        <div className="mb-3 flex h-11 w-11 items-center justify-center rounded-md bg-primary text-primary-foreground">
          <ShieldCheck className="h-5 w-5" />
        </div>
        <CardTitle className="text-2xl">InsureFlow AI</CardTitle>
        <p className="text-sm text-muted-foreground">Sign in to classify insurance emails from Gmail.</p>
      </CardHeader>
      <CardContent>{children}</CardContent>
    </Card>
  );
}
