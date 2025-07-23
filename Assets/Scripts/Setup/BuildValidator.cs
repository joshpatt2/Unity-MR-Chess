#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.XR.Management;
using UnityEditor.XR.Management;
using System.IO;

namespace MRChess.Setup
{
    /// <summary>
    /// Build validation script to check for common Unity XR build issues
    /// </summary>
    public class BuildValidator : EditorWindow
    {
        private Vector2 scrollPosition;
        private static bool hasValidated = false;

        [MenuItem("MR Chess/Advanced Build Validator")]
        public static void ShowWindow()
        {
            var window = GetWindow<BuildValidator>("Build Validator");
            window.minSize = new Vector2(400, 600);
            window.Show();
        }

        [InitializeOnLoadMethod]
        private static void AutoValidateOnProjectLoad()
        {
            EditorApplication.delayCall += () =>
            {
                if (!hasValidated)
                {
                    ValidateBuildSetup(false);
                    hasValidated = true;
                }
            };
        }

        private void OnGUI()
        {
            GUILayout.Label("MR Chess Build Validator", EditorStyles.boldLabel);
            GUILayout.Space(10);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            if (GUILayout.Button("Run Full Validation", GUILayout.Height(30)))
            {
                ValidateBuildSetup(true);
            }

            GUILayout.Space(10);

            if (GUILayout.Button("Fix Android Settings", GUILayout.Height(25)))
            {
                AdvancedBuildScript.ConfigureAndroidSettings();
            }

            if (GUILayout.Button("Fix Common Issues", GUILayout.Height(25)))
            {
                AdvancedBuildScript.FixCommonBuildIssues();
            }

            GUILayout.Space(10);
            GUILayout.Label("Build Quick Actions:", EditorStyles.boldLabel);

            if (GUILayout.Button("Build Development APK", GUILayout.Height(30)))
            {
                AdvancedBuildScript.BuildDevelopmentAPK();
            }

            if (GUILayout.Button("Build Release APK", GUILayout.Height(30)))
            {
                AdvancedBuildScript.BuildReleaseAPK();
            }

            EditorGUILayout.EndScrollView();
        }

        public static void ValidateBuildSetup(bool verbose = true)
        {
            if (verbose) Debug.Log("🔍 Starting build validation...");

            bool hasErrors = false;
            bool hasWarnings = false;

            // Check Unity version
            if (verbose) Debug.Log($"Unity Version: {Application.unityVersion}");

            // Check build target
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                Debug.LogWarning("⚠️ Build target is not set to Android. Switch to Android in Build Settings.");
                hasWarnings = true;
            }

            // Check player settings
            ValidatePlayerSettings(ref hasErrors, ref hasWarnings, verbose);

            // Check XR settings
            ValidateXRSettings(ref hasErrors, ref hasWarnings, verbose);

            // Check scenes
            ValidateScenes(ref hasErrors, ref hasWarnings, verbose);

            // Check Android SDK
            ValidateAndroidSDK(ref hasErrors, ref hasWarnings, verbose);

            // Check required packages
            ValidatePackages(ref hasErrors, ref hasWarnings, verbose);

            // Summary
            if (verbose)
            {
                if (!hasErrors && !hasWarnings)
                {
                    Debug.Log("✅ Build validation passed! Ready to build.");
                }
                else if (hasErrors)
                {
                    Debug.LogError("❌ Build validation failed. Please fix errors before building.");
                }
                else
                {
                    Debug.LogWarning("⚠️ Build validation passed with warnings. Consider fixing warnings for optimal builds.");
                }
            }
        }

        /// <summary>
        /// Parameterless wrapper for Unity batchmode execution
        /// </summary>
        public static void ValidateBuildSetupBatch()
        {
            ValidateBuildSetup(true);
        }

