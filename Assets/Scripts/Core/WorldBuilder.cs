using UnityEngine;
using UnityEngine.Rendering;
using Autobazar.Managers;
using Autobazar.Player;
using Autobazar.Interaction;
using Autobazar.Vehicles;
using Autobazar.People;
using Autobazar.UI;
using Autobazar.Buildings;

namespace Autobazar.Core
{
    /// <summary>
    /// Postaví celou scénu autobazaru z primitiv. Žádné ruční skládání ani křehké
    /// .unity soubory – stačí menu "Autobazar/Build Phase 1 Scene" nebo GameBootstrap.
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

            GameState.InputLocked = false;

            var root = new GameObject(RootName);

            BuildAtmosphere();
            BuildGround(root.transform);
            BuildManagers(root.transform);
            var player = BuildPlayer(root.transform);
            BuildCamera(player.transform);
            BuildBuildings(root.transform);
            BuildZones(root.transform);
            BuildParkingAndCars(root.transform);
            BuildEnvironment(root.transform);
            BuildDecor(root.transform);

            Debug.Log("[Autobazar] Scéna Fáze 1 postavena. Stiskni Play (WASD + myš + E).");
        }

        // ---------------- Atmosféra (obloha, světlo, mlha) ----------------

        private static void BuildAtmosphere()
        {
            // Slunce
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
            sun.transform.rotation = Quaternion.Euler(45f, 40f, 0f);

            // Procedurální obloha
            var skyShader = Shader.Find("Skybox/Procedural");
            if (skyShader != null)
            {
                var sky = new Material(skyShader);
                if (sky.HasProperty("_SkyTint")) sky.SetColor("_SkyTint", new Color(0.52f, 0.66f, 0.9f));
                if (sky.HasProperty("_GroundColor")) sky.SetColor("_GroundColor", new Color(0.32f, 0.32f, 0.34f));
                if (sky.HasProperty("_AtmosphereThickness")) sky.SetFloat("_AtmosphereThickness", 1.1f);
                if (sky.HasProperty("_Exposure")) sky.SetFloat("_Exposure", 1.2f);
                if (sky.HasProperty("_SunSize")) sky.SetFloat("_SunSize", 0.045f);
                RenderSettings.skybox = sky;
                RenderSettings.ambientMode = AmbientMode.Skybox;
                RenderSettings.ambientIntensity = 1f;
                DynamicGI.UpdateEnvironment();
            }
            else
            {
                RenderSettings.ambientMode = AmbientMode.Flat;
                RenderSettings.ambientLight = new Color(0.55f, 0.57f, 0.6f);
            }

            // Mlha – dodá hloubku a schová „konec světa"
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.72f, 0.79f, 0.88f);
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 45f;
            RenderSettings.fogEndDistance = 150f;
        }

        private static void BuildGround(Transform parent)
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.SetParent(parent, false);
            ground.transform.localScale = new Vector3(6f, 1f, 6f);
            ground.GetComponent<Renderer>().sharedMaterial =
                MaterialFactory.Create(new Color(0.16f, 0.16f, 0.18f), smoothness: 0.35f);
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
            if (Object.FindFirstObjectByType<DialogUI>() == null) go.AddComponent<DialogUI>();
            if (Object.FindFirstObjectByType<CarServiceUI>() == null) go.AddComponent<CarServiceUI>();
            if (Object.FindFirstObjectByType<CustomerSpawner>() == null) go.AddComponent<CustomerSpawner>();
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

            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            SafeDestroy(body.GetComponent<Collider>());
            body.transform.SetParent(player.transform, false);
            body.transform.localPosition = new Vector3(0f, 1f, 0f);
            body.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
            body.GetComponent<Renderer>().sharedMaterial =
                MaterialFactory.Create(new Color(0.2f, 0.45f, 0.85f), smoothness: 0.4f);

            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            SafeDestroy(head.GetComponent<Collider>());
            head.transform.SetParent(player.transform, false);
            head.transform.localPosition = new Vector3(0f, 1.75f, 0f);
            head.transform.localScale = new Vector3(0.55f, 0.55f, 0.55f);
            head.GetComponent<Renderer>().sharedMaterial =
                MaterialFactory.Create(new Color(0.95f, 0.8f, 0.66f));

            var nose = GameObject.CreatePrimitive(PrimitiveType.Cube);
            nose.name = "Nose";
            SafeDestroy(nose.GetComponent<Collider>());
            nose.transform.SetParent(player.transform, false);
            nose.transform.localPosition = new Vector3(0f, 1.75f, 0.27f);
            nose.transform.localScale = new Vector3(0.12f, 0.12f, 0.12f);
            nose.GetComponent<Renderer>().sharedMaterial =
                MaterialFactory.Create(new Color(0.9f, 0.7f, 0.55f));

            TextFactory.Create("PRODAVAČ", player.transform, new Vector3(0f, 2.5f, 0f),
                50, 0.075f, Color.white, TextAnchor.LowerCenter, true);

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

            cam.clearFlags = CameraClearFlags.Skybox;
            cam.fieldOfView = 60f;
            cam.farClipPlane = 200f;

            if (Object.FindFirstObjectByType<AudioListener>() == null)
                camGo.AddComponent<AudioListener>();

            var controller = camGo.GetComponent<CameraController>();
            if (controller == null) controller = camGo.AddComponent<CameraController>();
            controller.SetTarget(target);
        }

        // ---------------- Budovy ----------------

        private static void BuildBuildings(Transform parent)
        {
            // Kancelář (západ)
            var wall = new Color(0.86f, 0.83f, 0.72f);
            CreateBox("Office", parent, new Vector3(-22f, 2f, 0f), new Vector3(8f, 4f, 6f), wall, smoothness: 0.1f);
            CreateBox("Office_Roof", parent, new Vector3(-22f, 4.1f, 0f), new Vector3(8.6f, 0.3f, 6.6f), new Color(0.4f, 0.2f, 0.18f));
            CreateBox("Office_Door", parent, new Vector3(-18.1f, 1.1f, 0f), new Vector3(0.2f, 2.2f, 1.6f),
                new Color(0.32f, 0.22f, 0.16f), collider: false);
            // okna kanceláře
            CreateGlassBox("Office_Win1", parent, new Vector3(-18.1f, 2.6f, -1.8f), new Vector3(0.1f, 1.2f, 1.4f));
            CreateGlassBox("Office_Win2", parent, new Vector3(-18.1f, 2.6f, 1.8f), new Vector3(0.1f, 1.2f, 1.4f));
            CreateSign("KANCELÁŘ", parent, new Vector3(-22f, 4.9f, 0f), new Color(1f, 0.95f, 0.7f));

            // Servis / garáž (východ), otevřená doprostřed
            var garage = new Color(0.5f, 0.52f, 0.56f);
            CreateBox("Garage_BackWall", parent, new Vector3(25.5f, 2f, 0f), new Vector3(0.3f, 4f, 8f), garage);
            CreateBox("Garage_WallN", parent, new Vector3(22f, 2f, 4f), new Vector3(7f, 4f, 0.3f), garage);
            CreateBox("Garage_WallS", parent, new Vector3(22f, 2f, -4f), new Vector3(7f, 4f, 0.3f), garage);
            CreateBox("Garage_Roof", parent, new Vector3(22f, 4.05f, 0f), new Vector3(7.4f, 0.3f, 8.4f), new Color(0.32f, 0.34f, 0.38f));
            // žlutočerný pruh nad vraty
            CreateBox("Garage_Stripe", parent, new Vector3(18.6f, 3.8f, 0f), new Vector3(0.2f, 0.5f, 8f),
                MaterialFactory.CreateEmissive(new Color(0.9f, 0.7f, 0.1f), new Color(0.9f, 0.6f, 0.1f), 0.6f));
            CreateSign("SERVIS", parent, new Vector3(19.5f, 4.7f, 0f), new Color(1f, 0.8f, 0.4f));

            BuildOfficeDesk(parent);
        }

        private static void BuildOfficeDesk(Transform parent)
        {
            // Počítač na smlouvy – stojí před kanceláří, dosažitelný z plochy bazaru.
            var deskRoot = new GameObject("OfficeComputer");
            deskRoot.transform.SetParent(parent, false);
            deskRoot.transform.position = new Vector3(-16.8f, 0f, 3f);

            CreateBoxLocal("Desk", deskRoot.transform, new Vector3(0f, 0.45f, 0f), new Vector3(1.6f, 0.9f, 0.8f),
                new Color(0.35f, 0.25f, 0.18f));
            CreateBoxLocal("Stand", deskRoot.transform, new Vector3(0f, 1.0f, 0f), new Vector3(0.12f, 0.3f, 0.12f),
                new Color(0.1f, 0.1f, 0.12f), collider: false);

            var screen = CreateBoxLocal("Screen", deskRoot.transform, new Vector3(0f, 1.32f, 0.04f),
                new Vector3(0.75f, 0.46f, 0.06f), Color.black, collider: false);
            screen.GetComponent<Renderer>().sharedMaterial =
                MaterialFactory.CreateEmissive(new Color(0.1f, 0.2f, 0.35f), new Color(0.2f, 0.5f, 0.9f), 0.9f);

            deskRoot.AddComponent<OfficePaperwork>();

            CreateSign("SMLOUVY", parent, new Vector3(-16.8f, 2.1f, 3f), new Color(0.7f, 0.9f, 1f));
        }

        // ---------------- Zóny ----------------

        private static void BuildZones(Transform parent)
        {
            CreateZonePatch("Zone_Entrance", parent, new Vector3(0f, 0.03f, -21f),
                new Vector3(12f, 0.02f, 5f), new Color(0.28f, 0.45f, 0.8f));
            CreateSign("VSTUP / ZÁKAZNÍCI", parent, new Vector3(0f, 1.4f, -19.5f), new Color(0.7f, 0.88f, 1f));

            CreateZonePatch("Zone_Delivery", parent, new Vector3(13f, 0.03f, -18f),
                new Vector3(6f, 0.02f, 6f), new Color(0.28f, 0.65f, 0.34f));
            CreateSign("PŘEDÁNÍ AUT", parent, new Vector3(13f, 1.4f, -18f), new Color(0.65f, 1f, 0.72f));

            CreateSign("TESTOVACÍ OKRUH (brzy)", parent, new Vector3(0f, 1.6f, 21f), new Color(1f, 0.85f, 0.5f));
        }

        // ---------------- Parkoviště a auta ----------------

        private static void BuildParkingAndCars(Transform parent)
        {
            var defs = new (string name, CarType type, int price, int cond, int attr, bool service, Color color)[]
            {
                ("Škoda Felicia", CarType.Levne,     35000,  60, 40, true,  new Color(0.75f, 0.2f, 0.2f)),
                ("VW Golf",       CarType.Rodinne,    95000,  80, 65, false, new Color(0.78f, 0.78f, 0.8f)),
                ("BMW E46",       CarType.Sportovni, 150000,  55, 82, true,  new Color(0.12f, 0.22f, 0.55f)),
                ("Ford Transit",  CarType.Pracovni,  120000,  70, 45, false, new Color(0.92f, 0.92f, 0.94f)),
                ("Toyota Corolla",CarType.Rodinne,   110000,  90, 70, false, new Color(0.18f, 0.5f, 0.28f)),
            };

            float[] xs = { -12f, -6f, 0f, 6f, 12f };
            float z = 6f;

            for (int i = 0; i < defs.Length; i++)
            {
                var d = defs[i];

                CreateZonePatch($"ParkingSpot_{i}", parent, new Vector3(xs[i], 0.02f, z),
                    new Vector3(2.6f, 0.02f, 5f), new Color(0.22f, 0.22f, 0.24f));
                // bílé čáry parkovacího místa
                CreateBox($"Line_{i}_L", parent, new Vector3(xs[i] - 1.3f, 0.04f, z), new Vector3(0.08f, 0.02f, 5f),
                    new Color(0.85f, 0.85f, 0.85f), collider: false);
                CreateBox($"Line_{i}_R", parent, new Vector3(xs[i] + 1.3f, 0.04f, z), new Vector3(0.08f, 0.02f, 5f),
                    new Color(0.85f, 0.85f, 0.85f), collider: false);

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

            // Lesklý metalízový lak (sdílený karoserií a kabinou).
            var paint = MaterialFactory.Create(bodyColor, smoothness: 0.72f, metallic: 0.55f);

            var body = CreateBoxLocal("Body", car.transform, new Vector3(0f, 0.55f, 0f), new Vector3(1.9f, 0.55f, 4.1f), bodyColor);
            body.GetComponent<Renderer>().sharedMaterial = paint;

            var cabin = CreateBoxLocal("Cabin", car.transform, new Vector3(0f, 1.0f, -0.15f), new Vector3(1.7f, 0.55f, 2.1f), bodyColor, collider: false);
            cabin.GetComponent<Renderer>().sharedMaterial = paint;

            // Skla
            var glass = MaterialFactory.CreateGlass(new Color(0.1f, 0.13f, 0.16f));
            var windows = CreateBoxLocal("Windows", car.transform, new Vector3(0f, 1.05f, -0.15f), new Vector3(1.74f, 0.45f, 1.75f), Color.black, collider: false);
            windows.GetComponent<Renderer>().sharedMaterial = glass;

            // Nárazníky
            var bumper = new Color(0.09f, 0.09f, 0.1f);
            CreateBoxLocal("Bumper_F", car.transform, new Vector3(0f, 0.4f, 2.0f), new Vector3(1.92f, 0.35f, 0.25f), bumper, collider: false);
            CreateBoxLocal("Bumper_R", car.transform, new Vector3(0f, 0.4f, -2.0f), new Vector3(1.92f, 0.35f, 0.25f), bumper, collider: false);

            // Světlomety + zadní světla (svítící)
            var headMat = MaterialFactory.CreateEmissive(Color.white, new Color(1f, 0.97f, 0.85f), 1.5f);
            var tailMat = MaterialFactory.CreateEmissive(new Color(0.3f, 0f, 0f), new Color(1f, 0.12f, 0.12f), 1.4f);
            SetMat(CreateBoxLocal("Head_L", car.transform, new Vector3(0.62f, 0.55f, 2.06f), new Vector3(0.34f, 0.18f, 0.08f), Color.white, false), headMat);
            SetMat(CreateBoxLocal("Head_R", car.transform, new Vector3(-0.62f, 0.55f, 2.06f), new Vector3(0.34f, 0.18f, 0.08f), Color.white, false), headMat);
            SetMat(CreateBoxLocal("Tail_L", car.transform, new Vector3(0.62f, 0.55f, -2.06f), new Vector3(0.34f, 0.18f, 0.08f), Color.red, false), tailMat);
            SetMat(CreateBoxLocal("Tail_R", car.transform, new Vector3(-0.62f, 0.55f, -2.06f), new Vector3(0.34f, 0.18f, 0.08f), Color.red, false), tailMat);

            // Kola
            var wheelColor = new Color(0.05f, 0.05f, 0.05f);
            BuildWheel(car.transform, new Vector3(0.96f, 0.35f, 1.3f), wheelColor);
            BuildWheel(car.transform, new Vector3(-0.96f, 0.35f, 1.3f), wheelColor);
            BuildWheel(car.transform, new Vector3(0.96f, 0.35f, -1.3f), wheelColor);
            BuildWheel(car.transform, new Vector3(-0.96f, 0.35f, -1.3f), wheelColor);

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
            wheel.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            wheel.transform.localScale = new Vector3(0.72f, 0.12f, 0.72f);
            wheel.GetComponent<Renderer>().sharedMaterial = MaterialFactory.Create(color, smoothness: 0.2f);

            // stříbrný střed kola
            var hub = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hub.name = "Hub";
            SafeDestroy(hub.GetComponent<Collider>());
            hub.transform.SetParent(wheel.transform, false);
            hub.transform.localScale = new Vector3(0.45f, 1.05f, 0.45f);
            hub.GetComponent<Renderer>().sharedMaterial = MaterialFactory.Create(new Color(0.7f, 0.7f, 0.74f), 0.7f, 0.8f);
        }

        // ---------------- Prostředí (plot, lampy, stromy, brána) ----------------

        private static void BuildEnvironment(Transform parent)
        {
            var env = new GameObject("Environment");
            env.transform.SetParent(parent, false);

            BuildFence(env.transform);

            var lampSpots = new[]
            {
                new Vector3(-14f, 0f, -10f), new Vector3(14f, 0f, -10f),
                new Vector3(-14f, 0f, 14f), new Vector3(14f, 0f, 14f),
            };
            foreach (var p in lampSpots) BuildLamp(env.transform, p);

            var treeSpots = new[]
            {
                new Vector3(-25f, 0f, -22f), new Vector3(25f, 0f, -22f),
                new Vector3(-26f, 0f, 18f), new Vector3(26f, 0f, 16f), new Vector3(-26f, 0f, -2f),
            };
            foreach (var p in treeSpots) BuildTree(env.transform, p);

            BuildEntranceArch(env.transform);
        }

        private static void BuildFence(Transform parent)
        {
            var concrete = new Color(0.4f, 0.4f, 0.43f);
            CreateBox("Wall_N", parent, new Vector3(0f, 0.6f, 26f), new Vector3(56f, 1.2f, 0.4f), concrete);
            CreateBox("Wall_E", parent, new Vector3(28f, 0.6f, 0f), new Vector3(0.4f, 1.2f, 52f), concrete);
            CreateBox("Wall_W", parent, new Vector3(-28f, 0.6f, 0f), new Vector3(0.4f, 1.2f, 52f), concrete);
            // jižní zeď s mezerou pro vstup (x v [-6,6])
            CreateBox("Wall_S_L", parent, new Vector3(-17f, 0.6f, -26f), new Vector3(22f, 1.2f, 0.4f), concrete);
            CreateBox("Wall_S_R", parent, new Vector3(17f, 0.6f, -26f), new Vector3(22f, 1.2f, 0.4f), concrete);

            var post = new Color(0.2f, 0.2f, 0.22f);
            CreateBox("Post_NE", parent, new Vector3(28f, 1f, 26f), new Vector3(0.6f, 2f, 0.6f), post, collider: false);
            CreateBox("Post_NW", parent, new Vector3(-28f, 1f, 26f), new Vector3(0.6f, 2f, 0.6f), post, collider: false);
            CreateBox("Post_SE", parent, new Vector3(28f, 1f, -26f), new Vector3(0.6f, 2f, 0.6f), post, collider: false);
            CreateBox("Post_SW", parent, new Vector3(-28f, 1f, -26f), new Vector3(0.6f, 2f, 0.6f), post, collider: false);
        }

        private static void BuildLamp(Transform parent, Vector3 basePos)
        {
            CreateBox("Lamp_Pole", parent, basePos + Vector3.up * 2.5f, new Vector3(0.15f, 5f, 0.15f),
                new Color(0.13f, 0.13f, 0.15f), collider: false);

            var headPos = basePos + Vector3.up * 5f;
            var bulb = CreateBox("Lamp_Bulb", parent, headPos, new Vector3(0.55f, 0.22f, 0.55f), Color.white, collider: false);
            bulb.GetComponent<Renderer>().sharedMaterial =
                MaterialFactory.CreateEmissive(new Color(1f, 0.96f, 0.82f), new Color(1f, 0.92f, 0.7f), 2f);

            var lightGo = new GameObject("Lamp_Light");
            lightGo.transform.SetParent(parent, false);
            lightGo.transform.position = headPos + Vector3.down * 0.3f;
            var pl = lightGo.AddComponent<Light>();
            pl.type = LightType.Point;
            pl.color = new Color(1f, 0.9f, 0.72f);
            pl.range = 16f;
            pl.intensity = 2.2f;
        }

        private static void BuildTree(Transform parent, Vector3 basePos)
        {
            CreatePart(PrimitiveType.Cylinder, "Tree_Trunk", parent, basePos + Vector3.up * 1.5f,
                new Vector3(0.4f, 1.5f, 0.4f), new Color(0.34f, 0.23f, 0.14f));
            CreatePart(PrimitiveType.Sphere, "Tree_Canopy1", parent, basePos + Vector3.up * 3.2f,
                new Vector3(2.6f, 2.4f, 2.6f), new Color(0.2f, 0.44f, 0.22f));
            CreatePart(PrimitiveType.Sphere, "Tree_Canopy2", parent, basePos + Vector3.up * 4.3f,
                new Vector3(1.8f, 1.7f, 1.8f), new Color(0.24f, 0.5f, 0.26f));
        }

        private static void BuildEntranceArch(Transform parent)
        {
            var dark = new Color(0.2f, 0.2f, 0.22f);
            CreateBox("Arch_PostL", parent, new Vector3(-6f, 2.5f, -26f), new Vector3(0.6f, 5f, 0.6f), dark);
            CreateBox("Arch_PostR", parent, new Vector3(6f, 2.5f, -26f), new Vector3(0.6f, 5f, 0.6f), dark);
            var banner = CreateBox("Arch_Banner", parent, new Vector3(0f, 5.4f, -26f), new Vector3(12.8f, 1.6f, 0.4f), dark);
            banner.GetComponent<Renderer>().sharedMaterial =
                MaterialFactory.CreateEmissive(new Color(0.1f, 0.25f, 0.6f), new Color(0.2f, 0.45f, 1f), 1.1f);
            TextFactory.Create("AUTOBAZAR", parent, new Vector3(0f, 5.4f, -25.7f),
                70, 0.26f, Color.white, TextAnchor.MiddleCenter, true);
        }

        // ---------------- Dekorace ----------------

        private static void BuildDecor(Transform parent)
        {
            var cone = new Color(0.95f, 0.45f, 0.05f);
            for (int i = 0; i < 6; i++)
            {
                float angle = -60f + i * 24f;
                float rad = angle * Mathf.Deg2Rad;
                var pos = new Vector3(Mathf.Sin(rad) * 9f, 0.4f, 18f + Mathf.Cos(rad) * 4f);
                CreatePart(PrimitiveType.Cylinder, $"Cone_{i}", parent, pos, new Vector3(0.3f, 0.4f, 0.3f), cone);
            }
        }

        // ---------------- Pomocné stavební metody ----------------

        private static GameObject CreateBox(string name, Transform parent, Vector3 worldPos, Vector3 scale,
            Color color, bool collider = true, float smoothness = 0.2f)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = worldPos;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = MaterialFactory.Create(color, smoothness);
            if (!collider) SafeDestroy(go.GetComponent<Collider>());
            return go;
        }

        private static GameObject CreateBox(string name, Transform parent, Vector3 worldPos, Vector3 scale, Material material)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = worldPos;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = material;
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

        private static GameObject CreatePart(PrimitiveType type, string name, Transform parent, Vector3 worldPos,
            Vector3 scale, Color color)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            SafeDestroy(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            go.transform.position = worldPos;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = MaterialFactory.Create(color);
            return go;
        }

        private static void CreateGlassBox(string name, Transform parent, Vector3 worldPos, Vector3 scale)
        {
            var go = CreateBox(name, parent, worldPos, scale, new Color(0.1f, 0.13f, 0.16f), collider: false);
            go.GetComponent<Renderer>().sharedMaterial = MaterialFactory.CreateGlass(new Color(0.12f, 0.18f, 0.24f));
        }

        private static void CreateZonePatch(string name, Transform parent, Vector3 worldPos, Vector3 scale, Color color)
        {
            CreateBox(name, parent, worldPos, scale, color, collider: false);
        }

        private static void CreateSign(string text, Transform parent, Vector3 worldPos, Color? color = null)
        {
            TextFactory.Create(text, parent, worldPos, 60, 0.22f,
                color ?? new Color(1f, 0.95f, 0.6f), TextAnchor.MiddleCenter, true);
        }

        private static void SetMat(GameObject go, Material mat)
        {
            if (go != null) go.GetComponent<Renderer>().sharedMaterial = mat;
        }

        private static void SafeDestroy(Object obj)
        {
            if (obj == null) return;
            if (Application.isPlaying) Object.Destroy(obj);
            else Object.DestroyImmediate(obj);
        }
    }
}
