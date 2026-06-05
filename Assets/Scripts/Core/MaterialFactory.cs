using UnityEngine;
using UnityEngine.Rendering;

namespace Autobazar.Core
{
    /// <summary>
    /// Vytváří materiály nezávisle na použité render pipeline (Built-in i URP).
    /// Umí běžný barevný materiál, lesklý/kovový lak, emisní (svítící) a sklo.
    /// </summary>
    public static class MaterialFactory
    {
        private static Shader _cachedShader;

        private static Shader GetShader()
        {
            if (_cachedShader != null) return _cachedShader;

            if (GraphicsSettings.currentRenderPipeline != null)
                _cachedShader = Shader.Find("Universal Render Pipeline/Lit");

            if (_cachedShader == null) _cachedShader = Shader.Find("Standard");
            if (_cachedShader == null) _cachedShader = Shader.Find("Legacy Shaders/Diffuse");
            if (_cachedShader == null) _cachedShader = Shader.Find("Sprites/Default");

            return _cachedShader;
        }

        /// <summary>Základní barevný materiál s volitelným leskem a kovovostí.</summary>
        public static Material Create(Color color, float smoothness = 0.25f, float metallic = 0f)
        {
            var mat = new Material(GetShader());

            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", color);

            if (mat.HasProperty("_Smoothness")) mat.SetFloat("_Smoothness", smoothness);
            if (mat.HasProperty("_Glossiness")) mat.SetFloat("_Glossiness", smoothness);
            if (mat.HasProperty("_Metallic")) mat.SetFloat("_Metallic", metallic);

            return mat;
        }

        /// <summary>Svítící materiál (světlomety, cedule, lampy).</summary>
        public static Material CreateEmissive(Color baseColor, Color emissionColor, float intensity = 1.5f)
        {
            var mat = Create(baseColor, 0.4f, 0f);
            SetEmission(mat, emissionColor * intensity);
            return mat;
        }

        /// <summary>Tmavé lesklé „sklo" (neprůhledné, ale leskem připomíná okna).</summary>
        public static Material CreateGlass(Color tint)
        {
            return Create(tint, smoothness: 0.92f, metallic: 0.1f);
        }

        /// <summary>Materiál s texturou (např. Kenney colormap) – barva bílá, ať vynikne textura.</summary>
        public static Material CreateTextured(Texture texture)
        {
            var mat = Create(Color.white, 0.3f, 0f);
            if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", texture);
            if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", texture);
            return mat;
        }

        /// <summary>Zapne/nastaví emisi na existujícím materiálu (pro zvýraznění při interakci).</summary>
        public static void SetEmission(Material mat, Color color)
        {
            if (mat == null) return;
            if (color.maxColorComponent > 0f) mat.EnableKeyword("_EMISSION");
            if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", color);
            mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        }
    }
}
