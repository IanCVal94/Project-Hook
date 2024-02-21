using UnityEngine;

namespace World
{
    public abstract class OnElevatorEnter : MonoBehaviour
    {
        public abstract void OnEnter(ElevatorOut e);
    }
}