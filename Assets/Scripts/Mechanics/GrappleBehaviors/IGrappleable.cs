using System;
using A2DK.Phys;
using UnityEngine;

namespace Mechanics {
    public enum GrappleapleType
    {
        SWING,
        PULL,
        BREAK
    }
    
    public interface IGrappleable
    {
        public (Vector2 curPoint, IGrappleable attachedTo) AttachGrapple(Actor grappler,
            Vector2 rayCastHit);

        /**
         * Called every frame. Returns new position of grapple regardless of attached PhysObj movement.
         */
        public Vector2 ContinuousGrapplePos(Vector2 grapplePos, Actor grapplingActor);

        public PhysObj GetPhysObj();

        public void DetachGrapple();

        public GrappleapleType GetGrappleType();
    }
}