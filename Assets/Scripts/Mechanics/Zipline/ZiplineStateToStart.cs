using UnityEngine;

namespace Mechanics
{
    public class ZiplineStateToStart : ZiplineState
    {
        public override void TouchGrapple()
        {
            MySM.Transition<ZiplineStateToEnd>();
        }

        public override Vector2 CalculateVelocity() => MySM.MyPhysObj.VToStart();

        public override void FixedUpdate()
        {
            if (MySM.MyPhysObj.ReachedStart())
            {
                // MySM.MyPhysObj.SetPosStart();
                MySM.Transition<ZiplineStateStart>();
            }
        }
    }
}