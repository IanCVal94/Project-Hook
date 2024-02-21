using System;
using A2DK.Phys;
using ASK.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace VFX
{
    [RequireComponent(typeof(LineRenderer))]
    public class ScarfPoint : MonoBehaviour
    {
        [SerializeField] private float velocityDecay;
        [SerializeField] private Transform attachedTo;

        private LineRenderer _lr;
        private Scarf _scarfManager;

        [SerializeField] private Vector2 gravity;
        public int FlipGravityX;
        public void SetAttachedTo(Transform i) => attachedTo = i;

        private void Awake()
        {
            _lr = GetComponent<LineRenderer>();
            _scarfManager = GetComponentInParent<Scarf>();
        }

        private void Update()
        {
            if (attachedTo == null) Destroy(this);
            _lr.SetPosition(0, transform.position);
            _lr.SetPosition(1, attachedTo.position);
        }

        public void CalcPos(float minSpacing, float maxSpacing)
        {
            Vector2 g = new Vector2(gravity.x * FlipGravityX, gravity.y);
            transform.position += (Vector3)g * Game.TimeManager.GetTimeScale();
            
            Vector2 targetPos = attachedTo.position;
            Vector2 curPos = transform.position;
            
            Vector3 newPos = Vector3.Lerp(transform.position, targetPos, velocityDecay);
            if (Vector3.Distance(newPos, attachedTo.position) < minSpacing)
            {
                newPos = Vector3.MoveTowards(curPos, targetPos, ((curPos - targetPos).magnitude - minSpacing));
            }

            transform.position = newPos;
            // transform.position = Vector3.Lerp(transform.position, newPos, Game.TimeManager.TimeScale);
        }
    }
}