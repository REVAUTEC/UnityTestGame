#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Autobazar.Core;

namespace Autobazar.EditorTools
{
    /// <summary>
    /// Menu „Checkpoint Rush" pro postavení/přestavění/vyčištění závodní scény přímo v editoru.
    /// </summary>
    public static class Phase1SceneBuilder
    {
        [MenuItem("Checkpoint Rush/Build Scene")]
        public static void BuildScene()
        {
            RaceWorldBuilder.BuildWorld();
            EditorSceneManager.MarkAllScenesDirty();
            Debug.Log("[CheckpointRush] Hotovo. Ulož scénu (Ctrl+S) a stiskni Play.");
        }

        [MenuItem("Checkpoint Rush/Rebuild Scene (Clear + Build)")]
        public static void RebuildScene()
        {
            ClearAll();
            RaceWorldBuilder.BuildWorld();
            EditorSceneManager.MarkAllScenesDirty();
            Debug.Log("[CheckpointRush] Scéna přestavěna. Ulož (Ctrl+S) a Play.");
        }

        [MenuItem("Checkpoint Rush/Clear Scene")]
        public static void ClearScene()
        {
            ClearAll();
            EditorSceneManager.MarkAllScenesDirty();
            Debug.Log("[CheckpointRush] Scéna vyčištěna.");
        }

        private static void ClearAll()
        {
            DestroyByName("RaceWorld");
            DestroyByName("Track");
            DestroyByName("Sun");
            // úklid po staré verzi (autobazar)
            DestroyByName("AutobazarWorld");
            DestroyByName("GameManagers");
        }

        private static void DestroyByName(string name)
        {
            var go = GameObject.Find(name);
            if (go != null) Object.DestroyImmediate(go);
        }
    }
}
#endif
