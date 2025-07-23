# ✅ Android Build Configuration - Next Steps Complete Guide

Your macOS system is **successfully configured** for building Android APKs with Unity! Here's exactly what to do next:

## 🎯 Current Status
- ✅ Android SDK installed with command-line tools 6.0
- ✅ Android platforms API 26 & 33 installed  
- ✅ Android NDK 25.2.9519653 installed
- ✅ Build tools 34.0.0 installed
- ✅ Environment variables configured
- ✅ Java OpenJDK 24 installed
- ✅ ADB working for device deployment

## 🚀 Next Steps (Choose Your Approach)

### Option A: Quick Unity Setup (Recommended)

1. **Open Unity:**
   ```bash
   ./configure-unity-android.sh
   ```
   This opens Unity and shows configuration instructions.

2. **In Unity Editor, run the auto-configuration:**
   - Go to: `MR Chess → Auto-Configure Android SDK`
   - This automatically sets up all Android paths in Unity

3. **Switch to Android Platform:**
   - `File → Build Settings`
   - Select `Android` → `Switch Platform`

4. **Test Build:**
   ```bash
   ./build.sh development
   ```

### Option B: Manual Unity Setup

1. **Open Unity and load your project**

2. **Configure External Tools:**
   - `Edit → Preferences → External Tools`
   - Set these paths:
     - **Android SDK Tools:** `/opt/homebrew/share/android-commandlinetools`
     - **Android NDK:** `/opt/homebrew/share/android-commandlinetools/ndk/25.2.9519653`
     - **JDK:** `/opt/homebrew/opt/openjdk`

3. **Configure Build Settings:**
   - `File → Build Settings → Android → Switch Platform`
   - Add scene: `Assets/chess.unity`

4. **Configure Player Settings:**
   - In Build Settings, click `Player Settings`
   - **Product Name:** MR Chess
   - **Company Name:** MR Chess Team
   - **Bundle Identifier:** com.mrchessteam.mrchess
   - **Minimum API Level:** Android 8.0 (API level 26)
   - **Target API Level:** Android 13 (API level 33)
   - **Scripting Backend:** IL2CPP
   - **Target Architectures:** ARM64 only ✅

5. **Configure XR Settings:**
   - `Edit → Project Settings → XR Plug-in Management`
   - Install required packages if not present:
     - XR Interaction Toolkit
     - OpenXR Plugin
   - Enable `OpenXR` for Android platform
   - Configure OpenXR features for Meta Quest

## 🔧 Troubleshooting

### If Unity shows "Command-line Tools not found":
1. Use Unity's Android SDK fixer: `MR Chess → Fix Android SDK Setup`
2. Or manually set SDK path: `/opt/homebrew/share/android-commandlinetools`

### If packages are missing:
- `Window → Package Manager`
- Install: XR Interaction Toolkit, OpenXR Plugin, AR Foundation

### If build fails:
1. Check Unity Console for specific errors
2. Ensure all packages are installed
3. Run: `./validate-android-setup.sh`

## 📱 Building & Deployment

### Build APK:
```bash
# Development build (with debugging)
./build.sh development

# Release build (optimized)
./build.sh release
```

### Deploy to Meta Quest:
1. **Enable Developer Mode** in Meta Quest mobile app
2. **Connect Quest** via USB-C cable
3. **Allow USB debugging** when prompted on headset
4. **Install APK:**
   ```bash
   adb install Builds/Development/MRChess-Dev.apk
   ```

### Check deployment:
```bash
# See connected devices
adb devices

# View app logs
adb logcat -s Unity
```

## 🎮 Expected Build Outputs

- **Development APK:** `Builds/Development/MRChess-Dev.apk` (~150-200MB)
- **Release APK:** `Builds/Release/MRChess.apk` (~80-120MB)

## ✅ Validation Commands

Use these anytime to check your setup:
```bash
# Validate overall setup
./validate-android-setup.sh

# Configure Unity integration  
./configure-unity-android.sh

# Test build (validation only)
./build.sh development --validate-only
```

---

**You're all set!** Your Android development environment is properly configured. The main thing now is to open Unity and either use the auto-configuration or manually set the SDK paths as shown above.

**Quick Start Command:**
```bash
./configure-unity-android.sh
```

This will open Unity with detailed configuration instructions. Once configured, you can build your first APK! 🚀
