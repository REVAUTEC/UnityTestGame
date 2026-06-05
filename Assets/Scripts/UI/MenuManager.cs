using UnityEngine;
using UnityEngine.UI;
using Autobazar.Core;

namespace Autobazar.UI
{
    /// <summary>
    /// Úvodní/pauzovací nabídka s názvem hry a ovládáním. Na začátku je hra pozastavená
    /// (Enter spustí), klávesa P kdykoli zobrazí pauzu. Ovládá se klávesnicí.
    /// </summary>
    public class MenuManager : MonoBehaviour
    {
        public static MenuManager Instance { get; private set; }

        private GameObject _root;
        private Text _titleText, _bodyText, _hintText;
        private Font _font;
        private bool _shown;

        private const string Controls =
            "<b>OVLÁDÁNÍ</b>\n" +
            "WASD – pohyb     Myš – rozhlížení     Shift – sprint\n" +
            "E – interakce (auta, zákazníci, počítač v kanceláři)\n" +
            "V nabídce: čísla 1–5     Esc – zavřít\n" +
            "U auta: 5 = zkušební jízda (W/S/A/D, projeď checkpointy)\n" +
            "P – pauza / tato nabídka\n\n" +
            "<b>CÍL</b>\n" +
            "Obsluhuj zákazníky, opravuj a prodávej auta, vydělávej\n" +
            "peníze a buduj reputaci. Stihni co nejvíc za den!";

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (_font == null) _font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            BuildUI();
        }

        private void Start()
        {
            Show("AUTOBAZAR TYCOON 3D", "Stiskni ENTER pro start");
        }

        private void Update()
        {
            if (_shown)
            {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space))
                    Hide();
            }
            else
            {
                // Pauza – ale ne během denní statistiky (tu řeší DayManager).
                bool dayOver = Managers.DayManager.Instance != null && Managers.DayManager.Instance.IsDayOver;
                if (!dayOver && Input.GetKeyDown(KeyCode.P))
                    Show("PAUZA", "Stiskni ENTER pro pokračování");
            }
        }

        private void Show(string title, string hint)
        {
            _titleText.text = title;
            _bodyText.text = Controls;
            _hintText.text = hint;
            _root.SetActive(true);
            _shown = true;
            GameState.InputLocked = true;
        }

        private void Hide()
        {
            _root.SetActive(false);
            _shown = false;
            GameState.InputLocked = false;
        }

        private void BuildUI()
        {
            var canvasGo = new GameObject("Menu_Canvas");
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20; // úplně navrchu
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            _root = new GameObject("MenuRoot");
            _root.transform.SetParent(canvasGo.transform, false);
            var rrt = _root.AddComponent<RectTransform>();
            rrt.anchorMin = Vector2.zero; rrt.anchorMax = Vector2.one;
            rrt.offsetMin = Vector2.zero; rrt.offsetMax = Vector2.zero;
            _root.AddComponent<Image>().color = new Color(0.03f, 0.05f, 0.09f, 0.93f);

            // barevný pruh za názvem
            var bar = new GameObject("TitleBar");
            bar.transform.SetParent(_root.transform, false);
            var brt = bar.AddComponent<RectTransform>();
            brt.anchorMin = new Vector2(0.5f, 1f); brt.anchorMax = new Vector2(0.5f, 1f);
            brt.pivot = new Vector2(0.5f, 1f);
            brt.sizeDelta = new Vector2(1100, 110);
            brt.anchoredPosition = new Vector2(0, -120);
            bar.AddComponent<Image>().color = new Color(0.1f, 0.35f, 0.7f, 0.9f);

            _titleText = MakeText(_root.transform, new Vector2(0.5f, 1f), new Vector2(0, -130), new Vector2(1080, 90), 64, TextAnchor.MiddleCenter, Color.white);
            _bodyText = MakeText(_root.transform, new Vector2(0.5f, 0.5f), new Vector2(0, 10), new Vector2(1200, 520), 32, TextAnchor.UpperCenter, new Color(0.92f, 0.95f, 1f));
            _hintText = MakeText(_root.transform, new Vector2(0.5f, 0f), new Vector2(0, 90), new Vector2(1100, 70), 38, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.35f));

            _root.SetActive(false);
        }

        private Text MakeText(Transform parent, Vector2 anchorPivot, Vector2 pos, Vector2 size, int fontSize, TextAnchor align, Color color)
        {
            var go = new GameObject("Text");
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = anchorPivot;
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
            var t = go.AddComponent<Text>();
            t.font = _font; t.fontSize = fontSize; t.alignment = align; t.color = color;
            t.supportRichText = true;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            return t;
        }
    }
}
