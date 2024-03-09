using System;
using System.Collections;
using ASK.Core;
using ASK.Helpers;
using A2DK.Phys;
using Combat;
using Mechanics;
using Player;
using World;
using Helpers;

using MyBox;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider2D))]
public class PlayerActor : Actor, IFilterLoggerTarget {
    private MovementStateMachine _movementStateMachine => _core.MovementStateMachine;
    private GrapplerStateMachine GrapplerStateMachine => _core.GrapplerStateMachine;
    [SerializeField, AutoProperty(AutoPropertyMode.Parent)] private BoxCollider2D _collider;
    // [SerializeField] private SpriteRenderer sprite;

    private bool _hitWallCoroutineRunning;
    private float _hitWallPrevSpeed;
    
    [Foldout("Movement Events", true)]
    public ActorEvent OnJumpFromGround;
    public ActorEvent OnDoubleJump;
    
    private Func<Vector2, Vector2> _deathRecoilFunc;

    private float _divePosY;

    private PlayerCore _core;

    private void OnEnable()
    {
        Room.RoomTransitionEvent += OnRoomTransition;
        _core = GetComponent<PlayerCore>();
        _core.PlayerDamageable.OnDamaged += TakeDamage;
    }

    private void OnDisable()
    {
        Room.RoomTransitionEvent -= OnRoomTransition;
        _core.PlayerDamageable.OnDamaged -= TakeDamage;
    }

    private void FixedUpdate()
    {
        ApplyVelocity(ResolveJostle());

        int inputX = _core.Input.GetMovementInput();
        
        Vector2 newV = velocity;
        newV = _movementStateMachine.CurrState.PhysTick(velocity, newV, inputX);
        newV = GrapplerStateMachine.CurrState.PhysTick(velocity, newV, inputX);
        velocity = newV;
        
        MoveTick();
    }

    #region Movement
    public Vector2 CalcMovementX(Vector2 curV, int moveDirection, int acceleration, int resistance) {
        int vxSign = (int) Mathf.Sign(curV.x);
        // if (Mathf.Abs(smActor.velocityX))
        // int acceleration = moveDirection == vxSign || moveDirection == 0 ? core.AirResistance : core.MaxAirAcceleration;
        
        
        int targetVelocityX = moveDirection * _core.MoveSpeed;
        // float maxSpeedChange = acceleration * Game.TimeManager.FixedDeltaTime;
        // velocityX = Mathf.MoveTowards(velocityX, targetVelocityX, maxSpeedChange);

        float accel = 0;
        if (moveDirection == 0) {
            accel = resistance;
        } else if (moveDirection == vxSign && Mathf.Abs(curV.x) >= Mathf.Abs(targetVelocityX)) {
            accel = resistance/2;
        } else {
            accel = acceleration;
        }

        accel *= Game.TimeManager.FixedDeltaTime;
        return curV + new Vector2(Math.Clamp(targetVelocityX - curV.x, -accel, accel), 0);
        // return new Vector2(Mathf.MoveTowards(velocityX, targetVelocityX, accel), velocityY);
    }

    #endregion

    #region Jumping

    /// <summary>
    /// Function that bounces the player.
    /// </summary>
    /// <param name="jumpHeight"></param>
    public void Bounce(int jumpHeight)
    {
        float applyV = GetJumpSpeedFromHeight(jumpHeight);
        velocityY = Math.Max(applyV, velocityY + applyV);
    }

    public void DoubleJump()
    {
        float doubleJumpVelocity = GetJumpSpeedFromHeight(_core.DoubleJumpHeight);
        float addDoubleJumpVelocity = GetJumpSpeedFromHeight(_core.AddDoubleJumpHeight);
        velocityY = Math.Max(doubleJumpVelocity, velocityY + addDoubleJumpVelocity);

        OnDoubleJump?.Invoke(transform.position);
    }

    public void JumpFromGround(int jumpHeight)
    {
        Bounce(jumpHeight);

        OnJumpFromGround?.Invoke(transform.position);
    }

    public void JumpCut()
    {
        if (velocityY > 0f)
        {
            velocityY *= _core.JumpCutMultiplier;
        }
    }
    
    public void Boost(Vector2 launchInfo, float launchMultiplier)
    {
        //ApplyVelocity(launchInfo*launchMultiplier);

        velocity = velocity+(launchInfo*launchMultiplier);
        //OnElevator?.Invoke(transform.position);
    }
    #endregion

    #region  Death

    public void Die(Func<Vector2, Vector2> recoilFunc = null)
    {
        if (recoilFunc == null) recoilFunc = v => Vector2.zero;
        _deathRecoilFunc = recoilFunc;
        DeathRecoil();
        _core.DeathManager.Die();
        // Game.Instance.ScreenShakeManagerInstance.Screenshake(
        //     _core.SpawnManager.CurrentRoom.GetComponentInChildren<CinemachineVirtualCamera>(),
        //     10,
        //     1
        //     );
    }

    public void DeathRecoil()
    {
        velocity = _deathRecoilFunc(velocity);
    }

    public void DeadStop()
    {
        velocity = Vector2.zero;
    }

    #endregion

    #region Actor Overrides
    public override bool Collidable(PhysObj collideWith) {
        return collideWith.Collidable(this);
    }

