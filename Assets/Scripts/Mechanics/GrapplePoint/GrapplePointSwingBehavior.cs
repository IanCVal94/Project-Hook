using A2DK.Phys;
using ASK.Core;
using UnityEngine;
using VFX;

namespace Mechanics
{
    public class GrapplePointSwingBehavior : SwingGrappleBehavior
    {
        [SerializeField] private Pendulum pendulum;
        [SerializeField] private GrapplePoint gPoint;
        
        public override (Vector2 curPoint, IGrappleable attachedTo) AttachGrapple(Actor grappler, Vector2 rayCastHit)
        {
            pendulum.Simulated = false;
            return base.AttachGrapple(grappler, rayCastHit);
        }

        public override Vector2 ContinuousGrapplePos(Vector2 grapplePos, Actor grapplingActor)
        {
            Vector2 actorPos = grapplingActor.transform.position;
            Vector2 pendulumPos = pendulum.transform.position;
            Vector2 direction = actorPos - pendulumPos;
            
            float target = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90;
            pendulum.StepRotation(target);
            
            return base.ContinuousGrapplePos(grapplePos, grapplingActor);
        }

        public override void DetachGrapple()
        {
            pendulum.ResetRot(pendulum.transform.eulerAngles.z);
            pendulum.Simulated = true;
            base.DetachGrapple();
        }

        protected override PhysObj ResolveMyPhysObj() => gPoint;
    }
}