using System.Collections.Generic;
using UnityEngine;
using Autobazar.Core;
using Autobazar.Player;
using Autobazar.Vehicles;

namespace Autobazar.Racing
{
    /// <summary>
    /// Řídí celý závod: postaví brány (podle TrackLayout), řeší odpočet, měří čas, hlídá
    /// průjezd branami v pořadí, vyhodnotí cíl a ukládá nejlepší čas. Stavový automat.
    /// </summary>
    public class RaceManager : MonoBehaviour
    {
        private enum State { Menu, Countdown, Racing, Finished }

        [SerializeField] private float gateHitRadius = 8f;
        [SerializeField] private float countdownFrom = 3.5f;

        private const string BestKey = "CR_BestTime";

        private State _state;
        private CarController _car;
        private CameraController _cam;
        private RaceUI _ui;

        private readonly List<Vector3> _gatePos = new List<Vector3>();
        private readonly List<Material> _gateMat = new List<Material>();
        private int[] _order;
        private int _target;
        private float _raceTime;
        private float _bestTime;
        private float _countdown;

        private Vector3 _startPos;
        private Quaternion _startRot;

        private void Start()
        {
            _car = Object.FindFirstObjectByType<CarController>();
            _cam = Camera.main != null ? Camera.main.GetComponent<CameraController>() : null;
            _ui = Object.FindFirstObjectByType<RaceUI>();
            _bestTime = PlayerPrefs.GetFloat(BestKey, 0f);

            BuildTrack();
            ComputeStart();
            PlaceCarAtStart();
            if (_cam != null && _car != null) _cam.SetTarget(_car.transform);

            if (_ui != null)
            {
                _ui.SetBest(_bestTime);
                _ui.SetHudVisible(false);
                _ui.ShowMenu(_bestTime);
            }
            _state = State.Menu;
        }

        private void Update()
        {
            switch (_state)
            {
                case State.Menu:
                    if (StartPressed()) BeginCountdown();
                    break;

                case State.Countdown:
                    _countdown -= Time.deltaTime;
                    if (_countdown > 0f)
                    {
                        if (_ui != null) _ui.ShowCountdown(Mathf.CeilToInt(_countdown).ToString());
                    }
                    else
                    {
                        if (_ui != null) { _ui.HideCountdown(); _ui.Flash("START!", 0.8f); }
                        _raceTime = 0f;
                        if (_car != null) _car.ControlEnabled = true;
                        _state = State.Racing;
                    }
                    break;

                case State.Racing:
                    _raceTime += Time.deltaTime;
                    if (_ui != null)
                    {
                        _ui.SetTime(_raceTime);
                        if (_car != null) _ui.SetSpeed(_car.SpeedKmh);
                        _ui.SetCheckpoint(_target, _order.Length);
                    }
                    CheckGate();
                    if (Input.GetKeyDown(KeyCode.R)) BeginCountdown();
                    else if (Input.GetKeyDown(KeyCode.Escape)) ToMenu();
                    break;

                case State.Finished:
                    if (Input.GetKeyDown(KeyCode.R)) BeginCountdown();
                    else if (StartPressed()) ToMenu();
                    break;
            }
        }

        private static bool StartPressed()
            => Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space);

        private void CheckGate()
        {
            int gate = _order[_target];
            Vector3 a = _car.transform.position; a.y = 0f;
            Vector3 b = _gatePos[gate]; b.y = 0f;
            if (Vector3.Distance(a, b) > gateHitRadius) return;

            SetGate(gate, GateMode.Passed);
            _target++;

            if (_target >= _order.Length) { Finish(); return; }

            int next = _order[_target];
            SetGate(next, next == 0 ? GateMode.Finish : GateMode.Active);
            if (_ui != null) _ui.Flash(next == 0 ? "Poslední – do CÍLE!" : "Dobře!", 0.6f);
        }

        private void Finish()
        {
            if (_car != null) _car.ControlEnabled = false;
            _state = State.Finished;

            bool record = _bestTime <= 0f || _raceTime < _bestTime;
            if (record) { _bestTime = _raceTime; PlayerPrefs.SetFloat(BestKey, _bestTime); PlayerPrefs.Save(); }
            if (_ui != null) { _ui.SetBest(_bestTime); _ui.ShowResult(_raceTime, _bestTime, record); }
        }

