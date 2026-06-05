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
    /// Postaví scénu „Checkpoint Rush": asfaltová trať se středovými čárami, zem,
    /// zlatá hodina (světlo + mlha), auto s prachem a kamerou, hustá příroda a manažeři.
    /// </summary>
    public static class RaceWorldBuilder
    {
        private const string RootName = "RaceWorld";

        private static readonly string[] Trees =
            { "tree_default", "tree_oak", "tree_pineDefaultA", "tree_fat", "tree_detailed", "tree_pineRoundB", "tree_pineTallA", "tree_pineGroundA" };
        private static readonly string[] Rocks =
            { "rock_largeA", "rock_largeB", "rock_largeC", "rock_tallA", "rock_tallB", "stone_largeA", "stone_tallC", "rock_smallA" };
        private static readonly string[] Foliage =
            { "grass", "grass_large", "plant_bush", "plant_bushSmall", "plant_bushLarge", "flower_redA", "flower_yellowB", "flower_purpleC", "mushroom_redGroup" };

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
            BuildRoad(root.transform);
            var car = BuildCar(root.transform);
            BuildCamera(car.transform);
            BuildManagers(root.transform);
            BuildScenery(root.transform);

            Debug.Log("[CheckpointRush] Scéna postavena. Stiskni Play a Enter.");
        }

        // ---------------- Atmosféra (zlatá hodina) ----------------

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
            sun.intensity = 1.35f;
            sun.color = new Color(1f, 0.86f, 0.66f);            // teplé pozdně odpolední světlo
            sun.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(26f, 35f, 0f); // nízké slunce → dlouhé stíny

            var skyShader = Shader.Find("Skybox/Procedural");
            if (skyShader != null)
            {
                var sky = new Material(skyShader);
                if (sky.HasProperty("_SkyTint")) sky.SetColor("_SkyTint", new Color(0.55f, 0.6f, 0.78f));
                if (sky.HasProperty("_GroundColor")) sky.SetColor("_GroundColor", new Color(0.32f, 0.3f, 0.27f));
                if (sky.HasProperty("_AtmosphereThickness")) sky.SetFloat("_AtmosphereThickness", 1.35f);
                if (sky.HasProperty("_Exposure")) sky.SetFloat("_Exposure", 1.25f);
                if (sky.HasProperty("_SunSize")) sky.SetFloat("_SunSize", 0.06f);
                RenderSettings.skybox = sky;
                RenderSettings.ambientMode = AmbientMode.Skybox;
                DynamicGI.UpdateEnvironment();
            }
            else
            {
                RenderSettings.ambientMode = AmbientMode.Flat;
                RenderSettings.ambientLight = new Color(0.55f, 0.52f, 0.5f);
            }

            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.85f, 0.78f, 0.68f);  // teplý opar
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 80f;
            RenderSettings.fogEndDistance = 300f;
        }

        private static void BuildGround(Transform parent)
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.SetParent(parent, false);
            ground.transform.localScale = new Vector3(22f, 1f, 22f); // 220 x 220
            ground.GetComponent<Renderer>().sharedMaterial =
                MaterialFactory.Create(new Color(0.24f, 0.36f, 0.2f), smoothness: 0.05f); // tráva
        }

        // ---------------- Silnice ----------------

        private static void BuildRoad(Transform parent)
        {
            var road = new GameObject("Road");
            road.transform.SetParent(parent, false);

            var asphalt = new Color(0.12f, 0.12f, 0.14f);
            var white = new Color(0.85f, 0.85f, 0.82f);
            int n = TrackLayout.GateCount;

            for (int i = 0; i < n; i++)
            {
                Vector3 a = TrackLayout.GatePos(i);
                Vector3 b = TrackLayout.GatePos((i + 1) % n);
                Vector3 dir = b - a; dir.y = 0f;
                float len = dir.magnitude;
                if (len < 0.01f) continue;
                Quaternion rot = Quaternion.LookRotation(dir.normalized, Vector3.up);
                Vector3 mid = (a + b) * 0.5f; mid.y = 0.03f;

                CreateOrientedBox(road.transform, mid, rot, new Vector3(12f, 0.08f, len + 1.2f), asphalt, 0.45f);

                // středové přerušované čáry
                int dashes = Mathf.Max(1, Mathf.RoundToInt(len / 5f));
                for (int d = 0; d < dashes; d++)
                {
                    float t = (d + 0.5f) / dashes;
                    Vector3 p = Vector3.Lerp(a, b, t); p.y = 0.06f;
                    CreateOrientedBox(road.transform, p, rot, new Vector3(0.35f, 0.02f, 2f), white, 0.2f);
                }
            }
        }

        // ---------------- Auto, kamera, manažeři ----------------

        private static GameObject BuildCar(Transform parent)
        {
            var car = new GameObject("Car");
            car.transform.SetParent(parent, false);
            car.transform.position = Vector3.zero;

            var prefab = ModelLibrary.Load("Cars", "sedan-sports");
            if (prefab == null || ModelLibrary.Spawn(car, prefab, ModelLibrary.CarTargetLength,
                    ModelLibrary.Fit.CarLength, new Color(0.8f, 0.15f, 0.15f), "Cars", ModelLibrary.CarYaw) == null)
            {
                var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
                body.name = "Model";
                body.transform.SetParent(car.transform, false);
                body.transform.localPosition = new Vector3(0f, 0.5f, 0f);
                body.transform.localScale = new Vector3(1.8f, 0.8f, 4f);
                body.GetComponent<Renderer>().sharedMaterial = MaterialFactory.Create(new Color(0.8f, 0.15f, 0.15f), 0.6f, 0.4f);
            }

            car.AddComponent<CarController>();
            car.AddComponent<CarEffects>();
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
            cam.farClipPlane = 320f;

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

            // Stromy a kameny v širokém prstenci kolem trati.
            for (int i = 0; i < 70; i++)
            {
                float ang = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float r = Random.Range(56f, 96f);
                var pos = new Vector3(Mathf.Sin(ang) * r, 0f, Mathf.Cos(ang) * r);
                if (i % 3 == 0)
                    PlaceModel(scenery.transform, "Props", Rocks[Random.Range(0, Rocks.Length)], pos,
                        Random.Range(2f, 4.5f), new Color(0.5f, 0.5f, 0.52f));
                else
                    PlaceModel(scenery.transform, "Props", Trees[Random.Range(0, Trees.Length)], pos,
                        Random.Range(4f, 8f), new Color(0.24f, 0.48f, 0.26f));
            }

            // Tráva, keře, kytky – po celé ploše (i uvnitř oválu).
            for (int i = 0; i < 90; i++)
            {
                float ang = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float r = Random.Range(6f, 100f);
                if (r > 30f && r < 52f) continue; // ať nerostou na silnici
                var pos = new Vector3(Mathf.Sin(ang) * r, 0f, Mathf.Cos(ang) * r);
                PlaceModel(scenery.transform, "Props", Foliage[Random.Range(0, Foliage.Length)], pos,
                    Random.Range(0.6f, 1.8f), new Color(0.3f, 0.55f, 0.28f));
            }

            // Kužely rámující silnici (vnitřní i vnější okraj).
            int cones = 30;
            for (int i = 0; i < cones; i++)
            {
                float a = 2f * Mathf.PI * i / cones;
                PlaceCone(scenery.transform, new Vector3(Mathf.Sin(a) * (TrackLayout.RadiusX + 7f), 0f, Mathf.Cos(a) * (TrackLayout.RadiusZ + 7f)));
                PlaceCone(scenery.transform, new Vector3(Mathf.Sin(a) * (TrackLayout.RadiusX - 7f), 0f, Mathf.Cos(a) * (TrackLayout.RadiusZ - 7f)));
            }
        }

        private static void PlaceModel(Transform parent, string category, string modelName, Vector3 pos, float size, Color tint)
        {
            var prefab = ModelLibrary.Load(category, modelName);
            if (prefab == null) return;
            var go = new GameObject(modelName);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.transform.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            ModelLibrary.Spawn(go, prefab, size, ModelLibrary.Fit.Height, tint, category, 0f);
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
            StripCollider(c);
            c.transform.SetParent(parent, false);
            c.transform.position = pos + Vector3.up * 0.4f;
            c.transform.localScale = new Vector3(0.3f, 0.4f, 0.3f);
            c.GetComponent<Renderer>().sharedMaterial = MaterialFactory.Create(new Color(0.95f, 0.45f, 0.05f));
        }

        private static void CreateOrientedBox(Transform parent, Vector3 pos, Quaternion rot, Vector3 scale, Color color, float smoothness)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "RoadPiece";
            StripCollider(go);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.transform.rotation = rot;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = MaterialFactory.Create(color, smoothness);
        }

        private static void StripCollider(GameObject go)
        {
            var col = go.GetComponent<Collider>();
            if (col == null) return;
            if (Application.isPlaying) Object.Destroy(col); else Object.DestroyImmediate(col);
        }
    }
}
