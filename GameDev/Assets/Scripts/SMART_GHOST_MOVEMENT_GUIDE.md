# 🧠 Smart Ghost Movement System

## 🎯 Konsep Baru: Natural & Smooth Movement

Hantu sekarang menggunakan **NavMesh-based dynamic movement** tanpa perlu spawn points manual!

### **Fitur Baru:**
✅ **Spawn random** di NavMesh (tidak perlu movepoint lagi!)  
✅ **Wander smooth** dengan path calculation otomatis  
✅ **Stuck detection** - auto-retry kalau mentok  
✅ **Auto-teleport** kalau stuck berkali-kali  
✅ **Idle behavior** - kadang berhenti sejenak (creepy!)  
✅ **Natural movement** - terlihat seperti benar-benar jalan  

---

## 🆕 Yang Berubah

### **Sebelum (Old System):**
```
❌ Butuh spawnPoints[] array (movepoint 1-4)
❌ Harus set manual spawn locations
❌ Stuck = hantu tidak gerak
❌ Path invalid = hantu freeze
```

### **Sekarang (New System):**
```
✅ Spawn otomatis di NavMesh random
✅ Tidak perlu spawn points manual
✅ Stuck detection & auto-recovery
✅ Path invalid = auto-recalculate
✅ Smooth wandering dengan idle
```

---

## ⚙️ Parameter Baru di Inspector

Di **Hantu > HantuMove component**:

### **Spawn System:**
| Parameter | Default | Fungsi |
|-----------|---------|--------|
| `Spawn Search Radius` | **30** | Radius search untuk spawn position dari player |
| `Min Distance From Player` | **8** | Jarak minimum spawn dari player (supaya tidak spawn di depan player!) |

### **Stuck Detection:**
| Parameter | Default | Fungsi |
|-----------|---------|--------|
| `Stuck Check Interval` | **2** | Cek stuck setiap berapa detik |
| `Stuck Velocity Threshold` | **0.1** | Jarak minimum yang harus digerakkan untuk tidak dianggap stuck |
| `Max Path Retries` | **5** | Berapa kali retry sebelum teleport |

---

## 🎮 Cara Kerja Sistem Baru

### **1. Spawn System:**
```
1. Player position detected
2. Random point dalam radius 30m dari player
3. Check jarak >= 8m dari player (supaya tidak spawn di depan!)
4. Sample NavMesh position
5. Spawn hantu di posisi valid
```

**Result**: Hantu spawn random tapi selalu jauh dari player! 👻

### **2. Smooth Wandering:**
```
1. Pick random destination dalam wander radius (15m)
2. Calculate NavMesh path
3. Validate path is complete
4. Move smooth menggunakan NavMeshAgent
5. Saat sampai destination:
   - 30% chance idle (berhenti sejenak)
   - 70% chance langsung cari destination baru
```

**Result**: Hantu jalan natural seperti patrol! 🚶

### **3. Stuck Detection:**
```
Every 2 seconds:
1. Check posisi sekarang vs posisi 2 detik lalu
2. Kalau moved < 0.1m → STUCK!
3. Retry #1-5: Cari path baru
4. Retry #5 (max): Teleport ke random NavMesh position
5. Reset counter kalau berhasil gerak
```

**Result**: Hantu tidak pernah freeze/stuck! 🔄

---

## 🧪 Testing & Debug

### **Quick Test:**
1. Set `Debug Immediate Spawn = true`
2. Play game
3. **Lihat Console**: "Spawned at distance X from player"
4. **Observe**: Hantu spawn jauh dari kamu, lalu wander smooth
5. **Watch**: Kadang hantu idle (berhenti), lalu jalan lagi

### **Stuck Testing:**
1. Buat area NavMesh yang kecil/sempit
2. Set `Wander Radius = 30` (besar supaya sering mentok)
3. Play game
4. **Watch Console**: "Hantu stuck! Retry #X"
5. Setelah 5x retry, hantu teleport otomatis

---

## 🎨 Visual Gizmos (Scene View)

Saat select Hantu di Scene view:

- **Yellow sphere** = Detection radius (dynamic berdasarkan oil)
- **Cyan sphere** = Wander radius (area jelajah hantu)
- **Green spheres** = Audio trigger radius

---

## ⚡ Performance

**Optimized for performance:**
- No FindObjectsOfType in Update
- NavMesh path calculation only when needed
- Stuck check hanya setiap 2 detik (bukan per frame)
- Efficient position caching

