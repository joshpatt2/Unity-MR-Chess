using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
using System.Linq;

namespace MRChess.Setup
{
    /// <summary>
    /// Unity Editor tool to automatically configure Android SDK paths and settings.
    /// This script helps resolve Android SDK detection issues in Unity.
    /// </summary>
    public class AndroidSDKFixer : EditorWindow
    {
        private static readonly string ANDROID_SDK_PATH = "/opt/homebrew/share/android-commandlinetools";
        private static readonly string ANDROID_NDK_PATH = "/opt/homebrew/share/android-commandlinetools/ndk/25.2.9519653";
        
        [MenuItem("MR Chess/Fix Android SDK Setup")]
        public static void ShowWindow()
        {
            GetWindow<AndroidSDKFixer>("Android SDK Fixer");
        }
        
        [MenuItem("MR Chess/Auto-Configure Android SDK")]
        public static void AutoConfigureAndroidSDK()
        {
            Debug.Log("🔧 Configuring Android SDK paths and settings for Unity...");
            
            // Set Android SDK path
            if (Directory.Exists(ANDROID_SDK_PATH))
            {
                EditorPrefs.SetString("AndroidSdkRoot", ANDROID_SDK_PATH);
                Debug.Log($"✅ Android SDK path set to: {ANDROID_SDK_PATH}");
            }
            else
            {
                Debug.LogError($"❌ Android SDK not found at: {ANDROID_SDK_PATH}");
                return;
            }
            
            // Set Android NDK path
            if (Directory.Exists(ANDROID_NDK_PATH))
            {
                EditorPrefs.SetString("AndroidNdkRoot", ANDROID_NDK_PATH);
                Debug.Log($"✅ Android NDK path set to: {ANDROID_NDK_PATH}");
            }
            else
            {
                Debug.LogWarning($"⚠️ Android NDK not found at: {ANDROID_NDK_PATH}");
            }
            
            // Set JDK path (try to detect automatically)
            string jdkPath = DetectJDKPath();
            if (!string.IsNullOrEmpty(jdkPath))
            {
                EditorPrefs.SetString("JdkPath", jdkPath);
                Debug.Log($"✅ JDK path set to: {jdkPath}");
            }
            
            // Configure Android Player Settings for Quest
            ConfigureAndroidPlayerSettings();
            
            Debug.Log("🎯 Android SDK configuration complete!");
            Debug.Log("💡 Try building now with: File → Build Settings → Android");
        }
        
        private static void ConfigureAndroidPlayerSettings()
        {
            Debug.Log("🔧 Configuring Android Player Settings for Meta Quest...");
            
            // Switch to Android platform if not already
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
                Debug.Log("✅ Switched to Android build target");
            }
            
            // Set minimum SDK version to 26 (Android 8.0) - required for Quest
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            Debug.Log("✅ Minimum SDK version set to Android 8.0 (API 26)");
            
            // Set target SDK version to 33 (Android 13) - recommended for Quest
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel33;
            Debug.Log("✅ Target SDK version set to Android 13 (API 33)");
            
            // Set architecture to ARM64 only (required for Quest)
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            Debug.Log("✅ Target architecture set to ARM64");
            
            // Set scripting backend to IL2CPP (required for ARM64)
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            Debug.Log("✅ Scripting backend set to IL2CPP");
            
            // Configure Quest-specific settings
            PlayerSettings.productName = "MR Chess";
            PlayerSettings.companyName = "MR Chess Team";
            
            // Set a proper bundle identifier if it's still default
            string currentBundleId = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android);
            if (string.IsNullOrEmpty(currentBundleId) || currentBundleId == "com.DefaultCompany.DefaultProduct")
            {
                PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.mrchessteam.mrchess");
                Debug.Log("✅ Bundle identifier set to: com.mrchessteam.mrchess");
            }
            
