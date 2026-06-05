using UnityEngine;

namespace Autobazar.Racing
{
    /// <summary>
    /// Sdílené rozložení trati (ovál z bran). Používá ho RaceWorldBuilder (silnice)
    /// i RaceManager (brány, start), aby vždy seděly na stejných místech.
    /// </summary>
    public static class TrackLayout
    {
        public const int GateCount = 10;
        public const float RadiusX = 42f;
        public const float RadiusZ = 34f;

        public static Vector3 GatePos(float idx)
        {
            float a = 2f * Mathf.PI * idx / GateCount;
            return new Vector3(RadiusX * Mathf.Sin(a), 0f, RadiusZ * Mathf.Cos(a));
        }
    }
}
