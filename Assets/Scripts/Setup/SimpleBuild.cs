#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine.XR.Management;
using UnityEditor.XR.Management;
using System.IO;
using System.Linq;

public class SimpleBuild
{
    [MenuItem("MR Chess/Simple Build APK")]
    public static void BuildAPK()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log("=== SimpleBuild.BuildAPK Starting ===");
#endif
        
        // Set Android platform FIRST
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log("Switching to Android platform...");
#endif
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log("Switched to Android platform");
#endif
        }
        
        // CRITICAL: Set target architecture FIRST and explicitly
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log("Setting target architecture to ARM64...");
#endif
        
        // First ensure IL2CPP is selected (required for ARM64)
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Log("Set scripting backend to IL2CPP");
#endif
        
        // Now set ARM64 architecture
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        
        // Force Unity to refresh settings
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        // Verify architecture was set
        Debug.Log("Current target architectures: " + PlayerSettings.Android.targetArchitectures);
        
        // Validate that architecture was actually set
        if (PlayerSettings.Android.targetArchitectures == AndroidArchitecture.None)
        {
            Debug.LogError("❌ Failed to set target architecture! Android SDK may not be configured.");
            Debug.LogError("Please configure Android SDK in Unity Preferences > External Tools");
            return;
        }
        
        if (PlayerSettings.Android.targetArchitectures != AndroidArchitecture.ARM64)
        {
            Debug.LogWarning("⚠️ Target architecture is not ARM64. Attempting to fix...");
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            
            // Double-check
            if (PlayerSettings.Android.targetArchitectures != AndroidArchitecture.ARM64)
            {
                Debug.LogError("❌ Cannot set ARM64 architecture. Please check Android SDK configuration.");
                return;
            }
        }
        
        // Basic settings
        PlayerSettings.productName = "MR Chess";
        PlayerSettings.companyName = "MR Chess Team";
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.mrchessteam.mrchess");
        PlayerSettings.bundleVersion = "1.0";
        PlayerSettings.Android.bundleVersionCode = 1;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel33;
        
        // Fix Input System setting for Android VR - CRITICAL for Quest
        ConfigureInputSystem();
        
        // Check and configure Android SDK
        if (!CheckAndroidSDK())
        {
            Debug.LogError("❌ Android SDK not properly configured. Build cannot proceed.");
            Debug.LogError("Please install Android Build Support via Unity Hub or configure SDK paths manually.");
            return;
        }
        
        // Configure XR settings for Quest 3
        ConfigureXRSettings();
        
        // Configure OpenXR interaction profiles for Quest
        ConfigureOpenXRInteractionProfiles();
        
        Debug.Log("Android settings configured!");
        
        // Validate settings before building
        if (PlayerSettings.Android.targetArchitectures != AndroidArchitecture.ARM64)
        {
            Debug.LogError("❌ Target architecture not set to ARM64!");
            return;
        }
        
        Debug.Log("✅ Validated: Target architecture is ARM64");
        
        // Verify we're on Android platform
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
        {
            Debug.LogError("❌ Active build target is not Android!");
            return;
        }
        
        Debug.Log("✅ Validated: Active build target is Android");
        
        // Create build directory
        Directory.CreateDirectory("Builds/Development");
        
        // Build options
        BuildPlayerOptions buildOptions = new BuildPlayerOptions();
        buildOptions.scenes = new string[] { "Assets/chess.unity" };
        buildOptions.locationPathName = "Builds/Development/MRChess-Simple.apk";
        buildOptions.target = BuildTarget.Android;
        buildOptions.options = BuildOptions.Development;
        
        Debug.Log("Starting build...");
        BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
        
        if (report.summary.result == BuildResult.Succeeded)
        {
            Debug.Log("✅ BUILD SUCCEEDED!");
            Debug.Log("APK created at: " + buildOptions.locationPathName);
        }
        else
        {
            Debug.LogError("❌ BUILD FAILED!");
            Debug.LogError("Result: " + report.summary.result);
        }
        
        Debug.Log("=== SimpleBuild.BuildAPK Complete ===");
    }
    
    private static bool CheckAndroidSDK()
    {
        Debug.Log("Checking Android SDK configuration...");
        
        var androidSdkPath = EditorPrefs.GetString("AndroidSdkRoot");
        var jdkPath = EditorPrefs.GetString("JdkPath");
        
        if (string.IsNullOrEmpty(androidSdkPath))
        {
            Debug.LogError("❌ Android SDK path not configured.");
            Debug.LogError("REQUIRED PATH: /opt/homebrew/share/android-commandlinetools");
            Debug.LogError("📋 COPY THIS PATH: /opt/homebrew/share/android-commandlinetools");
            Debug.LogError("Go to: Unity > Preferences > External Tools > Android > SDK Tools");
            EditorUtility.DisplayDialog("Android SDK Required", 
                "Android SDK path is not set!\n\n" +
                "EXACT STEPS:\n" +
                "1. Go to Unity > Preferences (or Unity > Settings)\n" +
                "2. Click 'External Tools' in left sidebar\n" +
                "3. Find 'Android' section\n" +
                "4. Set 'SDK Tools' field to:\n" +
                "/opt/homebrew/share/android-commandlinetools\n\n" +
                "COPY THIS EXACT PATH:\n" +
                "/opt/homebrew/share/android-commandlinetools", "Copy Path");
            return false;
        }
        
        if (string.IsNullOrEmpty(jdkPath))
        {
            Debug.LogError("❌ JDK path not configured.");
            Debug.LogError("REQUIRED PATH: /Library/Java/JavaVirtualMachines/temurin-11.jdk/Contents/Home");
            Debug.LogError("📋 COPY THIS PATH: /Library/Java/JavaVirtualMachines/temurin-11.jdk/Contents/Home");
            Debug.LogError("Go to: Unity > Preferences > External Tools > Android > JDK");
            Debug.LogError("NOTE: Unity requires JDK 11 64-bit for Android development");
            EditorUtility.DisplayDialog("JDK 11 Required", 
                "JDK path is not set!\n\n" +
                "Unity Android development requires JDK 11 64-bit.\n\n" +
                "EXACT STEPS:\n" +
                "1. Go to Unity > Preferences (or Unity > Settings)\n" +
                "2. Click 'External Tools' in left sidebar\n" +
                "3. Find 'Android' section\n" +
                "4. Set 'JDK' field to:\n" +
                "/Library/Java/JavaVirtualMachines/temurin-11.jdk/Contents/Home\n\n" +
                "COPY THIS EXACT PATH:\n" +
                "/Library/Java/JavaVirtualMachines/temurin-11.jdk/Contents/Home", "Copy Path");
            return false;
        }
        
        // Check if Android SDK directory exists
        if (!System.IO.Directory.Exists(androidSdkPath))
        {
            Debug.LogError($"❌ Android SDK directory not found: {androidSdkPath}");
            return false;
        }
        
        // Check for command-line tools structure that Unity expects
        string cmdlineToolsPath = System.IO.Path.Combine(androidSdkPath, "cmdline-tools");
        if (!System.IO.Directory.Exists(cmdlineToolsPath))
        {
            Debug.LogError($"❌ Command-line tools directory not found: {cmdlineToolsPath}");
            Debug.LogError("Unity expects cmdline-tools to be present in the SDK directory");
            return false;
        }
        
        // Check for versioned command-line tools (Unity expects specific versions)
        string[] expectedVersions = { "latest", "6.0", "19.0" };
        bool hasVersionedTools = false;
        
        foreach (string version in expectedVersions)
        {
            string versionPath = System.IO.Path.Combine(cmdlineToolsPath, version);
            if (System.IO.Directory.Exists(versionPath))
            {
                hasVersionedTools = true;
                Debug.Log($"✅ Found command-line tools version: {version}");
                break;
            }
        }
        
        if (!hasVersionedTools)
        {
            Debug.LogError("❌ No versioned command-line tools found.");
            Debug.LogError("Unity expects command-line tools in a versioned directory like 'cmdline-tools/6.0/' or 'cmdline-tools/latest/'");
            return false;
        }
        
        Debug.Log($"✅ Android SDK found: {androidSdkPath}");
        if (!string.IsNullOrEmpty(jdkPath))
        {
            Debug.Log($"✅ JDK found: {jdkPath}");
        }
        
        return true;
    }
    
    private static void ConfigureInputSystem()
    {
        Debug.Log("Configuring Input System for Android...");
        
        try
        {
            // Fix Active Input Handling to Input System Package (New) only
            // This fixes the "Both" setting which is unsupported on Android
            var ps = (SerializedObject)typeof(PlayerSettings).GetMethod("GetSerializedObject", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)?.Invoke(null, null);
            
            if (ps != null)
            {
                ps.Update();
                var activeInputHandlerProp = ps.FindProperty("activeInputHandler");
                if (activeInputHandlerProp != null)
                {
                    // Set to Input System Package (New) = 1, Both = 2, Legacy = 0
                    activeInputHandlerProp.intValue = 1; // Input System Package (New) only
                    ps.ApplyModifiedProperties();
                    Debug.Log("✅ Set Active Input Handling to Input System Package (New) only");
                }
                else
                {
                    Debug.LogWarning("⚠️ Could not find activeInputHandler property");
                }
            }
            else
            {
                Debug.LogWarning("⚠️ Could not access PlayerSettings serialized object");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"⚠️ Input System configuration warning: {ex.Message}");
            Debug.Log("Please manually set Project Settings → Player → Active Input Handling to 'Input System Package (New)'");
        }
    }
    
    private static void ConfigureXRSettings()
    {
        Debug.Log("Configuring XR settings for Quest 3...");
        
        try
        {
            // Get XR General Settings for Android
            var xrGeneralSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Android);
            if (xrGeneralSettings == null)
            {
                Debug.LogWarning("⚠️ XR General Settings not found. Please install XR Management package.");
                return;
            }
            
            // Initialize XR on startup
            xrGeneralSettings.InitManagerOnStart = true;
            Debug.Log("✅ Set Initialize XR on Startup: true");
            
            // Get the manager instance
            var manager = xrGeneralSettings.Manager;
            if (manager == null)
            {
                Debug.LogWarning("⚠️ XR Manager not found.");
                return;
            }
            
            Debug.Log("✅ XR Manager found and configured");
            
            // Configure graphics APIs for Quest
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new UnityEngine.Rendering.GraphicsDeviceType[] { 
                UnityEngine.Rendering.GraphicsDeviceType.Vulkan,
                UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3 
            });
            Debug.Log("✅ Set Graphics APIs: Vulkan, OpenGLES3");
            
            // Quest-specific settings
            PlayerSettings.Android.blitType = AndroidBlitType.Always;
            PlayerSettings.Android.startInFullscreen = true;
            PlayerSettings.Android.renderOutsideSafeArea = true;
            Debug.Log("✅ Configured Quest-specific Android settings");
            
            Debug.Log("✅ XR Settings configured for Quest 3!");
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"⚠️ XR Configuration warning: {ex.Message}");
            Debug.Log("Manual XR setup may be required in Project Settings → XR Plug-in Management");
        }
    }
    
    private static void ConfigureOpenXRInteractionProfiles()
    {
        Debug.Log("Configuring OpenXR interaction profiles for Quest...");
        
        try
        {
#if UNITY_XR_OPENXR
            // Get OpenXR settings for Android
            var openXRSettings = UnityEngine.XR.OpenXR.OpenXRSettings.GetSettingsForBuildTargetGroup(BuildTargetGroup.Android);
            if (openXRSettings == null)
            {
                Debug.LogWarning("⚠️ OpenXR settings not found. Please install OpenXR package and configure it in XR Plug-in Management.");
                return;
            }
            
            // Get all features
            var features = openXRSettings.GetFeatures();
            
            // Enable Oculus Touch Controller Profile
            var oculusTouchFeature = features.FirstOrDefault(f => f.GetType().Name.Contains("OculusTouchControllerProfile"));
            if (oculusTouchFeature != null)
            {
                oculusTouchFeature.enabled = true;
                Debug.Log("✅ Enabled Oculus Touch Controller Profile");
            }
            else
            {
                Debug.LogWarning("⚠️ Oculus Touch Controller Profile not found");
            }
            
            // Enable Meta Quest Pro Touch Profile if available
            var questProTouchFeature = features.FirstOrDefault(f => f.GetType().Name.Contains("MetaQuestProTouchControllerProfile"));
            if (questProTouchFeature != null)
            {
                questProTouchFeature.enabled = true;
                Debug.Log("✅ Enabled Meta Quest Pro Touch Controller Profile");
            }
            
            // Enable Hand Interaction Profile
            var handInteractionFeature = features.FirstOrDefault(f => f.GetType().Name.Contains("HandInteractionProfile"));
            if (handInteractionFeature != null)
            {
                handInteractionFeature.enabled = true;
                Debug.Log("✅ Enabled Hand Interaction Profile");
            }
            
            // Ensure at least one interaction profile is enabled
            bool hasEnabledProfile = features.Any(f => f.enabled && f.GetType().Name.Contains("Profile"));
            if (!hasEnabledProfile)
            {
                // Fallback: try to enable the first available profile
                var firstProfile = features.FirstOrDefault(f => f.GetType().Name.Contains("Profile"));
                if (firstProfile != null)
                {
                    firstProfile.enabled = true;
                    Debug.Log($"✅ Enabled fallback interaction profile: {firstProfile.GetType().Name}");
                }
            }
            
            Debug.Log("✅ OpenXR interaction profiles configured!");
#else
            Debug.LogWarning("⚠️ OpenXR package not available. Please install it via Package Manager.");
#endif
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"⚠️ OpenXR interaction profile configuration warning: {ex.Message}");
            Debug.Log("Manual OpenXR setup may be required in Project Settings → XR Plug-in Management → OpenXR");
        }
    }
    
    [MenuItem("MR Chess/Copy Android SDK Path")]
    public static void CopyAndroidSDKPath()
    {
        string sdkPath = "/opt/homebrew/share/android-commandlinetools";
        EditorGUIUtility.systemCopyBuffer = sdkPath;
        Debug.Log($"📋 Copied to clipboard: {sdkPath}");
        EditorUtility.DisplayDialog("Path Copied", 
            $"Android SDK path copied to clipboard:\n\n{sdkPath}\n\n" +
            "Now paste this in:\n" +
            "Unity > Preferences > External Tools > Android > SDK Tools", "OK");
    }
    
    [MenuItem("MR Chess/Copy JDK Path")]
    public static void CopyJDKPath()
    {
        string jdkPath = "/Library/Java/JavaVirtualMachines/temurin-11.jdk/Contents/Home";
        EditorGUIUtility.systemCopyBuffer = jdkPath;
        Debug.Log($"📋 Copied to clipboard: {jdkPath}");
        EditorUtility.DisplayDialog("JDK 11 Path Copied", 
            $"JDK 11 path copied to clipboard:\n\n{jdkPath}\n\n" +
            "Unity requires JDK 11 64-bit for Android development.\n\n" +
            "Now paste this in:\n" +
            "Unity > Preferences > External Tools > Android > JDK", "OK");
    }
    
    [MenuItem("MR Chess/Show Setup Instructions")]
    public static void ShowSetupInstructions()
    {
        EditorUtility.DisplayDialog("Unity Android Setup Instructions", 
            "STEP-BY-STEP SETUP:\n\n" +
            "1. Go to Unity > Preferences (⌘,)\n" +
            "2. Click 'External Tools' in left sidebar\n" +
            "3. Under 'Android' section:\n\n" +
            "SDK Tools field:\n" +
            "/opt/homebrew/share/android-commandlinetools\n\n" +
            "JDK field (MUST be JDK 11):\n" +
            "/Library/Java/JavaVirtualMachines/temurin-11.jdk/Contents/Home\n\n" +
            "4. Close Preferences\n" +
            "5. Try build again: MR Chess > Simple Build APK\n\n" +
            "NOTE: Unity requires JDK 11 64-bit for Android development!\n\n" +
            "Use 'MR Chess > Copy Android SDK Path' and 'MR Chess > Copy JDK Path' to copy paths to clipboard!", "Got It");
    }
}
#endif
