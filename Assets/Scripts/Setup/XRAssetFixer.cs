#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

namespace MRChess.Setup
{
    /// <summary>
    /// Fixes XR Simulation asset conflicts and errors
    /// </summary>
    public class XRAssetFixer
    {
        [MenuItem("MR Chess/Fix XR Asset Conflicts")]
        public static void FixXRAssetConflicts()
        {
            Debug.Log("=== Fixing XR Asset Conflicts ===");

            try
            {
                // Clean up problematic XR temp files
                CleanXRTempFiles();
                
                // Refresh asset database
                AssetDatabase.Refresh();
                
                // Reimport XR assets
                ReimportXRAssets();
                
                Debug.Log("✅ XR asset conflicts resolved successfully!");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to fix XR asset conflicts: {e.Message}");
            }
        }

        private static void CleanXRTempFiles()
        {
            string tempPath = "Assets/XR/Temp";
            
            if (AssetDatabase.IsValidFolder(tempPath))
            {
                Debug.Log("Removing XR temp folder...");
                AssetDatabase.DeleteAsset(tempPath);
            }

            // Clean up any orphaned meta files
            string[] metaFiles = Directory.GetFiles("Assets/XR", "*.meta", SearchOption.AllDirectories);
            foreach (string metaFile in metaFiles)
            {
                string assetPath = metaFile.Substring(0, metaFile.Length - 5); // Remove .meta
                if (!File.Exists(assetPath) && !Directory.Exists(assetPath))
                {
                    File.Delete(metaFile);
                    Debug.Log($"Removed orphaned meta file: {metaFile}");
                }
            }
        }

        private static void ReimportXRAssets()
        {
            Debug.Log("Reimporting XR assets...");
            
            // Reimport XR folder if it exists
            if (AssetDatabase.IsValidFolder("Assets/XR"))
            {
                AssetDatabase.ImportAsset("Assets/XR", ImportAssetOptions.ImportRecursive);
            }
            
            // Force reimport of XR simulation settings
            string[] xrAssets = AssetDatabase.FindAssets("XRSimulation", new[] { "Assets/XR" });
            foreach (string guid in xrAssets)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
                    Debug.Log($"Reimported XR asset: {assetPath}");
                }
            }
            
            // Force XR settings refresh
            AssetDatabase.Refresh();
            
            Debug.Log("XR assets reimported successfully");
        }

        [MenuItem("MR Chess/Clean Unity Cache")]
        public static void CleanUnityCache()
        {
            Debug.Log("=== Cleaning Unity Cache ===");

            // Close Unity first
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
            }

            // Clear various Unity caches
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            
            // Force shader recompilation by refreshing shader assets
            #if UNITY_2021_1_OR_NEWER
            // Note: ShaderUtil.ClearShaderErrors() requires specific shader parameter in newer Unity versions
            // Instead, we refresh the asset database which handles shader recompilation
            string[] shaderGUIDs = AssetDatabase.FindAssets("t:Shader");
            foreach (string guid in shaderGUIDs)
            {
                string shaderPath = AssetDatabase.GUIDToAssetPath(guid);
                AssetDatabase.ImportAsset(shaderPath, ImportAssetOptions.ForceUpdate);
            }
            #endif
            
            // Force reimport of assets
            AssetDatabase.ImportAsset("Assets", ImportAssetOptions.ImportRecursive);
            
            // Clear asset database
            AssetDatabase.Refresh();
            
            Debug.Log("✅ Unity cache cleared!");
        }
    }
}
#endif
