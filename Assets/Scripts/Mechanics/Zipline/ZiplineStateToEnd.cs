using UnityEngine;

namespace Mechanics
{
    public class ZiplineStateToEnd : ZiplineState
    {
        public override void TouchGrapple()
        {
            MySM.Transition<ZiplineStateToStart>();
        }

        public override Vector2 CalculateVelocity() => MySM.MyPhysObj.VToEnd();

        public override void FixedUpdate()
        {
            if (MySM.MyPhysObj.ReachedEnd())
            {
                // MySM.MyPhysObj.SetPosEnd();
                MySM.Transition<ZiplineStateEnd>();
            }
        }
    }
}