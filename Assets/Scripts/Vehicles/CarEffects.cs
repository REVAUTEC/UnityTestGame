using UnityEngine;

namespace Autobazar.Vehicles
{
    /// <summary>
    /// Prach za koly. Vytvoří částicový systém a podle rychlosti auta řídí, kolik prachu
    /// víří. Materiál i textura se generují v kódu (žádný soubor, žádné „růžové" částice).
    /// </summary>
    [RequireComponent(typeof(CarController))]
    public class CarEffects : MonoBehaviour
    {
        private CarController _car;
        private ParticleSystem _dust;

        private void Start()
        {
            _car = GetComponent<CarController>();
            BuildDust();
        }

        private void Update()
        {
            if (_dust == null || _car == null) return;
            var emission = _dust.emission;
            emission.rateOverTime = _car.ControlEnabled
                ? _car.Speed01 * (_car.Boosting ? 95f : 60f)
                : 0f;
        }

        private void BuildDust()
        {
            var go = new GameObject("Dust");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(0f, 0.15f, -1.8f);
            go.transform.localRotation = Quaternion.LookRotation(new Vector3(0f, 0.35f, -1f)); // dozadu a nahoru

            _dust = go.AddComponent<ParticleSystem>();

            var main = _dust.main;
            main.startLifetime = 0.85f;
            main.startSpeed = 0.7f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.4f, 0.9f);
            main.startColor = new Color(0.62f, 0.57f, 0.47f, 0.55f);
            main.gravityModifier = -0.03f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 220;

            var emission = _dust.emission;
            emission.rateOverTime = 0f;

            var shape = _dust.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 22f;
            shape.radius = 0.25f;

            var sizeOver = _dust.sizeOverLifetime;
            sizeOver.enabled = true;
            sizeOver.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 0.5f, 1f, 1.7f));

            var colorOver = _dust.colorOverLifetime;
            colorOver.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(new Color(0.62f, 0.57f, 0.47f), 0f), new GradientColorKey(new Color(0.5f, 0.46f, 0.4f), 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.55f, 0.2f), new GradientAlphaKey(0f, 1f) });
            colorOver.color = grad;

            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.material = DustMaterial();
        }

        private static Material DustMaterial()
        {
            var sh = Shader.Find("Sprites/Default");
            if (sh == null) sh = Shader.Find("Legacy Shaders/Particles/Alpha Blended");
            var mat = new Material(sh);
            mat.mainTexture = SoftCircle();
            return mat;
        }

        private static Texture2D SoftCircle()
        {
            const int s = 64;
            var tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            float c = (s - 1) * 0.5f;
            var px = new Color32[s * s];
            for (int y = 0; y < s; y++)
                for (int x = 0; x < s; x++)
                {
                    float d = Mathf.Clamp01(Vector2.Distance(new Vector2(x, y), new Vector2(c, c)) / c);
                    float a = Mathf.Clamp01(1f - d); a *= a;
                    px[y * s + x] = new Color32(255, 255, 255, (byte)(a * 255f));
                }
            tex.SetPixels32(px);
            tex.Apply();
            return tex;
        }
    }
}
