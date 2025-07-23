#!/bin/bash

# Quick fix for Unity Android SDK Command-line Tools issue
echo "🔧 Fixing Unity Android SDK Command-line Tools issue..."

# Ensure the 6.0 directory exists (Unity expects this)
if [ ! -d "$ANDROID_HOME/cmdline-tools/6.0" ]; then
    echo "Creating cmdline-tools/6.0 for Unity compatibility..."
    cp -r "$ANDROID_HOME/cmdline-tools/latest" "$ANDROID_HOME/cmdline-tools/6.0"
    echo "✅ Created cmdline-tools/6.0"
else
    echo "✅ cmdline-tools/6.0 already exists"
fi

# Test if sdkmanager works from both paths
echo
echo "🧪 Testing SDK Manager accessibility..."
echo "From latest: $($ANDROID_HOME/cmdline-tools/latest/bin/sdkmanager --version 2>/dev/null || echo 'FAILED')"
echo "From 6.0: $($ANDROID_HOME/cmdline-tools/6.0/bin/sdkmanager --version 2>/dev/null || echo 'FAILED')"

echo
echo "📋 Unity External Tools Settings:"
echo "================================="
echo "Android SDK Tools: $ANDROID_HOME"
echo "Android NDK: $ANDROID_HOME/ndk/$(ls $ANDROID_HOME/ndk | head -n 1)"
echo "JDK: $(dirname $(dirname $(which java)))"
echo
echo "✅ Command-line tools fix applied!"
echo "💡 In Unity: Edit > Preferences > External Tools > Set Android SDK Tools path"
