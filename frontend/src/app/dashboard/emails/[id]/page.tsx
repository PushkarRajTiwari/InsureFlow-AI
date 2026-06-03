"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import { ArrowLeft } from "lucide-react";
import { AppHeader } from "@/components/AppHeader";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { apiFetch, EmailDetail, getToken } from "@/lib/api";

export default function EmailDetailPage() {
  const params = useParams<{ id: string }>();
  const router = useRouter();
  const [email, setEmail] = useState<EmailDetail | null>(null);
  const [error, setError] = useState("");

  useEffect(() => {
    if (!getToken()) {
      router.push("/login");
      return;
    }

    apiFetch<EmailDetail>(`/api/emails/${params.id}`)
      .then(setEmail)
      .catch((requestError) => setError(requestError.message));
  }, [params.id, router]);

  return (
    <main className="min-h-screen bg-background">
      <AppHeader />
      <div className="mx-auto max-w-5xl space-y-5 px-6 py-6">
        <Button asChild variant="ghost">
          <Link href="/dashboard">
            <ArrowLeft className="h-4 w-4" />
            Back
          </Link>
        </Button>

        {error ? <div className="rounded-md border border-border bg-card p-4 text-sm text-muted-foreground">{error}</div> : null}
        {!email ? (
          <div className="rounded-md border border-border bg-card p-4 text-sm text-muted-foreground">Loading...</div>
        ) : (
          <>
            <Card>
              <CardHeader>
                <CardTitle className="text-xl">{email.subject}</CardTitle>
                <p className="text-sm text-muted-foreground">
                  {email.sender} · {new Date(email.date).toLocaleString()} · Thread {email.threadId}
                </p>
              </CardHeader>
              <CardContent>
                <p className="whitespace-pre-wrap text-sm leading-6">{email.bodyPreview}</p>
              </CardContent>
            </Card>

            <section className="grid gap-4 md:grid-cols-3">
              <Card>
                <CardHeader>
                  <CardTitle>AI Category</CardTitle>
                </CardHeader>
                <CardContent className="text-lg font-semibold">{email.category ?? "Pending"}</CardContent>
              </Card>
              <Card>
                <CardHeader>
                  <CardTitle>Confidence</CardTitle>
                </CardHeader>
                <CardContent className="text-lg font-semibold">{email.confidence == null ? "-" : `${Math.round(email.confidence * 100)}%`}</CardContent>
              </Card>
              <Card>
                <CardHeader>
                  <CardTitle>Reasoning</CardTitle>
                </CardHeader>
                <CardContent className="text-sm leading-6 text-muted-foreground">{email.reasoning ?? "No reasoning available."}</CardContent>
              </Card>
            </section>
          </>
        )}
      </div>
    </main>
  );
}
