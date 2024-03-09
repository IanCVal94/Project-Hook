using System;
using System.Data;
using Combat;
using UnityEngine;

using MyBox;

using UnityEngine.Serialization;

namespace Player
{
    //L: The purpose of this class is to ensure that all player components are initialized properly, and it helps keep all of the player properties in one place.
    [RequireComponent(typeof(PlayerActor))]
    [RequireComponent(typeof(MovementStateMachine))]
    [RequireComponent(typeof(PlayerGrapplerStateMachine))]
    [RequireComponent(typeof(ParryStateMachine))]
    [RequireComponent(typeof(PlayerInputController))]
    [RequireComponent(typeof(PlayerDeathManager))]
    [RequireComponent(typeof(PlayerHealth))]
    public class PlayerCore : MonoBehaviour
    {
        // public GrappleHook MyGrappleHook;

        #region Player Properties
        [Foldout("Move", true)]
        [SerializeField] public int MoveSpeed;

        [SerializeField] public int MaxAcceleration;

        [SerializeField] public int MaxAirAcceleration;

        [SerializeField] public int MaxDeceleration;

        [SerializeField] public int AirResistance;

        [Tooltip("Timer between the player crashing into a wall and them getting their speed back (called a cornerboost)")]
        [SerializeField] public float CornerboostTimer;

        [Tooltip("Cornerboost speed multiplier")]
        [SerializeField] public float CornerboostMultiplier;
        
        [Tooltip("Delay for jumping off whatever the player is riding")]
        [SerializeField]
        public float JostleBoostGraceTime;

        [Foldout("Jump", true)]
        [SerializeField] public int JumpHeight;

        [SerializeField] public int DoubleJumpHeight;
        
        [SerializeField] public int AddDoubleJumpHeight;

        [SerializeField] public float JumpCoyoteTime;

        [SerializeField] public float JumpBufferTime;

        [Tooltip("Y velocity after the player hits their head on the ceiling")]
        [SerializeField] public float BonkHeadV;

        [SerializeField, Range(0f, 1f)] public float JumpCutMultiplier;

        [Foldout("Dive", true)]
        [SerializeField] public int DiveVelocity;

        [SerializeField] public int DiveDeceleration;

        [Foldout("Dogo", true)]

        [Tooltip("Velocity multiplier from the discarded direction after you it a wall")]
        [SerializeField] public float HitWallGrappleMult;

        [Tooltip("Velocity multiplier for when you move left/right while grappling")]
        [SerializeField] public float MoveXGrappleMult;

        [FormerlySerializedAs("ParryPreCollisionWindow")]
        [Foldout("Combat", true)]

        [Tooltip("Time window before hitting a wall")]
        [SerializeField] public float PunchPreCollisionWindow;

        [Tooltip("Velocity to add after punch bouncing")]
        [SerializeField] public float PunchBounceBoost;

        [Tooltip("After taking damage set speed to this")]
        [SerializeField] public float TakeDamageSpeed;
        
        [Foldout("RoomTransitions", true)]
        [SerializeField, Range(0f, 1f)] public float RoomTransitionVCutX = 0.5f;

        [SerializeField, Range(0f, 1f)] public float RoomTransitionVCutY = 0.5f;
        
        [SerializeField] public float DeathTime;

        #endregion

        public MovementStateMachine MovementStateMachine { get; private set; }
        public PlayerGrapplerStateMachine GrapplerStateMachine { get; private set; }
        public ParryStateMachine ParryStateMachine { get; private set; }
        public PlayerActor Actor { get; private set; }
        public Puncher Puncher { get; private set; }

        private PlayerDeathManager _pDM;
        public PlayerDeathManager DeathManager
        {
            get
            {
                if (_pDM == null) _pDM = GetComponent<PlayerDeathManager>();
                return _pDM;
            }
        }

        private PlayerHealth _playerHealth;
        public PlayerHealth Health {
            get {
                if (_playerHealth == null) _playerHealth = GetComponent<PlayerHealth>();
                return _playerHealth;
            }
        }
        
        private Damageable _playerDamageable;

        public Damageable PlayerDamageable
        {
            get
            {
                if (_playerDamageable == null) _playerDamageable = GetComponentInChildren<Damageable>();
                return _playerDamageable;
            }
        }

        private PlayerInputController _input;
        public PlayerInputController Input
        {
            get
            {
                if (_input == null) _input = GetComponentInChildren<PlayerInputController>();
                return _input;
            }
        }

        [NonSerialized] public PlayerAnimationStateManager AnimManager;
        
        private void Awake()
        {
            // InitializeSingleton(false); //L: Don't make player persistent, bc then there'll be multiple players OO
            MovementStateMachine = gameObject.GetComponent<MovementStateMachine>();
            GrapplerStateMachine = gameObject.GetComponent<PlayerGrapplerStateMachine>();
            ParryStateMachine = gameObject.GetComponent<ParryStateMachine>();
            Actor = gameObject.GetComponent<PlayerActor>();
            AnimManager = GetComponentInChildren<PlayerAnimationStateManager>();
            Puncher = GetComponentInChildren<Puncher>();
            
            if (AnimManager == null) throw new ConstraintException("PlayerCore must have AnimManager");
            //gameObject.AddComponent<PlayerCrystalResponse>();
            //gameObject.AddComponent<PlayerSpikeResponse>();
        }
    }
}