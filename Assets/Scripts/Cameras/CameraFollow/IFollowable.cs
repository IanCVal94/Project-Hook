namespace Cameras
{
    public interface IFollowable
    {
        /**
         * Higher is better
         */
        public int ResolvePriority();
    }
}