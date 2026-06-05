using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Autobazar.Managers
{
    /// <summary>
    /// Postaví a aktualizuje celé herní UI (HUD) z kódu – není potřeba nic ručně
    /// klikat v editoru. Singleton: UIManager.Instance.
    ///
    /// Ukazuje: peníze, reputaci, počet prodaných aut, aktuální úkol,
    /// výzvu k interakci ("[E] ...") a dočasné hlášky.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        private Text _moneyText;
        private Text _reputationText;
        private Text _carsSoldText;
        private Text _taskText;
        private Text _promptText;
        private Text _messageText;

        private Font _font;
        private Coroutine _messageRoutine;

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
            // Napojení na manažery: nejdřív si stáhneme aktuální hodnoty (pull),
            // pak se přihlásíme k odběru budoucích změn (push).
            if (EconomyManager.Instance != null)
            {
                SetMoney(EconomyManager.Instance.Money);
                SetCarsSold(EconomyManager.Instance.CarsSold);
                EconomyManager.Instance.OnMoneyChanged += SetMoney;
                EconomyManager.Instance.OnCarsSoldChanged += SetCarsSold;
            }

            if (ReputationManager.Instance != null)
            {
                SetReputation(ReputationManager.Instance.Reputation);
                ReputationManager.Instance.OnReputationChanged += SetReputation;
            }

            if (TaskManager.Instance != null)
            {
                SetTask(TaskManager.Instance.CurrentTask);
                TaskManager.Instance.OnTaskChanged += SetTask;
            }

            HidePrompt();
        }

        private void OnDestroy()
        {
            // Korektní odhlášení, ať nepadají eventy do zničeného objektu.
            if (EconomyManager.Instance != null)
            {
                EconomyManager.Instance.OnMoneyChanged -= SetMoney;
                EconomyManager.Instance.OnCarsSoldChanged -= SetCarsSold;
            }
            if (ReputationManager.Instance != null)
                ReputationManager.Instance.OnReputationChanged -= SetReputation;
            if (TaskManager.Instance != null)
                TaskManager.Instance.OnTaskChanged -= SetTask;
        }

        // ---------- Veřejné API pro aktualizaci UI ----------

        public void SetMoney(int value) { if (_moneyText) _moneyText.text = $"Peníze: {value:n0} Kč"; }
        public void SetReputation(int value) { if (_reputationText) _reputationText.text = $"Reputace: {value}/100"; }
        public void SetCarsSold(int value) { if (_carsSoldText) _carsSoldText.text = $"Prodáno aut: {value}"; }
        public void SetTask(string task) { if (_taskText) _taskText.text = "ÚKOL: " + task; }

        public void ShowPrompt(string prompt)
        {
            if (!_promptText) return;
            _promptText.text = prompt;
            _promptText.enabled = true;
        }

        public void HidePrompt()
        {
            if (_promptText) _promptText.enabled = false;
        }

        /// <summary>Ukáže dočasnou hlášku uprostřed obrazovky (např. info o autě nebo "Auto prodáno").</summary>
        public void ShowMessage(string message, float duration = 3.5f)
        {
            if (!_messageText) return;
            if (_messageRoutine != null) StopCoroutine(_messageRoutine);
            _messageRoutine = StartCoroutine(MessageRoutine(message, duration));
        }

        private IEnumerator MessageRoutine(string message, float duration)
        {
            _messageText.text = message;
            _messageText.enabled = true;
            yield return new WaitForSeconds(duration);
            _messageText.enabled = false;
        }

        // ---------- Stavba UI ----------

        private void BuildUI()
        {
            // Canvas
            var canvasGo = new GameObject("HUD_Canvas");
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGo.AddComponent<GraphicRaycaster>();

            // Levý horní panel se statistikami
            _moneyText = CreateText(canvasGo.transform, "Money",
                new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
                new Vector2(30, -30), new Vector2(600, 50), 34, TextAnchor.UpperLeft);
            _reputationText = CreateText(canvasGo.transform, "Reputation",
                new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
                new Vector2(30, -80), new Vector2(600, 50), 34, TextAnchor.UpperLeft);
            _carsSoldText = CreateText(canvasGo.transform, "CarsSold",
                new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
                new Vector2(30, -130), new Vector2(600, 50), 34, TextAnchor.UpperLeft);

            // Úkol nahoře uprostřed
            _taskText = CreateText(canvasGo.transform, "Task",
                new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1),
                new Vector2(0, -30), new Vector2(1100, 60), 30, TextAnchor.UpperCenter);
            _taskText.color = new Color(1f, 0.9f, 0.4f);

            // Výzva k interakci dole uprostřed
            _promptText = CreateText(canvasGo.transform, "Prompt",
                new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                new Vector2(0, 120), new Vector2(900, 60), 32, TextAnchor.LowerCenter);
            _promptText.color = new Color(0.6f, 1f, 0.6f);

            // Dočasná hláška uprostřed
            _messageText = CreateText(canvasGo.transform, "Message",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, -150), new Vector2(1200, 200), 30, TextAnchor.MiddleCenter);
            _messageText.color = Color.white;
            _messageText.enabled = false;
        }

        /// <summary>Vytvoří jeden UI Text se zadaným ukotvením a pozicí.</summary>
        private Text CreateText(Transform parent, string name,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
            Vector2 anchoredPos, Vector2 size, int fontSize, TextAnchor alignment)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.sizeDelta = size;
            rt.anchoredPosition = anchoredPos;

            var text = go.AddComponent<Text>();
            text.font = _font;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.text = "";

            // Lehký stín pro čitelnost na světlém pozadí.
            var shadow = go.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.6f);
            shadow.effectDistance = new Vector2(2, -2);

            return text;
        }
    }
}
