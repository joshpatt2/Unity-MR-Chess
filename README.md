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
│   ├── Chess/              # Chess game logic
│   ├── MR/                 # Mixed Reality components
│   ├── UI/                 # User interface
│   └── Managers/           # Game managers
├── Prefabs/                # Game objects
├── Materials/              # URP materials
├── Scenes/                 # Unity scenes
└── Settings/               # XR and URP settings
```

## Getting Started

### 1. Unity Project Setup

1. Create new Unity 3D project
2. Install required packages via Package Manager:
   - XR Interaction Toolkit
   - OpenXR Plugin
   - Universal Render Pipeline
3. Import Meta XR All-in-One SDK
4. Configure XR settings for OpenXR

### 2. Meta Quest Setup

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

### Development Build
```bash
# Build for development with debugging
Unity -batchmode -quit -projectPath . -buildTarget Android -buildPath "./Builds/Development"
```

### Release Build
```bash
# Optimized build for distribution
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
