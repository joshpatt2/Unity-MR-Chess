using UnityEngine;
using UnityEditor;
using UnityEditor.XR.Management;
using UnityEngine.XR.Management;
using System.IO;
using System.Linq;

namespace MRChess.Editor
{
    /// <summary>
    /// Unity Editor validation tool for MR Chess Android build setup
    /// Accessible via: MR Chess > Validate Build Setup
    /// </summary>
    public class BuildSetupValidator : EditorWindow
    {
        private Vector2 scrollPosition;
        private bool hasValidated = false;
        private ValidationResult lastResult;

        [MenuItem("MR Chess/Validate Build Setup")]
        public static void ShowWindow()
        {
            GetWindow<BuildSetupValidator>("MR Chess - Build Setup Validation");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("🔧 MR Chess - Build Setup Validation", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox(
                "This tool validates your Unity project configuration for building Android APKs for Meta Quest devices.",
                MessageType.Info
            );

            EditorGUILayout.Space();

            if (GUILayout.Button("🔍 Validate Configuration", GUILayout.Height(30)))
            {
                ValidateSetup();
            }

            EditorGUILayout.Space();

            if (hasValidated && lastResult != null)
            {
                DisplayValidationResults();
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("🚀 Quick Actions", EditorStyles.boldLabel);
            
            if (GUILayout.Button("📱 Switch to Android Platform"))
            {
                SwitchToAndroidPlatform();
            }

            if (GUILayout.Button("⚙️ Open Player Settings"))
            {
                SettingsService.OpenProjectSettings("Project/Player");
            }

            if (GUILayout.Button("🥽 Open XR Management Settings"))
            {
                SettingsService.OpenProjectSettings("Project/XR Plug-in Management");
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "💡 For detailed setup instructions, see BUILD_INSTRUCTIONS.md in your project root.",
                MessageType.None
            );
        }

        private void ValidateSetup()
        {
            lastResult = new ValidationResult();
            
            // Platform check
            lastResult.isAndroidPlatform = EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android;
            
            // Player settings validation
            lastResult.bundleIdentifier = PlayerSettings.applicationIdentifier;
            lastResult.isValidBundleId = !string.IsNullOrEmpty(lastResult.bundleIdentifier) && 
                                       lastResult.bundleIdentifier != "com.DefaultCompany.MRChess";
            
            lastResult.minSdkVersion = PlayerSettings.Android.minSdkVersion;
            lastResult.targetSdkVersion = PlayerSettings.Android.targetSdkVersion;
            lastResult.isValidApiLevels = lastResult.minSdkVersion >= AndroidSdkVersions.AndroidApiLevel26 &&
                                         lastResult.targetSdkVersion >= AndroidSdkVersions.AndroidApiLevel33;

            lastResult.scriptingImplementation = PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android);
            lastResult.isIL2CPP = lastResult.scriptingImplementation == ScriptingImplementation.IL2CPP;

            lastResult.targetArchitecture = PlayerSettings.Android.targetArchitectures;
            lastResult.isARM64Only = lastResult.targetArchitecture == AndroidArchitecture.ARM64;

            // XR Management validation
            var xrSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Android);
            lastResult.hasXRManagement = xrSettings != null && xrSettings.Manager != null;
            
            if (lastResult.hasXRManagement)
            {
                var activeLoaders = xrSettings.Manager.activeLoaders;
                lastResult.hasOpenXR = activeLoaders.Any(loader => loader.GetType().Name.Contains("OpenXR"));
            }

            // Scene validation
            lastResult.hasChessScene = File.Exists("Assets/chess.unity");
            lastResult.sceneInBuildSettings = false;
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (scene.path == "Assets/chess.unity")
                {
                    lastResult.sceneInBuildSettings = true;
                    break;
                }
            }

            hasValidated = true;
        }

