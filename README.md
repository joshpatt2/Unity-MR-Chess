# Unity MR Chess Game

A Mixed Reality Chess game for Meta Quest 2/3 using Unity's Universal Render Pipeline (URP) and OpenXR.

## Features

- **Mixed Reality Experience**: Anchored chessboard in real-world space using passthrough
- **Hand Tracking**: Natural piece movement with hand gestures
- **Controller Support**: Compatible with Meta Quest controllers
- **Chess Engine**: Complete rule validation and AI opponent
- **Spatial Anchors**: Persistent board placement across sessions

## Requirements

### Unity Setup
- Unity 2022.3 LTS or newer
- Universal Render Pipeline (URP)
- Android Build Support
- XR Interaction Toolkit
- Meta XR All-in-One SDK

### Hardware
- Meta Quest 2 or Quest 3
- Hand tracking enabled
- Guardian boundary setup

## Project Structure

```
Assets/
├── Scripts/
│   ├── Chess/              # Chess game logic (ChessBoard, ChessPiece, ChessMove)
│   ├── MR/                 # Mixed Reality components (SpatialAnchor, Interactions)
│   ├── AI/                 # Chess AI engine (Minimax algorithm)
│   ├── Managers/           # Game managers (GameManager)
│   └── Setup/              # Unity setup automation tools
├── Scenes/                 # Unity scenes (create MRChessGame.unity)
├── Prefabs/                # Game objects (to be created)
├── Materials/              # URP materials (to be created)
└── UNITY_SETUP_CHECKLIST.md

ProjectSettings/            # Unity project configuration
Packages/                   # Package dependencies (manifest.json)
Builds/                     # Build outputs
```

## Documentation

- **`QUICK_START.md`** - Immediate next steps after cloning
- **`SETUP.md`** - Complete setup instructions  
- **`DEVELOPMENT.md`** - Development workflow and best practices
- **`Assets/UNITY_SETUP_CHECKLIST.md`** - Unity Editor configuration checklist
- **`GITHUB_SETUP.md`** - Git and GitHub workflow

## Getting Started

### Quick Setup (5 minutes)

1. **Clone and Open Project**:
   ```bash
   git clone https://github.com/yourusername/Chess-AI.git
   cd Chess-AI
   ```

2. **Open in Unity Hub**:
   - Open Unity Hub → Add → Select project folder
   - Open with Unity 2022.3 LTS+ (project will auto-import packages)

3. **Follow Setup Guide**:
   - See `QUICK_START.md` for immediate next steps
   - Complete Unity configuration with `Assets/UNITY_SETUP_CHECKLIST.md`

### Current Project Status ✅

- ✅ **Complete Chess Engine** - Full rule validation, move generation, AI opponent
- ✅ **MR Components Ready** - Spatial anchors, hand tracking, piece interaction
- ✅ **Unity Package Setup** - XR Interaction Toolkit, OpenXR, URP configured  
- ✅ **Build System** - Automated build scripts for Meta Quest
- ✅ **Documentation** - Comprehensive setup and development guides

**Ready for:** Unity Editor setup → Scene creation → Quest testing

### Detailed Setup Instructions

#### 1. Unity Project Setup

1. **Package Installation** (auto-imported from manifest.json):
   - XR Interaction Toolkit (2.4.3+)
   - OpenXR Plugin (1.8.2+)  
   - Universal Render Pipeline (14.0.11+)
   - AR Foundation (5.0.7+)

2. **XR Configuration**:
   - Edit → Project Settings → XR Plug-in Management
   - Enable OpenXR for Android
   - Configure Meta Quest support and hand tracking

#### 2. Meta Quest Setup

1. Enable Developer Mode on Quest device
2. Install Meta Quest Developer Hub
3. Enable Passthrough API in project settings
4. Configure hand tracking in XR settings

### 3. Build Configuration

1. Switch platform to Android
2. Set minimum API level to Android 7.0 (API 24)
3. Configure XR settings for Quest
4. Enable hand tracking and passthrough

## Development

### Chess Engine Architecture

The chess engine follows a modular MVVM pattern:

- **Model**: Board state, pieces, and game rules
- **View**: 3D chess pieces and board visualization
- **ViewModel**: Game logic and move validation
- **Controller**: Input handling and MR interactions

### MR Integration

- **Spatial Anchoring**: Persistent board placement
- **Hand Tracking**: Natural piece interaction
- **Passthrough**: Real-world integration
- **Occlusion**: Realistic depth perception

## Building and Deployment

### Quick Build
```bash
# Development build with debugging
./build.sh

# Or use Unity Editor: File → Build Settings → Build
```

### Manual Build Commands
```bash
# Development build
Unity -batchmode -quit -projectPath . -buildTarget Android -buildPath "./Builds/Development"

# Release build  
Unity -batchmode -quit -projectPath . -buildTarget Android -buildPath "./Builds/Release"
```

### Installing on Quest
1. Connect Quest via USB or Oculus Link
2. Use `adb install path/to/chess-mr.apk`
3. Launch from Unknown Sources in Quest library

## Contributing

1. Fork the repository
2. Create a feature branch
3. Follow Unity C# coding conventions
4. Test on actual Quest hardware
5. Submit a pull request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For questions and support:
- Check Unity XR documentation
- Review Meta Quest development guides
- File issues in this repository
