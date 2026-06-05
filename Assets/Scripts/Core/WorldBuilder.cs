using UnityEngine;
using UnityEngine.Rendering;
using Autobazar.Managers;
using Autobazar.Player;
using Autobazar.Interaction;
using Autobazar.Vehicles;

namespace Autobazar.Core
{
    /// <summary>
    /// Postaví celou scénu autobazaru z jednoduchých primitiv (kostky, válce).
    /// Tím odpadá ruční sestavování scény v editoru – stačí zavolat BuildWorld()
    /// (přes menu "Autobazar/Build Phase 1 Scene" nebo přes GameBootstrap při Play).
    ///
    /// FÁZE 1: svět, hráč, kamera, interakce, 5 aut s daty, UI.
    /// </summary>
    public static class WorldBuilder
    {
        private const string RootName = "AutobazarWorld";

        public static void BuildWorld()
        {
            if (GameObject.Find(RootName) != null)
            {
                Debug.Log("[Autobazar] Svět už ve scéně existuje – nestavím znovu.");
                return;
            }

            var root = new GameObject(RootName);

            BuildGround(root.transform);
            BuildLighting();
            BuildManagers(root.transform);
            var player = BuildPlayer(root.transform);
            BuildCamera(player.transform);
            BuildBuildings(root.transform);
            BuildZones(root.transform);
            BuildParkingAndCars(root.transform);
            BuildDecor(root.transform);

            Debug.Log("[Autobazar] Scéna Fáze 1 postavena. Stiskni Play a vyzkoušej WASD + myš + E.");
        }

        // ---------------- Zem a osvětlení ----------------

        private static void BuildGround(Transform parent)
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.SetParent(parent, false);
            ground.transform.localScale = new Vector3(6f, 1f, 6f); // 60 x 60 m
            ground.GetComponent<Renderer>().sharedMaterial =
                MaterialFactory.Create(new Color(0.17f, 0.17f, 0.19f)); // asfalt
        }

        private static void BuildLighting()
        {
            bool hasDirectional = false;
            foreach (var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
                if (l.type == LightType.Directional) hasDirectional = true;

            if (!hasDirectional)
            {
                var sun = new GameObject("Sun");
                var light = sun.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.15f;
                light.shadows = LightShadows.Soft;
                sun.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            }

            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.55f, 0.57f, 0.6f);
        }

        // ---------------- Manažeři, hráč, kamera ----------------

        private static void BuildManagers(Transform parent)
        {
            var go = new GameObject("GameManagers");
            go.transform.SetParent(parent, false);

            if (Object.FindFirstObjectByType<EconomyManager>() == null) go.AddComponent<EconomyManager>();
            if (Object.FindFirstObjectByType<ReputationManager>() == null) go.AddComponent<ReputationManager>();
            if (Object.FindFirstObjectByType<TaskManager>() == null) go.AddComponent<TaskManager>();
            if (Object.FindFirstObjectByType<UIManager>() == null) go.AddComponent<UIManager>();
        }

        private static GameObject BuildPlayer(Transform parent)
        {
            var existing = Object.FindFirstObjectByType<PlayerController>();
            if (existing != null) return existing.gameObject;

            var player = new GameObject("Player");
            player.transform.SetParent(parent, false);
            player.transform.position = new Vector3(0f, 0.2f, -18f);

            var cc = player.AddComponent<CharacterController>();
            cc.height = 2f;
            cc.radius = 0.4f;
            cc.center = new Vector3(0f, 1f, 0f);

            player.AddComponent<PlayerController>();
            player.AddComponent<InteractionSystem>();

            // Vizuální tělo (kapsle bez vlastního collideru – kolize řeší CharacterController).
            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            SafeDestroy(body.GetComponent<Collider>());
            body.transform.SetParent(player.transform, false);
            body.transform.localPosition = new Vector3(0f, 1f, 0f);
            body.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
            body.GetComponent<Renderer>().sharedMaterial =
                MaterialFactory.Create(new Color(0.2f, 0.45f, 0.85f));

            // "Nos" ukazující směr pohledu.
            var nose = GameObject.CreatePrimitive(PrimitiveType.Cube);
            nose.name = "Nose";
            SafeDestroy(nose.GetComponent<Collider>());
            nose.transform.SetParent(player.transform, false);
            nose.transform.localPosition = new Vector3(0f, 1.35f, 0.42f);
            nose.transform.localScale = new Vector3(0.22f, 0.22f, 0.22f);
            nose.GetComponent<Renderer>().sharedMaterial =
                MaterialFactory.Create(new Color(0.95f, 0.8f, 0.2f));

            TextFactory.Create("PRODAVAČ", player.transform, new Vector3(0f, 2.5f, 0f),
                50, 0.08f, Color.white, TextAnchor.LowerCenter, true);

            return player;
        }

