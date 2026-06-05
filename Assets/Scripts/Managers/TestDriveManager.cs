using System.Collections.Generic;
using UnityEngine;
using Autobazar.Core;
using Autobazar.Player;
using Autobazar.Vehicles;

namespace Autobazar.Managers
{
    /// <summary>
    /// Řídí testovací jízdu: převezme auto, přepne kameru, postaví checkpointy a hlídá
    /// čas. Když hráč projede všechny checkpointy včas, zvýší se atraktivita auta.
    /// Po jízdě se auto i hráč vrátí na původní místo.
    /// </summary>
    public class TestDriveManager : MonoBehaviour
    {
        public static TestDriveManager Instance { get; private set; }

        [SerializeField] private float timeLimit = 50f;
        [SerializeField] private float checkpointRadius = 5f;

        private readonly Vector3[] _loop =
        {
            new Vector3(0f, 0f, -10f),
            new Vector3(-16f, 0f, -15f),
            new Vector3(-16f, 0f, 16f),
            new Vector3(0f, 0f, 20f),
            new Vector3(16f, 0f, 16f),
            new Vector3(16f, 0f, -15f),
        };

        private bool _active;
        private CarInteractable _car;
        private CarController _carController;
        private Vector3 _carPos;
        private Quaternion _carRot;

        private GameObject _player;
        private Vector3 _playerPos;
        private Quaternion _playerRot;
        private CameraController _camera;

        private GameObject _checkpointRoot;
        private readonly List<Transform> _checkpoints = new List<Transform>();
        private int _index;
        private float _timeLeft;

        public bool IsActive => _active;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void StartDrive(CarInteractable car)
        {
            if (_active || car == null) return;
            _active = true;
            _car = car;

            _carPos = car.transform.position;
            _carRot = car.transform.rotation;

            var pc = Object.FindFirstObjectByType<PlayerController>();
            _player = pc != null ? pc.gameObject : null;
            if (_player != null)
            {
                _playerPos = _player.transform.position;
                _playerRot = _player.transform.rotation;
                _player.SetActive(false);
            }

            _camera = Camera.main != null ? Camera.main.GetComponent<CameraController>() : null;
            if (_camera != null) _camera.SetTarget(car.transform);

            _carController = car.gameObject.AddComponent<CarController>();
            _carController.ControlEnabled = true;

            GameState.InputLocked = true; // pauza zákazníků během minihry

            BuildCheckpoints();
            _index = 0;
            _timeLeft = timeLimit;

            if (UIManager.Instance != null)
                UIManager.Instance.ShowMessage("ZKUŠEBNÍ JÍZDA!\nW/S = plyn/brzda, A/D = zatáčení.\nProjeď zelené checkpointy popořadě. Esc = konec.", 4.5f);
        }

        private void Update()
        {
            if (!_active) return;

            if (Input.GetKeyDown(KeyCode.Escape)) { EndDrive(false); return; }

            _timeLeft -= Time.deltaTime;
            if (_timeLeft <= 0f) { EndDrive(false); return; }

            if (_index < _checkpoints.Count && _car != null)
            {
                Vector3 a = _car.transform.position; a.y = 0f;
                Vector3 b = _checkpoints[_index].position; b.y = 0f;
                if (Vector3.Distance(a, b) < checkpointRadius)
                {
                    _checkpoints[_index].gameObject.SetActive(false);
                    _index++;
                    if (_index >= _checkpoints.Count) { EndDrive(true); return; }
                }
            }

            if (TaskManager.Instance != null)
                TaskManager.Instance.SetTask($"ZKUŠEBNÍ JÍZDA — checkpoint {_index + 1}/{_checkpoints.Count} — čas {Mathf.CeilToInt(_timeLeft)} s  (Esc = konec)");
        }

        private void EndDrive(bool success)
        {
            if (!_active) return;
            _active = false;

            if (_carController != null) { _carController.ControlEnabled = false; Destroy(_carController); }
            if (_car != null) { _car.transform.position = _carPos; _car.transform.rotation = _carRot; }

            if (_checkpointRoot != null) Destroy(_checkpointRoot);
            _checkpoints.Clear();

            if (_camera != null && _player != null) _camera.SetTarget(_player.transform);
            if (_player != null)
            {
                _player.transform.position = _playerPos;
                _player.transform.rotation = _playerRot;
                _player.SetActive(true);
            }

            GameState.InputLocked = false;

            if (_car != null)
            {
                if (success)
                {
                    var d = _car.Data;
                    d.attractiveness = Mathf.Clamp(d.attractiveness + 15, 0, 100);
                    _car.RefreshLabel();
                    if (UIManager.Instance != null)
                        UIManager.Instance.ShowMessage("Skvělá jízda! Auto je teď atraktivnější (+15).", 3.5f);
                }
                else if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowMessage("Zkušební jízda nedokončena.", 3f);
                }
            }

            if (TaskManager.Instance != null)
                TaskManager.Instance.SetTask("Počkej na zákazníka u vstupu, nebo vylepši auta v servisu.");

            _car = null;
        }

        private void BuildCheckpoints()
        {
            _checkpointRoot = new GameObject("TestDriveCheckpoints");
            _checkpoints.Clear();
            for (int i = 0; i < _loop.Length; i++)
            {
                var cp = BuildCheckpoint(_loop[i], i + 1);
                _checkpoints.Add(cp.transform);
            }
        }

        private GameObject BuildCheckpoint(Vector3 pos, int number)
        {
            var go = new GameObject($"CP_{number}");
            go.transform.SetParent(_checkpointRoot.transform, false);
            go.transform.position = pos;

            var pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pillar.name = "Pillar";
            DestroyCollider(pillar);
            pillar.transform.SetParent(go.transform, false);
            pillar.transform.localPosition = new Vector3(0f, 2.5f, 0f);
            pillar.transform.localScale = new Vector3(0.4f, 2.5f, 0.4f);
            pillar.GetComponent<Renderer>().sharedMaterial =
                MaterialFactory.CreateEmissive(new Color(0.2f, 0.8f, 0.3f), new Color(0.3f, 1f, 0.4f), 1.5f);

            TextFactory.Create(number.ToString(), go.transform, new Vector3(0f, 5.4f, 0f),
                90, 0.3f, Color.white, TextAnchor.MiddleCenter, true);

            return go;
        }

        private static void DestroyCollider(GameObject go)
        {
            var c = go.GetComponent<Collider>();
            if (c == null) return;
            if (Application.isPlaying) Destroy(c); else DestroyImmediate(c);
        }
    }
}
