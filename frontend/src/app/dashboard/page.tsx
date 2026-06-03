"use client";

import Link from "next/link";
import { useEffect, useMemo, useState } from "react";
import { useRouter } from "next/navigation";
import { useGoogleLogin } from "@react-oauth/google";
import { CalendarDays, Download, Inbox, Loader2, Search } from "lucide-react";
import { AppHeader } from "@/components/AppHeader";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { apiFetch, DashboardStats, EmailListItem, getToken } from "@/lib/api";

const categories = [
  "",
  "PolicyChange",
  "Claim",
  "Billing",
  "CoverageQuestion",
  "Renewal",
  "ProofOfInsurance",
  "GeneralInquiry",
  "Spam"
];

export default function DashboardPage() {
  const router = useRouter();
  const [emails, setEmails] = useState<EmailListItem[]>([]);
  const [stats, setStats] = useState<DashboardStats | null>(null);
  const [category, setCategory] = useState("");
  const [search, setSearch] = useState("");
  const [from, setFrom] = useState("");
  const [to, setTo] = useState("");
  const [loading, setLoading] = useState(true);
  const [syncing, setSyncing] = useState(false);
  const [status, setStatus] = useState("");

  const query = useMemo(() => {
    const params = new URLSearchParams();
    if (category) params.set("category", category);
    if (search) params.set("search", search);
    if (from) params.set("from", new Date(from).toISOString());
    if (to) params.set("to", new Date(to).toISOString());
    return params.toString();
  }, [category, from, search, to]);

  async function load() {
    if (!getToken()) {
      router.push("/login");
      return;
    }

    setLoading(true);
    const [emailData, statData] = await Promise.all([
      apiFetch<EmailListItem[]>(`/api/emails${query ? `?${query}` : ""}`),
      apiFetch<DashboardStats>("/api/dashboard/stats")
    ]);
    setEmails(emailData);
    setStats(statData);
    setLoading(false);
  }

  useEffect(() => {
    load().catch((error) => {
      setStatus(error.message);
      setLoading(false);
    });
  }, [query]);

  const connectMailbox = useGoogleLogin({
    flow: "auth-code",
    scope: "openid email profile https://www.googleapis.com/auth/gmail.readonly",
    prompt: "consent",
    onSuccess: async (codeResponse) => {
      await apiFetch("/api/mailbox/connect", {
        method: "POST",
        body: JSON.stringify({ code: codeResponse.code })
      });
      setStatus("Gmail connected.");
    }
  });

  async function syncInbox() {
    setSyncing(true);
    setStatus("Importing and classifying latest Gmail messages...");
    try {
      const result = await apiFetch<{ imported: number; totalChecked: number }>("/api/emails/sync", { method: "POST" });
      setStatus(`Imported ${result.imported} new emails from ${result.totalChecked} checked messages.`);
      await load();
    } catch (error) {
      setStatus(error instanceof Error ? error.message : "Sync failed.");
    } finally {
      setSyncing(false);
    }
  }

  return (
    <main className="min-h-screen bg-background">
      <AppHeader />
      <div className="mx-auto max-w-7xl space-y-6 px-6 py-6">
        <div className="flex flex-col justify-between gap-4 md:flex-row md:items-center">
          <div>
            <h1 className="text-2xl font-semibold tracking-normal">Email Classification Dashboard</h1>
            <p className="mt-1 text-sm text-muted-foreground">Latest imported Gmail messages classified for insurance agency workflows.</p>
          </div>
          <div className="flex flex-wrap gap-2">
            <Button variant="outline" onClick={() => connectMailbox()}>
              <Inbox className="h-4 w-4" />
              Connect Gmail
            </Button>
            <Button onClick={syncInbox} disabled={syncing}>
              {syncing ? <Loader2 className="h-4 w-4 animate-spin" /> : <Download className="h-4 w-4" />}
              Sync Inbox
            </Button>
          </div>
        </div>

        <section className="grid gap-4 md:grid-cols-5">
          <StatCard label="Total Emails" value={stats?.totalEmails ?? 0} />
          <StatCard label="Claims" value={stats?.claims ?? 0} />
          <StatCard label="Billing" value={stats?.billing ?? 0} />
          <StatCard label="Policy Changes" value={stats?.policyChanges ?? 0} />
          <StatCard label="Coverage Questions" value={stats?.coverageQuestions ?? 0} />
        </section>

        <section className="rounded-lg border border-border bg-card">
          <div className="grid gap-3 border-b border-border p-4 md:grid-cols-[1.4fr_0.8fr_0.7fr_0.7fr]">
            <div className="relative">
              <Search className="pointer-events-none absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
              <Input value={search} onChange={(event) => setSearch(event.target.value)} placeholder="Search sender or subject" className="pl-9" />
            </div>
            <select value={category} onChange={(event) => setCategory(event.target.value)} className="h-10 rounded-md border border-input bg-card px-3 text-sm">
              {categories.map((item) => (
                <option key={item} value={item}>
                  {item || "All categories"}
                </option>
              ))}
            </select>
            <DateInput value={from} onChange={setFrom} label="From" />
            <DateInput value={to} onChange={setTo} label="To" />
          </div>
          {status ? <div className="border-b border-border px-4 py-3 text-sm text-muted-foreground">{status}</div> : null}
          <div className="overflow-x-auto">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Sender</TableHead>
                  <TableHead>Subject</TableHead>
                  <TableHead>Date</TableHead>
                  <TableHead>Category</TableHead>
                  <TableHead>Confidence</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {loading ? (
                  <TableRow>
                    <TableCell colSpan={5}>Loading...</TableCell>
                  </TableRow>
                ) : emails.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={5}>No emails imported yet.</TableCell>
                  </TableRow>
                ) : (
                  emails.map((email) => (
                    <TableRow key={email.id}>
                      <TableCell className="max-w-xs truncate">{email.sender}</TableCell>
                      <TableCell className="max-w-md truncate">
                        <Link className="font-medium text-primary hover:underline" href={`/dashboard/emails/${email.id}`}>
                          {email.subject}
                        </Link>
                      </TableCell>
                      <TableCell>{new Date(email.date).toLocaleDateString()}</TableCell>
                      <TableCell>{email.category ?? "Pending"}</TableCell>
                      <TableCell>{email.confidence == null ? "-" : `${Math.round(email.confidence * 100)}%`}</TableCell>
                    </TableRow>
                  ))
                )}
              </TableBody>
            </Table>
          </div>
        </section>
      </div>
    </main>
  );
}

function StatCard({ label, value }: { label: string; value: number }) {
  return (
    <Card>
      <CardHeader className="pb-2">
        <CardTitle className="text-xs uppercase text-muted-foreground">{label}</CardTitle>
      </CardHeader>
      <CardContent>
        <div className="text-3xl font-semibold">{value}</div>
      </CardContent>
    </Card>
  );
}

function DateInput({ value, onChange, label }: { value: string; onChange: (value: string) => void; label: string }) {
  return (
    <label className="relative block">
      <CalendarDays className="pointer-events-none absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
      <input
        aria-label={label}
        value={value}
        onChange={(event) => onChange(event.target.value)}
        type="date"
        className="h-10 w-full rounded-md border border-input bg-card px-3 pl-9 text-sm outline-none focus-visible:ring-2 focus-visible:ring-ring"
      />
    </label>
  );
}
