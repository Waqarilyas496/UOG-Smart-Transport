import { useEffect, useRef, useState } from 'react';
import { useRouter } from 'expo-router';
import { Pressable, StyleSheet, View } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { WebView } from 'react-native-webview';

import { ThemedText } from '@/components/themed-text';
import { ThemedView } from '@/components/themed-view';
import { useAuth } from '@/context/AuthContext';
import { getLiveBus, LiveBus } from '@/services/api';

const POLL_MS = 5000;

const MAP_HTML = `
<!DOCTYPE html>
<html>
<head>
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <link rel="stylesheet" href="https://unpkg.com/leaflet@1.9.4/dist/leaflet.css"/>
  <script src="https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"></script>
  <style>html, body, #map { height: 100%; margin: 0; }</style>
</head>
<body>
  <div id="map"></div>
  <script>
    var map = L.map('map').setView([32.5, 74.0], 12);
    L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
      maxZoom: 19,
      attribution: '&copy; OpenStreetMap contributors'
    }).addTo(map);
    var marker = null;
    function moveMarker(lat, lng) {
      if (!marker) {
        marker = L.marker([lat, lng]).addTo(map);
        map.setView([lat, lng], 16);
      } else {
        marker.setLatLng([lat, lng]);
        map.panTo([lat, lng]);
      }
    }
  </script>
</body>
</html>
`;

export default function StudentDashboard() {
  const { user, signOut } = useAuth();
  const router = useRouter();
  const webRef = useRef<WebView>(null);

  const [mapReady, setMapReady] = useState(false);
  const [live, setLive] = useState<LiveBus | null>(null);
  const [error, setError] = useState<string | null>(null);

  const token = user?.token ?? '';

  useEffect(() => {
    let cancelled = false;

    async function poll() {
      try {
        const data = await getLiveBus(token);
        if (!cancelled) {
          setLive(data);
          setError(null);
        }
      } catch (e) {
        if (!cancelled) setError(e instanceof Error ? e.message : 'Could not load bus info.');
      }
    }

    poll();
    const id = setInterval(poll, POLL_MS);
    return () => {
      cancelled = true;
      clearInterval(id);
    };
  }, [token]);

  useEffect(() => {
    if (mapReady && live?.latitude != null && live.longitude != null) {
      webRef.current?.injectJavaScript(`moveMarker(${live.latitude}, ${live.longitude}); true;`);
    }
  }, [mapReady, live]);

  async function handleLogout() {
    await signOut();
    router.replace('/');
  }

  let status = 'Loading...';
  if (live) {
    if (!live.hasRoute) status = 'No route assigned to you yet. Ask the transport admin.';
    else if (!live.hasActiveTrip) status = 'No bus is running on your route right now.';
    else if (live.latitude == null) status = 'Bus has started. Waiting for its first location...';
    else status = 'Bus is live';
  }

  return (
    <ThemedView style={{ flex: 1 }}>
      <SafeAreaView style={styles.safeArea}>
        <ThemedText type="title">Welcome, {user?.fullName}</ThemedText>
        {live?.routeName && <ThemedText>Route: {live.routeName}</ThemedText>}
        <ThemedText>{status}</ThemedText>
        {live?.hasActiveTrip && (
          <ThemedText type="small">
            Bus {live.busNumber} / Driver {live.driverName}
            {live.recordedAt ? ` / last update ${new Date(live.recordedAt).toLocaleTimeString()}` : ''}
          </ThemedText>
        )}
        {error && <ThemedText style={styles.error}>{error}</ThemedText>}

        <View style={styles.mapBox}>
          <WebView
            ref={webRef}
            originWhitelist={['*']}
            source={{ html: MAP_HTML, baseUrl: 'https://uogtransport.app' }}
            javaScriptEnabled
            onLoadEnd={() => setMapReady(true)}
          />
        </View>

        <Pressable onPress={handleLogout}>
          <ThemedText type="small" style={{ textAlign: 'center' }}>
            Logout
          </ThemedText>
        </Pressable>
      </SafeAreaView>
    </ThemedView>
  );
}

const styles = StyleSheet.create({
  safeArea: { flex: 1, padding: 16, gap: 8 },
  mapBox: { flex: 1, borderRadius: 12, overflow: 'hidden', marginVertical: 8 },
  error: { color: '#dc2626' },
});