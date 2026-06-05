using UnityEngine;

namespace Autobazar.Managers
{
    /// <summary>
    /// Klidná ambientní hudba generovaná v kódu (žádný zvukový soubor není potřeba).
    /// Pomalé arpeggio v pentatonice + tichý pad. Smyčka je „bezešvá" (frekvence
    /// jsou naladěné na délku smyčky), takže nelupe na konci.
    /// </summary>
    public class MusicManager : MonoBehaviour
    {
        public static MusicManager Instance { get; private set; }

        [SerializeField, Range(0f, 1f)] private float volume = 0.45f;
        [SerializeField] private float loopLength = 16f;

        private const int SampleRate = 44100;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            var src = gameObject.AddComponent<AudioSource>();
            src.clip = GenerateClip();
            src.loop = true;
            src.volume = volume;
            src.spatialBlend = 0f; // 2D, hraje pořád stejně
            src.playOnAwake = false;
            src.Play();
        }

        private AudioClip GenerateClip()
        {
            int total = Mathf.RoundToInt(SampleRate * loopLength);
            var data = new float[total];

            // Naladíme frekvence na celé počty cyklů ve smyčce → bezešvé opakování.
            float Snap(float f) => Mathf.Round(f * loopLength) / loopLength;

            // C-dur pentatonika
            float[] arp =
            {
                Snap(261.63f), Snap(329.63f), Snap(392.00f), Snap(440.00f),
                Snap(392.00f), Snap(329.63f), Snap(293.66f), Snap(392.00f),
            };
            float noteDur = loopLength / arp.Length;
            float padFreq = Snap(130.81f);     // tichý bas C3
            float tremolo = Snap(0.125f);      // pomalé „dýchání" padu

            float max = 0.0001f;
            for (int i = 0; i < total; i++)
            {
                float t = i / (float)SampleRate;

                // Pad – jemný bas
                float pad = 0.06f * Mathf.Sin(2f * Mathf.PI * padFreq * t)
                                  * (0.8f + 0.2f * Mathf.Sin(2f * Mathf.PI * tremolo * t));

                // Arpeggio
                int idx = (int)(t / noteDur) % arp.Length;
                float nt = t - idx * noteDur;
                float env = Envelope(nt, noteDur);
                float freq = arp[idx];
                float note = 0.18f * env * (Mathf.Sin(2f * Mathf.PI * freq * t)
                                            + 0.3f * Mathf.Sin(2f * Mathf.PI * 2f * freq * t));

                float s = pad + note;
                data[i] = s;
                float a = Mathf.Abs(s);
                if (a > max) max = a;
            }

            // Normalizace na příjemnou hlasitost.
            float gain = 0.6f / max;
            for (int i = 0; i < total; i++) data[i] *= gain;

            var clip = AudioClip.Create("AmbientMusic", total, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        // Měkký náběh a doznění tónu, ať to nelupe.
        private static float Envelope(float nt, float noteDur)
        {
            const float attack = 0.15f;
            const float release = 0.6f;
            float e;
            if (nt < attack) e = nt / attack;
            else if (nt > noteDur - release) e = Mathf.Max(0f, (noteDur - nt) / release);
            else e = 1f;
            return e * 0.85f;
        }
    }
}
