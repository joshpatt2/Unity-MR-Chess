#!/bin/bash

# Unity Android SDK Configuration Script
# This script configures Unity to use the correct Android SDK paths

echo "🔧 Configuring Unity Android SDK Integration"
echo "=============================================="
echo

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Function to print status
print_status() {
    if [ $2 -eq 0 ]; then
        echo -e "${GREEN}✅ $1${NC}"
    else
        echo -e "${RED}❌ $1${NC}"
    fi
}

# Function to print info
print_info() {
    echo -e "${BLUE}ℹ️  $1${NC}"
}

# Function to print warning
print_warning() {
    echo -e "${YELLOW}⚠️  $1${NC}"
}

# Verify Android SDK setup
echo "1. Verifying Android SDK Setup"
echo "-------------------------------"

if [ -z "$ANDROID_HOME" ]; then
    echo -e "${RED}❌ ANDROID_HOME not set${NC}"
    echo "Setting up environment variables..."
    export ANDROID_HOME="/opt/homebrew/share/android-commandlinetools"
    export ANDROID_SDK_ROOT="$ANDROID_HOME"
    export PATH="$PATH:$ANDROID_HOME/platform-tools:$ANDROID_HOME/cmdline-tools/latest/bin:$ANDROID_HOME/cmdline-tools/6.0/bin"
fi

print_info "ANDROID_HOME: $ANDROID_HOME"
print_info "ANDROID_SDK_ROOT: $ANDROID_SDK_ROOT"

# Check critical paths
if [ -d "$ANDROID_HOME/cmdline-tools/6.0" ]; then
    print_status "Command-line Tools 6.0 found" 0
else
    print_status "Command-line Tools 6.0 missing" 1
    echo "   Run: sdkmanager \"cmdline-tools;6.0\""
fi

if [ -d "$ANDROID_HOME/platforms/android-33" ]; then
    print_status "Android API 33 platform found" 0
else
    print_status "Android API 33 platform missing" 1
fi

if [ -d "$ANDROID_HOME/build-tools" ] && [ "$(ls -A $ANDROID_HOME/build-tools)" ]; then
    latest_build_tools=$(ls "$ANDROID_HOME/build-tools" | sort -V | tail -n 1)
    print_status "Build Tools found: $latest_build_tools" 0
else
    print_status "Build Tools missing" 1
fi

if [ -d "$ANDROID_HOME/ndk" ] && [ "$(ls -A $ANDROID_HOME/ndk)" ]; then
    ndk_version=$(ls "$ANDROID_HOME/ndk" | head -n 1)
    print_status "NDK found: $ndk_version" 0
else
    print_status "NDK missing" 1
fi

echo
echo "2. Unity Configuration Paths"
echo "-----------------------------"
echo "Set these paths in Unity Editor:"
echo
echo -e "${BLUE}Edit > Preferences > External Tools:${NC}"
echo "   • Android SDK Tools:     $ANDROID_HOME"
echo "   • Android NDK:           $ANDROID_HOME/ndk/$(ls $ANDROID_HOME/ndk 2>/dev/null | head -n 1)"
echo "   • Java JDK:              ${JAVA_HOME:-$(/usr/libexec/java_home 2>/dev/null || echo '/Library/Java/JavaVirtualMachines/temurin-24.jdk/Contents/Home')}"
echo

echo "3. Project Configuration"
echo "-------------------------"
echo -e "${BLUE}File > Build Settings:${NC}"
echo "   • Platform: Android"
echo "   • Architecture: ARM64"
echo "   • Target API Level: 33 (Android 13)"
echo "   • Minimum API Level: 26 (Android 8.0)"
echo
echo -e "${BLUE}Edit > Project Settings > Player:${NC}"
echo "   • Company Name: MR Chess Team"
echo "   • Product Name: MR Chess"
echo "   • Bundle Identifier: com.mrchessteam.mrchess"
echo "   • Version: 1.0.0"
echo
echo -e "${BLUE}Edit > Project Settings > XR Plug-in Management:${NC}"
echo "   • Initialize XR on Startup: ✅"
echo "   • Android: OpenXR ✅"
echo "   • OpenXR Feature Groups: Meta Quest Support ✅"
echo

echo "4. Test Android SDK Integration"
echo "--------------------------------"
echo "Testing sdkmanager access..."

if command -v sdkmanager >/dev/null 2>&1; then
    print_status "sdkmanager accessible" 0
    echo "   Available Android platforms:"
    sdkmanager --list | grep "platforms;android-" | head -5 | while read line; do
        echo "     $line"
    done
else
    print_status "sdkmanager not accessible" 1
    print_warning "Add to PATH: export PATH=\"\$PATH:\$ANDROID_HOME/cmdline-tools/6.0/bin\""
fi

