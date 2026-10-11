import { useState } from 'react';
import { useLocalSearchParams, useRouter } from 'expo-router';
import { StyleSheet, TextInput, Pressable, ActivityIndicator } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';

import { ThemedText } from '@/components/themed-text';
import { ThemedView } from '@/components/themed-view';
import { Spacing } from '@/constants/theme';
import { useAuth } from '@/context/AuthContext';
import { login } from '@/services/api';

export default function LoginScreen() {
  const { role } = useLocalSearchParams<{ role: string }>();
  const router = useRouter();
  const { signIn } = useAuth();

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function handleLogin() {
    setError(null);
    setLoading(true);
    try {
      const result = await login({ email, password });

      const matches =
        role === 'Admin'
          ? ['TransportAdmin', 'SuperAdmin'].includes(result.role)
          : result.role === role;

      if (!matches) {
        setError(`This account is registered as ${result.role}, not ${role}.`);
        setLoading(false);
        return;
      }

      await signIn(result);

      const dashboards: Record<string, string> = {
        Student: '/(student)/dashboard',
        Driver: '/(driver)/dashboard',
        TransportAdmin: '/(admin)/dashboard',
        SuperAdmin: '/(admin)/dashboard',
      };
      router.replace((dashboards[result.role] ?? '/') as any);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Login failed.');
    } finally {
      setLoading(false);
    }
  }

  return (
    <ThemedView style={styles.container}>
      <SafeAreaView style={styles.safeArea}>
        <ThemedText type="title" style={styles.title}>
          {role} Login
        </ThemedText>

        <TextInput
          style={styles.input}
          placeholder="Email"
          autoCapitalize="none"
          keyboardType="email-address"
          value={email}
          onChangeText={setEmail}
        />
        <TextInput
          style={styles.input}
          placeholder="Password"
          secureTextEntry
          value={password}
          onChangeText={setPassword}
        />

        {error && <ThemedText style={styles.error}>{error}</ThemedText>}

        <Pressable style={styles.button} onPress={handleLogin} disabled={loading}>
          {loading ? (
            <ActivityIndicator color="#fff" />
          ) : (
            <ThemedText style={styles.buttonText}>Login</ThemedText>
          )}
        </Pressable>

        <Pressable onPress={() => router.push(`/(auth)/register?role=${role}` as any)}>
          <ThemedText type="small" style={styles.link}>
            Don&apos;t have an account? Register
          </ThemedText>
        </Pressable>
      </SafeAreaView>
    </ThemedView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1 },
  safeArea: {
    flex: 1,
    paddingHorizontal: Spacing.four,
    justifyContent: 'center',
    gap: Spacing.three,
  },
  title: { textAlign: 'center', marginBottom: Spacing.three },
  input: {
    borderWidth: 1,
    borderColor: '#ccc',
    borderRadius: Spacing.two,
    padding: Spacing.three,
    fontSize: 16,
  },
  button: {
    backgroundColor: '#2563eb',
    borderRadius: Spacing.two,
    padding: Spacing.three,
    alignItems: 'center',
    marginTop: Spacing.two,
  },
  buttonText: { color: '#fff', fontWeight: '600' },
  error: { color: '#dc2626', textAlign: 'center' },
  link: { textAlign: 'center', marginTop: Spacing.three },
});