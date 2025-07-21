# Unity Mixed Reality Chess - Setup Guide

## Prerequisites

### Software Requirements
- **Unity 2022.3 LTS or newer** (recommended: 2022.3.12f1)
- **Meta Quest Developer Hub** (for device management)
- **Android SDK** (included with Unity Android Build Support)
- **Git** (for version control)
- **Visual Studio Code** with C# extension (recommended)

### Hardware Requirements
- **Meta Quest 2 or Quest 3** device
- **Development PC** (Windows/Mac/Linux)
- **USB-C cable** for device connection
- **Guardian boundary** set up on Quest

## Step-by-Step Setup

### 1. Unity Project Setup

1. **Open Unity Hub**
2. **Click "Add"** and select this project folder
3. **Open the project** (Unity will configure it automatically)

### 2. Package Installation

Open **Window > Package Manager** and install:

#### Core Packages (Unity Registry)
- `XR Interaction Toolkit` (2.4.3+)
- `OpenXR Plugin` (1.8.2+)
- `Universal Render Pipeline` (14.0.8+)
- `AR Foundation` (5.0.7+)
- `Input System` (1.6.3+)

#### Meta Quest SDK
1. Open **Asset Store** in Unity
2. Search for "**Meta XR All-in-One SDK**"
3. **Download and Import**
4. Follow the **Meta Setup Wizard**

### 3. XR Configuration

1. **File > Build Settings**
2. **Switch Platform** to Android
3. **Open Player Settings**
4. **XR Plug-in Management**:
   - Enable **OpenXR**
   - Add **Meta Quest** interaction profile
5. **OpenXR Settings**:
   - Enable **Hand Tracking**
   - Enable **Passthrough**
   - Enable **Spatial Anchors**

### 4. Meta Quest Device Setup

1. **Enable Developer Mode** on Quest:
   - Install Meta Quest mobile app
   - Create developer account
   - Enable developer mode in app settings

2. **USB Connection**:
   - Connect Quest to PC via USB-C
   - Allow USB debugging on Quest
   - Verify connection with `adb devices`

### 5. Project Configuration

1. **Import Chess Piece Models** (create or download 3D models)
2. **Configure Materials** for URP
3. **Set up Scene Hierarchy**:
   - XR Origin (with camera and controllers)
   - Chess board prefab
   - UI Canvas
   - Game Manager

### 6. Building and Testing

#### Development Build
1. **File > Build Settings**
2. **Add Open Scenes**
3. **Player Settings**:
   - Company Name: Your name
   - Product Name: MR Chess
   - Package Name: com.yourname.mrchess
   - Minimum API Level: Android 7.0 (API 24)
   - Target API Level: Automatic (highest installed)
4. **Build and Run** (with Quest connected)

#### Testing in Editor
- Use **XR Device Simulator** for basic testing
- **Play Mode** testing with mouse/keyboard simulation

## Troubleshooting

### Common Issues

#### "OpenXR not found"
- Ensure OpenXR Plugin is installed
- Check XR Plug-in Management settings
- Restart Unity after installation

#### "Meta XR SDK errors"
- Import latest Meta XR All-in-One SDK
- Run Meta XR Setup Wizard
- Check for Unity version compatibility

#### "Hand tracking not working"
- Enable hand tracking in Quest settings
- Verify OpenXR hand tracking feature
- Check XR Interaction Toolkit configuration

#### "Build fails for Android"
- Install Android Build Support in Unity Hub
- Check Android SDK path in preferences
- Verify minimum API level compatibility

### Performance Tips

1. **Use URP** for better Quest performance
2. **Optimize textures** (max 1024x1024 for chess pieces)
3. **Limit draw calls** with object pooling
4. **Test on device frequently** (editor != Quest performance)

## Development Workflow

### Recommended Process
1. **Code in VS Code** with this workspace
2. **Test basic logic** in Unity editor
3. **Build to Quest** for MR testing
4. **Iterate quickly** with wireless debugging

### Git Workflow
```bash
# Create feature branch
git checkout -b feature/piece-animations

# Make changes and commit
git add .
git commit -m "Add piece movement animations"

# Push to GitHub
git push origin feature/piece-animations
```

## Next Steps

After completing setup:

1. **Test basic scene** with XR Origin
2. **Implement chess board placement** using spatial anchors
3. **Add piece interaction** with hand tracking
4. **Test chess logic** with move validation
5. **Polish UI and interactions**

## Resources

- [Unity XR Documentation](https://docs.unity3d.com/Manual/XR.html)
- [Meta Quest Development](https://developer.oculus.com/documentation/unity/)
- [XR Interaction Toolkit](https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@2.4/manual/index.html)
- [OpenXR Documentation](https://docs.unity3d.com/Packages/com.unity.xr.openxr@1.8/manual/index.html)