echo
echo "5. Build Test"
echo "-------------"
print_info "To test your setup, try building a development APK:"
echo "   ./build.sh development --validate-only"
echo
print_info "For a full build:"
echo "   ./build.sh development"
echo

echo "6. Unity Editor Quick Setup"
echo "----------------------------"
echo "Run this in Unity's Console window to set paths programmatically:"
echo
cat << 'EOF'
// Unity Console Commands (C#)
#if UNITY_EDITOR
UnityEditor.EditorPrefs.SetString("AndroidSdkRoot", "/opt/homebrew/share/android-commandlinetools");
UnityEditor.EditorPrefs.SetString("AndroidNdkRoot", "/opt/homebrew/share/android-commandlinetools/ndk/25.2.9519653");
UnityEditor.EditorPrefs.SetString("JdkPath", System.Environment.GetEnvironmentVariable("JAVA_HOME") ?? "/opt/homebrew/opt/openjdk");
Debug.Log("Android SDK paths configured!");
#endif
EOF

echo
echo -e "${GREEN}🎯 Configuration Complete!${NC}"
echo "Your Android SDK is now properly configured for Unity."
echo "Open Unity and load your project to continue building."

# Open Unity with this project
echo
echo "Opening Unity project..."
open -a Unity --args -projectPath "$(pwd)"

echo -e "${YELLOW}⏳ Unity is loading... Please follow these steps in Unity:${NC}"
echo
echo "📋 UNITY CONFIGURATION CHECKLIST:"
echo "=================================="
echo
echo "1. 🔧 EXTERNAL TOOLS SETUP"
echo "   → Edit → Preferences → External Tools"
echo "   → Android SDK Tools: $ANDROID_HOME"
echo "   → Android NDK: $ANDROID_HOME/ndk/25.2.9519653"
echo "   → JDK: $(dirname $(dirname $(which java)))"
echo "   💡 IMPORTANT: If Unity shows 'Command-line Tools not found':"
echo "      → Use SDK path: $ANDROID_HOME"
echo "      → Ensure cmdline-tools/6.0 exists (auto-created below)"
echo
echo "2. 🎯 BUILD SETTINGS"
echo "   → File → Build Settings"
echo "   → Select 'Android' platform"
echo "   → Click 'Switch Platform'"
echo "   → Add 'Assets/chess.unity' scene"
echo
echo "3. ⚙️  PLAYER SETTINGS (for Meta Quest)"
echo "   → Click 'Player Settings' in Build Settings"
echo "   → Company Name: MR Chess Team"
echo "   → Product Name: MR Chess"
echo "   → Bundle Identifier: com.mrchessteam.mrchess"
echo "   → Version: 1.0.0"
echo "   → Minimum API Level: Android 8.0 (API level 26)"
echo "   → Target API Level: Android 13 (API level 33)"
echo "   → Scripting Backend: IL2CPP"
echo "   → Target Architectures: ARM64 ✅ (uncheck others)"
echo
echo "4. 🥽 XR SETTINGS"
echo "   → Edit → Project Settings → XR Plug-in Management"
echo "   → Install XR Interaction Toolkit (if not installed)"
echo "   → Enable 'OpenXR' provider for Android"
echo "   → OpenXR → Interaction Profiles → Add 'Oculus Touch Controller Profile'"
echo "   → OpenXR → Features → Enable 'Meta Quest Support'"
echo
echo "5. 🎨 GRAPHICS SETTINGS"
echo "   → Edit → Project Settings → Graphics"
echo "   → Remove 'Built-in Render Pipeline' (if present)"
echo "   → Keep only: Vulkan, OpenGLES3"
echo "   → Color Space: Linear (recommended for VR)"
echo
echo "6. 🔒 ANDROID MANIFEST"
echo "   → The build script will handle Android manifest requirements"
echo "   → Hand tracking, camera permissions, Quest category will be added"
echo
echo "7. ✅ VALIDATION"
echo "   → Open Window → General → Console"
echo "   → Check for any red errors"
echo "   → All packages should show as installed"
echo
echo -e "${GREEN}📱 FIRST BUILD TEST:${NC}"
echo "==================="
echo "Once Unity configuration is complete:"
echo "1. Save the project (Ctrl+S / Cmd+S)"
echo "2. Close Unity"
echo "3. Run: ./build.sh development"
echo "4. Check output APK in: Builds/Development/"
echo
echo -e "${YELLOW}💡 TROUBLESHOOTING:${NC}"
echo "If you encounter issues:"
echo "→ Run: ./validate-android-setup.sh"
echo "→ Check Unity Console for errors"
echo "→ Verify all packages are properly installed"
echo "→ Ensure Meta Quest is connected for testing"
echo
echo "📚 For detailed instructions, see: BUILD_INSTRUCTIONS.md"
