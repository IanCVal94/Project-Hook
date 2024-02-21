using UnityEngine;

namespace Cameras
{
    public class DefaultFollow : MonoBehaviour, IFollowable
    {
        public int ResolvePriority() => 0;
    }
}