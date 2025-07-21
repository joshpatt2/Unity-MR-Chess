# Package Dependencies

This file lists the Unity packages required for the MR Chess project.

## Core Unity Packages
- com.unity.render-pipelines.universal
- com.unity.xr.interaction.toolkit
- com.unity.xr.openxr
- com.unity.xr.arfoundation
- com.unity.xr.arcore
- com.unity.inputsystem

## Meta Quest Packages
- Meta XR All-in-One SDK (from Asset Store)
- Meta XR Core SDK
- Meta XR Interaction SDK

## Installation Instructions

### Via Package Manager (Window > Package Manager):
1. Switch to "Unity Registry"
2. Search for and install each package above
3. Enable XR Interaction Toolkit samples if needed

### Meta XR SDK:
1. Open Asset Store in Unity
2. Search for "Meta XR All-in-One SDK"
3. Download and import
4. Follow Meta's setup wizard

### Manual Package Installation:
If needed, you can add packages via manifest.json in the Packages folder:

```json
{
  "dependencies": {
    "com.unity.render-pipelines.universal": "14.0.8",
    "com.unity.xr.interaction.toolkit": "2.4.3",
    "com.unity.xr.openxr": "1.8.2",
    "com.unity.xr.arfoundation": "5.0.7",
    "com.unity.inputsystem": "1.6.3"
  }
}
```