        private void DisplayValidationResults()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUILayout.LabelField("📋 Validation Results", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Platform Settings
            EditorGUILayout.LabelField("🎯 Platform Configuration", EditorStyles.boldLabel);
            DisplayCheck("Android Platform Selected", lastResult.isAndroidPlatform);
            
            EditorGUILayout.Space();

            // Player Settings
            EditorGUILayout.LabelField("⚙️ Player Settings", EditorStyles.boldLabel);
            DisplayCheck($"Valid Bundle Identifier ({lastResult.bundleIdentifier})", lastResult.isValidBundleId);
            DisplayCheck($"API Levels (Min: {lastResult.minSdkVersion}, Target: {lastResult.targetSdkVersion})", lastResult.isValidApiLevels);
            DisplayCheck("IL2CPP Scripting Backend", lastResult.isIL2CPP);
            DisplayCheck("ARM64 Architecture Only", lastResult.isARM64Only);

            EditorGUILayout.Space();

            // XR Settings
            EditorGUILayout.LabelField("🥽 XR Configuration", EditorStyles.boldLabel);
            DisplayCheck("XR Management Configured", lastResult.hasXRManagement);
            DisplayCheck("OpenXR Provider Enabled", lastResult.hasOpenXR);

            EditorGUILayout.Space();

            // Scene Settings
            EditorGUILayout.LabelField("🎮 Scene Configuration", EditorStyles.boldLabel);
            DisplayCheck("Chess Scene Exists", lastResult.hasChessScene);
            DisplayCheck("Scene Added to Build Settings", lastResult.sceneInBuildSettings);

            EditorGUILayout.Space();

            // Overall Status
            bool allValid = lastResult.isAndroidPlatform && lastResult.isValidBundleId && 
                           lastResult.isValidApiLevels && lastResult.isIL2CPP && 
                           lastResult.isARM64Only && lastResult.hasXRManagement && 
                           lastResult.hasOpenXR && lastResult.hasChessScene && 
                           lastResult.sceneInBuildSettings;

            if (allValid)
            {
                EditorGUILayout.HelpBox("✅ All checks passed! Your project is ready for Android builds.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("❌ Some issues found. Please resolve them before building.", MessageType.Warning);
                
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("🔧 Recommended Actions:", EditorStyles.boldLabel);
                
                if (!lastResult.isAndroidPlatform)
                    EditorGUILayout.LabelField("• Switch to Android platform in Build Settings");
                if (!lastResult.isValidBundleId)
                    EditorGUILayout.LabelField("• Set a valid Bundle Identifier in Player Settings");
                if (!lastResult.isValidApiLevels)
                    EditorGUILayout.LabelField("• Set Minimum API Level to 26+ and Target API Level to 33+");
                if (!lastResult.isIL2CPP)
                    EditorGUILayout.LabelField("• Set Scripting Backend to IL2CPP in Player Settings");
                if (!lastResult.isARM64Only)
                    EditorGUILayout.LabelField("• Enable only ARM64 architecture in Player Settings");
                if (!lastResult.hasXRManagement)
                    EditorGUILayout.LabelField("• Install and configure XR Management package");
                if (!lastResult.hasOpenXR)
                    EditorGUILayout.LabelField("• Enable OpenXR provider in XR Management");
                if (!lastResult.sceneInBuildSettings)
                    EditorGUILayout.LabelField("• Add chess.unity scene to Build Settings");
            }

            EditorGUILayout.EndScrollView();
        }

        private void DisplayCheck(string label, bool isValid)
        {
            GUIStyle style = new GUIStyle(EditorStyles.label);
            style.normal.textColor = isValid ? Color.green : Color.red;
            
            string icon = isValid ? "✅" : "❌";
            EditorGUILayout.LabelField($"{icon} {label}", style);
        }

        private void SwitchToAndroidPlatform()
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
                Debug.Log("✅ Switched to Android platform");
            }
            else
            {
                Debug.Log("✅ Already on Android platform");
            }
        }

        private class ValidationResult
        {
            public bool isAndroidPlatform;
            public string bundleIdentifier;
            public bool isValidBundleId;
            public AndroidSdkVersions minSdkVersion;
            public AndroidSdkVersions targetSdkVersion;
            public bool isValidApiLevels;
            public ScriptingImplementation scriptingImplementation;
            public bool isIL2CPP;
            public AndroidArchitecture targetArchitecture;
            public bool isARM64Only;
            public bool hasXRManagement;
            public bool hasOpenXR;
            public bool hasChessScene;
            public bool sceneInBuildSettings;
        }
    }
}