    public override bool OnCollide(PhysObj p, Vector2 direction) {
        if (p != this)
        {
            FilterLogger.Log(this, $"Player collided with object {p} from direction {direction}");
        }
        else
        {
            return false;
        }
        bool col = p.OnCollide(this, direction);
        if (col) {
            if (direction.x != 0) {
                
                Vector2 newV = Vector2.zero;
                Vector2 oldV = velocity;
                newV = HitWall((int)direction.x);
                newV = GrapplerStateMachine.ProcessCollideHorizontal(oldV, newV);
                newV = _core.ParryStateMachine.ProcessCollideHorizontal(oldV, newV);
                velocity += newV;
            } else if (direction.y != 0) {
                GrapplerStateMachine.CollideVertical();
                if (direction.y > 0) {
                    BonkHead();
                }
                if (direction.y < 0) {
                    _movementStateMachine.SetGrounded(true, IsMovingUp);
                    velocityY = 0;
                }
            }
        }

        return col;
    }

    public override bool IsGround(PhysObj whosAsking)
    {
        return false;
    }

    public override bool Squish(PhysObj p, Vector2 d)
    {
        if (OnCollide(p, d))
        {
            Debug.Log("Squish " + p);
            Die();
        }
        return false;
    }
    #endregion

    #region Jostling
    
    private Vector2 _gracePrevV;
    private GameTimer2 _graceTimer;

    protected override bool FloorStopped()
    {
        if (prevRidingOn == GetBelowPhysObj()) return false;
        return base.FloorStopped();
    }

    protected override Vector2 ResolveApplyV(Vector2 v)
    {
        v = base.ResolveApplyV(v);
        if (_graceTimer != null && GameTimer2.TimerRunning(_graceTimer))
        {
            GameTimerManager.Instance.RemoveTimer(_graceTimer);
            v = _gracePrevV;
        }
        return v;
    }

    protected override bool ShouldApplyV()
    {
        return base.ShouldApplyV() && GrapplerStateMachine.CurrState.ShouldApplyV();
    }

    public override Vector2 ResolveJostle()
    {
        if (base.FloorStopped())
        {
            _gracePrevV = prevRidingV;
            _graceTimer = GameTimerManager.Instance.StartTimer(_core.JostleBoostGraceTime, () => { }, IncrementType.FIXED_UPDATE);
        }
        return base.ResolveJostle();
    }
        
    public override void Ride(Vector2 direction)
    {
        Vector2 ret = _core.GrapplerStateMachine.ResolveRide(direction);
        base.Ride(ret);
    }
    
    public override PhysObj RidingOn()
    {
        PhysObj p = base.RidingOn();
        return GrapplerStateMachine.CalcRiding(p);
    }

    public override bool Push(Vector2 direction, Solid solid)
    {
        _core.GrapplerStateMachine.Push(direction, solid);
        return base.Push(direction, solid);
    }

    #endregion

    private Vector2 HitWall(int direction) {
        // _abilityStateMachine.HitWall();
        if (!_hitWallCoroutineRunning) {
            _hitWallPrevSpeed = velocityX;
            _hitWallCoroutineRunning = true;
            StartCoroutine(HitWallLogic(direction));
        }
        return Vector2.left * velocityX;
    }

    //Todo: change to fixedUpdate GameTimer
    private IEnumerator HitWallLogic(int direction) {
        for (float t = 0; t < _core.CornerboostTimer; t += Game.TimeManager.FixedDeltaTime) {
            bool movingWithDir = Math.Sign(velocityX) == Math.Sign(direction) || velocityX == 0;
            if (!movingWithDir) {
                break;
            }
            bool stillNextToWall = CheckCollisions(
                Vector2.right * direction, 
                (physObj, d) => {
                return physObj != this && physObj.Collidable(this);
            });
            if (!stillNextToWall) {
                velocityX = _hitWallPrevSpeed * _core.CornerboostMultiplier;
                break;
            }
            yield return new WaitForFixedUpdate();
        }

        _hitWallCoroutineRunning = false;
    }

    private void OnRoomTransition(Room roomEntering)
    {
        // if (EndCutsceneManager.IsBeegBouncing) return;
        
        velocityX *= _core.RoomTransitionVCutX;
        velocityY *= _core.RoomTransitionVCutY;
    }

    private float GetJumpSpeedFromHeight(float jumpHeight)
    {
        return Mathf.Sqrt(-2f * GravityUp * jumpHeight);
    }

    #region Combat

    public void ParryBounce(Vector2 punchDir)
    {
        punchDir = punchDir.normalized * _core.PunchBounceBoost;
        Vector2 v0 = velocity;
        
        //Split v0 into ortho + normal components
        Vector2 v0n = Vector3.Project(v0, punchDir);
        Vector2 v0o = v0 - v0n;
        velocity = v0o + (Vector2.Dot(punchDir, v0n) <= 0 ? v0n - punchDir : -punchDir);

        _movementStateMachine.RefreshAbilities();
    }

    void TakeDamage(Vector2 enemyPos)
    {
        _movementStateMachine.RefreshAbilities();
        var newV = (Vector2)transform.position - enemyPos;
        velocity = newV.normalized * _core.TakeDamageSpeed;
        GrapplerStateMachine.BreakGrapple();
        _core.Health.PlayerTakeDmg(1);
    }

    #endregion

    public LogLevel GetLogLevel()
    {
        return LogLevel.Error;
    }
    
    #if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Vector2 v = velocity;
        Puncher p = GetComponentInChildren<Puncher>();
        
        float a = p.transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
        Vector2 punchV = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
        
        //Split v0 into ortho + normal components
        Vector2 v0n = Vector3.Project(v, punchV);
        Vector2 v0o = v - v0n;
        Vector2 vf = v0o + (Vector2.Dot(punchV, v0n) >= 0 ? v0n : Vector2.zero) - punchV;
        
        Helper.DrawArrow(transform.position, v.normalized * (v.magnitude+8), Color.blue);
        Helper.DrawArrow(transform.position, punchV * 24, Color.red);
        Helper.DrawArrow(transform.position, vf.normalized * (vf.magnitude+8), Color.yellow);
    }
    #endif
}