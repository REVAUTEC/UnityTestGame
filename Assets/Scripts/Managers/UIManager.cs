using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Autobazar.Core;

namespace Autobazar.Managers
{
    /// <summary>
    /// Herní HUD (peníze, reputace, prodáno, úkol), výzva k interakci, hlášky
    /// a univerzální progress bar (servis, příprava smlouvy). Vše z kódu.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        private Text _moneyText, _reputationText, _carsSoldText, _taskText, _promptText, _messageText;
        private Image _promptPanel, _messagePanel;

        private GameObject _progressPanel;
        private Text _progressLabel;
        private RectTransform _progressFill;
        private bool _progressActive;
        private const float ProgressBarWidth = 600f;

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

        // ---------- Veřejné API ----------

        public void SetMoney(int value) { if (_moneyText) _moneyText.text = $"<b>{value:n0} Kč</b>"; }
        public void SetReputation(int value) { if (_reputationText) _reputationText.text = $"Reputace: {value}/100"; }
        public void SetCarsSold(int value) { if (_carsSoldText) _carsSoldText.text = $"Prodáno aut: {value}"; }
        public void SetTask(string task) { if (_taskText) _taskText.text = "ÚKOL:  " + task; }

        public void ShowPrompt(string prompt)
        {
            if (_promptText) _promptText.text = prompt;
            if (_promptPanel) _promptPanel.gameObject.SetActive(true);
        }

        public void HidePrompt()
        {
            if (_promptPanel) _promptPanel.gameObject.SetActive(false);
        }

        public void ShowMessage(string message, float duration = 3.5f)
        {
            if (!_messageText) return;
            if (_messageRoutine != null) StopCoroutine(_messageRoutine);
            _messageRoutine = StartCoroutine(MessageRoutine(message, duration));
        }

        private IEnumerator MessageRoutine(string message, float duration)
        {
            _messageText.text = message;
            if (_messagePanel) _messagePanel.gameObject.SetActive(true);
            yield return new WaitForSeconds(duration);
            if (_messagePanel) _messagePanel.gameObject.SetActive(false);
        }

        /// <summary>Univerzální progress bar. Po dokončení zavolá onComplete. Během běhu zamkne ovládání.</summary>
        public void ShowProgress(string label, float duration, Action onComplete)
        {
            if (_progressActive) return; // jen jeden progress naráz
            StartCoroutine(ProgressRoutine(label, Mathf.Max(0.1f, duration), onComplete));
        }

        private IEnumerator ProgressRoutine(string label, float duration, Action onComplete)
        {
            _progressActive = true;
            GameState.InputLocked = true;
            if (_progressPanel) _progressPanel.SetActive(true);

            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float frac = Mathf.Clamp01(t / duration);
                if (_progressFill) _progressFill.localScale = new Vector3(frac, 1f, 1f);
                if (_progressLabel) _progressLabel.text = $"{label}…  {Mathf.RoundToInt(frac * 100f)}%";
                yield return null;
            }

            if (_progressFill) _progressFill.localScale = Vector3.one;
            if (_progressPanel) _progressPanel.SetActive(false);
            GameState.InputLocked = false;
            _progressActive = false;

