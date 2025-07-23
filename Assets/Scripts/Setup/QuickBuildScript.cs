using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.IO;

public class QuickBuildScript
{
    [MenuItem("MR Chess/Quick Android Build")]
    public static void BuildAndroid()
    {
        // Set Android as target
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        
        // CRITICAL: Set architecture FIRST before other settings
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        
        // Basic player settings for Quest
        PlayerSettings.productName = "MR Chess";
        PlayerSettings.companyName = "MR Chess Team";
        PlayerSettings.bundleVersion = "1.0";
        PlayerSettings.Android.bundleVersionCode = 1;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24; // Fix ARCore issue
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
        
        // Fix Input System setting for Android VR
#if ENABLE_INPUT_SYSTEM
        EditorPrefs.SetInt("ActiveInputHandler", 2); // 2 = Input System Package (New)
#endif
        
        // VR Settings
        PlayerSettings.virtualRealitySupported = false; // We use XR Management instead
        
        Debug.Log("Android settings configured - ARM64 architecture set!");
        
        // Create scenes array
        string[] scenes = { "Assets/Scenes/SampleScene.unity" };
        
        // If we don't have scenes, create a minimal build
        if (!File.Exists("Assets/Scenes/SampleScene.unity"))
        {
            scenes = new string[0]; // Empty scenes - Unity will create a default scene
        }
        
        // Build options
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = scenes;
        buildPlayerOptions.locationPathName = "Builds/Development/MRChess-Quick.apk";
        buildPlayerOptions.target = BuildTarget.Android;
        buildPlayerOptions.options = BuildOptions.Development | BuildOptions.AllowDebugging;
        
        // Create build directory
        Directory.CreateDirectory("Builds/Development");
        
        // Build
        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Quick build succeeded: " + summary.totalSize + " bytes");
            Debug.Log("APK created at: " + buildPlayerOptions.locationPathName);
        }
        else
        {
            Debug.LogError("Quick build failed!");
        }
    }
}
