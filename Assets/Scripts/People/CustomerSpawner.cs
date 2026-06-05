using System.Collections.Generic;
using UnityEngine;
using Autobazar.Core;
using Autobazar.Managers;
using Autobazar.Vehicles;

namespace Autobazar.People
{
    /// <summary>
    /// Vytváří zákazníky u vstupu. Hlídá počet současně čekajících i volná místa.
    /// Přání zákazníka vybírá z typů aut, která jsou ještě na prodej (ať je vždy splnitelné).
    /// </summary>
    public class CustomerSpawner : MonoBehaviour
    {
        [SerializeField] private float firstSpawnDelay = 3f;
        [SerializeField] private float spawnInterval = 16f;
        [SerializeField] private float patienceDuration = 45f;

        private readonly Vector3 _spawnPoint = new Vector3(0f, 0.1f, -24f);
        private readonly Vector3[] _waitSpots =
        {
            new Vector3(-3.5f, 0.1f, -13f),
            new Vector3(0f,    0.1f, -13f),
            new Vector3(3.5f,  0.1f, -13f),
        };
        private bool[] _spotTaken;
        private readonly List<Customer> _active = new List<Customer>();
        private float _timer;

        private void Awake()
        {
            _spotTaken = new bool[_waitSpots.Length];
            _timer = firstSpawnDelay;
        }

        private void Update()
        {
            // Během dialogu, servisu, papírování i testovací jízdy nespawnujeme (čas se pozastaví).
            if (GameState.InputLocked) return;

            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                _timer = spawnInterval;
                TrySpawn();
            }
        }

        private void TrySpawn()
        {
            int spot = FreeSpot();
            if (spot < 0) return; // plno

            CarType? desired = PickDesiredType();
            if (desired == null) return; // není co prodávat

            var go = BuildCustomerObject(_spawnPoint, RandomShirtColor());
            var customer = go.AddComponent<Customer>();
            customer.SpotIndex = spot;
            customer.Setup(desired.Value, _waitSpots[spot], _spawnPoint, patienceDuration);
            customer.OnDone += HandleDone;

            _spotTaken[spot] = true;
            _active.Add(customer);

            if (TaskManager.Instance != null)
                TaskManager.Instance.SetTask("Přišel zákazník – dojdi k němu a stiskni E.");
        }

        private void HandleDone(Customer customer)
        {
            if (customer.SpotIndex >= 0 && customer.SpotIndex < _spotTaken.Length)
                _spotTaken[customer.SpotIndex] = false;
            _active.Remove(customer);
        }

        private int FreeSpot()
        {
            for (int i = 0; i < _spotTaken.Length; i++)
                if (!_spotTaken[i]) return i;
            return -1;
        }

        private CarType? PickDesiredType()
        {
            var types = new List<CarType>();
            foreach (var car in Object.FindObjectsByType<CarInteractable>(FindObjectsSortMode.None))
                if (!car.Data.isSold) types.Add(car.Data.type);

            if (types.Count == 0) return null;
            return types[Random.Range(0, types.Count)];
        }

        private static Color RandomShirtColor()
        {
            Color[] palette =
            {
                new Color(0.8f, 0.3f, 0.3f), new Color(0.3f, 0.5f, 0.8f),
                new Color(0.4f, 0.7f, 0.4f), new Color(0.8f, 0.7f, 0.3f),
                new Color(0.6f, 0.4f, 0.7f), new Color(0.3f, 0.7f, 0.7f),
            };
            return palette[Random.Range(0, palette.Length)];
        }

        private static GameObject BuildCustomerObject(Vector3 pos, Color shirt)
        {
            var go = new GameObject("Customer");
            go.transform.position = pos;

            var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            body.name = "Body";
            DestroyCollider(body);
            body.transform.SetParent(go.transform, false);
            body.transform.localPosition = new Vector3(0f, 1f, 0f);
            body.transform.localScale = new Vector3(0.7f, 1f, 0.7f);
            body.GetComponent<Renderer>().sharedMaterial = MaterialFactory.Create(shirt, 0.3f);

            var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "Head";
            DestroyCollider(head);
            head.transform.SetParent(go.transform, false);
            head.transform.localPosition = new Vector3(0f, 1.8f, 0f);
            head.transform.localScale = new Vector3(0.55f, 0.55f, 0.55f);
            head.GetComponent<Renderer>().sharedMaterial = MaterialFactory.Create(new Color(0.95f, 0.8f, 0.66f));

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
