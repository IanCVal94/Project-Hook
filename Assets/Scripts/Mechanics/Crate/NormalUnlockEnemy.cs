using UnityEngine;
using World;

namespace Mechanics
{
    public class NormalUnlockEnemy : UnlockEnemy
    {
        public override void Kill()
        {
            GetComponent<Crate>().Break(Vector2.zero);
        }
    }
}