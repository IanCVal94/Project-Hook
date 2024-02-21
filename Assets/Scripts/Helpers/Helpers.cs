using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using UnityEngine;

namespace Helpers
{
    public static class Helpers
    {
        public static Vector2 Abs(this Vector2 v) => new Vector2(Mathf.Abs(v.x), Mathf.Abs(v.y));
        
        public static Vector2 CombineVectorsWithReset(Vector2 original, Vector2 apply)
        {
            Vector2 proj = Vector3.Project(original, apply);
            bool oppositeDir = Vector2.Dot(apply, proj) < 0;
            return apply + (oppositeDir ? Vector2.zero : proj);
        }
        public static void SetNoise(this CinemachineVirtualCamera c, NoiseSettings n) => c.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_NoiseProfile = n;
        
        public static float ClosestBetween(float a, float b, float x) {
            if (x <= a || x >= b) return x;
            return x < (b-a)/2 + a ? a : b;
        }
        
        public static Vector2 Rotate(Vector2 v, float delta) {
            return new Vector2(
                v.x * Mathf.Cos(delta) - v.y * Mathf.Sin(delta),
                v.x * Mathf.Sin(delta) + v.y * Mathf.Cos(delta)
            );
        }
        
        public static Dictionary<K, E> CollapseToDictionary<K, E>(this IGrouping<bool, KeyValuePair<K, E>> grouping)
        {
            return grouping.ToDictionary(kv => kv.Key, kv => kv.Value);
        }
    }
}