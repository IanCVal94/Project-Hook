using UnityEngine;
using VFX.Lines;

namespace VFX
{
    public class HangingPlant : MonoBehaviour
    {
        //Tiled function - do not remove
        public void SetConnector(GameObject fixture)
        {
            var lines = GetComponentsInChildren<LineRendererAnchors>();
            foreach (var l in lines)
            {
                l.Anchor1 = fixture.transform;
            }

            var springs = fixture.GetComponentsInChildren<SpringJoint2D>();
            foreach (var s in springs)
            {
                print(s);
                s.connectedBody = GetComponentInChildren<Rigidbody2D>();
            }
        }
    }
}