#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

namespace MRChess.Setup
{
    /// <summary>
    /// Android SDK path validator and fixer
    /// Addresses the Android command-line tools path inconsistency warning
    /// </summary>
    [InitializeOnLoad]
    public static class AndroidSDKPathFixer
    {
        static AndroidSDKPathFixer()
        {
            // Run validation on Unity startup
            EditorApplication.delayCall += ValidateAndFixAndroidSDKPath;
        }
        
        [MenuItem("MR Chess/Fix Android SDK Path")]
        public static void ValidateAndFixAndroidSDKPath()
        {
            var androidSdkPath = EditorPrefs.GetString("AndroidSdkRoot");
            
            if (string.IsNullOrEmpty(androidSdkPath))
            {
                Debug.LogWarning("Android SDK path not configured. Please set it in Unity Preferences.");
                return;
            }
            
            // Check for the cmdline-tools path inconsistency that causes warnings
            var cmdlineToolsPath = Path.Combine(androidSdkPath, "cmdline-tools");
            
            if (Directory.Exists(cmdlineToolsPath))
            {
                // Look for directories with version numbers followed by dashes (like "6.0-2")
                var directories = Directory.GetDirectories(cmdlineToolsPath);
                
                foreach (var dir in directories)
                {
                    var dirName = Path.GetFileName(dir);
                    
                    // Check if this looks like a versioned directory with extra suffix
                    if (dirName.Contains("-") && dirName.Length > 3)
                    {
                        var parts = dirName.Split('-');
                        if (parts.Length == 2 && IsVersionNumber(parts[0]))
                        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                            Debug.LogWarning($"Found inconsistent cmdline-tools directory: {dirName}");
                            Debug.LogWarning("This may cause Android SDK warnings during build.");
                            Debug.LogWarning($"Consider renaming '{dir}' to '{Path.Combine(cmdlineToolsPath, parts[0])}'");
#endif
                            
                            // Optionally offer to fix it automatically
                            if (EditorUtility.DisplayDialog("Android SDK Path Fix", 
                                $"Found inconsistent cmdline-tools directory: {dirName}\n\n" +
                                "This causes the warning: 'Observed package id 'cmdline-tools;6.0' in inconsistent location'\n\n" +
                                "Would you like to attempt to fix this automatically?", 
                                "Fix It", "Skip"))
                            {
                                TryFixCmdlineToolsPath(dir, Path.Combine(cmdlineToolsPath, parts[0]));
                            }
                        }
                    }
                }
            }
        }
        
        private static bool IsVersionNumber(string version)
        {
            // Check if the string looks like a version number (e.g., "6.0", "19.0")
            return System.Text.RegularExpressions.Regex.IsMatch(version, @"^\d+(\.\d+)*$");
        }
        
        private static void TryFixCmdlineToolsPath(string oldPath, string newPath)
        {
            try
            {
                if (Directory.Exists(newPath))
                {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    Debug.Log($"Command-line tools already fixed: {newPath} exists");
#endif
                    return;
                }
                
                if (!Directory.Exists(oldPath))
                {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    Debug.Log($"Source directory does not exist, no fix needed: {oldPath}");
#endif
                    return;
                }
                
                Directory.Move(oldPath, newPath);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.Log($"Successfully renamed cmdline-tools directory from {Path.GetFileName(oldPath)} to {Path.GetFileName(newPath)}");
#endif
                
                EditorUtility.DisplayDialog("Fix Complete", 
                    "Android SDK cmdline-tools path has been fixed!\n\n" +
                    "The build warning about inconsistent package location should now be resolved.", "OK");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to fix cmdline-tools path: {ex.Message}");
                EditorUtility.DisplayDialog("Fix Failed", 
                    $"Could not automatically fix the cmdline-tools path:\n\n{ex.Message}\n\n" +
                    "Please manually rename the directory or reinstall Android command-line tools.", "OK");
            }
        }
        
