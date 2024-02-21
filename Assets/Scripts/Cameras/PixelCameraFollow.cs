using UnityEngine;
using UnityEditor;

namespace VFX {
    public class PixelCameraFollow : MonoBehaviour {
        [SerializeField] private Transform follow;

        [SerializeField] private AnimationCurve smoothingCurve;
        
        [SerializeField] private float smoothIntensity = 0.5f;

        private Vector2 _pixelOffset;

        private void Awake() {
            if (follow == null) follow = transform.parent;
        }

        private int RoundToZero(float x) {
            if (x >= 0) return (int)Mathf.Floor(x);
            else return (int)Mathf.Ceil(x);
        }

        private Vector2Int RoundToZero(Vector2 v) {
            return new Vector2Int(RoundToZero(v.x),RoundToZero(v.y));
        }

        private void Update()
        {
            Vector2 fPos = follow.transform.position;
            Vector2 tPos = (Vector2)transform.position + _pixelOffset;
            // tPos = (tPos + fPos)/smoothIntensity;
            tPos = Vector2.Lerp(tPos, fPos, smoothIntensity);
            Vector2Int snappedTPos = RoundToZero(tPos);

            _pixelOffset = tPos - snappedTPos;
            transform.position = new Vector3(snappedTPos.x, snappedTPos.y, transform.position.z);
        }
    }
}