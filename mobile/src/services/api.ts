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
export type RegisterPayload = {
  fullName: string;
  email: string;
  password: string;
  role: 'Student' | 'Driver';
  phoneNumber?: string;
  rollNumber?: string;
  batch?: string;
  department?: string;
  semester?: number;
  licenseNumber?: string;
};

export function register(payload: RegisterPayload): Promise<AuthResponse> {
  return request<AuthResponse>('/api/Auth/register', {
    method: 'POST',
    body: JSON.stringify(payload),
  });
}
export type DriverMe = {
  driverId: string;
  fullName: string;
  licenseNumber: string;
  busId: string | null;
  busNumber: string | null;
  routeId: string | null;
  routeName: string | null;
  activeTripId: string | null;
};

export type Trip = {
  id: string;
  busNumber: string;
  routeName: string;
  status: string;
};

function authHeaders(token: string) {
  return { Authorization: `Bearer ${token}` };
}

export function getDriverMe(token: string): Promise<DriverMe> {
  return request<DriverMe>('/api/Drivers/me', { headers: authHeaders(token) });
}

export function startTrip(token: string, busId: string, routeId: string): Promise<Trip> {
  return request<Trip>('/api/Trips/start', {
    method: 'POST',
    headers: authHeaders(token),
    body: JSON.stringify({ busId, routeId }),
  });
}

export function endTrip(token: string, tripId: string): Promise<void> {
  return request<void>(`/api/Trips/${tripId}/end`, {
    method: 'POST',
    headers: authHeaders(token),
  });
}

export function sendLocation(
  token: string,
  tripId: string,
  data: {
    latitude: number;
    longitude: number;
    speed: number | null;
    heading: number | null;
    recordedAt: string;
  }
): Promise<unknown> {
  return request(`/api/trips/${tripId}/gpslocations`, {
    method: 'POST',
    headers: authHeaders(token),
    body: JSON.stringify(data),
  });
}