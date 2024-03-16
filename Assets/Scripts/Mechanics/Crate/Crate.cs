using System;
using A2DK.Phys;
using ASK.Core;
using UnityEngine;
using Combat;
using Helpers;
using UnityEngine.Events;

namespace Mechanics {
    [RequireComponent(typeof(CrateStateMachine))]
    public class Crate : Actor, IPunchable
    {

        [SerializeField] private float _groundedFrictionAccel;
        public float GroundedFrictionAccel => _groundedFrictionAccel;
        [SerializeField] private float _airborneFrictionAccel;
        public float AirborneFrictionAccel => _airborneFrictionAccel;
        
        private CrateStateMachine _stateMachine;

        private bool _beingGrappled = false;

        [SerializeField] private float breakVelocity;
        [SerializeField] private UnityEvent<Vector2, Vector2> onBreak;
        [SerializeField] private UnityEvent<Vector2> onPunch;

        private GameTimer2 _punchTimer;
        [SerializeField] private float breakTimeWindow = 0.25f;

        protected void Awake()
        {
            _stateMachine = GetComponent<CrateStateMachine>();
        }

        public override bool Collidable(PhysObj collideWith)
        {
            //TODO: :grimmace:
            if (collideWith as Solid)
            {
                return collideWith.Collidable(this);
            }
            else if (collideWith as Actor)
            {
                return false;
            }

            return false;
        }

        public override bool Squish(PhysObj p, Vector2 d)
        {
            Destroy(gameObject);
            return false;
        }

        void FixedUpdate()
        {
            _stateMachine.CurrState.SetGrounded(IsGrounded(), IsMovingUp);
            ApplyVelocity(ResolveJostle());
            velocityX = _stateMachine.ApplyXFriction(velocityX);
            MoveTick();
        }

        public bool ShouldBreak(Vector2 direction, PhysObj against, bool col)
        {
            if (GameTimer2.TimerRunning(_punchTimer) && col && breakVelocity < 1000) return true;
            return ((against.velocity - this.velocity) * direction).magnitude >= breakVelocity && !_beingGrappled;
        }

        public override bool OnCollide(PhysObj p, Vector2 direction) {
            bool col = base.OnCollide(p, direction);
            if (p is Crate otherCrate)
            {
                if (!_beingGrappled && otherCrate.ShouldBreak(direction, this, col))
                {
                    otherCrate.BreakAgainst(this);
                }
            } else if (p is BreakableSolid otherSolid)
            {
                if (otherSolid.ShouldBreak(direction, this))
                {
                    otherSolid.BreakAgainst(this);
                    return false;
                }
            } else if (p is Semisolid && _beingGrappled)
            {
                return false;
            }
            
            if (col) {
                if (ShouldBreak(direction, p, col))
                {
                    BreakAgainst(p);
                    return true;
                }
                
                if (direction.x != 0) {
                    velocityX = 0;
                } else if (direction.y != 0) {
                    if (direction.y > 0) {
                        BonkHead();
                    }
                    if (direction.y < 0) {
                        Land();
                    }
                }
            }

            return col;
        }

        public void Break(Vector2 v)
        {
            onBreak?.Invoke(v, transform.position);
            gameObject.SetActive(false);
        }
        
        private void BreakAgainst(PhysObj p)
        {
            Break(p.velocity - velocity);
        }

        public float ApplyXFriction(float prevXVelocity, float frictionAccel)
        {
            if (_beingGrappled) return prevXVelocity;
            
            float accel = frictionAccel;
            accel *= Game.TimeManager.FixedDeltaTime;
            return Mathf.SmoothStep(prevXVelocity, 0f, accel);
        }

        public override void Fall()
        {
            if (!_beingGrappled) base.Fall();
        }

        public bool ReceivePunch(Vector2 v)
        {
            velocity = v;
            onPunch?.Invoke(v);
            _punchTimer = GameTimerManager.Instance.StartTimer(breakTimeWindow, () => {}, IncrementType.FIXED_UPDATE);
            return true;
        }

        public void SetBeingGrappled(bool b) => _beingGrappled = b;
    }
}