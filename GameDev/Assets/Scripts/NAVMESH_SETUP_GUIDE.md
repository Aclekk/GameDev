# NavMesh Setup Guide untuk Hantu

## Perbaikan yang Sudah Dilakukan

Script `HantuMove.cs` sudah diperbaiki dengan peningkatan berikut:

### 1. **NavMesh Agent Configuration** yang Lebih Baik
- Acceleration: 8f (lebih responsif)
- Angular Speed: 120f (rotasi lebih smooth)
- Obstacle Avoidance: HighQuality (menghindari tabrakan lebih baik)
- Radius & Height yang tepat untuk ghost

### 2. **Spawn System yang Lebih Robust**
- Validasi NavMesh sebelum spawn
- Mencari posisi valid dalam radius 5m-30m jika spawn point tidak di NavMesh
- Disable/Enable NavMeshAgent saat teleport untuk menghindari bug
- Error logging yang jelas jika NavMesh tidak di-bake

### 3. **Wander System yang Lebih Stabil**
- Validasi path sebelum set destination (mencegah path invalid)
- 30 percobaan untuk mencari target (dari 10)
- Check `isOnNavMesh` untuk auto-reposisi jika hantu terlempar keluar NavMesh
- Velocity threshold ditingkatkan ke 0.1f untuk animasi lebih smooth

### 4. **Safety Checks**
- Auto-reposisi jika hantu tidak di NavMesh
- Path validation sebelum movement
- Handling untuk NavMeshPathStatus.PathInvalid

---

## Cara Setup NavMesh di Unity (PENTING!)

Agar hantu bisa bergerak dengan NavMesh Agent, kamu **HARUS** bake NavMesh terlebih dahulu:

### Langkah 1: Buka Navigation Window
1. Klik menu **Window > AI > Navigation**
2. Window Navigation akan terbuka

### Langkah 2: Tandai GameObject yang Walkable
1. Pilih **Mansion** GameObject di hierarchy
2. Di Inspector, bagian atas, klik dropdown **Navigation** (atau klik **Static** checkbox)
3. Centang **Navigation Static**
4. Pilih **Walkable** atau **Not Walkable** untuk setiap bagian mansion

### Langkah 3: Bake NavMesh
1. Di Navigation window, pilih tab **Bake**
2. Setting yang direkomendasikan:
   - **Agent Radius**: 0.5
   - **Agent Height**: 2.0
   - **Max Slope**: 45
   - **Step Height**: 0.4
3. Klik tombol **Bake** di bagian bawah
4. Tunggu proses selesai (area biru akan muncul di scene view)

### Langkah 4: Verifikasi NavMesh
1. Di Scene view, area biru = area yang bisa dilalui hantu
2. Pastikan **semua spawn points** (movepoint 1-4) ada di area biru
3. Pastikan **koridor dan ruangan mansion** ter-cover area biru

### Langkah 5: Test di Play Mode
1. Play game
2. Lihat Console untuk log spawn
3. Jika ada error "TIDAK BISA menemukan NavMesh", berarti spawn point tidak di NavMesh
4. Pindahkan spawn point ke area biru atau re-bake NavMesh

---

## Troubleshooting

### ❌ "NavMeshAgent tidak ada di NavMesh"
**Solusi**: Pastikan NavMesh sudah di-bake dan spawn points ada di area biru NavMesh

### ❌ "Tidak bisa menemukan wander target"
**Solusi**: Perbesar area NavMesh dengan bake lebih banyak lantai/ground

### ❌ Hantu tiba-tiba freeze atau stuck
**Solusi**: 
- Script sekarang auto-reposisi jika terlempar keluar NavMesh
- Check di Scene view apakah hantu ada di area NavMesh (biru)
- Pastikan tidak ada obstacle yang blocking path

### ❌ Hantu spawn di posisi aneh/tinggi
**Solusi**: Script sekarang otomatis mencari posisi NavMesh terdekat dalam radius 30m

---

## Parameter yang Bisa Diatur (Inspector)

Di Inspector Hantu GameObject:

### NavMesh Settings
- **Wander Speed**: 2f (kecepatan jalan normal)
- **Chase Speed**: 4f (kecepatan kejar - untuk fitur future)
- **Wander Radius**: 15f (seberapa jauh hantu bisa jalan dari posisi spawn)

### Spawn System
- **Min Spawn Time**: 30 detik
- **Max Spawn Time**: 70 detik
- **Debug Immediate Spawn**: true untuk testing langsung spawn

### Lantern Detection
- **Lantern Detection Radius**: 10f (jarak cahaya bikin hantu kabur)

---

## Tips Optimasi

1. **Bake NavMesh hanya di area yang dibutuhkan** (jangan seluruh scene)
2. **Set spawn points di titik strategis** mansion yang pasti ada NavMesh
3. **Test dengan Debug Immediate Spawn = true** untuk testing cepat
4. **Lihat Gizmos** saat hantu selected untuk visualisasi radius

---

Dibuat: 2024
Script: HantuMove.cs v2.0
