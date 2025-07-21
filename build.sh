#!/bin/bash

# Unity MR Chess Build Script
# Usage: ./build.sh [development|release]

set -e

# Configuration
UNITY_PATH="/Applications/Unity/Hub/Editor/2022.3.12f1/Unity.app/Contents/MacOS/Unity"
PROJECT_PATH="$(pwd)"
BUILD_TYPE="${1:-development}"

# Build paths
DEV_BUILD_PATH="./Builds/Development"
RELEASE_BUILD_PATH="./Builds/Release"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${GREEN}Unity MR Chess Build Script${NC}"
echo "=========================================="

# Check if Unity exists
if [ ! -f "$UNITY_PATH" ]; then
    echo -e "${RED}Error: Unity not found at $UNITY_PATH${NC}"
    echo "Please update UNITY_PATH in this script"
    exit 1
fi

# Check if project exists
if [ ! -d "$PROJECT_PATH/Assets" ]; then
    echo -e "${RED}Error: Not in Unity project root directory${NC}"
    echo "Please run this script from the project root"
    exit 1
fi

# Create build directories
mkdir -p "$DEV_BUILD_PATH"
mkdir -p "$RELEASE_BUILD_PATH"

echo -e "${YELLOW}Build Type: $BUILD_TYPE${NC}"

if [ "$BUILD_TYPE" = "development" ]; then
    echo -e "${YELLOW}Building Development Version...${NC}"
    "$UNITY_PATH" \
        -batchmode \
        -quit \
        -projectPath "$PROJECT_PATH" \
        -buildTarget Android \
        -buildPath "$DEV_BUILD_PATH/MRChess-Dev.apk" \
        -logFile "$DEV_BUILD_PATH/build.log" \
        -developmentBuild \
        -allowDebugging
        
elif [ "$BUILD_TYPE" = "release" ]; then
    echo -e "${YELLOW}Building Release Version...${NC}"
    "$UNITY_PATH" \
        -batchmode \
        -quit \
        -projectPath "$PROJECT_PATH" \
        -buildTarget Android \
        -buildPath "$RELEASE_BUILD_PATH/MRChess.apk" \
        -logFile "$RELEASE_BUILD_PATH/build.log"
        
else
    echo -e "${RED}Error: Invalid build type. Use 'development' or 'release'${NC}"
    exit 1
fi

# Check build result
if [ $? -eq 0 ]; then
    echo -e "${GREEN}Build completed successfully!${NC}"
    if [ "$BUILD_TYPE" = "development" ]; then
        echo "APK location: $DEV_BUILD_PATH/MRChess-Dev.apk"
        echo "Log file: $DEV_BUILD_PATH/build.log"
    else
        echo "APK location: $RELEASE_BUILD_PATH/MRChess.apk"
        echo "Log file: $RELEASE_BUILD_PATH/build.log"
    fi
else
    echo -e "${RED}Build failed! Check log file for details.${NC}"
    exit 1
fi

echo ""
echo -e "${GREEN}Next steps:${NC}"
echo "1. Connect Meta Quest via USB"
echo "2. Enable Developer Mode and USB Debugging"
echo "3. Install APK: adb install path/to/MRChess.apk"
echo "4. Launch from Unknown Sources in Quest library"