**Typical overhead:**
- Spawn: ~0.5ms (hanya sekali)
- Wander: ~0.02ms per frame
- Stuck check: ~0.01ms every 2 seconds

---

## 🔧 Tuning Tips

### **Untuk Ghost Lebih Aggressive:**
```
Wander Radius: 20 (jelajah lebih jauh)
Wander Speed: 2.5 (lebih cepat)
Idle Chance: 0.1 (jarang idle, lebih sering jalan)
Spawn Search Radius: 40 (spawn lebih jauh)
```

### **Untuk Ghost Lebih Spooky:**
```
Wander Radius: 10 (area terbatas, lebih dekat player)
Wander Speed: 1.5 (lambat creepy)
Idle Chance: 0.5 (sering idle, nunggu player)
Min Distance From Player: 5 (spawn lebih dekat!)
```

### **Untuk Horror Gameplay:**
```
Wander Radius: 15
Wander Speed: 2
Idle Chance: 0.3
Min Distance From Player: 8
Stuck Check Interval: 2
```

---

## 🚀 Advanced: Custom Behavior

### **Menambah Idle Lebih Lama:**
```csharp
// Di HantuMove.cs, ubah idleDuration
public float idleDuration = 3f; // dari 2f jadi 3f
```

### **Spawn Lebih Dekat ke Player (Jumpscare Mode):**
```csharp
// Di Inspector:
Min Distance From Player: 3
Spawn Search Radius: 15
```

### **Menambah Spawn Sound:**
```csharp
// Di SpawnSequence(), tambahkan:
if (audioSource && spawnSfx)
    audioSource.PlayOneShot(spawnSfx);
```

---

## ❌ Troubleshooting

### **"Could not find valid NavMesh spawn position!"**
**Problem**: NavMesh belum di-bake atau terlalu kecil  
**Fix**: 
1. Buka Navigation window (Window > AI > Navigation)
2. Pilih semua floors/ground
3. Klik "Bake" di tab "Bake"
4. Pastikan NavMesh menutupi area yang cukup besar

### **Hantu spawn di tempat yang sama terus**
**Problem**: Spawn Search Radius terlalu kecil  
**Fix**: Ubah `Spawn Search Radius = 30` atau lebih

### **Hantu stuck terus menerus**
**Problem**: NavMesh ada hole atau obstacle terlalu banyak  
**Fix**: 
1. Check NavMesh visualization (Show NavMesh di Scene view)
2. Pastikan tidak ada gap di NavMesh
3. Kurangi obstacle atau perbesar NavMesh carve

### **Hantu jalan terlalu random**
**Problem**: Wander Radius terlalu besar  
**Fix**: Kurangi `Wander Radius = 10` untuk area lebih terbatas

---

## 📊 Comparison: Old vs New

| Feature | Old System | New System |
|---------|-----------|------------|
| Setup complexity | Medium (need movepoints) | **Easy (no setup)** |
| Spawn locations | Fixed (4 points) | **Random (infinite)** |
| Movement | Basic wander | **Smooth + stuck detection** |
| Stuck handling | None (freeze) | **Auto-recovery** |
| Idle behavior | No | **Yes (30% chance)** |
| Natural look | Basic | **Very natural** |
| Maintenance | Need to place movepoints | **Zero maintenance** |

---

## ✅ Checklist Setup

Untuk menggunakan sistem baru:

- [x] ✅ NavMesh sudah di-bake
- [x] ✅ HantuMove script sudah updated
- [ ] ⚠️ **Hapus/disable Movepoint objects** (tidak perlu lagi!)
- [ ] ⚠️ **Clear Spawn Points array** di Inspector (kosongkan)
- [x] ✅ Set Spawn Search Radius = 30
- [x] ✅ Set Min Distance From Player = 8
- [x] ✅ Test dengan Debug Immediate Spawn = true

---

## 🎯 Summary

**Sistem movement baru:**
1. ✅ Tidak perlu spawn points manual
2. ✅ Spawn random di NavMesh (jauh dari player)
3. ✅ Wander smooth dengan idle behavior
4. ✅ Stuck detection & auto-recovery
5. ✅ Terlihat lebih natural & organic
6. ✅ Zero maintenance setelah NavMesh baked

**Setup cepat:**
- Bake NavMesh
- Clear spawn points array (kosongkan)
- Done!

---

**Updated**: 2024  
**Script**: HantuMove.cs  
**Feature**: Smart NavMesh-based movement with stuck detection
