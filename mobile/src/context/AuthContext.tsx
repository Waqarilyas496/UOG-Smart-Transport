import { createContext, useContext, useState, ReactNode } from 'react';
import * as SecureStore from 'expo-secure-store';

import type { AuthResponse } from '@/services/api';

type AuthContextType = {
  user: AuthResponse | null;
  signIn: (data: AuthResponse) => Promise<void>;
  signOut: () => Promise<void>;
};

const AuthContext = createContext<AuthContextType | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthResponse | null>(null);

  async function signIn(data: AuthResponse) {
    setUser(data);
    await SecureStore.setItemAsync('auth', JSON.stringify(data));
  }

  async function signOut() {
    setUser(null);
    await SecureStore.deleteItemAsync('auth');
  }

  return <AuthContext.Provider value={{ user, signIn, signOut }}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth must be used inside AuthProvider');
  return ctx;
}