using UnityEngine;
using UnityEngine.Rendering;

namespace Autobazar.Core
{
    /// <summary>
    /// Vytváří materiály nezávisle na použité render pipeline.
    /// Díky tomu se objekty nezobrazí růžově ("missing shader"),
    /// ať už máš Built-in render pipeline nebo URP.
    /// </summary>
    public static class MaterialFactory
    {
        private static Shader _cachedShader;

        /// <summary>Vrátí vhodný "lit" shader pro aktuální render pipeline.</summary>
        private static Shader GetShader()
        {
            if (_cachedShader != null) return _cachedShader;

            // Pokud je nastavená nějaká Scriptable Render Pipeline (typicky URP),
            // použijeme URP/Lit. Jinak Built-in "Standard".
            if (GraphicsSettings.currentRenderPipeline != null)
                _cachedShader = Shader.Find("Universal Render Pipeline/Lit");

            if (_cachedShader == null) _cachedShader = Shader.Find("Standard");
            if (_cachedShader == null) _cachedShader = Shader.Find("Legacy Shaders/Diffuse");
            if (_cachedShader == null) _cachedShader = Shader.Find("Sprites/Default"); // poslední záchrana

            return _cachedShader;
        }

        /// <summary>Vytvoří nový barevný materiál.</summary>
        public static Material Create(Color color, float smoothness = 0.2f, float metallic = 0f)
        {
            var mat = new Material(GetShader());

            // Built-in shader používá "_Color", URP používá "_BaseColor" – nastavíme oba, když existují.
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);

            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", smoothness);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);

            return mat;
        }
    }
}
