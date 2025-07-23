#!/bin/bash

# Direct Unity Build with Gradle Override
# Bypasses Unity's Gradle template system

set -e

UNITY_PATH="/Applications/Unity/Hub/Editor/2022.3.62f1/Unity.app/Contents/MacOS/Unity"
PROJECT_PATH="/Users/joshuapatterson/Chess-AI"
BUILD_PATH="/Users/joshuapatterson/Chess-AI/Builds/Development/MRChess-Dev.apk"

# Colors
GREEN='\033[0;32m'
RED='\033[0;31m'
BLUE='\033[0;34m'
NC='\033[0m'

log_info() { echo -e "${GREEN}[INFO]${NC} $1"; }
log_error() { echo -e "${RED}[ERROR]${NC} $1"; }
log_step() { echo -e "${BLUE}[STEP]${NC} $1"; }

echo -e "${GREEN}Unity Direct Build with Gradle Fix${NC}"
echo "================================================"

BUILD_DIR=$(dirname "$BUILD_PATH")
mkdir -p "$BUILD_DIR"

# Clean any existing Gradle cache that might have resource shrinking enabled
log_step "Cleaning Gradle and Unity caches..."
rm -rf Library/Bee
rm -rf Temp
rm -rf ~/.gradle/caches/modules-*/modules-*/com.android.tools.build

log_step "Building with Unity command line (bypassing templates)..."

# Build directly with Unity command line, no custom methods
"$UNITY_PATH" \
    -batchmode \
    -quit \
    -projectPath "$PROJECT_PATH" \
    -buildTarget Android \
    -buildAndroidPlayer "$BUILD_PATH" \
    -logFile "$BUILD_DIR/direct-build.log" \
    -nographics

# Check result
if [ -f "$BUILD_PATH" ]; then
    log_info "✅ Build completed successfully!"
    APK_SIZE=$(du -h "$BUILD_PATH" | cut -f1)
    log_info "📦 APK Size: $APK_SIZE"
    log_info "📍 APK Location: $BUILD_PATH"
else
    log_error "❌ Build failed!"
    log_error "Checking for specific Gradle errors..."
    
    if grep -q "Resource shrinker" "$BUILD_DIR/direct-build.log"; then
        log_error "Resource shrinking error still present!"
        echo ""
        echo "The issue persists. This suggests Unity's build system is"
        echo "hardcoded to enable resource shrinking in certain conditions."
        echo ""
        echo "Possible solutions:"
        echo "1. Use an older Unity version (2022.3.50f1 or earlier)"
        echo "2. Disable release optimizations globally"
        echo "3. Build for development only (not release)"
        echo "4. Use Unity Cloud Build with custom Gradle settings"
    else
        grep -i "error\|exception\|failed" "$BUILD_DIR/direct-build.log" | tail -5
    fi
    
    exit 1
fi
