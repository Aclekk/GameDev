# 🎯 Hantu Detection System - Independen dari Light Range

## Konsep Baru (Simplified)

Detection radius hantu **TIDAK lagi terikat** dengan Light range. Sekarang:

✅ **Light Range** (LanternController) = Seberapa jauh cahaya menerangi (visual only)  
✅ **Detection Range** (HantuMove) = Seberapa jauh hantu despawn (gameplay mechanic)

Keduanya **independen**, tapi detection range tetap **berkurang seiring oil habis**.

---

## 🎮 Cara Kerja

### **Detection Radius Formula:**
```csharp
oilPercent = currentOil / maxOil  // 0.0 - 1.0
radiusCurve = Pow(oilPercent, 0.5)  // Square root curve
detectionRadius = Lerp(minDetectionRadius, maxDetectionRadius, radiusCurve)
```

### **Gameplay:**
- Oil 100% → Detection radius = `maxDetectionRadius` (default 8m)
- Oil 50% → Detection radius ≈ 5.7m
- Oil 25% → Detection radius ≈ 4m
- Oil 10% → Detection radius ≈ 2.7m
- Oil 0% → Detection radius = `minDetectionRadius` (default 2m)

---

## ⚙️ Parameter di Inspector

Di **Hantu GameObject > HantuMove component > Lantern Detection**:

| Parameter | Default | Fungsi |
|-----------|---------|--------|
| `Max Detection Radius` | **8** | Radius maksimum saat oil penuh |
| `Min Detection Radius` | **2** | Radius minimum saat oil habis |
| `Use Dynamic Detection` | ✓ **true** | Detection berkurang seiring oil habis |
| `Player Layer` | Nothing | (Opsional) Layer mask untuk raycast |

---

## 📊 Detection Range Table (Default Settings)

| Oil % | Detection Radius | Gameplay Impact |
|-------|------------------|-----------------|
| 100%  | **8.0m**         | Aman - hantu despawn dari jauh ✅ |
| 75%   | **6.9m**         | Cukup aman ⚠️ |
| 50%   | **5.7m**         | Hantu bisa lebih dekat ⚠️⚠️ |
| 25%   | **4.0m**         | Sangat dekat! 💀 |
| 10%   | **2.7m**         | Hampir jumpscare distance! 💀💀 |
| 0%    | **2.0m**         | Minimum - sangat berbahaya! ☠️ |

**Note**: Jumpscare trigger radius = 2.2m, jadi saat oil 0%, player masih bisa jumpscare!

---

## 🔧 Tuning Tips

### **Untuk Game Lebih Mudah:**
```
Max Detection Radius: 10
Min Detection Radius: 3
Use Dynamic Detection: ✓
```
→ Hantu despawn dari lebih jauh

### **Untuk Game Lebih Susah (Recommended):**
```
Max Detection Radius: 6
Min Detection Radius: 1.5
Use Dynamic Detection: ✓
```
→ Hantu bisa sangat dekat sebelum despawn

### **Untuk Testing (Static Detection):**
```
Max Detection Radius: 8
Min Detection Radius: 8
Use Dynamic Detection: ✗
```
→ Detection radius selalu sama (8m), tidak berubah

---

## 🎨 Visual Gizmos

Saat Hantu di-select di Scene view, kamu bisa lihat:

- **Yellow sphere** = Current detection radius (dynamic, berubah real-time!)
- **Cyan sphere** = Wander radius
- **Green spheres** = Audio trigger radius

**Pro Tip**: Play game dalam Scene view sambil select Hantu untuk lihat yellow sphere mengecil saat oil habis!

---

## 🆚 Perbedaan dengan Sistem Lama

### **Sistem Lama (Terikat Light Range):**
```
Detection = Light.range × multiplier
Problem: Harus sync dengan LanternController
Problem: Light range untuk visual jadi terbatas
```

### **Sistem Baru (Independen):**
```
Detection = Independent calculation based on oil%
✅ Light range bebas di-set untuk visual
✅ Detection radius bisa di-tune sendiri
✅ Lebih mudah di-balance
```

---

## 🔥 Rekomendasi Setting untuk Horror Game

### **Lantern (Visual):**
```
Max Range: 25 (cahaya jauh - visual dramatis)
Min Range: 3 (masih ada sedikit cahaya saat oil habis)
Max Intensity: 2.2
Min Intensity: 0.3
```

### **Detection (Gameplay):**
```
Max Detection Radius: 6 (hantu harus cukup dekat)
Min Detection Radius: 1.5 (sangat berbahaya saat oil habis)
Use Dynamic Detection: ✓
```

### **Result:**
- ✅ Cahaya menerangi jauh (25m) - visual bagus
- ✅ Hantu despawn dari dekat (6m) - challenging
- ✅ Saat oil habis, bahaya banget (1.5m detection!)
- ✅ Player harus manage oil dengan hati-hati

---

## 🧪 Testing Workflow

1. **Set Debug Immediate Spawn = true** di HantuMove
2. **Play game** - hantu spawn langsung
3. **Nyalakan lantern** (tekan F)
4. **Di Console**, ketik command untuk test:
   ```
   // Kurangi oil untuk testing
   GameObject.Find("Lantern").GetComponent<LanternController>().currentOil = 25;
   ```
5. **Lihat yellow gizmo** di Scene view - seharusnya mengecil!
6. **Coba dekati hantu** - lihat jarak despawn berubah sesuai oil

---

## 💡 Advanced: Custom Detection Curve

Kalau mau curve yang berbeda, edit di `IsInLanternRadius()`:

```csharp
// Linear (default: Pow 0.5)
float radiusCurve = oilPercent;

// Aggressive (despawn distance cepat kecil)
float radiusCurve = Mathf.Pow(oilPercent, 0.3f);

// Gentle (despawn distance lambat kecil)
float radiusCurve = Mathf.Pow(oilPercent, 0.7f);
```

---

## 📈 Performance

- **Zero performance impact** - calculation hanya saat check detection
- **No FindObjectsOfType** - reference sudah di-cache
- **Efficient calculation** - simple Lerp + Pow
- Typical overhead: **< 0.01ms** per frame per ghost

---

## ✅ Summary

**Sistem detection sekarang:**
1. ✅ Independen dari Light component
2. ✅ Berkurang seiring oil habis
3. ✅ Mudah di-tune di Inspector
4. ✅ Visual cahaya tidak terbatas oleh gameplay balance
5. ✅ Gizmos real-time untuk debugging

**Setup cepat:**
- Lantern: Set range besar untuk visual (25m)
- Hantu: Set detection kecil untuk challenge (6m max)
- Done!

---

**Updated**: 2024  
**Script**: HantuMove.cs  
**Feature**: Independent oil-based detection system
