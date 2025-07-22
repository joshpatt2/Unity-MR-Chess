# Quick Unity Setup Steps 🚀

## Current Status ✅
Your project is ready with:
- ✅ C# scripts created and error-free
- ✅ Package dependencies configured
- ✅ Build scripts ready
- ✅ Git repository initialized

## Next: Open in Unity Editor

### Step 1: Open Unity Hub
```bash
# Option A: From command line (if Unity Hub CLI installed)
open -a "Unity Hub"

# Option B: Open Unity Hub manually from Applications
```

### Step 2: Add Project
1. Click **"Add"** in Unity Hub
2. Navigate to: `/Users/joshuapatterson/Chess-AI`
3. Click **"Add Project"**

### Step 3: Open Project
1. Select the project in Unity Hub
2. Click **"Open"** (Unity will import packages automatically)
3. **Wait for import** (first time may take 5-10 minutes)

### Step 4: Verify Package Installation
1. **Window → Package Manager**
2. **In Project** tab, verify these packages:
   - ✅ AR Foundation (5.0.7)
   - ✅ ARCore XR Plugin (5.0.7) 
   - ✅ Universal Render Pipeline (14.0.11)
   - ✅ XR Interaction Toolkit (2.4.3)
   - ✅ OpenXR Plugin (1.8.2)
   - ✅ Input System (1.6.3)

### Step 5: Quick XR Setup
1. **Edit → Project Settings**
2. **XR Plug-in Management**:
   - ✅ Check "OpenXR" 
   - Click Android tab → ✅ Check "OpenXR"
3. **OpenXR** (click gear icon):
   - ✅ Meta Quest Support
   - ✅ Hand Interaction Poses
   - ✅ Hand Tracking Subsystem

### Step 6: Platform Configuration  
1. **File → Build Settings**
2. **Switch Platform** to Android
3. **Player Settings** (gear icon):
   - Minimum API Level: **Android 7.0 (API 24)**
   - Target API Level: **Automatic**
   - Scripting Backend: **IL2CPP**
   - Target Architectures: **ARM64**

### Step 7: Auto-Setup Scene
1. **Create new Scene**: File → New Scene → 3D
2. **Save as**: `Assets/Scenes/MRChessGame.unity`
3. **Add XRSceneSetup**:
   - Create empty GameObject
   - Add Component: `XR Scene Setup`
   - Click **"Setup XR Components"**
   - Click **"Create Chess Scene Structure"**

## What to Expect
- Unity will create XR Origin with camera
- Scene structure for chess game
- Ready for chessboard and piece setup

## If You Run Into Issues
- **Console errors**: Check the Unity Console window
- **Missing packages**: Package Manager → Refresh
- **XR not working**: Restart Unity after XR setup
- **Build errors**: Check Android Build Support is installed

## Ready for Next Phase
After Unity setup is complete, we'll:
1. 🎯 Create chessboard and piece prefabs
2. 🤚 Configure hand tracking interactions  
3. 📱 Test build on Meta Quest
4. 🔗 Set up spatial anchors for MR

---

**Need help?** The detailed guides are in:
- `Assets/UNITY_SETUP_CHECKLIST.md` - Complete checklist
- `SETUP.md` - Full setup instructions
- `DEVELOPMENT.md` - Development workflow
