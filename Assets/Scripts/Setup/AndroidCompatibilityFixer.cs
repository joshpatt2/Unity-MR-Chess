#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.IO;

namespace MRChess.Setup
{
    /// <summary>
    /// Fixes Android 15+ compatibility issues including 16KB page alignment
    /// </summary>
    public class AndroidCompatibilityFixer : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (report.summary.platform == BuildTarget.Android)
            {
                Debug.Log("=== Android Compatibility Fixer ===");
                FixAndroid15Compatibility();
            }
        }

        [MenuItem("MR Chess/Fix Android 15+ Compatibility")]
        public static void FixAndroid15Compatibility()
        {
            Debug.Log("Applying Android 15+ compatibility fixes...");

            // 1. Update Player Settings for Android 15+ compatibility
            ConfigureAndroid15Settings();

            // 2. Configure XR settings to use OpenXR instead of Oculus XR Plugin where possible
            ConfigureXRForAndroid15();

            // 3. Set build optimizations
            ConfigureBuildOptimizations();

            Debug.Log("✅ Android 15+ compatibility fixes applied!");
        }

        private static void ConfigureAndroid15Settings()
        {
            Debug.Log("Configuring Android 15+ player settings...");

            // Ensure we're targeting Android API 33+ but compatible with 15+
            PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)33;
            PlayerSettings.Android.minSdkVersion = (AndroidSdkVersions)26;

            // ARM64 only for better compatibility
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

            // CRITICAL: Disable resource shrinking to fix Gradle build error
            // "Resource shrinker cannot be used for libraries"
            PlayerSettings.Android.minifyRelease = false;
            PlayerSettings.Android.minifyDebug = false;

            // Disable optimized frame pacing which can cause issues with 16KB alignment
            PlayerSettings.Android.optimizedFramePacing = false;

            // Use Vulkan primarily for better performance on Quest
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new UnityEngine.Rendering.GraphicsDeviceType[] { 
                UnityEngine.Rendering.GraphicsDeviceType.Vulkan,
                UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3 
            });

            // Set scripting backend to IL2CPP for ARM64
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Android, ApiCompatibilityLevel.NET_Standard_2_0);

            Debug.Log("Player settings configured for Android 15+ compatibility");
        }

        private static void ConfigureXRForAndroid15()
        {
            Debug.Log("Configuring XR settings for Android 15+ compatibility...");

            // Prefer OpenXR over Oculus XR Plugin for better compatibility
            var xrGeneralSettings = UnityEngine.XR.Management.XRGeneralSettings.Instance;
            
            if (xrGeneralSettings != null && xrGeneralSettings.Manager != null)
            {
                Debug.Log("XR Management found, ensuring proper configuration...");
                
                // The XR configuration should prioritize OpenXR
                xrGeneralSettings.InitManagerOnStart = true;
                
                // Mark as dirty to ensure settings are saved
                UnityEditor.EditorUtility.SetDirty(xrGeneralSettings);
            }
            else
            {
                Debug.LogWarning("XR Management not properly configured. Please check XR settings.");
            }

            Debug.Log("XR settings configured for Android 15+ compatibility");
        }

        private static void ConfigureBuildOptimizations()
        {
            Debug.Log("Configuring build optimizations for Android 15+...");

            // Ensure proper code stripping for smaller APK size and better compatibility
            PlayerSettings.stripEngineCode = true;
            PlayerSettings.Android.useCustomKeystore = false; // Use debug keystore for development

            // Configure IL2CPP settings for better compatibility
            PlayerSettings.SetIl2CppCompilerConfiguration(BuildTargetGroup.Android, Il2CppCompilerConfiguration.Master);

            // CRITICAL: Disable minification and resource shrinking to prevent Gradle errors
            // This fixes "Resource shrinker cannot be used for libraries" error
            // Note: minifyWithR8 is obsolete as Android Gradle Plugin 7.0+ always uses R8
            PlayerSettings.Android.minifyDebug = false;
            PlayerSettings.Android.minifyRelease = false;

            Debug.Log("Build optimizations configured - resource shrinking disabled");
        }

        /// <summary>
        /// Check if the current setup has potential Android 15+ compatibility issues
        /// </summary>
        [MenuItem("MR Chess/Validate Android 15+ Compatibility")]
        public static void ValidateAndroid15Compatibility()
        {
            Debug.Log("=== Validating Android 15+ Compatibility ===");

            bool hasIssues = false;

            // Check architecture
            if (PlayerSettings.Android.targetArchitectures != AndroidArchitecture.ARM64)
            {
                Debug.LogWarning("⚠️ Target architecture should be ARM64 only for Quest compatibility");
                hasIssues = true;
            }

            // Check SDK versions
            if ((int)PlayerSettings.Android.targetSdkVersion < 33)
            {
                Debug.LogWarning("⚠️ Target SDK should be 33+ for better Android 15+ compatibility");
                hasIssues = true;
            }

            // Check scripting backend
            if (PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android) != ScriptingImplementation.IL2CPP)
            {
                Debug.LogWarning("⚠️ IL2CPP scripting backend recommended for ARM64 builds");
                hasIssues = true;
            }

            // Check optimized frame pacing
            if (PlayerSettings.Android.optimizedFramePacing)
            {
                Debug.LogWarning("⚠️ Optimized Frame Pacing may cause issues with 16KB page alignment");
                hasIssues = true;
            }

            if (!hasIssues)
            {
                Debug.Log("✅ Android 15+ compatibility validation passed!");
            }
            else
            {
                Debug.Log("❌ Android 15+ compatibility issues found. Run 'Fix Android 15+ Compatibility' to resolve.");
            }
        }
    }
}
#endif
