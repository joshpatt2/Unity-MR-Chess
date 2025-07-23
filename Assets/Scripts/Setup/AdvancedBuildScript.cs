#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine.XR.Management;
using UnityEditor.XR.Management;
using System.IO;
using System.Linq;

namespace MRChess.Setup
{
    /// <summary>
    /// Advanced build script for MR Chess with comprehensive Android and XR setup
    /// </summary>
    public class AdvancedBuildScript
    {
        private const string PRODUCT_NAME = "MR Chess";
        private const string COMPANY_NAME = "MR Chess Team";
        private const string BUNDLE_ID = "com.mrchessteam.mrchess";
        private const int TARGET_SDK_VERSION = 33; // Android 13
        private const int MIN_SDK_VERSION = 26; // Android 8.0 (required for Quest)

        /// <summary>
        /// Simple method for batchmode execution
        /// </summary>
        [MenuItem("MR Chess/Build Development APK")]
        public static void BuildDevelopmentAPK()
        {
            Debug.Log("=== MR Chess Development Build Starting ===");
            BuildAPK(true);
        }

        /// <summary>
        /// Simple method for batchmode execution
        /// </summary>
        [MenuItem("MR Chess/Build Release APK")]
        public static void BuildReleaseAPK()
        {
            Debug.Log("=== MR Chess Release Build Starting ===");
            BuildAPK(false);
        }

        [MenuItem("MR Chess/Configure Android Settings")]
        public static void ConfigureAndroidSettings()
        {
            ConfigurePlayerSettings();
            ConfigureXRSettings();
            Debug.Log("Android settings configured successfully!");
        }

        public static void BuildAPK(bool isDevelopment = true)
        {
            try
            {
                Debug.Log($"Starting {(isDevelopment ? "Development" : "Release")} build...");

                // Configure settings
                ConfigurePlayerSettings();
                ConfigureXRSettings();

                // Set build target
                if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
                {
                    Debug.Log("Switching to Android build target...");
                    EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
                }

                // Prepare build options
                var buildOptions = PrepareBuildOptions(isDevelopment);

                // Create build directory
                var buildDir = Path.GetDirectoryName(buildOptions.locationPathName);
                Directory.CreateDirectory(buildDir);

                // Execute build
                Debug.Log($"Building to: {buildOptions.locationPathName}");
                BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
                
                // Handle build result
                HandleBuildResult(report);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Build failed with exception: {e.Message}");
                Debug.LogError($"Stack trace: {e.StackTrace}");
            }
        }

        private static void ConfigurePlayerSettings()
        {
            Debug.Log("Configuring player settings...");

            // Basic settings
            PlayerSettings.productName = PRODUCT_NAME;
            PlayerSettings.companyName = COMPANY_NAME;
            PlayerSettings.bundleVersion = "1.0.0";
            
            // Android specific settings
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, BUNDLE_ID);
            PlayerSettings.Android.bundleVersionCode = 1;
            PlayerSettings.Android.minSdkVersion = (AndroidSdkVersions)MIN_SDK_VERSION;
            PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)TARGET_SDK_VERSION;
            
            // Architecture - CRITICAL for Quest and Android 15+ compatibility
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            
            // CRITICAL: Disable resource shrinking to prevent Gradle build errors
            // "Resource shrinker cannot be used for libraries"
            PlayerSettings.Android.minifyRelease = false;
            PlayerSettings.Android.minifyDebug = false;
            
            // Android 15+ compatibility settings for 16KB page alignment
            PlayerSettings.Android.optimizedFramePacing = false; // Disable for better compatibility
            
