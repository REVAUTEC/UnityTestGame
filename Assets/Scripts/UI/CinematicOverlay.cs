using UnityEngine;
using UnityEngine.UI;

namespace Autobazar.UI
{
    /// <summary>
    /// Jednoduchý „filmový" nádech přes celou obrazovku: viněta (ztmavené rohy) a jemný
    /// teplý nádech. Funguje v Built-in render pipeline bez jakéhokoli balíčku.
    /// (Plný bloom/SSAO by chtěl URP – to je samostatný, větší krok.)
    /// </summary>
    public class CinematicOverlay : MonoBehaviour
    {
        private void Awake()
        {
            var canvasGo = new GameObject("Cinematic_Canvas");
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = -1; // nad 3D scénou, ale pod HUD

            // Teplý nádech
            CreateOverlay(canvasGo.transform, "WarmTint", null, new Color(1f, 0.93f, 0.8f, 0.06f));

            // Viněta z proceduráně vytvořené textury
            var vignette = BuildVignetteSprite();
            CreateOverlay(canvasGo.transform, "Vignette", vignette, Color.white);
        }

        private void CreateOverlay(Transform parent, string name, Sprite sprite, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var img = go.AddComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            img.raycastTarget = false;
        }

        private Sprite BuildVignetteSprite()
        {
            const int size = 256;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;

            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            float maxDist = center.magnitude;
            var pixels = new Color32[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), center) / maxDist; // 0 střed, 1 roh
                    float a = Mathf.Clamp01((d - 0.55f) / 0.45f);
                    a = a * a * 0.55f; // jemné ztmavení rohů
                    pixels[y * size + x] = new Color32(0, 0, 0, (byte)(a * 255f));
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();

            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}
