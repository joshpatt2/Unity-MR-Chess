# MR Chess - Build Instructions

## Quick Start

### Automated Build (Recommended)

```bash
# Build development APK
./build.sh development

# Build release APK  
./build.sh release

# Validate project setup only
./build.sh development --validate-only
```

### Manual Build (Unity Editor)

1. Open project in Unity Editor
2. Open `MR Chess > Validate Build Setup` to check configuration
3. Use `MR Chess > Build Development APK` or `MR Chess > Build Release APK`

## Prerequisites

### Unity Setup
- **Unity Version**: 2022.3.62f1 (LTS)
- **Platform**: Android
- **Architecture**: ARM64 (required for Quest)
- **Minimum SDK**: Android 8.0 (API level 26)
- **Target SDK**: Android 13 (API level 33)

### Required Packages
- ✅ XR Interaction Toolkit (2.4.3+)
- ✅ OpenXR Plugin (1.8.2+)
- ✅ Universal Render Pipeline (14.0.11+)
- ✅ Input System (1.6.3+)
- ✅ AR Foundation (5.0.7+)
- ✅ XR Management (4.4.0+)

### Android Development
- **Android SDK**: API level 33 (Android 13)
- **NDK**: Latest stable version
- **JDK**: OpenJDK 11 or newer
- **Build Tools**: 30.0.3+

## Build Configuration

### Player Settings
```
Product Name: MR Chess
Company Name: MR Chess Team
Bundle Identifier: com.mrchessteam.mrchess
Version: 1.0.0
Architecture: ARM64 only
Min SDK Version: 26 (Android 8.0)
Target SDK Version: 33 (Android 13)
Graphics APIs: Vulkan, OpenGLES3
```

### XR Settings
- XR Management: Enabled
- Initialize XR on Startup: ✅
- OpenXR Runtime: Oculus/Meta

### Android Manifest Features
- ✅ Hand Tracking Support
- ✅ Camera Permissions
- ✅ VR Headtracking
- ✅ Quest Category Intent Filter

## Build Outputs

### Development Build
- **Location**: `Builds/Development/MRChess-Dev.apk`
- **Features**: Debug symbols, profiler connection, development console
- **Size**: ~150-200MB (debug overhead)

### Release Build  
- **Location**: `Builds/Release/MRChess.apk`
- **Features**: Optimized, minified, production-ready
- **Size**: ~80-120MB (optimized)

## Deployment

### Meta Quest Setup
1. Enable **Developer Mode** in Meta Quest mobile app
2. Connect Quest to computer via USB-C
3. Enable **USB Debugging** when prompted on headset
4. Install APK: `adb install path/to/MRChess.apk`
5. Launch from **Unknown Sources** in Quest library

### ADB Commands
```bash
# Check connected devices
adb devices

# Install APK
adb install -r Builds/Development/MRChess-Dev.apk

# View logs (useful for debugging)
adb logcat -s Unity

# Uninstall app
adb uninstall com.mrchessteam.mrchess
```

## Troubleshooting

### Common Build Errors

#### "Failed to find target with hash string 'android-XX'"
**Solution**: Update Android SDK in Unity Preferences > External Tools

#### "NDK not found"
**Solution**: Install Android NDK through Unity Hub > Installs > Android Build Support

#### "Gradle build failed"
**Solution**: 
1. Check Android SDK/NDK paths in Unity Preferences
2. Clear `Library` folder and reimport project
3. Ensure JDK 11+ is installed

#### "XR Management not initialized"
**Solution**: 
1. Go to Edit > Project Settings > XR Plug-in Management
2. Enable OpenXR for Android platform
3. Configure OpenXR features for Quest

### Performance Issues

#### Low frame rate on Quest
- Reduce texture quality in Quality Settings
- Disable unnecessary post-processing effects  
- Use URP optimized shaders
- Enable Fixed Foveated Rendering

#### Large APK size
- Enable texture compression
- Remove unused assets
- Use asset bundles for large content

## Development Workflow

### Iteration Cycle
1. Make code changes
2. Use `./build.sh development` for quick builds
3. Install via ADB to Quest
4. Test in headset
5. Check logs with `adb logcat -s Unity`

### Debugging
- Use Development builds for debugging
- Connect Unity Profiler to running app
- Monitor performance with Quest Performance Overlay
- Use Unity Console for debug logs

## CI/CD Integration

### GitHub Actions
The build script can be integrated into CI/CD pipelines:

```yaml
- name: Build MR Chess APK
  run: |
    chmod +x ./build.sh
    ./build.sh release
    
- name: Upload APK Artifact
  uses: actions/upload-artifact@v3
  with:
    name: MRChess-APK
    path: Builds/Release/MRChess.apk
```

## Support

For build issues:
1. Run `./build.sh development --validate-only` to check setup
2. Use Unity's `MR Chess > Validate Build Setup` tool
3. Check build logs in `Builds/[Development|Release]/build.log`
4. Refer to Unity XR documentation for platform-specific issues

---

**Note**: This project targets Meta Quest 2/3 devices exclusively. The build system is optimized for Quest hardware and may not work on other Android VR devices without modification.
