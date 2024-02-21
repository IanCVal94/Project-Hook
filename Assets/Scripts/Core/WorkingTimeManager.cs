using UnityEngine;
using System;
using ASK.Core;

namespace Core
{
    [RequireComponent(typeof(Timescaler))]
    public class WorkingTimeManager : TimeManager
    {
        private float _currentTimeScale = 1;
        [SerializeField] private float timeAcceleration = 0.45f;

        /*
		 * Smooths timescale transition between different states.
		 *
		 * @return currentTimeScale
		 */
        public override float GetTimeScale()
        {
            float targetTimeScale = base.GetTimeScale();
            float scaleFactor = timeAcceleration * UnityEngine.Time.deltaTime;

            _currentTimeScale = Mathf.MoveTowards(_currentTimeScale, targetTimeScale, _currentTimeScale * scaleFactor);

            return _currentTimeScale;
        }
    }
}