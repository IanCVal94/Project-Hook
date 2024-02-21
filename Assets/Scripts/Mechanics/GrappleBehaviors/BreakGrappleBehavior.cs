using System;
using A2DK.Phys;
using UnityEngine;

namespace Mechanics
{
    public class BreakGrappleBehavior : MonoBehaviour, IGrappleable
    {
        private PhysObj p;
        
        private void Awake()
        {
            p = GetComponent<PhysObj>();
        }

        public (Vector2 curPoint, IGrappleable attachedTo) AttachGrapple(Actor grappler,
            Vector2 rayCastHit)
        {
            return (rayCastHit, this);
        }

        public Vector2 ContinuousGrapplePos(Vector2 grapplePos, Actor grapplingActor) => Vector2.zero;

        public PhysObj GetPhysObj() => p;

        public void DetachGrapple() {}
        public GrappleapleType GetGrappleType() => GrappleapleType.BREAK;
    }
}