        private void BeginCountdown()
        {
            if (_ui != null) { _ui.HideMenu(); _ui.HideResult(); _ui.SetHudVisible(true); }
            PlaceCarAtStart();
            if (_cam != null && _car != null) _cam.SetTarget(_car.transform);

            _target = 0;
            for (int i = 0; i < _gatePos.Count; i++) SetGate(i, GateMode.Inactive);
            int first = _order[0];
            SetGate(first, first == 0 ? GateMode.Finish : GateMode.Active);

            _countdown = countdownFrom;
            _state = State.Countdown;
        }

        private void ToMenu()
        {
            if (_car != null) _car.ControlEnabled = false;
            PlaceCarAtStart();
            if (_cam != null && _car != null) _cam.SetTarget(_car.transform);
            if (_ui != null) { _ui.HideResult(); _ui.SetHudVisible(false); _ui.ShowMenu(_bestTime); }
            _state = State.Menu;
        }

        private void PlaceCarAtStart()
        {
            if (_car == null) return;
            _car.ControlEnabled = false;
            _car.ResetMotion();
            _car.transform.SetPositionAndRotation(_startPos, _startRot);
            _car.SetGroundY(_startPos.y);
        }

        // ---------------- Trať ----------------

        private void ComputeStart()
        {
            _startPos = TrackLayout.GatePos(-0.5f) + Vector3.up * 0.05f;
            Vector3 dir = TrackLayout.GatePos(0f) - _startPos; dir.y = 0f;
            _startRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
        }

        private void BuildTrack()
        {
            var root = new GameObject("Track");
            var world = GameObject.Find("RaceWorld");
            if (world != null) root.transform.SetParent(world.transform, false);

            int n = TrackLayout.GateCount;
            _order = new int[n];
            for (int i = 0; i < n - 1; i++) _order[i] = i + 1;
            _order[n - 1] = 0;

            for (int i = 0; i < n; i++)
            {
                Vector3 pos = TrackLayout.GatePos(i);
                _gatePos.Add(pos);

                Vector3 tangent = (TrackLayout.GatePos(i + 1) - TrackLayout.GatePos(i - 1)); tangent.y = 0f; tangent.Normalize();
                Vector3 perp = Vector3.Cross(Vector3.up, tangent);

                var mat = MaterialFactory.CreateEmissive(new Color(0.1f, 0.12f, 0.2f), new Color(0.1f, 0.2f, 0.5f), 0.4f);
                _gateMat.Add(mat);

                CreatePillar(root.transform, pos + perp * 5f, mat);
                CreatePillar(root.transform, pos - perp * 5f, mat);

                string label = i == 0 ? "CÍL" : i.ToString();
                TextFactory.Create(label, root.transform, pos + Vector3.up * 6.5f, 90, 0.34f, Color.white, TextAnchor.MiddleCenter, true);
            }
        }

        private void CreatePillar(Transform parent, Vector3 pos, Material mat)
        {
            var p = GameObject.CreatePrimitive(PrimitiveType.Cube);
            p.name = "GatePillar";
            var col = p.GetComponent<Collider>();
            if (col != null) { if (Application.isPlaying) Destroy(col); else DestroyImmediate(col); }
            p.transform.SetParent(parent, false);
            p.transform.position = pos + Vector3.up * 2.75f;
            p.transform.localScale = new Vector3(0.5f, 5.5f, 0.5f);
            p.GetComponent<Renderer>().sharedMaterial = mat;
        }

        private enum GateMode { Inactive, Active, Finish, Passed }

        private void SetGate(int i, GateMode mode)
        {
            if (i < 0 || i >= _gateMat.Count) return;
            Color emis;
            switch (mode)
            {
                case GateMode.Active: emis = new Color(0.15f, 0.95f, 0.3f); break;
                case GateMode.Finish: emis = new Color(0.95f, 0.75f, 0.15f); break;
                case GateMode.Passed: emis = new Color(0.04f, 0.05f, 0.07f); break;
                default: emis = new Color(0.08f, 0.12f, 0.3f); break;
            }
            MaterialFactory.SetEmission(_gateMat[i], emis);
        }
    }
}
