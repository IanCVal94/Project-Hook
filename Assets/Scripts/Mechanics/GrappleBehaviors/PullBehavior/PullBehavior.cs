using A2DK.Phys;
using Mechanics.GrappleBehaviors.PullBehavior;
using UnityEngine;
using UnityEngine.Events;
using static Helpers.Helpers;

namespace Mechanics
{
    [RequireComponent(typeof(PullBehaviorStateMachine), typeof(Actor))]
    public class PullBehavior : MonoBehaviour, IGrappleable, IStickyable

    {
        private Actor _myActor;
        private PullBehaviorStateMachine _sm;

        [SerializeField] private float minPullV;
        public float MinPullV => minPullV;
        [SerializeField] private float initPullMag;
        public float InitPullMag => initPullMag;
        [SerializeField] private float grappleLerp;
        public float GrappleLerp => grappleLerp;
        [SerializeField] private float distanceScale;
        public float DistanceScale => distanceScale;
        [SerializeField] private float keepVGraceTime;
        public float KeepVGraceTime => keepVGraceTime;

        [SerializeField] private UnityEvent _onAttachGrapple;
        [SerializeField] private UnityEvent _onDetachGrapple;

        public bool IsInSticky => _sm.IsOnState<Sticky>();
        
        private void Awake()
        {
            _myActor = GetComponent<Actor>();
            _sm = GetComponent<PullBehaviorStateMachine>();
        }

        public (Vector2 curPoint, IGrappleable attachedTo) AttachGrapple(Actor grappler,
            Vector2 rayCastHit)
        {
            _sm.CurrState.AttachGrapple(grappler.GetComponent<GrapplerStateMachine>());
            _onAttachGrapple?.Invoke();
            
            Vector2 apply = (grappler.transform.position - transform.position).normalized * initPullMag;

            Vector2 newV = CombineVectorsWithReset(grappler.velocity, apply);
            _myActor.SetVelocity(newV);
            
            return (transform.position, this);
        }

        public Vector2 ContinuousGrapplePos(Vector2 grapplePos, Actor grapplingActor)
        {
            Vector2 rawV = grapplingActor.transform.position - transform.position;
            _sm.CurrState.ContinuousGrapplePos(rawV, _myActor);
            return transform.position;
        }
        
        public void OnStickyEnter(Collider2D stickyCollider)
        {
            _sm.CurrState.StickyEnter(_myActor.velocity, stickyCollider.transform);
        }

        public void OnStickyExit(Collider2D stickyCollider)
        {
            _sm.CurrState.StickyExit();
        }

        public PhysObj GetPhysObj() => _myActor;

        public void DetachGrapple()
        {
            _sm.CurrState.DetachGrapple();
            _onDetachGrapple?.Invoke();
        }

        public GrappleapleType GetGrappleType() => GrappleapleType.PULL;

        public void SetV(Vector2 inputBeforeStickyV)
        {
            _myActor.SetVelocity(inputBeforeStickyV);
        }

        public void BreakGrapple()
        {
            _onDetachGrapple?.Invoke();
            _sm.CurrState.BreakGrapple();
        }
    }
}