            Debug.Log("🎯 Android Player Settings configured for Meta Quest!");
        }
        
        private static string DetectJDKPath()
        {
            Debug.Log("🔍 Detecting JDK installation...");
            
            // First try to use macOS java_home utility (most reliable)
            try
            {
                var process = new System.Diagnostics.Process();
                process.StartInfo.FileName = "/usr/libexec/java_home";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();
                
                if (process.ExitCode == 0 && !string.IsNullOrEmpty(output))
                {
                    string javaHome = output.Trim();
                    Debug.Log($"✅ Found JDK via java_home: {javaHome}");
                    return javaHome;
                }
                else
                {
                    Debug.LogWarning($"⚠️ java_home failed: {error}");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"⚠️ java_home exception: {ex.Message}");
            }
            
            // Try common JDK locations on macOS
            string[] possiblePaths = {
                "/Library/Java/JavaVirtualMachines/temurin-24.jdk/Contents/Home",
                "/Library/Java/JavaVirtualMachines/temurin-21.jdk/Contents/Home", 
                "/Library/Java/JavaVirtualMachines/temurin-17.jdk/Contents/Home",
                "/Library/Java/JavaVirtualMachines/temurin-11.jdk/Contents/Home",
                "/Library/Java/JavaVirtualMachines/adoptopenjdk-11.jdk/Contents/Home",
                "/Library/Java/JavaVirtualMachines/openjdk-11.jdk/Contents/Home",
                "/opt/homebrew/opt/openjdk@21",
                "/opt/homebrew/opt/openjdk@17", 
                "/opt/homebrew/opt/openjdk@11",
                "/opt/homebrew/opt/openjdk",
                "/usr/local/opt/openjdk@21",
                "/usr/local/opt/openjdk@17",
                "/usr/local/opt/openjdk@11",
                "/usr/local/opt/openjdk"
            };
            
            foreach (string path in possiblePaths)
            {
                if (Directory.Exists(path))
                {
                    // Verify it's a valid JDK by checking for bin/javac
                    string javacPath = System.IO.Path.Combine(path, "bin", "javac");
                    if (File.Exists(javacPath))
                    {
                        Debug.Log($"✅ Found valid JDK at: {path}");
                        return path;
                    }
                }
            }
            
            // Try to detect from JAVA_HOME environment variable
            string envJavaHome = System.Environment.GetEnvironmentVariable("JAVA_HOME");
            if (!string.IsNullOrEmpty(envJavaHome) && Directory.Exists(envJavaHome))
            {
                Debug.Log($"✅ Found JDK via JAVA_HOME: {envJavaHome}");
                return envJavaHome;
            }
            
            // Last resort - try to find any JVM directory
            string jvmBase = "/Library/Java/JavaVirtualMachines";
            if (Directory.Exists(jvmBase))
            {
                var jdkDirs = Directory.GetDirectories(jvmBase)
                    .Where(dir => dir.Contains("jdk"))
                    .OrderByDescending(dir => dir) // Get latest version
                    .ToArray();
                    
                foreach (string jdkDir in jdkDirs)
                {
                    string homePath = System.IO.Path.Combine(jdkDir, "Contents", "Home");
                    if (Directory.Exists(homePath))
                    {
                        Debug.Log($"✅ Found JDK (fallback): {homePath}");
                        return homePath;
                    }
                }
            }
            
            Debug.LogError("❌ Could not detect JDK installation. Please install OpenJDK 11+ or set JAVA_HOME manually.");
            return string.Empty;
        }
        
        void OnGUI()
        {
            GUILayout.Label("Android SDK Configuration", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            GUILayout.Label($"Android SDK Path: {ANDROID_SDK_PATH}");
            GUILayout.Label($"Status: {(Directory.Exists(ANDROID_SDK_PATH) ? "✅ Found" : "❌ Missing")}");
            GUILayout.Space(5);
            
            GUILayout.Label($"Android NDK Path: {ANDROID_NDK_PATH}");
            GUILayout.Label($"Status: {(Directory.Exists(ANDROID_NDK_PATH) ? "✅ Found" : "❌ Missing")}");
            GUILayout.Space(10);
            
            if (GUILayout.Button("Auto-Configure Android SDK"))
            {
                AutoConfigureAndroidSDK();
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Open External Tools Preferences"))
            {
                EditorApplication.ExecuteMenuItem("Unity/Preferences...");
            }
            
            GUILayout.Space(5);
            
            if (GUILayout.Button("Open Build Settings"))
            {
                EditorApplication.ExecuteMenuItem("File/Build Settings...");
            }
            
            GUILayout.Space(20);
            
            GUILayout.Label("Current Unity Preferences:", EditorStyles.boldLabel);
            GUILayout.Label($"Android SDK: {EditorPrefs.GetString("AndroidSdkRoot", "Not set")}");
            GUILayout.Label($"Android NDK: {EditorPrefs.GetString("AndroidNdkRoot", "Not set")}");
            GUILayout.Label($"JDK: {EditorPrefs.GetString("JdkPath", "Not set")}");
        }
    }
}
#endif
