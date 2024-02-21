using Combat;
using UnityEngine;
using UnityEngine.Events;

namespace Mechanics
{
    public class TV : MonoBehaviour, IPunchable
    {
        private bool _broken;

        [SerializeField] private UnityEvent<Vector2> onPunch;
        
        public bool ReceivePunch(Vector2 v)
        {
            if (_broken) return false;
            _broken = true;
            GetComponent<SpriteRenderer>().enabled = false;
            onPunch.Invoke(v);
            return false;
        }
    }
}