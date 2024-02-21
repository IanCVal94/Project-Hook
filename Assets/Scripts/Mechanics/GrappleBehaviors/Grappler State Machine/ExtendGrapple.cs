using ASK.Core;
using Helpers;
using Mechanics;
using UnityEngine;

namespace Mechanics
{
    public partial class GrapplerStateMachine
    {
        public class ExtendGrapple : GrappleState
        {
            private float _grappleDuration;
            private Timescaler.TimeScale _timescale;
            // private GameTimer2 _timescaleTimer;
            
            public override void Enter(GrappleStateInput i) {
                _grappleDuration = 0;
                _timescale = Game.TimeManager.ApplyTimescale(MySM.GrappleBulletTimeScale, 2);
                Input.CurGrappleExtendPos = MySM.MyPhysObj.transform.position;

                // _timescaleTimer = GameTimerManager.Instance.StartTimer(
                //     MyCore.GrappleBulletTimeDuration,
                //     () => { ResetTimeScale(); print("Timer done");},
                //     // ResetTimeScale,
                //     IncrementType.FIXED_UPDATE
                // );
            }

            public override void Exit(GrappleStateInput i) {
                base.Exit(i);
                Game.TimeManager.RemoveTimescale(_timescale);
                // GameTimerManager.Instance.RemoveTimer(_timescaleTimer);
                Input.CurrentGrapplePos = Input.CurGrappleExtendPos;
            }
            
            public override void FixedUpdate()
            {
                base.FixedUpdate();

                if (MySM.GrappleTooFar())
                {
                    GrappleFinished();
                    return;
                }
                
                _grappleDuration += Game.TimeManager.FixedDeltaTime;
                
                var updateData = MySM.GrappleExtendUpdate(_grappleDuration, MySM.GetGrappleInputPos());
                
                Input.CurGrappleExtendPos = updateData.curPoint;
                Input.AttachedTo = updateData.attachedTo;
                
                if (updateData.attachedTo != null)
                {
                    GrappleapleType grappleType = updateData.attachedTo.GetGrappleType();
                    
                    if (grappleType == GrappleapleType.BREAK)
                    {
                        MySM.Transition<Idle>();
                        return;
                    }
                    if (grappleType == GrappleapleType.SWING) MySM.Transition<Swinging>();
                    if (grappleType == GrappleapleType.PULL) MySM.Transition<Pulling>();
                    MySM.OnGrappleAttach?.Invoke();
                }
            }

            public override void GrappleStarted() {}

            public override void GrappleFinished() {
                MySM.Transition<Idle>();
            }

            public override void CollideHorizontal()
            {
                if (MySM.GrappleCollideWallStop)
                {
                    MySM.Transition<Idle>();
                }
                base.CollideHorizontal();
            }
            public override void CollideVertical()
            {
                if (MySM.GrappleCollideWallStop)
                {
                    MySM.Transition<Idle>();
                }
                base.CollideVertical();
            }

            public override Vector2 PhysTick(Vector2 velocity, Vector2 newV, int getMovementInput) => newV;
        }
    }
}