using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Autobazar.Core;
using Autobazar.Managers;
using Autobazar.People;
using Autobazar.Vehicles;

namespace Autobazar.UI
{
    /// <summary>
    /// Dialog se zákazníkem. Ukáže, co zákazník hledá, a seznam aut. Hráč stiskne
    /// číslo auta (1–5) a nabídne ho; Esc dialog zavře. Ovládání klávesnicí je
    /// jednoduché a spolehlivé (nepotřebuje EventSystem ani odemykání myši).
    /// </summary>
    public class DialogUI : MonoBehaviour
    {
        public static DialogUI Instance { get; private set; }

        private GameObject _panel;
        private Text _titleText, _bodyText;
        private Font _font;

        private Customer _customer;
        private readonly List<CarInteractable> _offered = new List<CarInteractable>();

        private bool IsOpen => _panel != null && _panel.activeSelf;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (_font == null) _font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            BuildUI();
        }

        private void Update()
        {
            if (!IsOpen) return;

            if (Input.GetKeyDown(KeyCode.Escape)) { Close(); return; }

            for (int i = 0; i < _offered.Count && i < 9; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i) || Input.GetKeyDown(KeyCode.Keypad1 + i))
                {
                    Offer(i);
                    break;
                }
            }
        }

        public void Open(Customer customer)
        {
            if (customer == null) return;
            _customer = customer;

            _offered.Clear();
            _offered.AddRange(Object.FindObjectsByType<CarInteractable>(FindObjectsSortMode.None));
            _offered.Sort((a, b) => string.Compare(a.Data.carName, b.Data.carName, System.StringComparison.Ordinal));

            RefreshTexts();
            GameState.InputLocked = true;
            _panel.SetActive(true);
        }

        public void Close()
        {
            if (_panel != null) _panel.SetActive(false);
            GameState.InputLocked = false;
            _customer = null;
        }

        private void Offer(int index)
        {
            if (_customer == null || index < 0 || index >= _offered.Count) return;

            var result = DealManager.Agree(_customer, _offered[index]);
            if (UIManager.Instance != null) UIManager.Instance.ShowMessage(result.Message, 4f);

            if (result.Success) Close();
            else RefreshTexts();
        }

        private void RefreshTexts()
        {
            if (_customer == null) return;

            _titleText.text = $"Zákazník hledá:  <color=#ffd24a>{CarData.TypeText(_customer.DesiredType)} auto</color>";

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Nabídni mu auto – stiskni jeho číslo:\n");
            for (int i = 0; i < _offered.Count; i++)
            {
                var d = _offered[i].Data;
                if (d.isSold)
                    sb.AppendLine($"<color=#777777>{i + 1})  {d.carName}  —  prodáno</color>");
                else if (d.isReserved)
                    sb.AppendLine($"<color=#777777>{i + 1})  {d.carName}  —  rezervováno</color>");
                else
                    sb.AppendLine($"<b>{i + 1})</b>  {d.carName}  —  {d.price:n0} Kč   <color=#ffd24a>[{d.GetTypeText()}]</color>   <color=#9fd8ff>stav {d.condition}%</color>");
            }
            sb.AppendLine("\n<color=#bbbbbb>Esc = zavřít</color>");
            _bodyText.text = sb.ToString();
        }

        // ---------- Stavba UI ----------

        private void BuildUI()
        {
            var canvasGo = new GameObject("Dialog_Canvas");
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10; // nad HUD
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            _panel = new GameObject("DialogPanel");
            _panel.transform.SetParent(canvasGo.transform, false);
            var prt = _panel.AddComponent<RectTransform>();
            prt.anchorMin = prt.anchorMax = prt.pivot = new Vector2(0.5f, 0.5f);
            prt.sizeDelta = new Vector2(940, 660);
            prt.anchoredPosition = Vector2.zero;
            _panel.AddComponent<Image>().color = new Color(0.05f, 0.06f, 0.09f, 0.95f);

            // Barevná hlavička
            var header = new GameObject("Header");
            header.transform.SetParent(_panel.transform, false);
            var hrt = header.AddComponent<RectTransform>();
            hrt.anchorMin = new Vector2(0f, 1f); hrt.anchorMax = new Vector2(1f, 1f); hrt.pivot = new Vector2(0.5f, 1f);
            hrt.sizeDelta = new Vector2(0f, 86f); hrt.anchoredPosition = Vector2.zero;
            header.AddComponent<Image>().color = new Color(0.12f, 0.4f, 0.75f, 0.97f);

            _titleText = CreateText(_panel.transform, "Title", new Vector2(0.5f, 1f),
                new Vector2(0, -43), new Vector2(880, 70), 40, TextAnchor.MiddleCenter, Color.white);

            _bodyText = CreateText(_panel.transform, "Body", new Vector2(0.5f, 1f),
                new Vector2(0, -110), new Vector2(860, 470), 30, TextAnchor.UpperLeft, Color.white);

            _panel.SetActive(false);
        }

        private Text CreateText(Transform parent, string name, Vector2 anchorPivot, Vector2 anchoredPos,
            Vector2 size, int fontSize, TextAnchor alignment, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = anchorPivot;
            rt.sizeDelta = size;
            rt.anchoredPosition = anchoredPos;

            var text = go.AddComponent<Text>();
            text.font = _font;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = color;
            text.supportRichText = true;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }
    }
}
