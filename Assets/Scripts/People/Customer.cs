using System;
using UnityEngine;
using Autobazar.Core;
using Autobazar.Interaction;
using Autobazar.Managers;
using Autobazar.Player;
using Autobazar.Vehicles;
using Autobazar.UI;

namespace Autobazar.People
{
    /// <summary>
    /// Zákazník: přijde ke vstupu, postaví se na volné místo, má přání (typ auta)
    /// a trpělivost, která ubývá. Hráč s ním promluví (E) a nabídne auto.
    /// Když dostane vhodné auto, odejde spokojený; když mu dojde trpělivost, odejde naštvaný.
    /// </summary>
    public class Customer : MonoBehaviour, IInteractable
    {
        public enum State { Entering, Waiting, Leaving }

        public CarType DesiredType { get; private set; }
        public State CurrentState { get; private set; } = State.Entering;
        public int SpotIndex { get; set; } = -1;

        private Vector3 _waitTarget;
        private Vector3 _exitTarget;
        private float _patience = 1f;
        private float _patienceDuration = 45f;
        private float _moveSpeed = 2.6f;
        private bool _finished;

        private TextMesh _wishLabel;
        private TextMesh _patienceLabel;
        private Vector3 _wishBaseScale;

        public event Action<Customer> OnDone;

        public void Setup(CarType desired, Vector3 waitTarget, Vector3 exitTarget, float patienceDuration)
        {
            DesiredType = desired;
            _waitTarget = waitTarget;
            _exitTarget = exitTarget;
            _patienceDuration = Mathf.Max(5f, patienceDuration);
        }

        private void OnEnable() => InteractionSystem.Register(this);
        private void OnDisable() => InteractionSystem.Unregister(this);

        private void Start()
        {
            _wishLabel = TextFactory.Create($"Chci: {CarData.TypeText(DesiredType)} auto",
                transform, new Vector3(0f, 2.55f, 0f), 50, 0.085f, Color.white, TextAnchor.LowerCenter, true);
            _wishBaseScale = _wishLabel.transform.localScale;

            _patienceLabel = TextFactory.Create("",
                transform, new Vector3(0f, 2.2f, 0f), 50, 0.07f, Color.green, TextAnchor.LowerCenter, true);

            UpdateLabels();
        }

        private void Update()
        {
            if (_finished) return;

            switch (CurrentState)
            {
                case State.Entering:
                    MoveTo(_waitTarget);
                    if (Reached(_waitTarget)) CurrentState = State.Waiting;
                    break;

                case State.Waiting:
                    if (!GameState.InputLocked) // během dialogu trpělivost neubývá
                    {
                        _patience -= Time.deltaTime / _patienceDuration;
                        if (_patience <= 0f) { _patience = 0f; LeaveUnhappy(); }
                    }
                    break;

                case State.Leaving:
                    MoveTo(_exitTarget);
                    if (Reached(_exitTarget)) Finish();
                    break;
            }

            UpdateLabels();
        }

        private void MoveTo(Vector3 target)
        {
            Vector3 p = transform.position;
            Vector3 t = new Vector3(target.x, p.y, target.z);
            transform.position = Vector3.MoveTowards(p, t, _moveSpeed * Time.deltaTime);

            Vector3 dir = t - p; dir.y = 0f;
            if (dir.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 8f * Time.deltaTime);
        }

        private bool Reached(Vector3 target)
        {
            Vector3 a = transform.position; a.y = 0f;
            Vector3 b = target; b.y = 0f;
            return Vector3.Distance(a, b) < 0.35f;
        }

        private void UpdateLabels()
        {
            if (_patienceLabel == null) return;

            if (CurrentState == State.Waiting)
            {
                int pct = Mathf.RoundToInt(_patience * 100f);
                _patienceLabel.text = $"Trpělivost: {pct}%";
                _patienceLabel.color = Color.Lerp(Color.red, Color.green, _patience);
            }
            else if (CurrentState == State.Leaving)
            {
                _patienceLabel.text = "";
            }
        }

        /// <summary>Úspěšný prodej – zákazník odejde spokojený.</summary>
        public void CompleteSale()
        {
            if (_finished || CurrentState == State.Leaving) return;
            if (_wishLabel != null) { _wishLabel.text = "Děkuji!"; _wishLabel.color = Color.green; }
            CurrentState = State.Leaving;
        }

        private void LeaveUnhappy()
        {
            if (_finished || CurrentState == State.Leaving) return;
            if (_wishLabel != null) { _wishLabel.text = "Odcházím!"; _wishLabel.color = Color.red; }
            CurrentState = State.Leaving;

            if (ReputationManager.Instance != null) ReputationManager.Instance.ChangeReputation(-5);
            if (UIManager.Instance != null) UIManager.Instance.ShowMessage("Zákazník odešel naštvaný (-5 reputace).", 3f);
            if (TaskManager.Instance != null)
                TaskManager.Instance.SetTask("Obsluhuj zákazníky rychleji – počkej na dalšího u vstupu.");
        }

        private void Finish()
        {
            _finished = true;
            OnDone?.Invoke(this);
            Destroy(gameObject);
        }

        // ---------- IInteractable ----------

        public string GetInteractionPrompt() => "Promluvit se zákazníkem";
        public Transform GetTransform() => transform;
        public bool CanInteract() => CurrentState == State.Waiting && !_finished;

        public void SetHighlighted(bool on)
        {
            if (_wishLabel != null)
                _wishLabel.transform.localScale = _wishBaseScale * (on ? 1.25f : 1f);
        }

        public void Interact(PlayerController player)
        {
            if (DialogUI.Instance != null) DialogUI.Instance.Open(this);
        }
    }
}
