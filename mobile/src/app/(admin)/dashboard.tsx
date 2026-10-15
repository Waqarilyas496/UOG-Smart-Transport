import { useCallback, useEffect, useState } from 'react';
import { useRouter } from 'expo-router';
import { Pressable, RefreshControl, ScrollView, StyleSheet, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';

import { ThemedText } from '@/components/themed-text';
import { ThemedView } from '@/components/themed-view';
import { useAuth } from '@/context/AuthContext';
import {
  BusItem,
  DriverItem,
  getBuses,
  getDrivers,
  getRoutes,
  getTrips,
  RouteItem,
  TripItem,
} from '@/services/api';

type Tab = 'Buses' | 'Routes' | 'Drivers' | 'Trips';
const TABS: Tab[] = ['Buses', 'Routes', 'Drivers', 'Trips'];

export default function AdminDashboard() {
  const { user, signOut } = useAuth();
  const router = useRouter();
  const token = user?.token ?? '';

  const [tab, setTab] = useState<Tab>('Buses');
  const [buses, setBuses] = useState<BusItem[]>([]);
  const [routes, setRoutes] = useState<RouteItem[]>([]);
  const [drivers, setDrivers] = useState<DriverItem[]>([]);
  const [trips, setTrips] = useState<TripItem[]>([]);
  const [refreshing, setRefreshing] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    try {
      const [b, r, d, t] = await Promise.all([
        getBuses(token),
        getRoutes(token),
        getDrivers(token),
        getTrips(token),
      ]);
      setBuses(b);
      setRoutes(r);
      setDrivers(d);
      setTrips(t);
      setError(null);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Could not load data.');
    }
  }, [token]);

  useEffect(() => {
    load();
  }, [load]);

  async function onRefresh() {
    setRefreshing(true);
    await load();
    setRefreshing(false);
  }

  async function handleLogout() {
    await signOut();
    router.replace('/');
  }

  const activeTrips = trips.filter((t) => t.status === 'Active').length;

  return (
    <ThemedView style={{ flex: 1 }}>
      <SafeAreaView style={{ flex: 1 }}>
        <ScrollView
          contentContainerStyle={styles.scroll}
          refreshControl={<RefreshControl refreshing={refreshing} onRefresh={onRefresh} />}
        >
          <ThemedText type="title">Welcome, {user?.fullName}</ThemedText>
          <ThemedText type="small">Pull down to refresh</ThemedText>

          {error && <ThemedText style={styles.error}>{error}</ThemedText>}

          <View style={styles.cards}>
            <StatCard label="Buses" value={buses.length} />
            <StatCard label="Routes" value={routes.length} />
            <StatCard label="Drivers" value={drivers.length} />
            <StatCard label="Active trips" value={activeTrips} />
          </View>

          <View style={styles.tabs}>
            {TABS.map((t) => (
              <Pressable
                key={t}
                onPress={() => setTab(t)}
                style={[styles.tab, tab === t && styles.tabActive]}
              >
                <ThemedText style={tab === t ? styles.tabTextActive : undefined}>{t}</ThemedText>
              </Pressable>
            ))}
          </View>

          {tab === 'Buses' &&
            (buses.length === 0 ? (
              <ThemedText>No buses yet.</ThemedText>
            ) : (
              buses.map((b) => (
                <Row
                  key={b.id}
                  title={b.busNumber}
                  lines={[`Capacity ${b.capacity} / ${b.status}`, `Route: ${b.routeName ?? 'Not assigned'}`]}
                />
              ))
            ))}

          {tab === 'Routes' &&
            (routes.length === 0 ? (
              <ThemedText>No routes yet.</ThemedText>
            ) : (
              routes.map((r) => (
                <Row
                  key={r.id}
                  title={r.routeName}
                  lines={[`${r.startPoint} to ${r.endPoint}`, r.isActive ? 'Active' : 'Inactive']}
                />
              ))
            ))}

          {tab === 'Drivers' &&
            (drivers.length === 0 ? (
              <ThemedText>No drivers yet.</ThemedText>
            ) : (
              drivers.map((d) => (
                <Row
                  key={d.id}
                  title={d.fullName}
                  lines={[d.email, `License ${d.licenseNumber} / Bus: ${d.assignedBusNumber ?? 'Not assigned'}`]}
                />
              ))
            ))}

          {tab === 'Trips' &&
            (trips.length === 0 ? (
              <ThemedText>No trips yet.</ThemedText>
            ) : (
              trips.map((t) => (
                <Row
                  key={t.id}
                  title={`${t.busNumber} / ${t.status}`}
                  lines={[
                    `${t.driverName} / ${t.routeName}`,
                    `Started ${new Date(t.startTime).toLocaleString()}`,
                  ]}
                />
              ))
            ))}

          <Pressable onPress={handleLogout}>
            <ThemedText type="small" style={{ textAlign: 'center', marginTop: 24 }}>
              Logout
            </ThemedText>
          </Pressable>
        </ScrollView>
      </SafeAreaView>
    </ThemedView>
  );
}

function StatCard({ label, value }: { label: string; value: number }) {
  return (
    <View style={styles.card}>
      <ThemedText type="title">{value}</ThemedText>
      <ThemedText type="small">{label}</ThemedText>
    </View>
  );
}

function Row({ title, lines }: { title: string; lines: string[] }) {
  return (
    <View style={styles.row}>
      <ThemedText type="defaultSemiBold">{title}</ThemedText>
      {lines.map((l, i) => (
        <ThemedText key={i} type="small">
          {l}
        </ThemedText>
      ))}
    </View>
  );
}

const styles = StyleSheet.create({
  scroll: { padding: 16, gap: 12, paddingBottom: 48 },
  error: { color: '#dc2626' },
  cards: { flexDirection: 'row', flexWrap: 'wrap', gap: 10 },
  card: {
    flexGrow: 1,
    minWidth: '45%',
    borderWidth: 1,
    borderColor: '#9ca3af',
    borderRadius: 12,
    padding: 14,
    alignItems: 'center',
  },
  tabs: { flexDirection: 'row', gap: 8, marginTop: 8 },
  tab: {
    flex: 1,
    paddingVertical: 10,
    alignItems: 'center',
    borderRadius: 8,
    borderWidth: 1,
    borderColor: '#9ca3af',
  },
  tabActive: { backgroundColor: '#2563eb', borderColor: '#2563eb' },
  tabTextActive: { color: '#fff', fontWeight: '600' },
  row: { borderWidth: 1, borderColor: '#9ca3af', borderRadius: 10, padding: 12, gap: 2 },
});