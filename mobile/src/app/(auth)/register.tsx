import { useState } from 'react';
import { useLocalSearchParams, useRouter } from 'expo-router';
import { StyleSheet, Pressable, ActivityIndicator, ScrollView } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';

import { AppInput } from '@/components/app-input';
import { ThemedText } from '@/components/themed-text';
import { ThemedView } from '@/components/themed-view';
import { Spacing } from '@/constants/theme';
import { useAuth } from '@/context/AuthContext';
import { register } from '@/services/api';

export default function RegisterScreen() {
  const { role } = useLocalSearchParams<{ role: string }>();
  const router = useRouter();
  const { signIn } = useAuth();

  const isStudent = role === 'Student';
  const isDriver = role === 'Driver';

  const [fullName, setFullName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [phoneNumber, setPhoneNumber] = useState('');
  const [rollNumber, setRollNumber] = useState('');
  const [batch, setBatch] = useState('');
  const [department, setDepartment] = useState('');
  const [semester, setSemester] = useState('');
  const [licenseNumber, setLicenseNumber] = useState('');

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function handleRegister() {
    setError(null);

    if (!fullName || !email || !password) {
      setError('Name, email and password are required.');
      return;
    }
    if (isStudent && (!rollNumber || !batch)) {
      setError('Roll number and batch are required.');
      return;
    }
    if (isDriver && !licenseNumber) {
      setError('License number is required.');
      return;
    }

    setLoading(true);
    try {
      const result = await register({
        fullName,
        email,
        password,
        role: role as 'Student' | 'Driver',
        phoneNumber: phoneNumber || undefined,
        rollNumber: isStudent ? rollNumber : undefined,
        batch: isStudent ? batch : undefined,
        department: isStudent ? department : undefined,
        semester: isStudent && semester ? Number(semester) : undefined,
        licenseNumber: isDriver ? licenseNumber : undefined,
      });

      await signIn(result);
      router.replace((isStudent ? '/(student)/dashboard' : '/(driver)/dashboard') as any);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Registration failed.');
    } finally {
      setLoading(false);
    }
  }

  if (!isStudent && !isDriver) {
    return (
      <ThemedView style={styles.container}>
        <SafeAreaView style={styles.safeArea}>
          <ThemedText style={styles.center}>
            Admin accounts are created by the university. Please log in instead.
          </ThemedText>
          <Pressable onPress={() => router.back()}>
            <ThemedText type="small" style={styles.link}>Back to login</ThemedText>
          </Pressable>
        </SafeAreaView>
      </ThemedView>
    );
  }

  return (
    <ThemedView style={styles.container}>
      <SafeAreaView style={{ flex: 1 }}>
        <ScrollView contentContainerStyle={styles.scroll}>
          <ThemedText type="title" style={styles.title}>{role} Register</ThemedText>

          <AppInput placeholder="Full name" value={fullName} onChangeText={setFullName} />
          <AppInput
            placeholder="Email"
            autoCapitalize="none"
            keyboardType="email-address"
            value={email}
            onChangeText={setEmail}
          />
          <AppInput placeholder="Password" secureTextEntry value={password} onChangeText={setPassword} />
          <AppInput
            placeholder="Phone number (optional)"
            keyboardType="phone-pad"
            value={phoneNumber}
            onChangeText={setPhoneNumber}
          />

          {isStudent && (
            <>
              <AppInput placeholder="Roll number" value={rollNumber} onChangeText={setRollNumber} />
              <AppInput placeholder="Batch (e.g. 2023-2027)" value={batch} onChangeText={setBatch} />
              <AppInput placeholder="Department" value={department} onChangeText={setDepartment} />
              <AppInput
                placeholder="Semester"
                keyboardType="number-pad"
                value={semester}
                onChangeText={setSemester}
              />
            </>
          )}

          {isDriver && (
            <AppInput placeholder="License number" value={licenseNumber} onChangeText={setLicenseNumber} />
          )}

          {error && <ThemedText style={styles.error}>{error}</ThemedText>}

          <Pressable style={styles.button} onPress={handleRegister} disabled={loading}>
            {loading ? <ActivityIndicator color="#fff" /> : <ThemedText style={styles.buttonText}>Register</ThemedText>}
          </Pressable>

          <Pressable onPress={() => router.back()}>
            <ThemedText type="small" style={styles.link}>Already have an account? Login</ThemedText>
          </Pressable>
        </ScrollView>
      </SafeAreaView>
    </ThemedView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1 },
  safeArea: { flex: 1, padding: Spacing.four, justifyContent: 'center', gap: Spacing.three },
  scroll: { padding: Spacing.four, gap: Spacing.three, paddingBottom: 48 },
  title: { textAlign: 'center', marginBottom: Spacing.three },
  center: { textAlign: 'center' },
  button: {
    backgroundColor: '#2563eb',
    borderRadius: 8,
    padding: Spacing.three,
    alignItems: 'center',
    marginTop: Spacing.two,
  },
  buttonText: { color: '#fff', fontWeight: '600' },
  error: { color: '#dc2626', textAlign: 'center' },
  link: { textAlign: 'center', marginTop: Spacing.three },
});