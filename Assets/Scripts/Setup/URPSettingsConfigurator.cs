using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace MRChess.Setup
{
    /// <summary>
    /// Automatically configures URP settings for optimal MR performance on Quest devices
    /// </summary>
    [CreateAssetMenu(fileName = "MRChessURPSettings", menuName = "MR Chess/URP Settings Configurator")]
    public class URPSettingsConfigurator : ScriptableObject
    {
        [Header("Quest Optimized Settings")]
        [SerializeField] private bool useOptimizedSettings = true;
        
        [Header("Rendering")]
        [SerializeField] private RenderingMode renderingMode = RenderingMode.Forward;
        [SerializeField] private bool enableDepthTexture = false;
        [SerializeField] private bool enableOpaqueTexture = false;
        
        [Header("Quality")]
        [SerializeField] private int msaaSampleCount = 4;
        [SerializeField] private float renderScale = 1.0f;
        
        [Header("Shadows")]
        [SerializeField] private UnityEngine.ShadowQuality mainLightShadows = UnityEngine.ShadowQuality.HardShadows;
        [SerializeField] private UnityEngine.ShadowQuality additionalLightShadows = UnityEngine.ShadowQuality.Disable;
        [SerializeField] private int shadowDistance = 30;

        [ContextMenu("Apply Quest Optimized Settings")]
        public void ApplyOptimizedSettings()
        {
            var urpAsset = GraphicsSettings.defaultRenderPipeline as UniversalRenderPipelineAsset;
            if (urpAsset == null)
            {
                Debug.LogError("No URP Asset found! Please create and assign a URP Asset in Graphics Settings.");
                return;
            }

            Debug.Log("Applying Quest-optimized URP settings...");
            
            // Note: Many URP settings are not directly accessible via script
            // This serves as a reference for manual configuration
            
            var qualitySettings = QualitySettings.GetQualityLevel();
            
            // Configure quality settings that can be set via script
            QualitySettings.antiAliasing = msaaSampleCount;
            QualitySettings.shadows = UnityEngine.ShadowQuality.HardShadows;
            QualitySettings.shadowDistance = shadowDistance;
            QualitySettings.shadowResolution = ShadowResolution.Medium;
            QualitySettings.shadowProjection = ShadowProjection.CloseFit;
            QualitySettings.shadowCascades = 0; // No cascades for mobile VR
            
            // VR specific settings
            QualitySettings.vSyncCount = 0; // Disable VSync for VR
            QualitySettings.lodBias = 1.0f;
            QualitySettings.maximumLODLevel = 0;
            
            Debug.Log("Quest optimization settings applied! " +
                     "Please manually configure URP Asset settings according to the documentation.");
        }

        [ContextMenu("Log Recommended URP Settings")]
        public void LogRecommendedSettings()
        {
            Debug.Log("=== RECOMMENDED URP SETTINGS FOR META QUEST ===\n" +
                     "URP Asset Settings:\n" +
                     "- Rendering Mode: Forward\n" +
                     "- Depth Texture: Disabled\n" +
                     "- Opaque Texture: Disabled\n" +
                     "- MSAA: 4x (or 2x for Quest 2)\n" +
                     "- Render Scale: 1.0\n" +
                     "- Main Light: Shadows Enabled (Hard)\n" +
                     "- Additional Lights: Disabled or Per Vertex\n" +
                     "- Shadow Distance: 30m\n" +
                     "- Shadow Cascades: No Cascades\n" +
                     "- Store Actions: Auto\n" +
                     "- HDR: Disabled\n" +
                     "- Post Processing: Minimal (only essential effects)\n" +
                     "- SRP Batcher: Enabled\n" +
                     "- Dynamic Batching: Disabled\n" +
                     "- GPU Instancing: Enabled");
        }
    }
}
