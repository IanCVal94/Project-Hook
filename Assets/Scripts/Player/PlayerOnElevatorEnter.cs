using System.Collections;
using Mechanics;
using UnityEngine;
using ASK.Helpers;
using World;

namespace Player
{
    public class PlayerOnElevatorEnter : OnElevatorEnter
    {
        [SerializeField] private float delay;
        
        public override void OnEnter(ElevatorOut elevator)
        {
            StartCoroutine(Helper.DelayAction(delay, () => Teleport(elevator)));
        }

        private void Teleport(ElevatorOut elevator)
        {
            transform.position = elevator.Destination.transform.position;
        }
    }
}