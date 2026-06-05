using UnityEngine;
using UnityEngine.UI;

namespace Autobazar.Racing
{
    /// <summary>
    /// Veškeré UI závodu: HUD (čas, rychlost, brána, nejlepší čas), velký odpočet,
    /// úvodní menu a výsledková obrazovka. Staví se z kódu.
    /// </summary>
    public class RaceUI : MonoBehaviour
    {
        private Font _font;
        private GameObject _hud, _menu, _result;
        private Text _timeText, _speedText, _checkpointText, _bestText;
        private Text _countdownText, _menuBest, _resultText, _flashText;
        private Coroutine _flashRoutine;

        private void Awake()
        {
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (_font == null) _font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            BuildUI();
        }

        public static string FormatTime(float t)
        {
            if (t < 0f) t = 0f;
            int m = (int)(t / 60f);
            float s = t - m * 60f;
            return m > 0 ? $"{m}:{s:00.00}" : $"{s:0.00}";
        }

        // ---- HUD ----
        public void SetHudVisible(bool v) { if (_hud) _hud.SetActive(v); }
        public void SetTime(float t) { if (_timeText) _timeText.text = FormatTime(t); }
        public void SetSpeed(float kmh) { if (_speedText) _speedText.text = $"{Mathf.RoundToInt(kmh)} km/h"; }
        public void SetCheckpoint(int cur, int total) { if (_checkpointText) _checkpointText.text = $"Brána {Mathf.Min(cur + 1, total)}/{total}"; }
        public void SetBest(float best) { if (_bestText) _bestText.text = best > 0f ? $"Rekord: {FormatTime(best)}" : "Rekord: —"; }

        // ---- Odpočet ----
        public void ShowCountdown(string text)
        {
            if (!_countdownText) return;
            _countdownText.text = text;
            _countdownText.gameObject.SetActive(true);
        }
        public void HideCountdown() { if (_countdownText) _countdownText.gameObject.SetActive(false); }

        // ---- Menu ----
        public void ShowMenu(float best)
        {
            if (_menuBest) _menuBest.text = best > 0f ? $"Tvůj nejlepší čas: {FormatTime(best)}" : "Zatím bez rekordu – zajeď první!";
            if (_menu) _menu.SetActive(true);
        }
        public void HideMenu() { if (_menu) _menu.SetActive(false); }

        // ---- Výsledek ----
        public void ShowResult(float time, float best, bool record)
        {
            if (_resultText)
            {
                string head = record ? "<color=#ffd24a>NOVÝ REKORD!</color>" : "CÍL!";
                _resultText.text =
                    $"{head}\n\nTvůj čas:  <b>{FormatTime(time)}</b>\n" +
                    $"Nejlepší:  {FormatTime(best)}\n\n" +
                    "<color=#a7e0a7>R = jet znovu</color>     <color=#bbbbbb>Enter = menu</color>";
            }
            if (_result) _result.SetActive(true);
        }
        public void HideResult() { if (_result) _result.SetActive(false); }

        public void Flash(string text, float duration = 1.2f)
        {
            if (!_flashText) return;
            if (_flashRoutine != null) StopCoroutine(_flashRoutine);
            _flashRoutine = StartCoroutine(FlashRoutine(text, duration));
        }

        private System.Collections.IEnumerator FlashRoutine(string text, float duration)
        {
            _flashText.text = text;
            _flashText.gameObject.SetActive(true);
            yield return new WaitForSeconds(duration);
            _flashText.gameObject.SetActive(false);
        }

        // ---------------- Stavba ----------------

        private void BuildUI()
        {
            var canvasGo = new GameObject("Race_Canvas");
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 5;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            var root = canvasGo.transform;

            // HUD
            _hud = new GameObject("HUD");
            _hud.transform.SetParent(root, false);
            var hudRt = _hud.AddComponent<RectTransform>();
            hudRt.anchorMin = Vector2.zero; hudRt.anchorMax = Vector2.one;
            hudRt.offsetMin = Vector2.zero; hudRt.offsetMax = Vector2.zero;

            Panel(_hud.transform, new Vector2(0.5f, 1), new Vector2(0, -16), new Vector2(360, 96), new Color(0, 0, 0, 0.45f));
            _timeText = Text(_hud.transform, new Vector2(0.5f, 1), new Vector2(0, -28), new Vector2(360, 80), 56, TextAnchor.UpperCenter, Color.white);

            Panel(_hud.transform, new Vector2(0, 1), new Vector2(20, -20), new Vector2(280, 56), new Color(0, 0, 0, 0.45f));
            _checkpointText = Text(_hud.transform, new Vector2(0, 1), new Vector2(38, -30), new Vector2(260, 44), 30, TextAnchor.UpperLeft, new Color(0.7f, 1f, 0.7f));

            Panel(_hud.transform, new Vector2(1, 1), new Vector2(-20, -20), new Vector2(300, 56), new Color(0, 0, 0, 0.45f));
            _bestText = Text(_hud.transform, new Vector2(1, 1), new Vector2(-38, -30), new Vector2(280, 44), 30, TextAnchor.UpperRight, new Color(1f, 0.9f, 0.5f));

            Panel(_hud.transform, new Vector2(0.5f, 0), new Vector2(0, 26), new Vector2(220, 70), new Color(0, 0, 0, 0.45f));
            _speedText = Text(_hud.transform, new Vector2(0.5f, 0), new Vector2(0, 40), new Vector2(220, 56), 40, TextAnchor.LowerCenter, Color.white);

            // Odpočet (uprostřed, velký)
            _countdownText = Text(root, new Vector2(0.5f, 0.5f), new Vector2(0, 40), new Vector2(900, 260), 180, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.3f));
            _countdownText.gameObject.SetActive(false);

