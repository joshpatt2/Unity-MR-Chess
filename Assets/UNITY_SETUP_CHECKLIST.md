# Unity MR Chess Setup Checklist

## 1. Project Setup ✅
- [x] Unity project created
- [x] Required packages added to manifest.json
- [x] C# scripts created

## 2. Unity Editor Configuration

### A. Package Manager Verification
Open Unity Editor → Window → Package Manager and verify these packages are installed:
- [x] Universal Render Pipeline (14.0.11)
- [x] XR Interaction Toolkit (2.4.3) 
- [x] OpenXR Plugin (1.8.2)
- [x] AR Foundation (5.0.7)
- [x] ARCore XR Plugin (5.0.7)
- [x] Input System (1.6.3)

### B. XR Settings Configuration
1. **Edit → Project Settings → XR Plug-in Management**
   - [ ] Enable "OpenXR" provider
   - [ ] Set OpenXR as default runtime

2. **OpenXR Feature Groups**
   - [ ] Enable "Hand Interaction Poses"
   - [ ] Enable "Hand Tracking Subsystem" 
   - [ ] Enable "Meta Quest Support"
   - [ ] Enable "Passthrough API"

3. **Android Settings**
   - [ ] Switch platform to Android (File → Build Settings)
   - [ ] Set minimum API level to 23+
   - [ ] Enable "Auto Graphics API" or set to Vulkan/OpenGLES3

### C. Render Pipeline Setup
1. **Graphics Settings**
   - [ ] Assign URP Asset (Create → Rendering → URP Asset)
   - [ ] Set in Project Settings → Graphics → Scriptable Render Pipeline Settings

2. **Quality Settings**
   - [ ] Assign URP Asset to quality levels

### D. Input System
1. **Project Settings → Player → Configuration**
   - [ ] Set "Active Input Handling" to "Input System Package (New)"

## 3. Scene Setup

### A. Create Main Scene
- [ ] Create new scene: `Assets/Scenes/MRChessGame.unity`
- [ ] Set up XR Origin with camera and controllers

### B. Essential GameObjects
- [ ] XR Origin (Camera Offset, Main Camera)
- [ ] XR Interaction Manager
- [ ] GameManager (with GameManager script)
- [ ] Chessboard parent object
- [ ] UI Canvas for game state

### C. Prefab Creation
- [ ] Chess pieces prefabs (King, Queen, Rook, Bishop, Knight, Pawn)
- [ ] Chessboard prefab
- [ ] Spatial anchor point prefab

## 4. Meta Quest Configuration

### A. Build Settings
- [ ] Platform: Android
- [ ] Texture Compression: ASTC
- [ ] Target API Level: Automatic (Highest Installed)

### B. Player Settings
- [ ] Company Name: [Your Company]
- [ ] Product Name: MR Chess
- [ ] Package Name: com.[company].mrchess
- [ ] Minimum API Level: Android 7.0 (API 24)
- [ ] Target API Level: Automatic (Highest Installed)

### C. XR Settings
- [ ] Stereo Rendering Mode: Single Pass Instanced
- [ ] Initialize XR on Startup: Enabled

## 5. Testing Setup
- [ ] Enable Developer Mode on Quest device
- [ ] Install ADB and setup device connection
- [ ] Test build and deploy to device

## Next Steps After Unity Setup
1. Create scene layout with XR components
2. Set up chessboard and piece prefabs
3. Configure spatial anchors
4. Test hand tracking and piece interaction
5. Build and test on Meta Quest device

## Common Issues & Solutions
- **OpenXR not starting**: Check XR Management settings and Quest developer mode
- **Hand tracking not working**: Verify Hand Tracking Subsystem is enabled
- **Build errors**: Check Android SDK/NDK versions and API levels
- **Performance issues**: Optimize URP settings for mobile VR
