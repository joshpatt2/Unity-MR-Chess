#!/bin/bash

# JDK Configuration Fix for Unity Android Development
echo "🔧 Fixing JDK Configuration for Unity"
echo "====================================="

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo "1. Detecting Java Installation"
echo "------------------------------"

# Check current Java version
if command -v java >/dev/null 2>&1; then
    java_version=$(java -version 2>&1 | head -n 1)
    echo -e "${GREEN}✅ Java found: $java_version${NC}"
else
    echo -e "${RED}❌ Java not found${NC}"
    exit 1
fi

# Get Java Home using macOS utility
if command -v /usr/libexec/java_home >/dev/null 2>&1; then
    detected_java_home=$(/usr/libexec/java_home 2>/dev/null)
    if [ $? -eq 0 ] && [ -n "$detected_java_home" ]; then
        echo -e "${GREEN}✅ JAVA_HOME detected: $detected_java_home${NC}"
    else
        echo -e "${RED}❌ Could not detect JAVA_HOME${NC}"
        exit 1
    fi
else
    echo -e "${RED}❌ java_home utility not found${NC}"
    exit 1
fi

echo
echo "2. Configuring Environment Variables"
echo "------------------------------------"

# Set JAVA_HOME for current session
export JAVA_HOME="$detected_java_home"
echo -e "${GREEN}✅ JAVA_HOME set for current session: $JAVA_HOME${NC}"

# Add to shell profile if not already present
if ! grep -q "JAVA_HOME.*java_home" ~/.zshrc 2>/dev/null; then
    echo 'export JAVA_HOME="$(/usr/libexec/java_home)"' >> ~/.zshrc
    echo -e "${GREEN}✅ Added JAVA_HOME to ~/.zshrc${NC}"
else
    echo -e "${YELLOW}⚠️ JAVA_HOME already configured in ~/.zshrc${NC}"
fi

echo
echo "3. Validating JDK Installation"
echo "-------------------------------"

# Check for javac (Java compiler)
javac_path="$JAVA_HOME/bin/javac"
if [ -f "$javac_path" ]; then
    javac_version=$("$javac_path" -version 2>&1)
    echo -e "${GREEN}✅ Java compiler found: $javac_version${NC}"
else
    echo -e "${RED}❌ Java compiler not found at: $javac_path${NC}"
fi

# Check for jar tool
jar_path="$JAVA_HOME/bin/jar"
if [ -f "$jar_path" ]; then
    echo -e "${GREEN}✅ JAR tool found${NC}"
else
    echo -e "${RED}❌ JAR tool not found at: $jar_path${NC}"
fi

echo
echo "4. Unity Compatibility Check"
echo "-----------------------------"

# Check Java version compatibility
java_version_num=$(java -version 2>&1 | grep -o '"[0-9]*' | head -1 | cut -d'"' -f2)
echo "Java version number: $java_version_num"

if [ "$java_version_num" -ge 11 ]; then
    echo -e "${GREEN}✅ Java $java_version_num is compatible with Unity (requires 11+)${NC}"
else
    echo -e "${RED}❌ Java $java_version_num is too old. Unity requires Java 11+${NC}"
    echo -e "${YELLOW}💡 Install a newer JDK version${NC}"
fi

echo
echo "5. Unity Configuration Commands"
echo "-------------------------------"
echo -e "${BLUE}Copy these settings for Unity External Tools:${NC}"
echo
echo "🔧 Edit → Preferences → External Tools:"
echo "   JDK Path: $JAVA_HOME"
echo "   Android SDK: /opt/homebrew/share/android-commandlinetools"
echo "   Android NDK: /opt/homebrew/share/android-commandlinetools/ndk/25.2.9519653"
echo

echo "📋 Or run this in Unity Console to auto-configure:"
echo 'EditorPrefs.SetString("JdkPath", "'"$JAVA_HOME"'");'
echo 'EditorPrefs.SetString("AndroidSdkRoot", "/opt/homebrew/share/android-commandlinetools");'
echo 'EditorPrefs.SetString("AndroidNdkRoot", "/opt/homebrew/share/android-commandlinetools/ndk/25.2.9519653");'
echo 'Debug.Log("Unity paths configured!");'

echo
echo -e "${GREEN}🎯 JDK Configuration Complete!${NC}"
echo "💡 Restart Unity if it was already open to apply the new JDK settings."
echo "💡 Use 'MR Chess → Auto-Configure Android SDK' in Unity for automatic setup."