            // Flash hláška
            _flashText = Text(root, new Vector2(0.5f, 0.5f), new Vector2(0, -180), new Vector2(1200, 120), 64, TextAnchor.MiddleCenter, new Color(0.7f, 1f, 0.7f));
            _flashText.gameObject.SetActive(false);

            BuildMenu(root);
            BuildResult(root);
        }

        private void BuildMenu(Transform root)
        {
            _menu = FullPanel(root, "Menu", new Color(0.03f, 0.05f, 0.09f, 0.94f));

            var bar = new GameObject("Bar");
            bar.transform.SetParent(_menu.transform, false);
            var brt = bar.AddComponent<RectTransform>();
            brt.anchorMin = new Vector2(0.5f, 1); brt.anchorMax = new Vector2(0.5f, 1); brt.pivot = new Vector2(0.5f, 1);
            brt.sizeDelta = new Vector2(1100, 120); brt.anchoredPosition = new Vector2(0, -140);
            bar.AddComponent<Image>().color = new Color(0.1f, 0.45f, 0.4f, 0.95f);

            Text(_menu.transform, new Vector2(0.5f, 1), new Vector2(0, -150), new Vector2(1100, 100), 72, TextAnchor.MiddleCenter, Color.white).text = "CHECKPOINT RUSH";
            Text(_menu.transform, new Vector2(0.5f, 0.5f), new Vector2(0, 70), new Vector2(1200, 360), 32, TextAnchor.UpperCenter, new Color(0.9f, 0.95f, 1f)).text =
                "Projeď všechny brány v pořadí co nejrychleji a překonej svůj rekord!\n\n" +
                "<b>OVLÁDÁNÍ</b>\nW – plyn     S – brzda/couvání     A/D – zatáčení\nShift – boost     R – restart     Esc – menu";
            _menuBest = Text(_menu.transform, new Vector2(0.5f, 0.5f), new Vector2(0, -150), new Vector2(1100, 60), 36, TextAnchor.MiddleCenter, new Color(1f, 0.9f, 0.5f));
            Text(_menu.transform, new Vector2(0.5f, 0), new Vector2(0, 90), new Vector2(1100, 70), 42, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.35f)).text = "Stiskni ENTER pro start";
        }

        private void BuildResult(Transform root)
        {
            _result = new GameObject("Result");
            _result.transform.SetParent(root, false);
            var rt = _result.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(820, 520);
            rt.anchoredPosition = Vector2.zero;
            _result.AddComponent<Image>().color = new Color(0.04f, 0.06f, 0.1f, 0.95f);

            _resultText = Text(_result.transform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760, 460), 40, TextAnchor.MiddleCenter, Color.white);
            _result.SetActive(false);
        }

        private GameObject FullPanel(Transform parent, string name, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            go.AddComponent<Image>().color = color;
            return go;
        }

        private Image Panel(Transform parent, Vector2 anchor, Vector2 pos, Vector2 size, Color color)
        {
            var go = new GameObject("Panel");
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
            rt.sizeDelta = size; rt.anchoredPosition = pos;
            var img = go.AddComponent<Image>();
            img.color = color; img.raycastTarget = false;
            return img;
        }

        private Text Text(Transform parent, Vector2 anchor, Vector2 pos, Vector2 size, int fontSize, TextAnchor align, Color color)
        {
            var go = new GameObject("Text");
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = anchor;
            rt.sizeDelta = size; rt.anchoredPosition = pos;
            var t = go.AddComponent<Text>();
            t.font = _font; t.fontSize = fontSize; t.alignment = align; t.color = color;
            t.supportRichText = true;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            var shadow = go.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.6f);
            shadow.effectDistance = new Vector2(2, -2);
            return t;
        }
    }
}
