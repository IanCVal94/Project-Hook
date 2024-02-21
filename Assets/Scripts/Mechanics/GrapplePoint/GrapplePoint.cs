using A2DK.Phys;
using UnityEngine;
using Combat;
using UnityEngine.Events;

namespace Mechanics {
    public class GrapplePoint : Solid, IPunchable
    {
        [SerializeField] private UnityEvent<Vector2> onBreak;

        private bool _broken = false;
        
        public override bool Collidable(PhysObj collideWith) => false;

        public bool ReceivePunch(Vector2 punchVelocity)
        {
            if (_broken) return false;
            
            Break(punchVelocity);
            return true;
        }

        private void Break(Vector2 punchVelocity)
        {
            _broken = true;
            onBreak.Invoke(punchVelocity);
        }
    }
}