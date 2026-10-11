import { useRouter } from 'expo-router';
import { Pressable } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';

import { ThemedText } from '@/components/themed-text';
import { ThemedView } from '@/components/themed-view';
import { useAuth } from '@/context/AuthContext';

export default function StudentDashboard() {
  const { user, signOut } = useAuth();
  const router = useRouter();

  return (
    <ThemedView style={{ flex: 1 }}>
      <SafeAreaView style={{ flex: 1, padding: 24, justifyContent: 'center', gap: 16 }}>
        <ThemedText type="title">Welcome, {user?.fullName}</ThemedText>
        <ThemedText>Student Dashboard</ThemedText>
        <Pressable
          onPress={async () => {
            await signOut();
            router.replace('/');
          }}
        >
          <ThemedText type="small">Logout</ThemedText>
        </Pressable>
      </SafeAreaView>
    </ThemedView>
  );
}