            onComplete?.Invoke();
        }

        // ---------- Stavba UI ----------

        private void BuildUI()
        {
            var canvasGo = new GameObject("HUD_Canvas");
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasGo.AddComponent<GraphicRaycaster>();
            var canvasT = canvasGo.transform;

            var panelColor = new Color(0f, 0f, 0f, 0.5f);

            CreatePanel(canvasT, new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, -20), new Vector2(430, 160), panelColor);
            _moneyText = CreateText(canvasT, "Money", new Vector2(0, 1), new Vector2(40, -34), new Vector2(380, 50), 40, TextAnchor.UpperLeft, new Color(0.6f, 1f, 0.6f));
            _reputationText = CreateText(canvasT, "Reputation", new Vector2(0, 1), new Vector2(40, -92), new Vector2(380, 40), 28, TextAnchor.UpperLeft, Color.white);
            _carsSoldText = CreateText(canvasT, "CarsSold", new Vector2(0, 1), new Vector2(40, -132), new Vector2(380, 40), 28, TextAnchor.UpperLeft, Color.white);

            CreatePanel(canvasT, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -22), new Vector2(1180, 60), panelColor);
            _taskText = CreateText(canvasT, "Task", new Vector2(0.5f, 1), new Vector2(0, -30), new Vector2(1140, 50), 30, TextAnchor.MiddleCenter, new Color(1f, 0.9f, 0.45f));

            _promptPanel = CreatePanel(canvasT, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 110), new Vector2(640, 66), new Color(0f, 0f, 0f, 0.62f));
            _promptText = CreateText(_promptPanel.transform, "Prompt", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(620, 60), 30, TextAnchor.MiddleCenter, new Color(0.7f, 1f, 0.7f));
            _promptPanel.gameObject.SetActive(false);

            _messagePanel = CreatePanel(canvasT, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -150), new Vector2(900, 190), new Color(0f, 0f, 0f, 0.68f));
            _messageText = CreateText(_messagePanel.transform, "Message", new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(860, 170), 30, TextAnchor.MiddleCenter, Color.white);
            _messagePanel.gameObject.SetActive(false);

            BuildProgressBar(canvasT);
        }

        private void BuildProgressBar(Transform parent)
        {
            var panel = CreatePanel(parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 70), new Vector2(700, 130), new Color(0f, 0f, 0f, 0.78f));
            _progressPanel = panel.gameObject;

            _progressLabel = CreateText(panel.transform, "ProgressLabel", new Vector2(0.5f, 0.5f),
                new Vector2(0, 32), new Vector2(660, 50), 30, TextAnchor.MiddleCenter, Color.white);

            // Pozadí lišty
            var bg = CreatePanel(panel.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, -22), new Vector2(ProgressBarWidth, 34), new Color(0.15f, 0.15f, 0.15f, 1f));

            // Výplň lišty (kotvená vlevo, škáluje se zleva)
            var fillGo = new GameObject("Fill");
            fillGo.transform.SetParent(bg.transform, false);
            var frt = fillGo.AddComponent<RectTransform>();
            frt.anchorMin = new Vector2(0f, 0.5f);
            frt.anchorMax = new Vector2(0f, 0.5f);
            frt.pivot = new Vector2(0f, 0.5f);
            frt.sizeDelta = new Vector2(ProgressBarWidth, 34);
            frt.anchoredPosition = Vector2.zero;
            fillGo.AddComponent<Image>().color = new Color(0.3f, 0.8f, 0.4f, 1f);
            frt.localScale = new Vector3(0f, 1f, 1f);
            _progressFill = frt;

            _progressPanel.SetActive(false);
        }

        private Image CreatePanel(Transform parent, Vector2 anchor, Vector2 pivot, Vector2 anchoredPos, Vector2 size, Color color)
        {
            var go = new GameObject("Panel");
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.sizeDelta = size;
            rt.anchoredPosition = anchoredPos;
            var img = go.AddComponent<Image>();
            img.color = color;
            return img;
        }

        private Text CreateText(Transform parent, string name, Vector2 anchorPivot, Vector2 anchoredPos,
            Vector2 size, int fontSize, TextAnchor alignment, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorPivot;
            rt.anchorMax = anchorPivot;
            rt.pivot = anchorPivot;
            rt.sizeDelta = size;
            rt.anchoredPosition = anchoredPos;

            var text = go.AddComponent<Text>();
            text.font = _font;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = color;
            text.supportRichText = true;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.text = "";

            var shadow = go.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.7f);
            shadow.effectDistance = new Vector2(2, -2);

            return text;
        }
    }
}