        private static void BuildCamera(Transform target)
        {
            Camera cam = Camera.main;
            GameObject camGo;

            if (cam != null)
            {
                camGo = cam.gameObject;
            }
            else
            {
                camGo = new GameObject("Main Camera");
                cam = camGo.AddComponent<Camera>();
                camGo.tag = "MainCamera";
            }

            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.55f, 0.75f, 0.95f); // obloha

            if (Object.FindFirstObjectByType<AudioListener>() == null)
                camGo.AddComponent<AudioListener>();

            var controller = camGo.GetComponent<CameraController>();
            if (controller == null) controller = camGo.AddComponent<CameraController>();
            controller.SetTarget(target);
        }

        // ---------------- Budovy ----------------

        private static void BuildBuildings(Transform parent)
        {
            // Kancelář (vlevo / západ)
            var officeColor = new Color(0.85f, 0.82f, 0.7f);
            CreateBox("Office", parent, new Vector3(-22f, 2f, 0f), new Vector3(8f, 4f, 6f), officeColor);
            CreateBox("Office_Door", parent, new Vector3(-18.1f, 1.2f, 0f), new Vector3(0.2f, 2.4f, 1.6f),
                new Color(0.35f, 0.25f, 0.18f), collider: false);
            CreateSign("KANCELÁŘ", parent, new Vector3(-22f, 4.8f, 0f));

            // Servis / garáž (vpravo / východ), otevřená směrem doprostřed (na západ)
            var garageColor = new Color(0.5f, 0.52f, 0.55f);
            CreateBox("Garage_BackWall", parent, new Vector3(25.5f, 2f, 0f), new Vector3(0.3f, 4f, 8f), garageColor);
            CreateBox("Garage_WallN", parent, new Vector3(22f, 2f, 4f), new Vector3(7f, 4f, 0.3f), garageColor);
            CreateBox("Garage_WallS", parent, new Vector3(22f, 2f, -4f), new Vector3(7f, 4f, 0.3f), garageColor);
            CreateBox("Garage_Roof", parent, new Vector3(22f, 4f, 0f), new Vector3(7.3f, 0.3f, 8.3f), garageColor);
            CreateSign("SERVIS", parent, new Vector3(20f, 4.6f, 0f));
        }

        // ---------------- Zóny ----------------

        private static void BuildZones(Transform parent)
        {
            // Vstup / zóna pro zákazníky (jih)
            CreateZonePatch("Zone_Entrance", parent, new Vector3(0f, 0.03f, -21f),
                new Vector3(12f, 0.02f, 5f), new Color(0.3f, 0.5f, 0.85f, 1f));
            CreateSign("VSTUP / ZÁKAZNÍCI", parent, new Vector3(0f, 2.6f, -23f), new Color(0.6f, 0.85f, 1f));

            // Zóna předání auta
            CreateZonePatch("Zone_Delivery", parent, new Vector3(13f, 0.03f, -18f),
                new Vector3(6f, 0.02f, 6f), new Color(0.3f, 0.7f, 0.35f, 1f));
            CreateSign("PŘEDÁNÍ AUT", parent, new Vector3(13f, 2.4f, -21f), new Color(0.6f, 1f, 0.7f));

            // Náznak testovacího okruhu (sever) – zatím jen cedule + kužely
            CreateSign("TESTOVACÍ OKRUH (brzy)", parent, new Vector3(0f, 2.6f, 22f), new Color(1f, 0.85f, 0.5f));
        }

        // ---------------- Parkoviště a auta ----------------

        private static void BuildParkingAndCars(Transform parent)
        {
            // 5 aut Fáze 1 (název, typ, cena, stav, atraktivita, potřebuje servis, barva)
            var defs = new (string name, CarType type, int price, int cond, int attr, bool service, Color color)[]
            {
                ("Škoda Felicia", CarType.Levne,     35000,  60, 40, true,  new Color(0.75f, 0.2f, 0.2f)),
                ("VW Golf",       CarType.Rodinne,    95000,  80, 65, false, new Color(0.75f, 0.75f, 0.78f)),
                ("BMW E46",       CarType.Sportovni, 150000,  55, 82, true,  new Color(0.15f, 0.25f, 0.6f)),
                ("Ford Transit",  CarType.Pracovni,  120000,  70, 45, false, new Color(0.9f, 0.9f, 0.92f)),
                ("Toyota Corolla",CarType.Rodinne,   110000,  90, 70, false, new Color(0.2f, 0.55f, 0.3f)),
            };

            float[] xs = { -12f, -6f, 0f, 6f, 12f };
            float z = 6f;

            for (int i = 0; i < defs.Length; i++)
            {
                var d = defs[i];

                // Parkovací místo (světlejší obdélník na zemi).
                CreateZonePatch($"ParkingSpot_{i}", parent, new Vector3(xs[i], 0.02f, z),
                    new Vector3(2.6f, 0.02f, 5f), new Color(0.32f, 0.32f, 0.35f, 1f));

                var data = new CarData
                {
                    carName = d.name,
                    type = d.type,
                    price = d.price,
                    condition = d.cond,
                    attractiveness = d.attr,
                    needsService = d.service
                };

                BuildCar(parent, data, new Vector3(xs[i], 0f, z), 180f, d.color);
            }
        }

        private static void BuildCar(Transform parent, CarData data, Vector3 position, float eulerY, Color bodyColor)
        {
            var car = new GameObject(data.carName);
            car.transform.SetParent(parent, false);
            car.transform.position = position;
            car.transform.rotation = Quaternion.Euler(0f, eulerY, 0f);

            // Karoserie (jediný díl s colliderem – brání projití hráče autem).
            CreateBoxLocal("Body", car.transform, new Vector3(0f, 0.6f, 0f),
                new Vector3(1.8f, 0.6f, 4f), bodyColor, collider: true);

            // Kabina.
            CreateBoxLocal("Cabin", car.transform, new Vector3(0f, 1.05f, -0.2f),
                new Vector3(1.6f, 0.6f, 2f), bodyColor * 0.8f, collider: false);

            // Okna (tmavý pruh).
            CreateBoxLocal("Windows", car.transform, new Vector3(0f, 1.1f, -0.2f),
                new Vector3(1.62f, 0.4f, 1.6f), new Color(0.1f, 0.12f, 0.15f), collider: false);

            // 4 kola.
            var wheelColor = new Color(0.08f, 0.08f, 0.08f);
            BuildWheel(car.transform, new Vector3(0.95f, 0.35f, 1.3f), wheelColor);
            BuildWheel(car.transform, new Vector3(-0.95f, 0.35f, 1.3f), wheelColor);
            BuildWheel(car.transform, new Vector3(0.95f, 0.35f, -1.3f), wheelColor);
            BuildWheel(car.transform, new Vector3(-0.95f, 0.35f, -1.3f), wheelColor);

            // Data + interakce (cedule se vytvoří v CarInteractable.Start za běhu).
            var interactable = car.AddComponent<CarInteractable>();
            interactable.SetData(data);
        }

        private static void BuildWheel(Transform parent, Vector3 localPos, Color color)
        {
            var wheel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            wheel.name = "Wheel";
            SafeDestroy(wheel.GetComponent<Collider>());
            wheel.transform.SetParent(parent, false);
            wheel.transform.localPosition = localPos;
            wheel.transform.localRotation = Quaternion.Euler(0f, 0f, 90f); // položit válec na bok
            wheel.transform.localScale = new Vector3(0.7f, 0.1f, 0.7f);    // průměr ~0.35, šířka ~0.2
            wheel.GetComponent<Renderer>().sharedMaterial = MaterialFactory.Create(color);
        }

        // ---------------- Dekorace ----------------

        private static void BuildDecor(Transform parent)
        {
            // Pár oranžových kuželů u "testovacího okruhu".
            var cone = new Color(0.95f, 0.45f, 0.05f);
            for (int i = 0; i < 6; i++)
            {
                float angle = -60f + i * 24f;
                float rad = angle * Mathf.Deg2Rad;
                var pos = new Vector3(Mathf.Sin(rad) * 9f, 0.4f, 18f + Mathf.Cos(rad) * 4f);
                CreateMarker($"Cone_{i}", parent, pos, cone);
            }
        }

        // ---------------- Pomocné stavební metody ----------------

        private static GameObject CreateBox(string name, Transform parent, Vector3 worldPos, Vector3 scale,
            Color color, bool collider = true)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = worldPos;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = MaterialFactory.Create(color);
            if (!collider) SafeDestroy(go.GetComponent<Collider>());
            return go;
        }

        private static GameObject CreateBoxLocal(string name, Transform parent, Vector3 localPos, Vector3 scale,
            Color color, bool collider = true)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = MaterialFactory.Create(color);
            if (!collider) SafeDestroy(go.GetComponent<Collider>());
            return go;
        }

        private static void CreateZonePatch(string name, Transform parent, Vector3 worldPos, Vector3 scale, Color color)
        {
            var go = CreateBox(name, parent, worldPos, scale, color, collider: false);
        }

        private static void CreateMarker(string name, Transform parent, Vector3 worldPos, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            SafeDestroy(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            go.transform.position = worldPos;
            go.transform.localScale = new Vector3(0.3f, 0.4f, 0.3f);
            go.GetComponent<Renderer>().sharedMaterial = MaterialFactory.Create(color);
        }

        private static void CreateSign(string text, Transform parent, Vector3 worldPos, Color? color = null)
        {
            // Root je v počátku bez transformace, takže lokální pozice == světová.
            TextFactory.Create(text, parent, worldPos, 60, 0.22f,
                color ?? new Color(1f, 0.95f, 0.6f), TextAnchor.MiddleCenter, true);
        }

        private static void SafeDestroy(Object obj)
        {
            if (obj == null) return;
            if (Application.isPlaying) Object.Destroy(obj);
            else Object.DestroyImmediate(obj);
        }
    }
}