        [MenuItem("MR Chess/Validate Android SDK")]
        public static void ValidateFullAndroidSDK()
        {
            var androidSdkPath = EditorPrefs.GetString("AndroidSdkRoot");
            var jdkPath = EditorPrefs.GetString("JdkPath");
            
            Debug.Log("=== Android SDK Validation ===");
            
            if (string.IsNullOrEmpty(androidSdkPath))
            {
                Debug.LogError("❌ Android SDK path not set");
            }
            else
            {
                Debug.Log($"✅ Android SDK path: {androidSdkPath}");
                
                if (!Directory.Exists(androidSdkPath))
                {
                    Debug.LogError($"❌ Android SDK directory does not exist: {androidSdkPath}");
                }
            }
            
            if (string.IsNullOrEmpty(jdkPath))
            {
                Debug.LogError("❌ JDK path not set");
            }
            else
            {
                Debug.Log($"✅ JDK path: {jdkPath}");
                
                if (!Directory.Exists(jdkPath))
                {
                    Debug.LogError($"❌ JDK directory does not exist: {jdkPath}");
                }
            }
            
            // Validate cmdline-tools structure
            ValidateAndFixAndroidSDKPath();
            
            // Validate Gradle configuration
            ValidateGradleConfiguration();
            
            Debug.Log("=== Android SDK Validation Complete ===");
        }
        
        [MenuItem("MR Chess/Fix Gradle Configuration")]
        public static void ValidateGradleConfiguration()
        {
            Debug.Log("=== Gradle Configuration Validation ===");
            
            // Check for the common Unity library Gradle template issues
            var unityLibraryTemplatePath = Path.Combine(Application.dataPath, "Plugins", "Android", "unityLibrary.gradle");
            
            if (File.Exists(unityLibraryTemplatePath))
            {
                var content = File.ReadAllText(unityLibraryTemplatePath);
                
                if (content.Contains("shrinkResources true"))
                {
                    Debug.LogWarning("⚠️ Found 'shrinkResources true' in unityLibrary.gradle template");
                    Debug.LogWarning("This will cause Gradle build failures: 'Resource shrinker cannot be used for libraries'");
                    
                    if (EditorUtility.DisplayDialog("Gradle Configuration Fix", 
                        "Found 'shrinkResources true' in Unity library Gradle template.\n\n" +
                        "This causes the error: 'Resource shrinker cannot be used for libraries'\n\n" +
                        "Would you like to fix this automatically?", 
                        "Fix It", "Skip"))
                    {
                        FixGradleTemplate(unityLibraryTemplatePath);
                    }
                }
                else
                {
                    Debug.Log("✅ Unity library Gradle template looks good");
                }
            }
            else
            {
                Debug.Log("ℹ️ No custom unityLibrary.gradle template found (using Unity defaults)");
            }
            
            // Also check for any generated build files that might need fixing
            CheckGeneratedGradleFiles();
            
            Debug.Log("=== Gradle Configuration Validation Complete ===");
        }
        
        private static void FixGradleTemplate(string templatePath)
        {
            try
            {
                var content = File.ReadAllText(templatePath);
                
                // Replace any instances of shrinkResources true with false for Unity libraries
                content = content.Replace("shrinkResources true", "shrinkResources false");
                
                File.WriteAllText(templatePath, content);
                
                Debug.Log($"✅ Fixed Gradle template: {templatePath}");
                EditorUtility.DisplayDialog("Fix Complete", 
                    "Gradle template has been fixed!\n\n" +
                    "The 'shrinkResources true' entries have been changed to 'shrinkResources false'.\n\n" +
                    "Note: You may need to clean and rebuild your project for changes to take effect.", "OK");
                    
                AssetDatabase.Refresh();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to fix Gradle template: {ex.Message}");
                EditorUtility.DisplayDialog("Fix Failed", 
                    $"Could not automatically fix the Gradle template:\n\n{ex.Message}\n\n" +
                    "Please manually edit the file to change 'shrinkResources true' to 'shrinkResources false'.", "OK");
            }
        }
        
        private static void CheckGeneratedGradleFiles()
        {
            // Check the generated build files in Library/Bee for resource shrinking issues
            var beePath = Path.Combine(Application.dataPath, "..", "Library", "Bee", "Android", "Prj", "IL2CPP", "Gradle", "unityLibrary", "build.gradle");
            
            if (File.Exists(beePath))
            {
                var content = File.ReadAllText(beePath);
                
                if (content.Contains("shrinkResources true"))
                {
                    Debug.LogWarning("⚠️ Found 'shrinkResources true' in generated Unity library build.gradle");
                    Debug.LogWarning("This is likely the source of the current build failure");
                    Debug.LogWarning("Solution: Fix the Gradle template and clean/rebuild the project");
                }
            }
        }
    }
}
#endif
