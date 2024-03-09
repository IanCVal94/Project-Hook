using System;
using System.Collections;
using ASK.Core;
using Mechanics;
using UnityEngine;
using ASK.Helpers;
using Cameras;
using UnityEngine.InputSystem;
using World;

namespace Player
{
    [RequireComponent(typeof(PlayerCore))]
    public class PlayerOnElevatorEnter : OnElevatorEnter
    {
        [SerializeField] private float delay;
        
        private bool hasPlayerInput = false;
        private Timescaler.TimeScale ts;
        public float timeScaleAmount = 0.1f;
        public float launchMultiplier = 3f; // 3 seems to work very well in general but could be messed with
        public float timeToClick = 6f;

        [SerializeField] private float minBoost = 50;

        private PlayerCore _core;

        private Vector2 _prevV;

        private void Awake()
        {
            _core = GetComponent<PlayerCore>();
        }

        public override void OnEnter(ElevatorOut elevator)
        {
            StartCoroutine(Helper.DelayAction(delay, () => Teleport(elevator)));
            _prevV = _core.Actor.velocity;
        }

        private void Teleport(ElevatorOut elevator)
        {
            transform.position = elevator.Destination.transform.position;
            
            // Start a coroutine to wait for player input
            StartCoroutine(WaitForPlayerInput());
        }

        private IEnumerator WaitForPlayerInput()
        {
            // Start the timer
            float timer = 0f;

            // Apply the time scale
            ts = Game.TimeManager.ApplyTimescale(timeScaleAmount, 2);

            // Continue looping until player input is received or the timer reaches 10 seconds
            while (!_core.Input.GetParryInput() && timer < timeToClick)
            {
                // Increment the timer
                timer += Time.unscaledDeltaTime;

                yield return null; // Yield execution until the next frame
            }

            // Player input received
            Vector3 mousePosition = _core.Input.GetAimPos(_core.Actor.transform.position);
            //Cursor.lockState = CursorLockMode.None;
            Game.TimeManager.RemoveTimescale(ts);

            // Calculate the direction to the mouse position
            Vector2 launchDirection = (mousePosition - transform.position).normalized;

            float magnitude = Mathf.Max(_prevV.magnitude * launchMultiplier, minBoost);
            
            // Call the Boost method on playerActor with launchDirection as parameter
            _core.Actor.SetVelocity(launchDirection * magnitude);
        }
    }
}