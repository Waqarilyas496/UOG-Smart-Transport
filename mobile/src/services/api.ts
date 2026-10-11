const API_BASE_URL = 'http://192.168.100.14:5194';

type ApiError = { message: string };

async function request<T>(
  endpoint: string,
  options: RequestInit = {}
): Promise<T> {
  const response = await fetch(`${API_BASE_URL}${endpoint}`, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      ...options.headers,
    },
  });

  if (!response.ok) {
    const errorBody = (await response.json().catch(() => ({ message: 'Something went wrong' }))) as ApiError;
    throw new Error(errorBody.message || `Request failed with status ${response.status}`);
  }

  // 204 No Content has no body to parse
  if (response.status === 204) {
    return undefined as T;
  }

  return response.json() as Promise<T>;
}

export type AuthResponse = {
  token: string;
  userId: string;
  fullName: string;
  email: string;
  role: string;
};

export type LoginPayload = {
  email: string;
  password: string;
};

export function login(payload: LoginPayload): Promise<AuthResponse> {
  return request<AuthResponse>('/api/Auth/login', {
    method: 'POST',
    body: JSON.stringify(payload),
  });
}