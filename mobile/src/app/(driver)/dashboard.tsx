import { useCallback, useEffect, useRef, useState } from 'react';
import { useRouter } from 'expo-router';
import * as Location from 'expo-location';
import { ActivityIndicator, Pressable, StyleSheet } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';

import { ThemedText } from '@/components/themed-text';
import { ThemedView } from '@/components/themed-view';
import { useAuth } from '@/context/AuthContext';
import { DriverMe, endTrip, getDriverMe, sendLocation, startTrip } from '@/services/api';

const SEND_INTERVAL_MS = 5000;

export default function DriverDashboard() {
  const { user, signOut } = useAuth();
  const router = useRouter();

  const [me, setMe] = useState<DriverMe | null>(null);
  const [tripId, setTripId] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [lastSent, setLastSent] = useState<string | null>(null);
  const [sentCount, setSentCount] = useState(0);

  const watcher = useRef<Location.LocationSubscription | null>(null);
  const lastSentAt = useRef(0);

  const token = user?.token ?? '';

  const stopWatching = useCallback(() => {
    watcher.current?.remove();
    watcher.current = null;
  }, []);

  const startWatching = useCallback(
    async (activeTripId: string) => {
      const { status } = await Location.requestForegroundPermissionsAsync();
      if (status !== 'granted') {
        setError('Location permission denied. GPS cannot be sent.');
        return false;
      }

      stopWatching();
      watcher.current = await Location.watchPositionAsync(
        { accuracy: Location.Accuracy.High, timeInterval: SEND_INTERVAL_MS, distanceInterval: 0 },
        async (pos) => {
          const now = Date.now();
          if (now - lastSentAt.current < SEND_INTERVAL_MS - 500) return;
          lastSentAt.current = now;

          try {
            await sendLocation(token, activeTripId, {
              latitude: pos.coords.latitude,
              longitude: pos.coords.longitude,
              speed: pos.coords.speed != null && pos.coords.speed >= 0 ? pos.coords.speed * 3.6 : null,
              heading: pos.coords.heading != null && pos.coords.heading >= 0 ? pos.coords.heading : null,
              recordedAt: new Date(pos.timestamp).toISOString(),
            });
            setSentCount((c) => c + 1);
            setLastSent(new Date().toLocaleTimeString());
            setError(null);
          } catch (e) {
            setError(e instanceof Error ? e.message : 'Failed to send location.');
          }
        }
      );
      return true;
    },
    [token, stopWatching]
  );

  useEffect(() => {
    (async () => {
      try {
        const data = await getDriverMe(token);
        setMe(data);
        if (data.activeTripId) {
          setTripId(data.activeTripId);
          await startWatching(data.activeTripId);
        }
      } catch (e) {
        setError(e instanceof Error ? e.message : 'Failed to load profile.');
      } finally {
        setLoading(false);
      }
    })();
    return stopWatching;
  }, [token, startWatching, stopWatching]);

  async function handleStart() {
    if (!me?.busId || !me.routeId) return;
    setBusy(true);
    setError(null);
    try {
      const trip = await startTrip(token, me.busId, me.routeId);
      setTripId(trip.id);
      setSentCount(0);
      await startWatching(trip.id);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Could not start trip.');
    } finally {
      setBusy(false);
    }
  }

  async function handleEnd() {
    if (!tripId) return;
    setBusy(true);
    setError(null);
    try {
      stopWatching();
      await endTrip(token, tripId);
      setTripId(null);
      setLastSent(null);
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Could not end trip.');
    } finally {
      setBusy(false);
    }
  }

  async function handleLogout() {
    stopWatching();
    await signOut();
    router.replace('/');
  }

  if (loading) {
    return (
      <ThemedView style={styles.center}>
        <ActivityIndicator />
      </ThemedView>
    );
  }

  const canStart = !!me?.busId && !!me?.routeId;

  return (
    <ThemedView style={{ flex: 1 }}>
      <SafeAreaView style={styles.safeArea}>
        <ThemedText type="title">Welcome, {user?.fullName}</ThemedText>
        <ThemedText>Bus: {me?.busNumber ?? 'Not assigned'}</ThemedText>
        <ThemedText>Route: {me?.routeName ?? 'Not assigned'}</ThemedText>
        <ThemedText>Trip status: {tripId ? 'ACTIVE (sending GPS)' : 'No active trip'}</ThemedText>

        {tripId && (
          <ThemedText type="small">
            Sent {sentCount} location(s){lastSent ? `, last at ${lastSent}` : ''}
          </ThemedText>
        )}

        {!canStart && !tripId && (
          <ThemedText style={styles.error}>
            No bus/route assigned to you yet. Ask the transport admin.
          </ThemedText>
        )}

        {error && <ThemedText style={styles.error}>{error}</ThemedText>}

        {!tripId ? (
          <Pressable
            style={[styles.button, !canStart && styles.disabled]}
            onPress={handleStart}
            disabled={busy || !canStart}
          >
            {busy ? <ActivityIndicator color="#fff" /> : <ThemedText style={styles.buttonText}>Start Trip</ThemedText>}
          </Pressable>
        ) : (
          <Pressable style={[styles.button, styles.endButton]} onPress={handleEnd} disabled={busy}>
            {busy ? <ActivityIndicator color="#fff" /> : <ThemedText style={styles.buttonText}>End Trip</ThemedText>}
          </Pressable>
        )}

        <Pressable onPress={handleLogout}>
          <ThemedText type="small" style={{ textAlign: 'center', marginTop: 16 }}>
            Logout
          </ThemedText>
        </Pressable>
      </SafeAreaView>
    </ThemedView>
  );
}

const styles = StyleSheet.create({
  center: { flex: 1, justifyContent: 'center', alignItems: 'center' },
  safeArea: { flex: 1, padding: 24, justifyContent: 'center', gap: 12 },
  button: { backgroundColor: '#16a34a', borderRadius: 8, padding: 16, alignItems: 'center', marginTop: 8 },
  endButton: { backgroundColor: '#dc2626' },
  disabled: { backgroundColor: '#9ca3af' },
  buttonText: { color: '#fff', fontWeight: '600' },
  error: { color: '#dc2626' },
});