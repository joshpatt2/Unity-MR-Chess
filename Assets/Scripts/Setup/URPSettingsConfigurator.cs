using System;
using UnityEngine;
using UnityEngine.Rendering;
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
        [SerializeField] private bool useOptimizedSettings = true; // Enable aggressive Quest performance optimizations
        
        [Header("Rendering")]
        [SerializeField] private RenderingMode renderingMode = RenderingMode.Forward;
        [SerializeField] private bool enableDepthTexture = false;
        [SerializeField] private bool enableOpaqueTexture = false;
        
        [Header("Quality")]
        [SerializeField] private int msaaSampleCount = 4;
        [Range(0.5f, 1.5f)]
        [SerializeField] private float renderScale = 1.0f; // VR render scale for performance optimization (0.7-1.0 recommended for Quest)
        
        [Header("Shadows")]
        [SerializeField] private UnityEngine.ShadowQuality mainLightShadows = UnityEngine.ShadowQuality.All; // Main light shadow quality
        [SerializeField] private UnityEngine.ShadowQuality additionalLightShadows = UnityEngine.ShadowQuality.Disable; // Additional lights shadow quality
        [SerializeField] private int shadowDistance = 30;

        [ContextMenu("Apply Quest Optimized Settings")]
        public void ApplyOptimizedSettings()
        {
            // Check if optimized settings should be applied
            if (!useOptimizedSettings)
            {
                Debug.LogWarning("Optimized settings are disabled. Enable 'Use Optimized Settings' to apply Quest optimizations.");
                return;
            }
            
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
            QualitySettings.shadows = mainLightShadows; // Use configurable main light shadows
            QualitySettings.shadowDistance = shadowDistance;
            QualitySettings.shadowResolution = UnityEngine.ShadowResolution.Medium;
            QualitySettings.shadowProjection = ShadowProjection.CloseFit;
            QualitySettings.shadowCascades = 0; // No cascades for mobile VR
            
            // VR specific settings
            QualitySettings.vSyncCount = 0; // Disable VSync for VR
            QualitySettings.lodBias = 1.0f;
            QualitySettings.maximumLODLevel = 0;
            
            // Apply VR render scale settings
            ApplyVRRenderScale();
            
            Debug.Log($"Quest optimization settings applied!\n" +
                     $"Shadow Configuration:\n" +
                     $"- Main Light Shadows: {mainLightShadows}\n" +
                     $"- Additional Light Shadows: {additionalLightShadows}\n" +
                     $"- Shadow Distance: {shadowDistance}m\n" +
                     $"- MSAA: {msaaSampleCount}x\n" +
                     $"- Render Scale: {renderScale}\n" +
                     $"Please manually configure URP Asset settings according to the documentation.");
            
            // Apply URP-specific shadow configurations
            ApplyURPShadowSettings(urpAsset);
        }
        
        [ContextMenu("Apply Custom Settings")]
        public void ApplyCustomSettings()
        {
            var urpAsset = GraphicsSettings.defaultRenderPipeline as UniversalRenderPipelineAsset;
            if (urpAsset == null)
            {
                Debug.LogError("No URP Asset found! Please create and assign a URP Asset in Graphics Settings.");
                return;
            }

            Debug.Log($"Applying custom URP settings (Optimizations: {(useOptimizedSettings ? "ON" : "OFF")})...");
            
            // Apply basic quality settings regardless of optimization preference
            QualitySettings.antiAliasing = msaaSampleCount;
            QualitySettings.shadows = mainLightShadows;
            QualitySettings.shadowDistance = shadowDistance;
            
            // Apply VR render scale
            ApplyVRRenderScale();
            
            // Apply more aggressive optimizations only if enabled
            if (useOptimizedSettings)
            {
                QualitySettings.shadowResolution = UnityEngine.ShadowResolution.Medium;
                QualitySettings.shadowProjection = ShadowProjection.CloseFit;
                QualitySettings.shadowCascades = 0; // No cascades for mobile VR
                QualitySettings.vSyncCount = 0; // Disable VSync for VR
                QualitySettings.lodBias = 1.0f;
                QualitySettings.maximumLODLevel = 0;
                Debug.Log("Applied Quest-specific optimizations");
            }
            else
            {
                Debug.Log("Skipped Quest-specific optimizations (Use Optimized Settings is disabled)");
            }
            
            Debug.Log($"Custom settings applied with optimization level: {(useOptimizedSettings ? "High Performance" : "Balanced")}");
        }
        
        [ContextMenu("Toggle Optimization Mode")]
        public void ToggleOptimizationMode()
        {
            useOptimizedSettings = !useOptimizedSettings;
            Debug.Log($"Optimization mode {(useOptimizedSettings ? "ENABLED" : "DISABLED")}. " +
                     $"Current mode: {(useOptimizedSettings ? "Quest Performance" : "Balanced Quality")}");
        }
        
        /// <summary>
        /// Get current optimization status
        /// </summary>
        public bool IsOptimizationEnabled => useOptimizedSettings;
        
        /// <summary>
        /// Get the configured rendering mode for reference
        /// </summary>
        public RenderingMode ConfiguredRenderingMode => renderingMode;
        
        /// <summary>
        /// Get depth texture setting for validation
        /// </summary>
        public bool ShouldEnableDepthTexture => enableDepthTexture;
        
        /// <summary>
        /// Get opaque texture setting for validation
        /// </summary>
        public bool ShouldEnableOpaqueTexture => enableOpaqueTexture;
        
        /// <summary>
        /// Apply URP-specific shadow settings if possible via reflection
        /// </summary>
        private void ApplyURPShadowSettings(UniversalRenderPipelineAsset urpAsset)
        {
            // Note: URP Asset shadow settings are not directly accessible via public API
            // This method documents what should be configured manually
            
            string shadowConfig = $"URP Shadow Settings to configure manually:\n" +
                                 $"- Main Light Shadows: {(mainLightShadows != UnityEngine.ShadowQuality.Disable ? "Enabled" : "Disabled")}\n" +
                                 $"- Additional Light Shadows: {(additionalLightShadows != UnityEngine.ShadowQuality.Disable ? "Enabled" : "Disabled")}\n" +
                                 $"- Shadow Distance: {shadowDistance}m";
                                 
            Debug.Log(shadowConfig);
        }
        
        /// <summary>
        /// Apply VR render scale settings for optimal Quest performance
        /// </summary>
        private void ApplyVRRenderScale()
        {
#if UNITY_XR_MANAGEMENT
            // Try to set render scale through XR settings if available
            var xrDisplaySubsystem = UnityEngine.XR.XRDisplaySubsystem.running;
            if (xrDisplaySubsystem != null)
            {
                try
                {
                    // Set render viewport scale for performance optimization
                    xrDisplaySubsystem.renderViewportScale = renderScale;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    Debug.Log($"XR Render scale set to: {renderScale}");
#endif
                }
                catch (System.Exception e)
                {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    Debug.LogWarning($"Could not set XR render scale via XRDisplaySubsystem: {e.Message}");
#endif
                }
            }
#endif

            // Fallback to legacy XRSettings for older Unity versions or setups
#pragma warning disable CS0618 // Type or member is obsolete
            try
            {
                if (UnityEngine.XR.XRSettings.enabled)
                {
                    UnityEngine.XR.XRSettings.renderViewportScale = renderScale;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    Debug.Log($"Legacy XR Render scale set to: {renderScale}");
#endif
                }
                else
                {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    Debug.Log($"XR not enabled. Render scale {renderScale} noted for manual URP Asset configuration.");
#endif
                }
            }
            catch (System.Exception e)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning($"Could not set legacy XR render scale: {e.Message}");
#endif
            }
