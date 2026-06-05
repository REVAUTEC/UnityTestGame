using UnityEngine;

namespace Autobazar.Core
{
    /// <summary>
    /// Pomocník pro vytváření 3D textových popisků (TextMesh) ve světě –
    /// např. plovoucí cedule nad auty nebo nápisy na budovách.
    /// Řeší typický problém, že nově přidaný TextMesh nemá přiřazený font a materiál.
    /// </summary>
    public static class TextFactory
    {
        private static Font _font;

        private static Font GetFont()
        {
            if (_font != null) return _font;
            // V Unity 6 se vestavěný font jmenuje "LegacyRuntime.ttf" (dříve "Arial.ttf").
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (_font == null) _font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            return _font;
        }

        /// <summary>
        /// Vytvoří plovoucí 3D text. Volitelně se otáčí za kamerou (billboard).
        /// </summary>
        public static TextMesh Create(string text, Transform parent, Vector3 localPosition,
            int fontSize = 48, float characterSize = 0.12f, Color? color = null,
            TextAnchor anchor = TextAnchor.MiddleCenter, bool billboard = true)
        {
            var go = new GameObject("Label");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;

            var tm = go.AddComponent<TextMesh>();
            tm.text = text;
            tm.font = GetFont();
            tm.fontSize = fontSize;
            tm.characterSize = characterSize;
            tm.anchor = anchor;
            tm.alignment = TextAlignment.Center;
            tm.color = color ?? Color.white;

            // Bez přiřazení materiálu fontu by text nebyl vidět.
            var mr = go.GetComponent<MeshRenderer>();
            if (GetFont() != null) mr.sharedMaterial = GetFont().material;

            if (billboard) go.AddComponent<Billboard>();

            return tm;
        }
    }
}
