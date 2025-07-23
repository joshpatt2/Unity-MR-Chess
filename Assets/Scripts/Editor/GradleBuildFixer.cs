#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

namespace MRChess.Setup
{
    /// <summary>
    /// Gradle build configuration fixer for Unity MR Chess project
    /// Addresses common Android build issues, especially resource shrinking problems
    /// </summary>
    public static class GradleBuildFixer
    {
        [MenuItem("MR Chess/Clean Android Build Cache")]
        public static void CleanAndroidBuildCache()
        {
            var beePath = Path.Combine(Application.dataPath, "..", "Library", "Bee", "Android");
            
            if (Directory.Exists(beePath))
            {
                try
                {
                    Directory.Delete(beePath, true);
                    Debug.Log("✅ Android build cache cleaned successfully");
                    
                    EditorUtility.DisplayDialog("Build Cache Cleaned", 
                        "Android build cache has been cleaned.\n\n" +
                        "This will force Unity to regenerate all Gradle files on the next build.", "OK");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Failed to clean build cache: {ex.Message}");
                    EditorUtility.DisplayDialog("Clean Failed", 
                        $"Could not clean the build cache:\n\n{ex.Message}", "OK");
                }
            }
            else
            {
                Debug.Log("ℹ️ No Android build cache found to clean");
            }
        }
        
        [MenuItem("MR Chess/Fix All Android Issues")]
        public static void FixAllAndroidIssues()
        {
            Debug.Log("=== Comprehensive Android Build Fix ===");
            
            // 1. Fix Android SDK path issues
            AndroidSDKPathFixer.ValidateAndFixAndroidSDKPath();
            
            // 2. Fix Gradle configuration issues
            AndroidSDKPathFixer.ValidateGradleConfiguration();
            
            // 3. Disable resource shrinking in all templates
            DisableResourceShrinking();
            
            // 4. Fix applicationId issues
            FixApplicationIdIssues();
            
            // 5. Clean build cache to force regeneration
            CleanAndroidBuildCache();
            
            // 6. Refresh assets
            AssetDatabase.Refresh();
            
            Debug.Log("✅ All Android build fixes applied");
            EditorUtility.DisplayDialog("Android Build Fix Complete", 
                "All known Android build issues have been fixed:\n\n" +
                "• Android SDK path validated\n" +
                "• Gradle templates corrected\n" +
                "• Resource shrinking disabled\n" +
                "• ApplicationId issues fixed\n" +
                "• Build cache cleaned\n" +
                "• Assets refreshed\n\n" +
                "You can now try building again.", "OK");
        }
        
        [MenuItem("MR Chess/Fix ApplicationId Issues")]
        public static void FixApplicationIdIssues()
        {
            Debug.Log("=== Fixing ApplicationId Issues ===");
            
            var templatesPath = Path.Combine(Application.dataPath, "Plugins", "Android");
            var unityLibraryFiles = new string[]
            {
                Path.Combine(templatesPath, "unityLibrary.gradle"),
                Path.Combine(templatesPath, "unityLibraryTemplate.gradle")
            };
            
            int filesFixed = 0;
            
            foreach (var gradleFile in unityLibraryFiles)
            {
                if (File.Exists(gradleFile))
                {
                    try
                    {
                        var content = File.ReadAllText(gradleFile);
                        var originalContent = content;
                        bool changed = false;
                        
                        // Remove any applicationId lines from Unity library templates
                        var lines = content.Split('\n');
                        var cleanedLines = new System.Collections.Generic.List<string>();
                        
                        foreach (var line in lines)
                        {
                            if (line.Trim().StartsWith("applicationId"))
                            {
                                Debug.Log($"Removing applicationId line from {Path.GetFileName(gradleFile)}: {line.Trim()}");
                                changed = true;
                                // Skip this line
                                continue;
                            }
                            cleanedLines.Add(line);
                        }
                        
                        if (changed)
                        {
                            content = string.Join("\n", cleanedLines);
                            File.WriteAllText(gradleFile, content);
                            Debug.Log($"✅ Removed applicationId from: {Path.GetFileName(gradleFile)}");
                            filesFixed++;
                        }
                        else
                        {
                            Debug.Log($"ℹ️ No applicationId found in: {Path.GetFileName(gradleFile)}");
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"Failed to process {Path.GetFileName(gradleFile)}: {ex.Message}");
                    }
                }
            }
            
            if (filesFixed > 0)
            {
                Debug.Log($"✅ ApplicationId issues fixed in {filesFixed} file(s)");
                AssetDatabase.Refresh();
                
                EditorUtility.DisplayDialog("ApplicationId Issues Fixed", 
                    $"ApplicationId has been removed from {filesFixed} Unity library template(s).\n\n" +
                    "Unity libraries cannot have applicationId - only applications can.", "OK");
            }
            else
            {
                Debug.Log("✅ No applicationId issues found in Unity library templates");
            }
            
            Debug.Log("=== ApplicationId Fix Complete ===");
        }
        
