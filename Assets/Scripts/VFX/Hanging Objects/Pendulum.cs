using System;
using ASK.Core;
using UnityEngine;

namespace VFX
{
    public class Pendulum : MonoBehaviour
    {
        private Func<float, float> _rotFunc;

        [SerializeField] private float speed;
        [SerializeField] private float damping;
        [SerializeField] private float manualSmoothSpeed;

        public bool Simulated = true;
        
        private float _time;

        public float EditorMaxRot;

        private void Awake()
        {
            ResetRot(0);
        }

        void Update()
        {
            if (Simulated)
            {
                float r = _rotFunc(_time);
                SetRotation(r);
                _time += Game.TimeManager.DeltaTime;
            }
        }

        public void ResetRot(float maxRot, bool goOver = false)
        {
            _rotFunc = GetRotFunc(maxRot, goOver);
            _time = 0;
        }

        public void SetRotation(float r)
        {
            var tr = transform.rotation;
            tr.eulerAngles = new Vector3(0, 0, r);
            transform.rotation = tr;
        }

        public void StepRotation(float r)
        {
            float cur = transform.eulerAngles.z;

            if (Mathf.Abs(cur - r) > 180) r += 360;
            
            float th = Mathf.SmoothStep(cur, r, manualSmoothSpeed*Game.TimeManager.DeltaTime);
            SetRotation(th);
        }

        private Func<float, float> GetRotFunc(float maxRot, bool goOver)
        {
            if (!goOver && maxRot > 180) maxRot -= 360;
            return t => Mathf.Cos(speed * t) * maxRot * Mathf.Exp(-damping/10 * t);
        }

        public void ApplyVelocity(Vector2 v)
        {
            ResetRot(transform.eulerAngles.z + 360, true);
        }
    }
}