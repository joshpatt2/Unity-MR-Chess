#!/bin/bash

echo "🔧 Simple Unity Android Build Test"
echo "=================================="

# Set Android environment
export ANDROID_HOME="/opt/homebrew/share/android-commandlinetools"
export ANDROID_SDK_ROOT="$ANDROID_HOME"
export PATH="$PATH:$ANDROID_HOME/platform-tools:$ANDROID_HOME/cmdline-tools/latest/bin:$ANDROID_HOME/cmdline-tools/6.0/bin"

echo "📋 Environment:"
echo "ANDROID_HOME: $ANDROID_HOME"
echo "Project Path: $(pwd)"
echo

# Create build directory
mkdir -p Builds/Development

echo "🏗️ Building Unity project directly..."

# Use Unity's basic command-line build
/Applications/Unity/Hub/Editor/2022.3.62f1/Unity.app/Contents/MacOS/Unity \
    -batchmode \
    -quit \
    -projectPath "$(pwd)" \
    -buildTarget Android \
    -logFile "$(pwd)/Builds/Development/direct-build.log"

build_result=$?

echo
echo "📋 Build Result: $build_result"

if [ -f "$(pwd)/Builds/Development/direct-build.log" ]; then
    echo "📄 Build log created"
    echo "🔍 Checking for APK files..."
    find "$(pwd)" -name "*.apk" -newer build.sh
    echo "🔍 Checking build directory..."
    ls -la "$(pwd)/Builds/Development/"
else
    echo "❌ No build log found"
fi

echo
echo "✅ Direct build test complete"
