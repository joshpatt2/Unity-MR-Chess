#!/bin/bash

# Minimal Unity Build Script for Debugging
echo "🔧 Minimal Unity Build Test"
echo "============================"

# Set environment variables
export ANDROID_HOME="/opt/homebrew/share/android-commandlinetools"
export ANDROID_SDK_ROOT="$ANDROID_HOME"
export PATH="$PATH:$ANDROID_HOME/platform-tools:$ANDROID_HOME/cmdline-tools/latest/bin:$ANDROID_HOME/cmdline-tools/6.0/bin"

echo "📋 Environment Check:"
echo "ANDROID_HOME: $ANDROID_HOME"
echo "Unity: $(which Unity || echo 'Unity CLI not found, using app bundle')"
echo "Java: $(java -version 2>&1 | head -n 1)"
echo

# Try to compile and build without custom build scripts
echo "🏗️ Attempting minimal Unity build..."

# Use Unity's basic build functionality
/Applications/Unity/Hub/Editor/*/Unity.app/Contents/MacOS/Unity \
    -batchmode \
    -quit \
    -projectPath "$(pwd)" \
    -buildTarget Android \
    -logFile ./minimal_test_build.log \
    -executeMethod UnityEditor.BuildPipeline.BuildPlayer \
    2>&1

build_result=$?

echo
if [ $build_result -eq 0 ]; then
    echo "✅ Unity build process completed successfully!"
else
    echo "❌ Unity build failed with exit code: $build_result"
    echo "📋 Checking log for details..."
    
    if [ -f "./minimal_test_build.log" ]; then
        echo "Last 20 lines of build log:"
        tail -20 ./minimal_test_build.log
    fi
fi

echo
echo "📁 Checking for output files..."
find . -name "*.apk" -newer build.sh 2>/dev/null | head -5

echo
echo "🔍 Recent Unity logs:"
ls -la *.log 2>/dev/null | head -3
