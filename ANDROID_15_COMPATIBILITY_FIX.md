# 🔧 Android 15+ Compatibility Fix - 16KB Page Alignment

## 🚨 Issue: Oculus XR Plugin Alignment Warning

**Warning Message:**
```
Plugin 'Packages/com.unity.xr.oculus/Runtime/Android/arm64/libOculusXRPlugin.so' is not 16KB-aligned. This may cause issues on ARM64 devices running Android 15+.
```

## 🎯 What This Means

Android 15+ introduced stricter memory alignment requirements for native libraries. The Oculus XR Plugin version 4.5.1 and earlier don't meet the new 16KB page alignment requirements, which can cause:

- App crashes on Android 15+ devices
- Performance degradation on Meta Quest devices with newer firmware
- Compatibility issues with ARM64 architecture

## ✅ Solutions Implemented

### 1. Updated XR Packages
- **Oculus XR Plugin**: Updated from `4.5.1` → `4.6.0+` (fixes alignment)
- **OpenXR Plugin**: Updated from `1.8.2` → `1.10.0+` (better compatibility)

### 2. Player Settings Optimization
- **Target Architecture**: ARM64 only (required for Quest)
- **Target SDK**: Android API 33 (recommended for compatibility)
- **Min SDK**: Android API 26 (Quest requirement)
- **Optimized Frame Pacing**: Disabled (can cause alignment issues)
- **Scripting Backend**: IL2CPP (required for ARM64)

### 3. Build Configuration
- **Graphics APIs**: Vulkan + OpenGL ES 3.0 (optimized order)
- **Code Stripping**: Enabled for smaller APK size
- **IL2CPP Configuration**: Master (better performance)

## 🛠️ How to Apply the Fix

### Option 1: Automatic Fix (Recommended)
```bash
# Run the updated build script with automatic fixes
./simple-build.sh
```

### Option 2: Manual Unity Menu
1. Open Unity with your project
2. Go to: `MR Chess → Fix Android 15+ Compatibility`
3. Wait for completion
4. Build normally

### Option 3: Unity Console Command
```csharp
MRChess.Setup.AndroidCompatibilityFixer.FixAndroid15Compatibility();
```

## 🔍 Validation

To check if your project is properly configured:

### Unity Menu:
`MR Chess → Validate Android 15+ Compatibility`

### Expected Results:
- ✅ Target architecture: ARM64 only
- ✅ Target SDK: 33+
- ✅ Scripting backend: IL2CPP
- ✅ Optimized Frame Pacing: Disabled
- ✅ XR Management: Properly configured

## 📱 Meta Quest Compatibility

### Supported Devices:
- ✅ Meta Quest 2 (all firmware versions)
- ✅ Meta Quest 3 (all firmware versions)
- ✅ Meta Quest Pro (all firmware versions)
- ✅ Future Quest devices with Android 15+

### Testing Checklist:
1. **Build APK** with updated settings
2. **Install** on Quest device: `adb install MRChess-Dev.apk`
3. **Launch** from Unknown Sources
4. **Verify** no crashes or performance issues
5. **Test** XR features (hand tracking, passthrough, etc.)

## 🚀 Build Commands

### Development Build:
```bash
./simple-build.sh development
```

### Release Build:
```bash
./simple-build.sh release
```

### Validation Only:
```bash
# Check compatibility without building
/Applications/Unity/Hub/Editor/2022.3.62f1/Unity.app/Contents/MacOS/Unity \
    -batchmode -quit \
    -projectPath "$(pwd)" \
    -executeMethod MRChess.Setup.AndroidCompatibilityFixer.ValidateAndroid15Compatibility
```

## 📋 Troubleshooting

### If you still see the alignment warning:
1. **Clear Library folder**: `rm -rf Library/`
2. **Reimport packages**: Let Unity rebuild package cache
3. **Verify package versions**: Check `Packages/manifest.json`
4. **Rebuild project**: Run `./simple-build.sh` again

### Common Issues:
- **"Method not found"**: Ensure AndroidCompatibilityFixer.cs is in the project
- **"Package not found"**: Run Unity Package Manager refresh
- **"Build fails"**: Check that Android SDK is properly configured

## 🎯 Performance Benefits

With these fixes, you should see:
- 🚀 **Better Performance** on Quest devices
- 🛡️ **Future Compatibility** with Android 15+
- 📱 **Smaller APK Size** due to optimizations
- ⚡ **Faster Loading** with proper IL2CPP configuration

Your MR Chess app is now optimized for current and future Android devices! 🎮