            // Graphics and rendering
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new UnityEngine.Rendering.GraphicsDeviceType[] { 
                UnityEngine.Rendering.GraphicsDeviceType.Vulkan,
                UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3 
            });
            
            // XR Settings - Use XR Management instead of legacy VR support
            // PlayerSettings.virtualRealitySupported = false; // Obsolete - handled by XR Management
            
            // Input System - Note: SetConfigurationData is not available in newer Unity versions
            // Input system configuration is handled through Project Settings instead
            
            // Performance settings for Quest
            PlayerSettings.Android.blitType = AndroidBlitType.Always;
            PlayerSettings.Android.startInFullscreen = true;
            PlayerSettings.Android.renderOutsideSafeArea = true;
            
            // Build optimization settings - CRITICAL: Disable resource shrinking to fix Gradle errors
            PlayerSettings.Android.useCustomKeystore = false; // Use debug keystore for development
            // Note: minifyWithR8 is obsolete as Android Gradle Plugin 7.0+ always uses R8
            PlayerSettings.Android.minifyDebug = false; // Disable debug minification
            PlayerSettings.Android.minifyRelease = false; // Disable release minification
            
            // Permissions for Quest
            PlayerSettings.Android.forceInternetPermission = false;
            PlayerSettings.Android.forceSDCardPermission = false;
            
            Debug.Log("Player settings configured!");
        }

        private static void ConfigureXRSettings()
        {
            Debug.Log("Configuring XR settings...");

            // Get XR General Settings
            var xrGeneralSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Android);
            if (xrGeneralSettings == null)
            {
                Debug.LogWarning("XR General Settings not found. Please ensure XR Management is properly installed.");
                return;
            }

            // Initialize XR on startup
            xrGeneralSettings.InitManagerOnStart = true;

            Debug.Log("XR settings configured!");
        }

        private static BuildPlayerOptions PrepareBuildOptions(bool isDevelopment)
        {
            var buildType = isDevelopment ? "Development" : "Release";
            var apkName = isDevelopment ? "MRChess-Dev.apk" : "MRChess.apk";
            var buildPath = $"Builds/{buildType}/{apkName}";

            var scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            if (scenes.Length == 0)
            {
                Debug.LogWarning("No scenes found in build settings. Adding main scene...");
                scenes = new[] { "Assets/chess.unity" };
            }

            var buildOptions = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = buildPath,
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android
            };

            if (isDevelopment)
            {
                buildOptions.options = BuildOptions.Development | 
                                     BuildOptions.AllowDebugging |
                                     BuildOptions.ConnectWithProfiler;
            }
            else
            {
                buildOptions.options = BuildOptions.None;
            }

            return buildOptions;
        }

        private static void HandleBuildResult(BuildReport report)
        {
            var summary = report.summary;
            
            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"✅ Build succeeded!");
                Debug.Log($"📦 APK Size: {summary.totalSize / (1024 * 1024)} MB");
                Debug.Log($"📍 Location: {summary.outputPath}");
                Debug.Log($"⏱️ Build Time: {summary.totalTime.TotalSeconds:F1} seconds");
                
                // Show in Finder (macOS)
                EditorUtility.RevealInFinder(summary.outputPath);
                
                Debug.Log("\n🎯 Next Steps:");
                Debug.Log("1. Connect Meta Quest via USB");
                Debug.Log("2. Enable Developer Mode and USB Debugging");
                Debug.Log($"3. Install: adb install \"{summary.outputPath}\"");
                Debug.Log("4. Launch from Unknown Sources in Quest library");
            }
            else
            {
                Debug.LogError($"❌ Build failed: {summary.result}");
                
                if (report.steps != null)
                {
                    foreach (var step in report.steps)
                    {
                        if (step.messages != null)
                        {
                            foreach (var message in step.messages)
                            {
                                if (message.type == LogType.Error || message.type == LogType.Exception)
                                {
                                    Debug.LogError($"Build Error: {message.content}");
                                }
                            }
                        }
                    }
                }
            }
        }

        [MenuItem("MR Chess/Fix Common Build Issues")]
        public static void FixCommonBuildIssues()
        {
            Debug.Log("Fixing common build issues...");

            // Fix Input System configuration
            var inputSystemSettings = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>("ProjectSettings/InputManager.asset");
            if (inputSystemSettings != null)
            {
                Debug.Log("✅ Input System settings found");
            }

            // Ensure Android SDK path is set
            var androidSdkPath = EditorPrefs.GetString("AndroidSdkRoot");
            if (string.IsNullOrEmpty(androidSdkPath))
            {
                Debug.LogWarning("⚠️ Android SDK path not set. Please configure in Unity Preferences > External Tools");
            }
            else
            {
                Debug.Log($"✅ Android SDK path: {androidSdkPath}");
            }

            // Check Java/JDK path
            var jdkPath = EditorPrefs.GetString("JdkPath");
            if (string.IsNullOrEmpty(jdkPath))
            {
                Debug.LogWarning("⚠️ JDK path not set. Please configure in Unity Preferences > External Tools");
            }
            else
            {
                Debug.Log($"✅ JDK path: {jdkPath}");
            }

            Debug.Log("Build issue check complete!");
        }
    }
}
#endif