        [MenuItem("MR Chess/Disable Resource Shrinking")]
        public static void DisableResourceShrinking()
        {
            Debug.Log("=== Disabling Resource Shrinking in All Templates ===");
            
            var templatesPath = Path.Combine(Application.dataPath, "Plugins", "Android");
            var gradleFiles = new string[]
            {
                Path.Combine(templatesPath, "mainTemplate.gradle"),
                Path.Combine(templatesPath, "unityLibrary.gradle"),
                Path.Combine(templatesPath, "unityLibraryTemplate.gradle")
            };
            
            int filesFixed = 0;
            
            foreach (var gradleFile in gradleFiles)
            {
                if (File.Exists(gradleFile))
                {
                    try
                    {
                        var content = File.ReadAllText(gradleFile);
                        var originalContent = content;
                        
                        // Replace any instances of shrinkResources true with false
                        content = content.Replace("shrinkResources true", "shrinkResources false");
                        
                        if (content != originalContent)
                        {
                            File.WriteAllText(gradleFile, content);
                            Debug.Log($"✅ Disabled resource shrinking in: {Path.GetFileName(gradleFile)}");
                            filesFixed++;
                        }
                        else
                        {
                            Debug.Log($"ℹ️ No changes needed in: {Path.GetFileName(gradleFile)}");
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogError($"Failed to process {Path.GetFileName(gradleFile)}: {ex.Message}");
                    }
                }
            }
            
            if (filesFixed > 0)
            {
                Debug.Log($"✅ Resource shrinking disabled in {filesFixed} file(s)");
                AssetDatabase.Refresh();
                
                EditorUtility.DisplayDialog("Resource Shrinking Disabled", 
                    $"Resource shrinking has been disabled in {filesFixed} Gradle template(s).\n\n" +
                    "This prevents build errors and ensures compatibility across all build configurations.", "OK");
            }
            else
            {
                Debug.Log("✅ Resource shrinking already disabled in all templates");
            }
            
            Debug.Log("=== Resource Shrinking Disable Complete ===");
        }
        
        /// <summary>
        /// Validates that Unity build settings are correct for Quest development
        /// </summary>
        [MenuItem("MR Chess/Validate Quest Build Settings")]
        public static void ValidateQuestBuildSettings()
        {
            Debug.Log("=== Quest Build Settings Validation ===");
            
            // Check platform
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                Debug.LogWarning("⚠️ Build target is not set to Android");
                
                if (EditorUtility.DisplayDialog("Platform Switch", 
                    "Build target is not set to Android.\n\n" +
                    "Quest builds require Android platform.\n\n" +
                    "Switch to Android now?", "Switch", "Skip"))
                {
                    EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
                    Debug.Log("✅ Switched to Android platform");
                }
            }
            else
            {
                Debug.Log("✅ Build target is correctly set to Android");
            }
            
            // Check texture compression
            var textureCompression = EditorUserBuildSettings.androidBuildSubtarget;
            Debug.Log($"📱 Android build subtarget: {textureCompression}");
            
            // Check API level
            var targetApiLevel = PlayerSettings.Android.targetSdkVersion;
            var minApiLevel = PlayerSettings.Android.minSdkVersion;
            Debug.Log($"🎯 Target SDK: {targetApiLevel}, Min SDK: {minApiLevel}");
            
            if (minApiLevel < AndroidSdkVersions.AndroidApiLevel26)
            {
                Debug.LogWarning("⚠️ Minimum API level is below 26 (Android 8.0)");
                Debug.LogWarning("Quest devices require Android 8.0 or higher");
            }
            
            // Check architecture
            var targetArchitectures = PlayerSettings.Android.targetArchitectures;
            if ((targetArchitectures & AndroidArchitecture.ARM64) == 0)
            {
                Debug.LogWarning("⚠️ ARM64 architecture not enabled");
                Debug.LogWarning("Quest devices require ARM64 support");
                
                if (EditorUtility.DisplayDialog("Architecture Fix", 
                    "ARM64 architecture is not enabled.\n\n" +
                    "Quest devices require ARM64 support.\n\n" +
                    "Enable ARM64 now?", "Enable", "Skip"))
                {
                    PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
                    Debug.Log("✅ Enabled ARM64 architecture");
                }
            }
            else
            {
                Debug.Log("✅ ARM64 architecture is enabled");
            }
            
            Debug.Log("=== Quest Build Settings Validation Complete ===");
        }
    }
}
#endif
