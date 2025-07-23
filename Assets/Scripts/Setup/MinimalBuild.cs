#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using System.IO;

public class MinimalBuild
{
    public static void BuildAPK()
    {
        Debug.Log("=== MinimalBuild.BuildAPK Starting ===");
        
        // Create build directory
        Directory.CreateDirectory("Builds/Development");
        
        // Build options
        BuildPlayerOptions buildOptions = new BuildPlayerOptions();
        buildOptions.scenes = new string[] { "Assets/chess.unity" };
        buildOptions.locationPathName = "Builds/Development/MRChess-Minimal.apk";
        buildOptions.target = BuildTarget.Android;
        buildOptions.targetGroup = BuildTargetGroup.Android;
        buildOptions.options = BuildOptions.Development;
        
        Debug.Log("Starting minimal build...");
        BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
        
        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log("✅ MINIMAL BUILD SUCCEEDED!");
            Debug.Log("APK created at: " + buildOptions.locationPathName);
        }
        else
        {
            Debug.LogError("❌ MINIMAL BUILD FAILED!");
            Debug.LogError("Result: " + report.summary.result);
        }
        
        Debug.Log("=== MinimalBuild.BuildAPK Complete ===");
    }
}
#endif
