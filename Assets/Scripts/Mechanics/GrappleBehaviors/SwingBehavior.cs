using A2DK.Phys;
#if UNITY_EDITOR
using ASK.Editor;
#endif
using UnityEngine;
using UnityEngine.Events;

namespace Mechanics
{
    public class SwingGrappleBehavior : MonoBehaviour, IGrappleable
    {
        private PhysObj _myPhysObj;
        [SerializeField] private UnityEvent onAttachGrapple;
        [SerializeField] private bool useAnchor;

        #if UNITY_EDITOR
        [ShowIf(ActionOnConditionFail.JustDisable, ConditionOperator.And,
            nameof(useAnchor))]
        [SerializeField]
        #endif
        private Transform anchor;

        #if UNITY_EDITOR
        [ShowIf(ActionOnConditionFail.JustDisable, ConditionOperator.And,
            nameof(useAnchor))]
        [SerializeField]
        #endif
        private Vector2 anchorOffset;
        
        void Awake()
        {
            _myPhysObj = ResolveMyPhysObj();
            if (anchor == null) anchor = transform;
        }

        protected virtual PhysObj ResolveMyPhysObj() => GetComponent<PhysObj>();

        public virtual (Vector2 curPoint, IGrappleable attachedTo) AttachGrapple(Actor grappler,
            Vector2 rayCastHit)
        {
            onAttachGrapple?.Invoke();
            return (GetGrapplePos(rayCastHit), this);
        }
        public virtual Vector2 ContinuousGrapplePos(Vector2 grapplePos, Actor grapplingActor) => GetGrapplePos(grapplePos);

        public Vector2 GetGrapplePos(Vector2 origPos)
        {
            if (useAnchor)
            {
                origPos = (Vector2)anchor.position + anchorOffset;
            }

            return origPos;
        }
        
        public PhysObj GetPhysObj() => _myPhysObj;

        public virtual void DetachGrapple() {}
        public GrappleapleType GetGrappleType() => GrappleapleType.SWING;
    }
}