using A2DK.Phys;
using Combat;
using Helpers;
using UnityEngine;
using UnityEngine.Serialization;

namespace Mechanics
{
    [RequireComponent(typeof(ZiplineStateMachine))]
    public class Zipline : Solid, IPunchable
    {
        [SerializeField] private Transform trackStart;
        [SerializeField] private Transform trackEnd;

        [SerializeField] private float mass;
        private ZiplineStateMachine _sm;

        [SerializeField] private float speed;
        public void SetSpeed(int s) => speed = s;
        
        void Awake()
        {
            // trackStart = GetComponentInParent<ZiplineHolder>().Endpoint.transform;
            if (trackStart == null || trackEnd == null)
            {
                Debug.LogError("NO TRACK START/END");
            }

            _sm = GetComponent<ZiplineStateMachine>();
        }

        public override bool OnCollide(PhysObj p, Vector2 direction)
        {
            IZiplineResponse response = p.GetComponent<IZiplineResponse>();

            if (response != null)
            {
                response.OnZiplineCollide(this, direction);
            }
            return base.OnCollide(p, direction);
        }
        
        public override bool Collidable(PhysObj collideWith)
        {
            return true;
        }

        private void FixedUpdate()
        {
            velocity = _sm.CalculateVelocity();
            MoveTick();
        }
        
        public void SetTrackEndpoint(GameObject endPoint)
        {
            trackEnd.position = endPoint.transform.position + new Vector3(4, 4, 0);
            DestroyImmediate(endPoint);
        }
        
        //Returns true if it's past any endpoint. Works for any two endpoints.
        // public bool ReachedEndpoint() =>
        //     Vector3.Dot(trackStart.position - transform.position, trackEnd.position - transform.position) >= 0;
        
        public bool ReachedStart() =>
            Vector3.Dot(trackStart.position - transform.position, trackStart.position - trackEnd.position) <= 0;
        public bool ReachedEnd() =>
            Vector3.Dot(trackEnd.position - transform.position, trackStart.position - trackEnd.position) >= 0;

        public Vector2 VToStart() => (trackStart.position - transform.position).normalized * speed;
        public Vector2 VToEnd() => (trackEnd.position - transform.position).normalized * speed;

        // public void SetPosStart() => Move(trackStart.position - transform.position);
        // public void SetPosEnd() => Move(trackEnd.position - transform.position);
        public bool ReceivePunch(Vector2 v)
        {
            _sm.TouchGrapple();
            return true;
        }
    }
}