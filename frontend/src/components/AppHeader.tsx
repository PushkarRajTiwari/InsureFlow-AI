"use client";

import { useRouter } from "next/navigation";
import { LogOut, MailCheck } from "lucide-react";
import { Button } from "@/components/ui/button";
import { clearSession } from "@/lib/api";

export function AppHeader() {
  const router = useRouter();

  return (
    <header className="border-b border-border bg-card">
      <div className="mx-auto flex h-16 max-w-7xl items-center justify-between px-6">
        <div className="flex items-center gap-3">
          <div className="flex h-9 w-9 items-center justify-center rounded-md bg-primary text-primary-foreground">
            <MailCheck className="h-5 w-5" />
          </div>
          <div>
            <div className="text-sm font-semibold">InsureFlow AI</div>
            <div className="text-xs text-muted-foreground">Gmail classification MVP</div>
          </div>
        </div>
        <Button
          variant="ghost"
          size="icon"
          title="Sign out"
          onClick={() => {
            clearSession();
            router.push("/login");
          }}
        >
          <LogOut className="h-4 w-4" />
        </Button>
      </div>
    </header>
  );
}
