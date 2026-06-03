const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:8080";

export type User = {
  id: string;
  email: string;
  name: string;
};

export type EmailListItem = {
  id: string;
  sender: string;
  subject: string;
  date: string;
  category: string | null;
  confidence: number | null;
};

export type EmailDetail = EmailListItem & {
  bodyPreview: string;
  threadId: string;
  reasoning: string | null;
};

export type DashboardStats = {
  totalEmails: number;
  claims: number;
  billing: number;
  policyChanges: number;
  coverageQuestions: number;
};

export function getToken() {
  if (typeof window === "undefined") {
    return null;
  }

  return window.localStorage.getItem("insureflow_token");
}

export function setSession(token: string, user: User) {
  window.localStorage.setItem("insureflow_token", token);
  window.localStorage.setItem("insureflow_user", JSON.stringify(user));
}

export function clearSession() {
  window.localStorage.removeItem("insureflow_token");
  window.localStorage.removeItem("insureflow_user");
}

export async function apiFetch<T>(path: string, init: RequestInit = {}): Promise<T> {
  const token = getToken();
  const response = await fetch(`${API_BASE_URL}${path}`, {
    ...init,
    headers: {
      "Content-Type": "application/json",
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...init.headers
    }
  });

  if (!response.ok) {
    const error = await response.text();
    throw new Error(error || `Request failed with ${response.status}`);
  }

  return response.json();
}
