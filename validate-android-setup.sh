#!/bin/bash

# Unity MR Chess - Android Build Setup Validation Script
# This script validates your macOS setup for building Android APKs with Unity

echo "🔧 Unity MR Chess - Android Build Setup Validation"
echo "=================================================="
echo

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Function to check if command exists
command_exists() {
    command -v "$1" >/dev/null 2>&1
}

# Function to print status
print_status() {
    if [ $2 -eq 0 ]; then
        echo -e "${GREEN}✅ $1${NC}"
    else
        echo -e "${RED}❌ $1${NC}"
    fi
}

# Function to print warning
print_warning() {
    echo -e "${YELLOW}⚠️  $1${NC}"
}

validation_failed=0

echo "1. Java Development Kit (JDK)"
echo "-----------------------------"
if command_exists java; then
    java_version=$(java -version 2>&1 | head -n 1)
    echo "   Found: $java_version"
    
    # Check if Java version is 11+
    if java -version 2>&1 | grep -q "openjdk version \"[1-9][1-9]\."; then
        print_status "Java 11+ detected" 0
    else
        print_status "Java 11+ required (Unity recommendation)" 1
        validation_failed=1
    fi
else
    print_status "Java not found" 1
    validation_failed=1
fi
echo

echo "2. Android SDK"
echo "--------------"
if [ -n "$ANDROID_HOME" ] && [ -d "$ANDROID_HOME" ]; then
    print_status "ANDROID_HOME set: $ANDROID_HOME" 0
    
    # Check required platforms
    if [ -d "$ANDROID_HOME/platforms/android-26" ]; then
        print_status "Android API 26 (minimum) installed" 0
    else
        print_status "Android API 26 missing" 1
        validation_failed=1
    fi
    
    if [ -d "$ANDROID_HOME/platforms/android-33" ]; then
        print_status "Android API 33 (target) installed" 0
    else
        print_status "Android API 33 missing" 1
        validation_failed=1
    fi
else
    print_status "ANDROID_HOME not set or SDK not found" 1
    validation_failed=1
fi
echo

echo "3. Android NDK"
echo "--------------"
if [ -n "$ANDROID_HOME" ] && [ -d "$ANDROID_HOME/ndk" ]; then
    ndk_versions=$(ls "$ANDROID_HOME/ndk" 2>/dev/null | wc -l | tr -d ' ')
    if [ "$ndk_versions" -gt 0 ]; then
        ndk_version=$(ls "$ANDROID_HOME/ndk" | head -n 1)
        print_status "NDK installed: $ndk_version" 0
    else
        print_status "NDK not found" 1
        validation_failed=1
    fi
else
    print_status "NDK not found" 1
    validation_failed=1
fi
echo

echo "4. Android Build Tools"
echo "----------------------"
if [ -n "$ANDROID_HOME" ] && [ -d "$ANDROID_HOME/build-tools" ]; then
    build_tools_versions=$(ls "$ANDROID_HOME/build-tools" 2>/dev/null | wc -l | tr -d ' ')
    if [ "$build_tools_versions" -gt 0 ]; then
        build_tools_version=$(ls "$ANDROID_HOME/build-tools" | tail -n 1)
        print_status "Build Tools installed: $build_tools_version" 0
    else
        print_status "Build Tools not found" 1
        validation_failed=1
    fi
else
    print_status "Build Tools not found" 1
    validation_failed=1
fi
echo

echo "5. Android Debug Bridge (ADB)"
echo "------------------------------"
if command_exists adb; then
    adb_version=$(adb version | head -n 1)
    print_status "ADB available: $adb_version" 0
else
    print_status "ADB not found" 1
    validation_failed=1
fi
echo

echo "6. Meta Quest Device Detection"
echo "-------------------------------"
if command_exists adb; then
    # Check for connected Quest devices
    quest_devices=$(adb devices | grep -E "(Quest|oculus)" | wc -l | tr -d ' ')
    if [ "$quest_devices" -gt 0 ]; then
        print_status "Meta Quest device detected" 0
        adb devices | grep -E "(Quest|oculus)" | while read line; do
            echo "   📱 $line"
        done
    else
        print_warning "No Meta Quest devices detected (ensure USB debugging is enabled)"
        echo "   💡 To connect your Quest:"
        echo "      1. Enable Developer Mode in Meta Quest mobile app"
        echo "      2. Connect Quest via USB-C cable"
        echo "      3. Allow USB debugging when prompted"
    fi
else
    print_status "Cannot check Quest devices (ADB not available)" 1
fi
echo

echo "7. Unity Project Structure"
echo "---------------------------"
if [ -f "Assets/chess.unity" ]; then
    print_status "Unity scene found" 0
else
    print_status "Unity scene not found" 1
    validation_failed=1
fi

if [ -f "ProjectSettings/ProjectSettings.asset" ]; then
    print_status "Unity project settings found" 0
else
    print_status "Unity project settings not found" 1
    validation_failed=1
fi
echo

echo "8. Build System"
echo "---------------"
if [ -f "build.sh" ]; then
    print_status "Build script found" 0
    if [ -x "build.sh" ]; then
        print_status "Build script is executable" 0
    else
        print_warning "Build script exists but is not executable"
        echo "   💡 Run: chmod +x build.sh"
    fi
else
    print_status "Build script not found" 1
    validation_failed=1
fi
echo

# Summary
echo "🎯 Validation Summary"
echo "===================="
if [ $validation_failed -eq 0 ]; then
    echo -e "${GREEN}✅ All checks passed! Your system is ready to build Android APKs with Unity.${NC}"
    echo
    echo "🚀 Next Steps:"
    echo "1. Run: ./configure-unity-android.sh (to open Unity with configuration)"
    echo "2. In Unity: File > Build Settings > Switch to Android platform"  
    echo "3. Configure Player Settings for Quest (see configuration script output)"
    echo "4. Install required packages: XR Interaction Toolkit, OpenXR Plugin"
    echo "5. Run: ./build.sh development"
else
    echo -e "${RED}❌ Some issues found. Please resolve them before building.${NC}"
    echo
    echo "🔧 Quick Fixes:"
    echo "1. Ensure Unity has Android Build Support module installed"
    echo "2. Run: ./configure-unity-android.sh to set up Unity integration"
    echo "3. Install missing Android SDK components with: sdkmanager [package-name]"
fi
echo

# Export environment variables for this session
echo "📋 Environment Variables (add these to ~/.zshrc for persistence):"
echo "export ANDROID_HOME=$ANDROID_HOME"
echo "export ANDROID_SDK_ROOT=\$ANDROID_HOME"
echo "export PATH=\$PATH:\$ANDROID_HOME/platform-tools:\$ANDROID_HOME/cmdline-tools/latest/bin"
