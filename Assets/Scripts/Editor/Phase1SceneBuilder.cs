#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Autobazar.Core;

namespace Autobazar.EditorTools
{
    /// <summary>
    /// Položky v horním menu Unity ("Autobazar"), které postaví nebo smažou
    /// scénu Fáze 1 přímo v editoru – nemusíš nic skládat ručně.
    /// </summary>
    public static class Phase1SceneBuilder
    {
        [MenuItem("Autobazar/Build Phase 1 Scene")]
        public static void BuildPhase1()
        {
            WorldBuilder.BuildWorld();
            EditorSceneManager.MarkAllScenesDirty();
            Debug.Log("[Autobazar] Hotovo. Ulož scénu (Ctrl+S) a stiskni Play.");
        }

        [MenuItem("Autobazar/Rebuild Scene (Clear + Build)")]
        public static void RebuildScene()
        {
            // Použij po každém stažení nového kódu, ať se svět postaví podle nové verze.
            DestroyByName("AutobazarWorld");
            DestroyByName("Sun");
            WorldBuilder.BuildWorld();
            EditorSceneManager.MarkAllScenesDirty();
            Debug.Log("[Autobazar] Scéna přestavěna podle aktuálního kódu. Ulož (Ctrl+S) a Play.");
        }

        [MenuItem("Autobazar/Clear Scene")]
        public static void ClearScene()
        {
            DestroyByName("AutobazarWorld");
            DestroyByName("Sun");
            EditorSceneManager.MarkAllScenesDirty();
            Debug.Log("[Autobazar] Scéna vyčištěna. Můžeš znovu spustit Build Phase 1 Scene.");
        }

        private static void DestroyByName(string name)
        {
            var go = GameObject.Find(name);
            if (go != null) Object.DestroyImmediate(go);
        }
    }
}
#endif
