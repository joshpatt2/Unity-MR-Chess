#!/bin/bash

# Simple Unity MR Chess Build Script
# Uses Unity's native command line parameters for more reliable building

set -e

# Configuration
UNITY_PATH="/Applications/Unity/Hub/Editor/2022.3.62f1/Unity.app/Contents/MacOS/Unity"
PROJECT_PATH="/Users/joshuapatterson/Chess-AI"
BUILD_PATH="/Users/joshuapatterson/Chess-AI/Builds/Development/MRChess-Dev.apk"
BUILD_TYPE="${1:-development}"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Functions
log_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

log_warn() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

log_step() {
    echo -e "${BLUE}[STEP]${NC} $1"
}

# Header
echo -e "${GREEN}Unity MR Chess Simple Build Script${NC}"
echo "=============================================="

# Validation
log_step "Validating environment..."

if [ ! -f "$UNITY_PATH" ]; then
    log_error "Unity not found at $UNITY_PATH"
    log_info "Please update UNITY_PATH in this script or install Unity 2022.3.62f1"
    exit 1
fi

if [ ! -d "$PROJECT_PATH/Assets" ]; then
    log_error "Not in Unity project root directory"
    log_info "Please run this script from the project root"
    exit 1
fi

# Create build directory
BUILD_DIR=$(dirname "$BUILD_PATH")
mkdir -p "$BUILD_DIR"

log_step "Applying Android 15+ compatibility fixes..."
"$UNITY_PATH" \
    -batchmode \
    -quit \
    -projectPath "$PROJECT_PATH" \
    -executeMethod MRChess.Setup.AndroidCompatibilityFixer.FixAndroid15Compatibility \
    -logFile "$BUILD_DIR/compatibility-fix.log" \
    -nographics

if [ $? -ne 0 ]; then
    log_warn "Compatibility fixes had issues, but continuing with build..."
fi

log_step "Building Android APK..."
log_info "Build target: Android"
log_info "Output path: $BUILD_PATH"

# Build using Unity's native Android build command
"$UNITY_PATH" \
    -batchmode \
    -quit \
    -projectPath "$PROJECT_PATH" \
    -buildTarget Android \
    -buildAndroidPlayer "$BUILD_PATH" \
    -logFile "$BUILD_DIR/simple-build.log" \
    -nographics

# Check build result
if [ $? -eq 0 ] && [ -f "$BUILD_PATH" ]; then
    log_info "✅ Build completed successfully!"
    
    APK_SIZE=$(du -h "$BUILD_PATH" | cut -f1)
    log_info "📦 APK Size: $APK_SIZE"
    log_info "📍 APK Location: $BUILD_PATH"
    log_info "📋 Build Log: $BUILD_DIR/simple-build.log"
    
    echo ""
    log_step "🎯 Next Steps:"
    echo "1. Connect Meta Quest via USB"
    echo "2. Enable Developer Mode and USB Debugging"
    echo "3. Install APK: adb install \"$BUILD_PATH\""
    echo "4. Launch from Unknown Sources in Quest library"
    
    # Auto-install if device is connected
    if command -v adb &> /dev/null; then
        if adb devices | grep -q "device$"; then
            echo ""
            read -p "Quest device detected. Install APK automatically? (y/N): " -n 1 -r
            echo
            if [[ $REPLY =~ ^[Yy]$ ]]; then
                log_step "Installing APK to connected Quest device..."
                adb install -r "$BUILD_PATH"
                if [ $? -eq 0 ]; then
                    log_info "✅ APK installed successfully!"
                    log_info "🚀 Launch 'MR Chess' from Unknown Sources in your Quest library"
                else
                    log_warn "APK installation failed. Try installing manually."
                fi
            fi
        fi
    fi
    
else
    log_error "❌ Build failed!"
    log_error "Check build log for details: $BUILD_DIR/simple-build.log"
    
    if [ -f "$BUILD_DIR/simple-build.log" ]; then
        echo ""
        log_step "Last 20 lines of build log:"
        tail -20 "$BUILD_DIR/simple-build.log"
    fi
    
    exit 1
fi
