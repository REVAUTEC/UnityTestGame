using UnityEngine;
using UnityEngine.Rendering;
using Autobazar.Player;
using Autobazar.Vehicles;
using Autobazar.Managers;
using Autobazar.UI;
using Autobazar.Racing;

namespace Autobazar.Core
{
    /// <summary>
    /// Postaví scénu závodní hry „Checkpoint Rush" z kódu: zem, obloha, auto s kamerou,
    /// kulisy (stromy, kameny, kužely) a manažery (závod, UI, hudba, viněta).
    /// </summary>
    public static class RaceWorldBuilder
    {
        private const string RootName = "RaceWorld";

        private static readonly string[] Trees =
            { "tree_default", "tree_oak", "tree_pineDefaultA", "tree_fat", "tree_detailed", "tree_pineRoundB", "tree_pineTallA" };
        private static readonly string[] Rocks =
            { "rock_largeA", "rock_largeB", "rock_largeC", "rock_tallA", "rock_tallB", "stone_largeA", "stone_tallC" };

        public static void BuildWorld()
        {
            if (GameObject.Find(RootName) != null)
            {
                Debug.Log("[CheckpointRush] Scéna už existuje – nestavím znovu.");
                return;
            }

            GameState.InputLocked = false;
            var root = new GameObject(RootName);

            BuildAtmosphere();
            BuildGround(root.transform);
            var car = BuildCar(root.transform);
            BuildCamera(car.transform);
            BuildManagers(root.transform);
            BuildScenery(root.transform);

            Debug.Log("[CheckpointRush] Scéna postavena. Stiskni Play a Enter.");
        }

        private static void BuildAtmosphere()
        {
            Light sun = null;
            foreach (var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
                if (l.type == LightType.Directional) { sun = l; break; }
            if (sun == null)
            {
                var go = new GameObject("Sun");
                sun = go.AddComponent<Light>();
                sun.type = LightType.Directional;
            }
            sun.intensity = 1.25f;
            sun.color = new Color(1f, 0.96f, 0.86f);
            sun.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(48f, 35f, 0f);

            var skyShader = Shader.Find("Skybox/Procedural");
            if (skyShader != null)
            {
                var sky = new Material(skyShader);
                if (sky.HasProperty("_SkyTint")) sky.SetColor("_SkyTint", new Color(0.5f, 0.66f, 0.92f));
                if (sky.HasProperty("_GroundColor")) sky.SetColor("_GroundColor", new Color(0.3f, 0.35f, 0.3f));
                if (sky.HasProperty("_AtmosphereThickness")) sky.SetFloat("_AtmosphereThickness", 1.1f);
                if (sky.HasProperty("_Exposure")) sky.SetFloat("_Exposure", 1.2f);
                RenderSettings.skybox = sky;
                RenderSettings.ambientMode = AmbientMode.Skybox;
                DynamicGI.UpdateEnvironment();
            }
            else
            {
                RenderSettings.ambientMode = AmbientMode.Flat;
                RenderSettings.ambientLight = new Color(0.55f, 0.57f, 0.6f);
            }

            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.72f, 0.8f, 0.9f);
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 70f;
            RenderSettings.fogEndDistance = 230f;
        }

        private static void BuildGround(Transform parent)
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.SetParent(parent, false);
            ground.transform.localScale = new Vector3(16f, 1f, 16f); // 160 x 160
            ground.GetComponent<Renderer>().sharedMaterial =
                MaterialFactory.Create(new Color(0.28f, 0.42f, 0.24f), smoothness: 0.1f); // tráva
        }

        private static GameObject BuildCar(Transform parent)
        {
            var car = new GameObject("Car");
            car.transform.SetParent(parent, false);
            car.transform.position = Vector3.zero;

            var prefab = ModelLibrary.Load("Cars", "sedan-sports");
            if (prefab == null || ModelLibrary.Spawn(car, prefab, ModelLibrary.CarTargetLength,
                    ModelLibrary.Fit.CarLength, new Color(0.8f, 0.15f, 0.15f), "Cars", ModelLibrary.CarYaw) == null)
            {
                // záloha: jednoduché auto z kostky
                var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
                body.name = "Body";
                body.transform.SetParent(car.transform, false);
                body.transform.localPosition = new Vector3(0f, 0.5f, 0f);
                body.transform.localScale = new Vector3(1.8f, 0.8f, 4f);
                body.GetComponent<Renderer>().sharedMaterial = MaterialFactory.Create(new Color(0.8f, 0.15f, 0.15f), 0.6f, 0.4f);
            }

            car.AddComponent<CarController>();
            return car;
        }

