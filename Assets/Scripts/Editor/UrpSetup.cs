#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Autobazar.Core;

namespace Autobazar.EditorTools
{
    /// <summary>
    /// Zapne URP + postprocessing (bloom, color grading, vignette, tonemapping, antialiasing)
    /// jedním klikem. Vytvoří pipeline asset, přiřadí ho, přestaví scénu (aby materiály
    /// vznikly jako URP) a přidá globální Volume s efekty.
    /// </summary>
    public static class UrpCinematicSetup
    {
        private const string Dir = "Assets/Settings";
        private static VolumeProfile _profile;

        [MenuItem("Checkpoint Rush/Setup Cinematic (URP)")]
        public static void Setup()
        {
            if (!AssetDatabase.IsValidFolder(Dir))
                AssetDatabase.CreateFolder("Assets", "Settings");

            // 1) Pipeline asset + renderer
            var rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
            AssetDatabase.CreateAsset(rendererData, Dir + "/CR_Renderer.asset");
            var urp = UniversalRenderPipelineAsset.Create(rendererData);
            AssetDatabase.CreateAsset(urp, Dir + "/CR_URP.asset");

            GraphicsSettings.defaultRenderPipeline = urp;
            QualitySettings.renderPipeline = urp;

            // 2) Postprocessing profil
            _profile = ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(_profile, Dir + "/CR_PostFX.asset");

            var bloom = _profile.Add<Bloom>(true);
            bloom.intensity.Override(0.8f);
            bloom.threshold.Override(1.0f);

            var ca = _profile.Add<ColorAdjustments>(true);
            ca.postExposure.Override(0.15f);
            ca.contrast.Override(14f);
            ca.saturation.Override(12f);

            var vig = _profile.Add<Vignette>(true);
            vig.intensity.Override(0.34f);

            var tone = _profile.Add<Tonemapping>(true);
            tone.mode.Override(TonemappingMode.ACES);

            AssetDatabase.SaveAssets();

            // Zbytek až po přepnutí pipeline (jeden editor frame), ať materiály vyjdou jako URP.
            EditorApplication.delayCall += Finish;
        }

        private static void Finish()
        {
            EditorApplication.delayCall -= Finish;

            // Přestavět scénu – materiály teď vzniknou jako URP/Lit (žádné růžové).
            foreach (var n in new[] { "RaceWorld", "Track", "Sun", "GameManagers" })
            {
                var go = GameObject.Find(n);
                if (go != null) Object.DestroyImmediate(go);
            }
            RaceWorldBuilder.BuildWorld();

            // Globální Volume s postprocessingem
            if (GameObject.Find("Global Volume") == null)
            {
                var volGo = new GameObject("Global Volume");
                var vol = volGo.AddComponent<Volume>();
                vol.isGlobal = true;
                vol.priority = 1f;
                vol.profile = _profile;
            }

            // Kamera – zapnout postprocessing + antialiasing
            var cam = Camera.main;
            if (cam != null)
            {
                var data = cam.GetUniversalAdditionalCameraData();
                data.renderPostProcessing = true;
                data.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                data.antialiasingQuality = AntialiasingQuality.High;
            }

            EditorSceneManager.MarkAllScenesDirty();
            Debug.Log("[CheckpointRush] URP + postprocessing zapnuto. Ulož (Ctrl+S) a stiskni Play.");
        }
    }
}
#endif
