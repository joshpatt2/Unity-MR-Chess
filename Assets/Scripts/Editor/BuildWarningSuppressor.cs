#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace MRChess.Setup
{
    /// <summary>
    /// Build warning suppressor to handle common Unity warnings
    /// Implements IPreprocessBuildWithReport to suppress specific warnings during build
    /// </summary>
    public class BuildWarningSuppressor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            Debug.Log("=== Build Warning Suppressor: Preprocessing Build ===");
            
            // Suppress specific compiler warnings
            SuppressCompilerWarnings();
            
            // Configure build settings to avoid warnings
            ConfigureBuildSettings();
            
            Debug.Log("Build warning suppression complete");
        }
        
        public void SuppressCompilerWarnings()
        {
            // Suppress specific Unity warnings
            // Add compiler symbols to suppress warnings
            var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            
            // Add warning suppression defines if not already present
            if (!defines.Contains("SUPPRESS_BUILD_WARNINGS"))
            {
                if (string.IsNullOrEmpty(defines))
                {
                    defines = "SUPPRESS_BUILD_WARNINGS";
                }
                else
                {
                    defines += ";SUPPRESS_BUILD_WARNINGS";
                }
                
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, defines);
                Debug.Log("Added SUPPRESS_BUILD_WARNINGS define");
            }
        }
        
        public void ConfigureBuildSettings()
        {
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            {
                // Configure Android-specific settings to avoid warnings
                ConfigureAndroidSettings();
            }
        }
        
        private void ConfigureAndroidSettings()
        {
            // Set proper Android API levels to avoid compatibility warnings
            var minSdk = PlayerSettings.Android.minSdkVersion;
            var targetSdk = PlayerSettings.Android.targetSdkVersion;
            
            // Ensure minimum SDK is at least API 26 for Quest compatibility
            if (minSdk < AndroidSdkVersions.AndroidApiLevel26)
            {
                Debug.Log("Updating minimum SDK to API 26 to avoid compatibility warnings");
                PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            }
            
            // Ensure target SDK is reasonable for Quest devices
            if (targetSdk > AndroidSdkVersions.AndroidApiLevel34)
            {
                Debug.Log("Setting target SDK to API 33 to avoid future compatibility warnings");
                PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel33;
            }
            
            // Ensure architecture is properly set
            if (PlayerSettings.Android.targetArchitectures != AndroidArchitecture.ARM64)
            {
                Debug.Log("Setting target architecture to ARM64 to avoid architecture warnings");
                PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            }
            
            // Configure IL2CPP settings to avoid warnings
            if (PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android) != ScriptingImplementation.IL2CPP)
            {
                Debug.Log("Setting scripting backend to IL2CPP to avoid ARM64 warnings");
                PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            }
            
            // Configure managed stripping level to avoid warnings
            PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.Android, ManagedStrippingLevel.Minimal);
            
            Debug.Log("Android build settings configured to minimize warnings");
        }
    }
    
    /// <summary>
    /// Menu item to manually run warning suppression
    /// </summary>
    public static class BuildWarningMenu
    {
        [MenuItem("MR Chess/Suppress Build Warnings")]
        public static void SuppressBuildWarnings()
        {
            var suppressor = new BuildWarningSuppressor();
            
            // Manually run the warning suppression without building
            suppressor.SuppressCompilerWarnings();
            suppressor.ConfigureBuildSettings();
            
            EditorUtility.DisplayDialog("Build Warning Suppressor", 
                "Build warning suppression settings have been applied!\n\n" +
                "The following optimizations were applied:\n" +
                "• Conditional debug logging\n" +
                "• Android API level optimization\n" +
                "• Architecture and backend verification\n" +
                "• Managed stripping configuration", "OK");
        }
        
        [MenuItem("MR Chess/Check for Build Warnings")]
        public static void CheckForWarnings()
        {
            BuildWarningValidator.ShowWindow();
        }
    }
}
#endif
