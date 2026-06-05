using UnityEngine;

namespace Autobazar.Core
{
    /// <summary>
    /// Načítá hotové 3D modely ze složky Assets/Resources/Kits/... a pomáhá je vsadit
    /// do scény. Když žádné modely nejsou, hra běží dál na jednoduchých kostkách (záloha).
    ///
    /// Modely stačí nahrát do správné podsložky – kód si je najde sám (proto "Resources").
    /// </summary>
    public static class ModelLibrary
    {
        // Cílová délka auta v metrech – model se na ni automaticky zvětší/zmenší.
        public const float CarTargetLength = 4.3f;
        // Když by modely aut koukaly obráceně, přehoď na 180.
        public const float CarModelYaw = 0f;

        private static GameObject[] _cars;
        private static bool _carsLoaded;

        public static GameObject[] Cars
        {
            get
            {
                if (!_carsLoaded)
                {
                    _cars = Resources.LoadAll<GameObject>("Kits/Cars");
                    _carsLoaded = true;
                }
                return _cars;
            }
        }

        public static bool HasCars => Cars != null && Cars.Length > 0;

        /// <summary>Zapomene načtené modely – zavolá se před každou stavbou scény,
        /// aby se nově nahrané modely hned projevily.</summary>
        public static void ClearCache()
        {
            _cars = null;
            _carsLoaded = false;
        }

        public static GameObject GetCar(int index)
        {
            if (!HasCars) return null;
            return Cars[((index % Cars.Length) + Cars.Length) % Cars.Length];
        }

        /// <summary>
        /// Vloží model auta do daného rootu, automaticky ho zvětší na rozumnou velikost
        /// a posadí na zem. Vrátí true při úspěchu.
        /// </summary>
        public static bool SpawnCarModel(GameObject carRoot, GameObject prefab)
        {
            if (carRoot == null || prefab == null) return false;

            var inst = Object.Instantiate(prefab);
            inst.name = "Model";
            inst.transform.SetParent(carRoot.transform, false);
            inst.transform.localPosition = Vector3.zero;
            inst.transform.localRotation = Quaternion.Euler(0f, CarModelYaw, 0f);

            var renderers = inst.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                DestroySafe(inst);
                return false;
            }

            // Auto-fit: spočítáme velikost a zvětšíme na cílovou délku.
            Bounds b = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);

            float currentLength = Mathf.Max(b.size.x, b.size.z, 0.01f);
            float scale = CarTargetLength / currentLength;
            inst.transform.localScale *= scale;

            // Posadíme model spodkem na zem.
            renderers = inst.GetComponentsInChildren<Renderer>();
            b = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);

            var lp = inst.transform.localPosition;
            lp.y = carRoot.transform.position.y - b.min.y;
            inst.transform.localPosition = lp;

            return true;
        }

        private static void DestroySafe(Object obj)
        {
            if (obj == null) return;
            if (Application.isPlaying) Object.Destroy(obj);
            else Object.DestroyImmediate(obj);
        }
    }
}
