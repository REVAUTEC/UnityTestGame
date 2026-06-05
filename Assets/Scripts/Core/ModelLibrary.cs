using UnityEngine;

namespace Autobazar.Core
{
    /// <summary>
    /// Načítá hotové 3D modely ze složek Assets/Resources/Kits/... a vsazuje je do scény:
    /// automaticky je zvětší na rozumnou velikost, posadí na zem a obarví – buď texturou
    /// (colormap.png ve stejné složce), nebo náhradní barvou, když textura není.
    /// Když model chybí, volající si postaví zálohu (kostky/kapsle).
    /// </summary>
    public static class ModelLibrary
    {
        public enum Fit { CarLength, Height }

        // Auta se zvětší na tuto délku; kdyby koukala obráceně, přehoď Yaw na 180.
        public const float CarTargetLength = 4.3f;
        public const float CarYaw = 0f;
        public const float PeopleYaw = 0f;

        /// <summary>Načte jeden model podle přesného názvu (bez přípony).</summary>
        public static GameObject Load(string category, string name)
            => Resources.Load<GameObject>($"Kits/{category}/{name}");

        /// <summary>Načte všechny modely v dané kategorii.</summary>
        public static GameObject[] LoadAll(string category)
            => Resources.LoadAll<GameObject>($"Kits/{category}");

        /// <summary>Najde sdílenou texturu (colormap) dané kategorie, nebo null.</summary>
        public static Texture2D Colormap(string category)
        {
            var tex = Resources.Load<Texture2D>($"Kits/{category}/colormap");
            if (tex != null) return tex;
            var all = Resources.LoadAll<Texture2D>($"Kits/{category}");
            return (all != null && all.Length > 0) ? all[0] : null;
        }

        /// <summary>
        /// Vloží model do rootu, zvětší na cílovou velikost, posadí na zem a obarví.
        /// Vrátí instanci, nebo null při neúspěchu.
        /// </summary>
        public static GameObject Spawn(GameObject root, GameObject prefab, float targetSize, Fit fit,
            Color fallbackTint, string category, float yawOffset = 0f)
        {
            if (root == null || prefab == null) return null;

            var inst = Object.Instantiate(prefab);
            inst.name = "Model";
            inst.transform.SetParent(root.transform, false);
            inst.transform.localPosition = Vector3.zero;
            inst.transform.localRotation = Quaternion.Euler(0f, yawOffset, 0f);

            var rends = inst.GetComponentsInChildren<Renderer>();
            if (rends.Length == 0) { DestroySafe(inst); return null; }

            // Auto-fit na cílovou velikost.
            Bounds b = rends[0].bounds;
            for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
            float dim = fit == Fit.Height ? b.size.y : Mathf.Max(b.size.x, b.size.z);
            inst.transform.localScale *= targetSize / Mathf.Max(dim, 0.01f);

            // Posadit spodkem na základnu rootu.
            rends = inst.GetComponentsInChildren<Renderer>();
            b = rends[0].bounds;
            for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
            inst.transform.position += new Vector3(0f, root.transform.position.y - b.min.y, 0f);

            // Materiál: textura (colormap) když je, jinak náhradní barva.
            var colormap = Colormap(category);
            Material mat = colormap != null
                ? MaterialFactory.CreateTextured(colormap)
                : MaterialFactory.Create(fallbackTint, 0.45f, 0.15f);

            foreach (var r in rends)
            {
                int count = Mathf.Max(1, r.sharedMaterials.Length);
                var arr = new Material[count];
                for (int i = 0; i < count; i++) arr[i] = mat;
                r.sharedMaterials = arr;
            }

            return inst;
        }

        /// <summary>Vsadí model postavy (prodavač/zákazník). Když model chybí, postaví kapsli.</summary>
        public static GameObject SpawnHuman(GameObject root, string preferredName, Color tint, float height)
        {
            GameObject prefab = Load("People", preferredName);
            if (prefab == null)
            {
                foreach (var m in LoadAll("People"))
                    if (m != null && m.name.StartsWith("character")) { prefab = m; break; }
            }

            if (prefab != null)
            {
                var inst = Spawn(root, prefab, height, Fit.Height, tint, "People", PeopleYaw);
                if (inst != null) return inst;
            }

            return BuildCapsulePerson(root, tint, height);
        }

        private static GameObject BuildCapsulePerson(GameObject root, Color tint, float height)
        {
            var holder = new GameObject("Model");
            holder.transform.SetParent(root.transform, false);
            holder.transform.localPosition = Vector3.zero;

            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            StripCollider(body);
            body.transform.SetParent(holder.transform, false);
            body.transform.localPosition = new Vector3(0f, height * 0.5f, 0f);
            body.transform.localScale = new Vector3(height * 0.35f, height * 0.5f, height * 0.35f);
            body.GetComponent<Renderer>().sharedMaterial = MaterialFactory.Create(tint, 0.3f);

            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            StripCollider(head);
            head.transform.SetParent(holder.transform, false);
            head.transform.localPosition = new Vector3(0f, height * 0.85f, 0f);
            head.transform.localScale = Vector3.one * (height * 0.28f);
            head.GetComponent<Renderer>().sharedMaterial = MaterialFactory.Create(new Color(0.95f, 0.8f, 0.66f));

            return holder;
        }

        private static void StripCollider(GameObject go)
        {
            var c = go.GetComponent<Collider>();
            if (c != null) DestroySafe(c);
        }

        private static void DestroySafe(Object obj)
        {
            if (obj == null) return;
            if (Application.isPlaying) Object.Destroy(obj);
            else Object.DestroyImmediate(obj);
        }
    }
}