#pragma warning restore CS0618 // Type or member is obsolete
        }

        [ContextMenu("Log Recommended URP Settings")]
        public void LogRecommendedSettings()
        {
            Debug.Log("=== RECOMMENDED URP SETTINGS FOR META QUEST ===\n" +
                     "URP Asset Settings:\n" +
                     $"- Rendering Mode: {renderingMode}\n" +
                     $"- Depth Texture: {(enableDepthTexture ? "Enabled" : "Disabled")}\n" +
                     $"- Opaque Texture: {(enableOpaqueTexture ? "Enabled" : "Disabled")}\n" +
                     $"- MSAA: {msaaSampleCount}x\n" +
                     $"- Render Scale: {renderScale}\n" +
                     $"- Main Light: Shadows {(mainLightShadows != UnityEngine.ShadowQuality.Disable ? "Enabled" : "Disabled")}\n" +
                     $"- Additional Light Shadows: {additionalLightShadows}\n" +
                     $"- Shadow Distance: {shadowDistance}m\n" +
                     "- Shadow Cascades: No Cascades\n" +
                     "- Store Actions: Auto\n" +
                     "- HDR: Disabled\n" +
                     "- Post Processing: Minimal (only essential effects)\n" +
                     "- SRP Batcher: Enabled\n" +
                     "- Dynamic Batching: Disabled\n" +
                     "- GPU Instancing: Enabled");
        }
        
        [ContextMenu("Validate Current VR Settings")]
        public void ValidateCurrentVRSettings()
        {
            var currentShadows = QualitySettings.shadows;
            var currentDistance = QualitySettings.shadowDistance;
            
            // Get current XR render scale
            float currentRenderScale = 1.0f;
            string renderScaleSource = "Not Available";
            
#pragma warning disable CS0618 // Type or member is obsolete
            try
            {
                if (UnityEngine.XR.XRSettings.enabled)
                {
                    currentRenderScale = UnityEngine.XR.XRSettings.renderViewportScale;
                    renderScaleSource = "XRSettings";
                }
            }
            catch
            {
                // XR not available or accessible
            }
#pragma warning restore CS0618 // Type or member is obsolete

#if UNITY_XR_MANAGEMENT
            var xrDisplaySubsystem = UnityEngine.XR.XRDisplaySubsystem.running;
            if (xrDisplaySubsystem != null)
            {
                try
                {
                    currentRenderScale = xrDisplaySubsystem.renderViewportScale;
                    renderScaleSource = "XRDisplaySubsystem";
                }
                catch
                {
                    // Could not access XR subsystem
                }
            }
#endif
            
            Debug.Log($"=== CURRENT VR SETTINGS VALIDATION ===\n" +
                     $"Configuration Status:\n" +
                     $"- Use Optimized Settings: {(useOptimizedSettings ? "✓ ENABLED" : "✗ DISABLED")}\n" +
                     $"- Optimization Level: {(useOptimizedSettings ? "Quest Performance" : "Balanced")}\n\n" +
                     $"Current Quality Settings:\n" +
                     $"- Shadows: {currentShadows}\n" +
                     $"- Shadow Distance: {currentDistance}m\n" +
                     $"- Render Scale: {currentRenderScale} (from {renderScaleSource})\n\n" +
                     $"Configured Values:\n" +
                     $"- Main Light Shadows: {mainLightShadows}\n" +
                     $"- Additional Light Shadows: {additionalLightShadows}\n" +
                     $"- Target Shadow Distance: {shadowDistance}m\n" +
                     $"- Target Render Scale: {renderScale}\n\n" +
                     $"Match Status:\n" +
                     $"- Shadows: {(currentShadows == mainLightShadows ? "✓ Match" : "✗ Mismatch")}\n" +
                     $"- Distance: {(Math.Abs(currentDistance - shadowDistance) < 0.1f ? "✓ Match" : "✗ Mismatch")}\n" +
                     $"- Render Scale: {(Math.Abs(currentRenderScale - renderScale) < 0.01f ? "✓ Match" : "✗ Mismatch")}");
        }
    }
}
