# Android Gradle Build Fix

## Problem
Unity Android builds were failing with the error:
```
Resource shrinker cannot be used for libraries.
Build file '/Users/.../Library/Bee/Android/Prj/IL2CPP/Gradle/unityLibrary/build.gradle' line: 64
```

## Root Cause
The Unity library Gradle template had `shrinkResources true` enabled in the release build configuration. Unity libraries (as opposed to applications) cannot use Android's resource shrinking feature.

## Solution Applied

### 1. Fixed Unity Library Gradle Template
Updated `/Assets/Plugins/Android/unityLibrary.gradle` to ensure:
- `shrinkResources false` in both debug and release build types
- Added clear comments explaining why resource shrinking must be disabled

### 2. Enhanced AndroidSDKPathFixer.cs
Added new functionality:
- `ValidateGradleConfiguration()` - Checks for common Gradle issues
- `FixGradleTemplate()` - Automatically fixes resource shrinking issues
- `CheckGeneratedGradleFiles()` - Validates generated build files

### 3. Created GradleBuildFixer.cs
New comprehensive fix tool with:
- `CleanAndroidBuildCache()` - Removes corrupted generated files
- `FixAllAndroidIssues()` - One-click fix for all Android problems
- `ValidateQuestBuildSettings()` - Ensures Quest-compatible settings

### 4. Build Cache Cleanup
Removed the corrupted generated Gradle files from:
```
Library/Bee/Android/Prj/IL2CPP/Gradle/unityLibrary/build.gradle
```

## How to Use

### Quick Fix (Recommended)
1. In Unity, go to **MR Chess → Fix All Android Issues**
2. This will automatically fix all known Android build problems

### Manual Steps
1. **MR Chess → Fix Gradle Configuration** - Fix template issues
2. **MR Chess → Clean Android Build Cache** - Remove corrupted files  
3. **MR Chess → Validate Quest Build Settings** - Check platform settings
4. **MR Chess → Validate Android SDK** - Verify SDK configuration

## Prevention
The enhanced AndroidSDKPathFixer now runs automatically on Unity startup and will catch these issues before they cause build failures.

## Key Changes Made

### unityLibrary.gradle Template
```groovy
buildTypes {
    debug {
        shrinkResources false  // ✅ Disabled for libraries
    }
    release {
        shrinkResources false  // ✅ Disabled for libraries (was true)
    }
}
```

### Editor Scripts
- Added comprehensive Gradle validation
- Automatic detection and fixing of resource shrinking issues  
- Build cache management
- Quest-specific build validation

This fix ensures that Unity will generate correct Gradle files that don't attempt to use resource shrinking in Unity libraries, preventing the build failure.