        private static void ValidatePlayerSettings(ref bool hasErrors, ref bool hasWarnings, bool verbose)
        {
            // Check architecture
            if (PlayerSettings.Android.targetArchitectures != AndroidArchitecture.ARM64)
            {
                Debug.LogError("❌ Target architecture must be ARM64 for Quest devices.");
                hasErrors = true;
            }
            else if (verbose)
            {
                Debug.Log("✅ Architecture: ARM64");
            }

            // Check SDK versions
            if ((int)PlayerSettings.Android.minSdkVersion < 26)
            {
                Debug.LogError("❌ Minimum SDK version must be 26 (Android 8.0) or higher for Quest.");
                hasErrors = true;
            }
            else if (verbose)
            {
                Debug.Log($"✅ Min SDK: {PlayerSettings.Android.minSdkVersion}");
            }

            // Check bundle identifier
            var bundleId = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android);
            if (string.IsNullOrEmpty(bundleId) || bundleId == "com.DefaultCompany.DefaultProduct")
            {
                Debug.LogWarning("⚠️ Bundle identifier should be changed from default value.");
                hasWarnings = true;
            }
            else if (verbose)
            {
                Debug.Log($"✅ Bundle ID: {bundleId}");
            }

            // Check VR support - Note: virtualRealitySupported is obsolete
            // Legacy VR support should be disabled in favor of XR Management
            // if (PlayerSettings.virtualRealitySupported) // Obsolete API
            // {
            //     Debug.LogWarning("⚠️ Legacy VR support is enabled. Use XR Management instead.");
            //     hasWarnings = true;
            // }
        }

        private static void ValidateXRSettings(ref bool hasErrors, ref bool hasWarnings, bool verbose)
        {
            var xrGeneralSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Android);
            
            if (xrGeneralSettings == null)
            {
                Debug.LogError("❌ XR Management settings not found. Install XR Management package.");
                hasErrors = true;
                return;
            }

            if (!xrGeneralSettings.InitManagerOnStart)
            {
                Debug.LogWarning("⚠️ XR Manager should initialize on start for VR apps.");
                hasWarnings = true;
            }

            if (verbose)
            {
                Debug.Log("✅ XR Management configured");
            }
        }

        private static void ValidateScenes(ref bool hasErrors, ref bool hasWarnings, bool verbose)
        {
            var scenes = EditorBuildSettings.scenes;
            
            if (scenes.Length == 0)
            {
                Debug.LogError("❌ No scenes in build settings.");
                hasErrors = true;
            }
            else
            {
                bool hasEnabledScene = false;
                foreach (var scene in scenes)
                {
                    if (scene.enabled)
                    {
                        hasEnabledScene = true;
                        if (!File.Exists(scene.path))
                        {
                            Debug.LogError($"❌ Scene not found: {scene.path}");
                            hasErrors = true;
                        }
                    }
                }

                if (!hasEnabledScene)
                {
                    Debug.LogError("❌ No enabled scenes in build settings.");
                    hasErrors = true;
                }
                else if (verbose)
                {
                    Debug.Log($"✅ Scenes: {scenes.Length} total, enabled scenes found");
                }
            }
        }

        private static void ValidateAndroidSDK(ref bool hasErrors, ref bool hasWarnings, bool verbose)
        {
            var androidSdkPath = EditorPrefs.GetString("AndroidSdkRoot");
            if (string.IsNullOrEmpty(androidSdkPath))
            {
                Debug.LogError("❌ Android SDK path not configured. Set in Unity Preferences > External Tools.");
                hasErrors = true;
            }
            else if (verbose)
            {
                Debug.Log("✅ Android SDK path configured");
            }

            var jdkPath = EditorPrefs.GetString("JdkPath");
            if (string.IsNullOrEmpty(jdkPath))
            {
                Debug.LogWarning("⚠️ JDK path not configured. Set in Unity Preferences > External Tools.");
                hasWarnings = true;
            }
            else if (verbose)
            {
                Debug.Log("✅ JDK path configured");
            }
        }

        private static void ValidatePackages(ref bool hasErrors, ref bool hasWarnings, bool verbose)
        {
            string[] requiredPackages = {
                "com.unity.xr.interaction.toolkit",
                "com.unity.xr.openxr",
                "com.unity.render-pipelines.universal",
                "com.unity.inputsystem"
            };

            foreach (var package in requiredPackages)
            {
                var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssetPath($"Packages/{package}");
                if (packageInfo == null)
                {
                    Debug.LogError($"❌ Required package missing: {package}");
                    hasErrors = true;
                }
                else if (verbose)
                {
                    Debug.Log($"✅ Package: {package} v{packageInfo.version}");
                }
            }
        }
    }
}
#endif
