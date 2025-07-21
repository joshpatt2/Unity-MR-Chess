<!-- Use this file to provide workspace-specific custom instructions to Copilot. For more details, visit https://code.visualstudio.com/docs/copilot/copilot-customization#_use-a-githubcopilotinstructionsmd-file -->

# Unity MR Chess Project Instructions

This is a Unity Mixed Reality Chess game project targeting Meta Quest 2/3 devices.

## Project Structure
- Follow Unity's recommended folder structure
- Use MVVM pattern for UI and game logic separation
- Keep MR-specific code modular and testable
- Use C# coding conventions and Unity best practices

## Key Technologies
- Unity 2022.3 LTS+ with URP (Universal Render Pipeline)
- OpenXR + Meta Quest SDK for VR/MR functionality
- XR Interaction Toolkit for hand tracking and controller input
- Passthrough API for mixed reality anchoring
- Spatial anchors for chessboard positioning

## Code Guidelines
- Use async/await patterns for XR operations
- Implement proper object pooling for chess pieces
- Follow Unity's component-based architecture
- Use ScriptableObjects for game configuration
- Implement proper error handling for MR features

## Dependencies
- Meta XR All-in-One SDK
- XR Interaction Toolkit
- OpenXR Plugin
- Universal Render Pipeline

## Target Platform
- Android APK for Meta Quest devices
- Mixed Reality (passthrough) mode
- Hand tracking and controller support
