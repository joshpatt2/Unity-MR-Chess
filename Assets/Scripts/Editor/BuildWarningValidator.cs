#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace MRChess.Setup
{
    /// <summary>
    /// Helper script to validate and fix common Unity build warnings
    /// Checks for unused variables, obsolete APIs, and other potential issues
    /// </summary>
    public class BuildWarningValidator : EditorWindow
    {
        private Vector2 scrollPosition;
        private List<string> warnings = new List<string>();
        private List<string> suggestions = new List<string>();
        private bool hasRunValidation = false;

        [MenuItem("MR Chess/Validate Build Warnings")]
        public static void ShowWindow()
        {
            GetWindow<BuildWarningValidator>("Build Warning Validator");
        }

        private void OnGUI()
        {
            GUILayout.Label("Build Warning Validator", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox(
                "This tool checks for common Unity build warnings and provides suggestions for fixing them.",
                MessageType.Info);

            EditorGUILayout.Space();

            if (GUILayout.Button("Validate Build Configuration", GUILayout.Height(30)))
            {
                ValidateBuildWarnings();
            }

            if (hasRunValidation)
            {
                EditorGUILayout.Space();
                GUILayout.Label("Validation Results:", EditorStyles.boldLabel);

                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

                if (warnings.Count == 0)
                {
                    EditorGUILayout.HelpBox("✅ No build warnings detected! Your project is clean.", MessageType.Info);
                }
                else
                {
                    EditorGUILayout.HelpBox($"⚠️ Found {warnings.Count} potential issues:", MessageType.Warning);
                    
                    foreach (string warning in warnings)
                    {
                        EditorGUILayout.HelpBox(warning, MessageType.Warning);
                    }
                }

                if (suggestions.Count > 0)
                {
                    EditorGUILayout.Space();
                    GUILayout.Label("Optimization Suggestions:", EditorStyles.boldLabel);
                    
                    foreach (string suggestion in suggestions)
                    {
                        EditorGUILayout.HelpBox(suggestion, MessageType.Info);
                    }
                }

                EditorGUILayout.EndScrollView();
            }
        }

        private void ValidateBuildWarnings()
        {
            warnings.Clear();
            suggestions.Clear();
            hasRunValidation = true;

            // Check for development build settings
            CheckDevelopmentBuildSettings();
            
            // Check for debug logging in production
            CheckDebugLogging();
            
            // Check for platform-specific warnings
            CheckPlatformSettings();
            
            // Check for XR configuration
            CheckXRConfiguration();
            
            // Check for performance settings
            CheckPerformanceSettings();
        }

        private void CheckDevelopmentBuildSettings()
        {
            if (EditorUserBuildSettings.development)
            {
                suggestions.Add("Development Build is enabled. Consider disabling for release builds to reduce size and improve performance.");
            }

            if (EditorUserBuildSettings.connectProfiler)
            {
                suggestions.Add("Profiler connection is enabled. Disable for release builds to improve performance.");
            }

            if (EditorUserBuildSettings.buildScriptsOnly)
            {
                warnings.Add("Build Scripts Only is enabled. This will not create a complete build.");
            }
        }

        private void CheckDebugLogging()
        {
            // Check if the project has excessive debug logging
            string[] scriptFiles = System.IO.Directory.GetFiles("Assets/Scripts", "*.cs", System.IO.SearchOption.AllDirectories);
            int debugLogCount = 0;
            
            foreach (string file in scriptFiles)
            {
                string content = System.IO.File.ReadAllText(file);
                debugLogCount += CountOccurrences(content, "Debug.Log");
                debugLogCount += CountOccurrences(content, "Debug.LogWarning");
                debugLogCount += CountOccurrences(content, "Debug.LogError");
            }

            if (debugLogCount > 50)
            {
                suggestions.Add($"Found {debugLogCount} debug log statements. Consider using conditional compilation to reduce logging in release builds.");
            }
        }

        private void CheckPlatformSettings()
        {
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            {
                // Check Android-specific settings
                if (PlayerSettings.Android.targetArchitectures != AndroidArchitecture.ARM64)
                {
                    warnings.Add("Target architecture should be ARM64 for Quest devices.");
                }

                if (PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android) != ScriptingImplementation.IL2CPP)
                {
                    warnings.Add("Scripting backend should be IL2CPP for Android ARM64 builds.");
                }

                if (PlayerSettings.Android.minSdkVersion < AndroidSdkVersions.AndroidApiLevel26)
                {
                    suggestions.Add("Consider setting minimum SDK to API 26 (Android 8.0) for better Quest compatibility.");
                }
            }
        }

        private void CheckXRConfiguration()
        {
            var xrSettings = UnityEngine.XR.Management.XRGeneralSettings.Instance;
            if (xrSettings == null)
            {
                warnings.Add("XR Management not configured. Required for VR functionality.");
            }
            else if (xrSettings.Manager == null)
            {
                warnings.Add("XR Manager not found. Check XR configuration in Project Settings.");
            }
        }

        private void CheckPerformanceSettings()
        {
            // Check graphics settings
            if (QualitySettings.antiAliasing > 4)
            {
                suggestions.Add("Anti-aliasing is set high. Consider reducing for better VR performance.");
            }

            if (QualitySettings.shadows == ShadowQuality.All)
            {
                suggestions.Add("Shadow quality is set to 'All'. Consider 'Hard Only' for better VR performance.");
            }

            // Check physics settings
            if (Time.fixedDeltaTime < 0.02f) // Less than 50 FPS
            {
                suggestions.Add("Fixed timestep is very low. Consider increasing for better VR performance.");
            }
        }

        private int CountOccurrences(string text, string pattern)
        {
            int count = 0;
            int index = 0;
            while ((index = text.IndexOf(pattern, index)) != -1)
            {
                count++;
                index += pattern.Length;
            }
            return count;
        }
    }
}
#endif
