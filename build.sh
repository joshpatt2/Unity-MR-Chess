#!/bin/bash

# Unity MR Chess Advanced Build Script
# Usage: ./build.sh [development|release] [--validate-only]

set -e

# Configuration
UNITY_PATH="/Applications/Unity/Hub/Editor/2022.3.62f1/Unity.app/Contents/MacOS/Unity"
PROJECT_PATH="$(pwd)"
BUILD_TYPE="${1:-development}"
VALIDATE_ONLY="${2}"

# Build paths
DEV_BUILD_PATH="./Builds/Development"
RELEASE_BUILD_PATH="./Builds/Release"

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
echo -e "${GREEN}Unity MR Chess Advanced Build Script${NC}"
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

# Check for required Android tools
if ! command -v adb &> /dev/null; then
    log_warn "ADB not found in PATH. Install Android SDK Platform Tools for device deployment"
fi

# Create build directories
mkdir -p "$DEV_BUILD_PATH"
mkdir -p "$RELEASE_BUILD_PATH"

# Validate build setup
log_step "Validating Unity project setup..."
"$UNITY_PATH" \
    -batchmode \
    -quit \
    -projectPath "$PROJECT_PATH" \
    -executeMethod MRChess.Setup.BuildValidator.ValidateBuildSetupBatch \
    -logFile "Logs/validation.log"

if [ $? -ne 0 ]; then
    log_error "Project validation failed. Check Logs/validation.log for details."
    exit 1
fi

log_info "Project validation passed!"

# Exit if validation-only mode
if [ "$VALIDATE_ONLY" = "--validate-only" ]; then
    log_info "Validation complete. Exiting (--validate-only mode)."
    exit 0
fi

# Configure Android settings
log_step "Configuring Android settings..."
"$UNITY_PATH" \
    -batchmode \
    -quit \
    -projectPath "$PROJECT_PATH" \
    -executeMethod MRChess.Setup.AdvancedBuildScript.ConfigureAndroidSettings \
    -logFile "Logs/android-config.log"

if [ $? -ne 0 ]; then
    log_error "Android configuration failed. Check Logs/android-config.log for details."
    exit 1
fi

log_info "Android settings configured!"

# Build
log_info "Build Type: $BUILD_TYPE"

if [ "$BUILD_TYPE" = "development" ]; then
    log_step "Building Development APK..."
    "$UNITY_PATH" \
        -batchmode \
        -quit \
        -projectPath "$PROJECT_PATH" \
        -executeMethod MRChess.Setup.AdvancedBuildScript.BuildDevelopmentAPK \
        -logFile "$DEV_BUILD_PATH/build.log"
        
    BUILD_LOG="$DEV_BUILD_PATH/build.log"
    APK_PATH="$DEV_BUILD_PATH/MRChess-Dev.apk"
        
elif [ "$BUILD_TYPE" = "release" ]; then
    log_step "Building Release APK..."
    "$UNITY_PATH" \
        -batchmode \
        -quit \
        -projectPath "$PROJECT_PATH" \
        -executeMethod MRChess.Setup.AdvancedBuildScript.BuildReleaseAPK \
        -logFile "$RELEASE_BUILD_PATH/build.log"
        
    BUILD_LOG="$RELEASE_BUILD_PATH/build.log"
    APK_PATH="$RELEASE_BUILD_PATH/MRChess.apk"
        
else
    log_error "Invalid build type. Use 'development' or 'release'"
    exit 1
fi

# Check build result
if [ $? -eq 0 ]; then
    log_info "✅ Build completed successfully!"
    
    if [ -f "$APK_PATH" ]; then
        APK_SIZE=$(du -h "$APK_PATH" | cut -f1)
        log_info "📦 APK Size: $APK_SIZE"
        log_info "📍 APK Location: $APK_PATH"
    fi
    
    log_info "📋 Build Log: $BUILD_LOG"
    
    echo ""
    log_step "🎯 Next Steps:"
    echo "1. Connect Meta Quest via USB"
    echo "2. Enable Developer Mode and USB Debugging"
    echo "3. Install APK: adb install \"$APK_PATH\""
    echo "4. Launch from Unknown Sources in Quest library"
    
    # Auto-install if device is connected
    if command -v adb &> /dev/null; then
        if adb devices | grep -q "device$"; then
            echo ""
            read -p "Quest device detected. Install APK automatically? (y/N): " -n 1 -r
            echo
            if [[ $REPLY =~ ^[Yy]$ ]]; then
                log_step "Installing APK to connected Quest device..."
                adb install -r "$APK_PATH"
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
    log_error "Check build log for details: $BUILD_LOG"
    exit 1
fi
