using UnityEngine;
using UnityEngine.UI;
using Autobazar.Core;

namespace Autobazar.Managers
{
    /// <summary>
    /// Denní cyklus: den má časový limit, během kterého chodí zákazníci. Po vypršení
    /// se ukáže statistika dne a hráč klávesou Enter spustí další den.
    /// Čas se zastaví během dialogů/servisu/jízdy (GameState.InputLocked).
    /// </summary>
    public class DayManager : MonoBehaviour
    {
        public static DayManager Instance { get; private set; }

        [SerializeField] private float dayLength = 180f; // délka dne v sekundách

        public int Day { get; private set; } = 1;

        private float _timeLeft;
        private bool _dayOver;
        private int _carsSoldAtStart;
        private int _moneyAtStart;
        private int _lostCustomersToday;

        private GameObject _panel;
        private Text _titleText, _bodyText;
        private Font _font;

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
            _timeLeft = dayLength;
            Snapshot();
        }

        private void Update()
        {
            if (_dayOver)
            {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space))
                    StartNextDay();
                return;
            }

            // Čas běží jen když hráč zrovna není v dialogu/servisu/jízdě.
            if (GameState.InputLocked) return;

            _timeLeft -= Time.deltaTime;
            if (UIManager.Instance != null) UIManager.Instance.SetDayInfo(Day, _timeLeft);

            if (_timeLeft <= 0f) EndDay();
        }

        public void ReportLostCustomer()
        {
            _lostCustomersToday++;
        }

        private void Snapshot()
        {
            _carsSoldAtStart = EconomyManager.Instance != null ? EconomyManager.Instance.CarsSold : 0;
            _moneyAtStart = EconomyManager.Instance != null ? EconomyManager.Instance.Money : 0;
            _lostCustomersToday = 0;
        }

        private void EndDay()
        {
            _dayOver = true;
            GameState.InputLocked = true;

            int soldToday = (EconomyManager.Instance != null ? EconomyManager.Instance.CarsSold : 0) - _carsSoldAtStart;
            int netToday = (EconomyManager.Instance != null ? EconomyManager.Instance.Money : 0) - _moneyAtStart;
            int money = EconomyManager.Instance != null ? EconomyManager.Instance.Money : 0;
            int rep = ReputationManager.Instance != null ? ReputationManager.Instance.Reputation : 0;

            string sign = netToday >= 0 ? "+" : "";
            _titleText.text = $"KONEC DNE {Day}";
            _bodyText.text =
                $"Prodáno aut: <color=#9fd8ff>{soldToday}</color>\n" +
                $"Čistý výdělek dne: <color=#a7e0a7>{sign}{netToday:n0} Kč</color>\n" +
                $"Ztracení zákazníci: <color=#ff9a9a>{_lostCustomersToday}</color>\n\n" +
                $"Pokladna celkem: <b>{money:n0} Kč</b>\n" +
                $"Reputace: {rep}/100\n\n" +
                "<color=#ffd24a>Enter = další den</color>";

            _panel.SetActive(true);
        }

        private void StartNextDay()
        {
            Day++;
            _timeLeft = dayLength;
            _dayOver = false;
            Snapshot();
            _panel.SetActive(false);
            GameState.InputLocked = false;

            if (TaskManager.Instance != null)
                TaskManager.Instance.SetTask($"Den {Day} začíná! Počkej na zákazníka u vstupu.");
            if (UIManager.Instance != null) UIManager.Instance.SetDayInfo(Day, _timeLeft);
        }

        private void BuildUI()
        {
            var canvasGo = new GameObject("Day_Canvas");
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 11; // nad ostatní UI
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            _panel = new GameObject("DayPanel");
            _panel.transform.SetParent(canvasGo.transform, false);
            var prt = _panel.AddComponent<RectTransform>();
            prt.anchorMin = prt.anchorMax = prt.pivot = new Vector2(0.5f, 0.5f);
            prt.sizeDelta = new Vector2(820, 560);
            prt.anchoredPosition = Vector2.zero;
            _panel.AddComponent<Image>().color = new Color(0.05f, 0.06f, 0.09f, 0.94f);

            _titleText = MakeText(_panel.transform, new Vector2(0.5f, 1f), new Vector2(0, -50), new Vector2(760, 70), 44, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.4f));
            _bodyText = MakeText(_panel.transform, new Vector2(0.5f, 1f), new Vector2(0, -150), new Vector2(700, 360), 32, TextAnchor.UpperCenter, Color.white);

            _panel.SetActive(false);
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
            t.font = _font;
            t.fontSize = fontSize;
            t.alignment = align;
            t.color = color;
            t.supportRichText = true;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            return t;
        }
    }
}
