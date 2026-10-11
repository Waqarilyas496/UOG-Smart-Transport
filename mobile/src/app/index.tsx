import { Link } from 'expo-router';
import { StyleSheet, Pressable, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';

import { ThemedText } from '@/components/themed-text';
import { ThemedView } from '@/components/themed-view';
import { Spacing } from '@/constants/theme';

type Role = {
  label: string;
  description: string;
  href: string;
};

const roles: Role[] = [
  { label: 'Student', description: 'Track your bus, view your pass', href: '/(auth)/login?role=Student' },
  { label: 'Driver', description: 'Start trips, share location', href: '/(auth)/login?role=Driver' },
  { label: 'Admin', description: 'Manage fleet and routes', href: '/(auth)/login?role=Admin' },
];

export default function RoleSelectionScreen() {
  return (
    <ThemedView style={styles.container}>
      <SafeAreaView style={styles.safeArea}>
        <ThemedText type="title" style={styles.title}>
          UOG Smart Transport
        </ThemedText>
        <ThemedText type="small" style={styles.subtitle}>
          Select your role to continue
        </ThemedText>

        <View style={styles.roleList}>
          {roles.map((role) => (
            <Link key={role.label} href={role.href as any} asChild>
              <Pressable style={styles.roleCard}>
                <ThemedText type="defaultSemiBold" style={styles.roleLabel}>
                  {role.label}
                </ThemedText>
                <ThemedText type="small">{role.description}</ThemedText>
              </Pressable>
            </Link>
          ))}
        </View>
      </SafeAreaView>
    </ThemedView>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
  },
  safeArea: {
    flex: 1,
    paddingHorizontal: Spacing.four,
    justifyContent: 'center',
    gap: Spacing.four,
  },
  title: {
    textAlign: 'center',
  },
  subtitle: {
    textAlign: 'center',
    marginBottom: Spacing.four,
  },
  roleList: {
    gap: Spacing.three,
  },
  roleCard: {
    padding: Spacing.four,
    borderRadius: Spacing.three,
    borderWidth: 1,
    borderColor: '#ccc',
    gap: Spacing.one,
  },
  roleLabel: {
    fontSize: 20,
  },
});