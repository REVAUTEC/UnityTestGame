using UnityEngine;
using UnityEngine.UI;
using Autobazar.Core;
using Autobazar.Managers;
using Autobazar.Vehicles;

namespace Autobazar.UI
{
    /// <summary>
    /// Servisní menu auta. Otevře se klávesou E u auta. Hráč vybere opravu (1–4),
    /// zaplatí, proběhne progress bar a auto se vylepší. Esc zavře.
    /// </summary>
    public class CarServiceUI : MonoBehaviour
    {
        public static CarServiceUI Instance { get; private set; }

        private GameObject _panel;
        private Text _titleText, _bodyText;
        private Font _font;
        private CarInteractable _car;

        private static readonly RepairType[] Order =
        {
            RepairType.Wash, RepairType.Polish, RepairType.Engine, RepairType.Brakes
        };

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

            for (int i = 0; i < Order.Length; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i) || Input.GetKeyDown(KeyCode.Keypad1 + i))
                {
                    DoRepair(Order[i]);
                    break;
                }
            }
        }

        public void Open(CarInteractable car)
        {
            if (car == null) return;
            _car = car;
            RefreshTexts();
            GameState.InputLocked = true;
            _panel.SetActive(true);
        }

        public void Close()
        {
            if (_panel != null) _panel.SetActive(false);
            GameState.InputLocked = false;
            _car = null;
        }

        private void DoRepair(RepairType type)
        {
            if (_car == null) return;

            var info = ServiceManager.GetInfo(type);

            if (EconomyManager.Instance == null || !EconomyManager.Instance.TrySpendMoney(info.Cost))
            {
                if (UIManager.Instance != null)
                    UIManager.Instance.ShowMessage($"Nedostatek peněz na: {info.Name} ({info.Cost:n0} Kč)", 2.5f);
                RefreshTexts();
                return;
            }

            // Zaplaceno – schováme menu (necháme zámek) a spustíme progress.
            var car = _car;
            _car = null;
            _panel.SetActive(false);

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowProgress($"Servis: {info.Name}", ServiceManager.GetDuration(type), () =>
                {
                    ServiceManager.Apply(type, car);
                    if (UIManager.Instance != null)
                        UIManager.Instance.ShowMessage($"{info.Name} hotovo!  ({info.Effect})", 3f);
                });
            }
            else
            {
                ServiceManager.Apply(type, car);
                GameState.InputLocked = false; // záloha, kdyby chybělo UIManager
            }
        }

        private void RefreshTexts()
        {
            if (_car == null) return;
            var d = _car.Data;

            _titleText.text = $"{d.carName}  —  Servis";

            var sb = new System.Text.StringBuilder();
            string serviceFlag = d.needsService ? "   <color=#ff9a3c>POTŘEBUJE SERVIS</color>" : "";
            sb.AppendLine($"Stav: <color=#9fd8ff>{d.condition}%</color>    Atraktivita: <color=#9fd8ff>{d.attractiveness}%</color>{serviceFlag}");
            sb.AppendLine($"Status: {d.GetStatusText()}\n");
            sb.AppendLine("Vyber opravu (zaplatí se hned):\n");

            for (int i = 0; i < Order.Length; i++)
            {
                var info = ServiceManager.GetInfo(Order[i]);
                sb.AppendLine($"<b>{i + 1})</b>  {info.Name}  —  {info.Cost:n0} Kč   <color=#a7e0a7>({info.Effect})</color>");
            }

            sb.AppendLine("\n<color=#bbbbbb>Esc = zavřít</color>");
            _bodyText.text = sb.ToString();
        }

        // ---------- Stavba UI ----------

        private void BuildUI()
        {
            var canvasGo = new GameObject("Service_Canvas");
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            _panel = new GameObject("ServicePanel");
            _panel.transform.SetParent(canvasGo.transform, false);
            var prt = _panel.AddComponent<RectTransform>();
            prt.anchorMin = prt.anchorMax = prt.pivot = new Vector2(0.5f, 0.5f);
            prt.sizeDelta = new Vector2(940, 600);
            prt.anchoredPosition = Vector2.zero;
            _panel.AddComponent<Image>().color = new Color(0.06f, 0.07f, 0.06f, 0.92f);

            _titleText = CreateText(_panel.transform, "Title", new Vector2(0.5f, 1f),
                new Vector2(0, -45), new Vector2(880, 70), 40, TextAnchor.MiddleCenter, Color.white);

            _bodyText = CreateText(_panel.transform, "Body", new Vector2(0.5f, 1f),
                new Vector2(0, -110), new Vector2(860, 420), 30, TextAnchor.UpperLeft, Color.white);

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
