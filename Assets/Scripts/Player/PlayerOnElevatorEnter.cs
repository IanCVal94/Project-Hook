using System;
using System.Collections;
using ASK.Core;
using Mechanics;
using UnityEngine;
using ASK.Helpers;
using Cameras;
using UnityEngine.InputSystem;
using World;
using UnityEngine.UI;

namespace Player
{
    [RequireComponent(typeof(PlayerCore))]
    public class PlayerOnElevatorEnter : OnElevatorEnter
    {
        [SerializeField] private float delay;
        private Text _timerText; // Reference to UI Text to display timer
        [SerializeField] private GameObject timerTextbox;
        [SerializeField] private GameObject destroyTextbox;

        private bool hasPlayerInput = false;
        private Timescaler.TimeScale ts;
        public float timeScaleAmount = 0.1f;
        public float launchMultiplier = 3f;
        public float timeToClick = 6f;
        [SerializeField] private float minBoost = 50;

        private PlayerCore _core;
        private Vector2 _prevV;

        private void Awake()
        {
            _core = GetComponent<PlayerCore>();
            _timerText = timerTextbox.GetComponentInChildren<Text>();
        }

        public override void OnEnter(ElevatorOut elevator)
        {
            StartCoroutine(Helper.DelayAction(delay, () => Teleport(elevator)));
            _prevV = _core.Actor.velocity;
        }

        private void Teleport(ElevatorOut elevator)
        {
            transform.position = elevator.Destination.transform.position;
            timerTextbox.SetActive(true);
            destroyTextbox.SetActive(true);
            StartCoroutine(WaitForPlayerInput());
        }

        private IEnumerator WaitForPlayerInput()
        {
            float timer = timeToClick;

            // Apply the time scale
            ts = Game.TimeManager.ApplyTimescale(timeScaleAmount, 2);

            while (timer > 0 && !_core.Input.GetParryInput())
            {
                // Decrement the timer
                timer -= Time.unscaledDeltaTime;

                // Update UI Text
                if (_timerText != null)
                {
                    _timerText.text = Mathf.CeilToInt(timer).ToString(); // Display timer
                }

                yield return null;
            }

            // Reset UI Text when timer is finished
            if (_timerText != null)
            {
                timerTextbox.SetActive(false);
                destroyTextbox.SetActive(false);
            }

            // Player input received
            Vector3 mousePosition = _core.Input.GetAimPos(_core.Actor.transform.position);
            Game.TimeManager.RemoveTimescale(ts);

            // Calculate the direction to the mouse position
            Vector2 launchDirection = (mousePosition - transform.position).normalized;

            float magnitude = Mathf.Max(_prevV.magnitude * launchMultiplier, minBoost);

            // Call the Boost method on playerActor with launchDirection as parameter
            _core.Actor.SetVelocity(launchDirection * magnitude);
        }
    }
}
