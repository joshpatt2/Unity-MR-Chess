#!/bin/bash

# Minimal Unity MR Chess Build Script
# Focuses on getting a working APK build

set -e

# Configuration
UNITY_PATH="/Applications/Unity/Hub/Editor/2022.3.62f1/Unity.app/Contents/MacOS/Unity"
PROJECT_PATH="/Users/joshuapatterson/Chess-AI"
BUILD_PATH="/Users/joshuapatterson/Chess-AI/Builds/Development/MRChess-Dev.apk"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

log_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

log_step() {
    echo -e "${BLUE}[STEP]${NC} $1"
}

echo -e "${GREEN}Unity MR Chess Minimal Build Script${NC}"
echo "=============================================="

# Validation
if [ ! -f "$UNITY_PATH" ]; then
    log_error "Unity not found at $UNITY_PATH"
    exit 1
fi

# Create build directory
BUILD_DIR=$(dirname "$BUILD_PATH")
mkdir -p "$BUILD_DIR"

log_step "Applying Android compatibility fixes (fixing resource shrinking issue)..."

# First, apply the Android compatibility fixes to disable resource shrinking
"$UNITY_PATH" \
    -batchmode \
    -quit \
    -projectPath "$PROJECT_PATH" \
    -executeMethod MRChess.Setup.AndroidCompatibilityFixer.FixAndroid15Compatibility \
    -logFile "$BUILD_DIR/compatibility-fix.log" \
    -nographics

if [ $? -ne 0 ]; then
    log_error "Compatibility fixes failed!"
    exit 1
fi

log_step "Building APK using Unity's AdvancedBuildScript method..."

# Use our custom build method
"$UNITY_PATH" \
    -batchmode \
    -quit \
    -projectPath "$PROJECT_PATH" \
    -executeMethod MRChess.Setup.AdvancedBuildScript.BuildDevelopmentAPK \
    -logFile "$BUILD_DIR/minimal-build.log" \
    -nographics

# Check if APK was created
if [ -f "$BUILD_PATH" ]; then
    log_info "✅ Build completed successfully!"
    
    APK_SIZE=$(du -h "$BUILD_PATH" | cut -f1)
    log_info "📦 APK Size: $APK_SIZE"
    log_info "📍 APK Location: $BUILD_PATH"
    
    echo ""
    log_step "🎯 Install APK:"
    echo "adb install \"$BUILD_PATH\""
    
else
    log_error "❌ Build failed - APK not found!"
    log_error "Check build log: $BUILD_DIR/minimal-build.log"
    
    # Show relevant parts of the log
    if [ -f "$BUILD_DIR/minimal-build.log" ]; then
        echo ""
        log_step "Searching for build errors in log:"
        grep -i "error\|exception\|failed\|building\|buildplayer" "$BUILD_DIR/minimal-build.log" | tail -10
    fi
    
    exit 1
fi