        private static void BuildCamera(Transform target)
        {
            Camera cam = Camera.main;
            GameObject camGo;
            if (cam != null) camGo = cam.gameObject;
            else { camGo = new GameObject("Main Camera"); cam = camGo.AddComponent<Camera>(); camGo.tag = "MainCamera"; }

            cam.clearFlags = CameraClearFlags.Skybox;
            cam.fieldOfView = 62f;
            cam.farClipPlane = 250f;

            if (Object.FindFirstObjectByType<AudioListener>() == null) camGo.AddComponent<AudioListener>();

            var controller = camGo.GetComponent<CameraController>();
            if (controller == null) controller = camGo.AddComponent<CameraController>();
            controller.SetTarget(target);
        }

        private static void BuildManagers(Transform parent)
        {
            var go = new GameObject("GameManagers");
            go.transform.SetParent(parent, false);
            go.AddComponent<RaceUI>();
            go.AddComponent<RaceManager>();
            go.AddComponent<MusicManager>();
            go.AddComponent<CinematicOverlay>();
        }

        // ---------------- Kulisy ----------------

        private static void BuildScenery(Transform parent)
        {
            var scenery = new GameObject("Scenery");
            scenery.transform.SetParent(parent, false);

            // Stromy a kameny v prstenci kolem trati.
            for (int i = 0; i < 40; i++)
            {
                float ang = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float r = Random.Range(46f, 74f);
                var pos = new Vector3(Mathf.Sin(ang) * r, 0f, Mathf.Cos(ang) * r);
                if (i % 3 == 0)
                    PlaceModel(scenery.transform, "Props", Rocks[Random.Range(0, Rocks.Length)], pos,
                        Random.Range(2f, 4f), ModelLibrary.Fit.Height, new Color(0.5f, 0.5f, 0.52f));
                else
                    PlaceModel(scenery.transform, "Props", Trees[Random.Range(0, Trees.Length)], pos,
                        Random.Range(4f, 7f), ModelLibrary.Fit.Height, new Color(0.24f, 0.5f, 0.26f));
            }

            // Kužely rámující trať (vnější prstenec).
            for (int i = 0; i < 28; i++)
            {
                float a = 2f * Mathf.PI * i / 28f;
                var pos = new Vector3(Mathf.Sin(a) * 42f, 0f, Mathf.Cos(a) * 38f);
                PlaceCone(scenery.transform, pos);
            }
        }

        private static void PlaceModel(Transform parent, string category, string modelName, Vector3 pos,
            float size, ModelLibrary.Fit fit, Color tint)
        {
            var prefab = ModelLibrary.Load(category, modelName);
            if (prefab == null) return; // kulisa je nepovinná
            var go = new GameObject(modelName);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            ModelLibrary.Spawn(go, prefab, size, fit, tint, category, 0f);
        }

        private static void PlaceCone(Transform parent, Vector3 pos)
        {
            var prefab = ModelLibrary.Load("Cars", "cone");
            if (prefab != null)
            {
                var go = new GameObject("Cone");
                go.transform.SetParent(parent, false);
                go.transform.position = pos;
                if (ModelLibrary.Spawn(go, prefab, 1.1f, ModelLibrary.Fit.Height, new Color(0.95f, 0.45f, 0.05f), "Cars", 0f) != null)
                    return;
            }
            var c = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            c.name = "Cone";
            var col = c.GetComponent<Collider>();
            if (col != null) { if (Application.isPlaying) Object.Destroy(col); else Object.DestroyImmediate(col); }
            c.transform.SetParent(parent, false);
            c.transform.position = pos + Vector3.up * 0.4f;
            c.transform.localScale = new Vector3(0.3f, 0.4f, 0.3f);
            c.GetComponent<Renderer>().sharedMaterial = MaterialFactory.Create(new Color(0.95f, 0.45f, 0.05f));
        }
    }
}
