using ASK.Helpers;
using UnityEngine;
using World;

namespace Spawning
{
    [RequireComponent(typeof(Room))]
    public class RoomSpawnSolver : MonoBehaviour, IFilterLoggerTarget
    {
        private Room _room;

        void Awake()
        {
            _room = GetComponent<Room>();
            FetchMechanics();
        }
        
        private void OnTriggerStay2D(Collider2D other)
        {
            var player = other.GetComponent<PlayerSpawnManager>();
            if (player != null)
            {
                if (_room.ContainsCollider(other) && player.CurrentRoom != _room)
                {
                    _room.TransitionToThisRoom();
                }
            }
        }
        
        void FetchMechanics()
        {
            // _resettables = GetComponentsInChildren<IResettable>(includeInactive:true);
            // _spawns = GetComponentsInChildren<Spawn>(includeInactive:true);
        }
        
        private void OnValidate()
        {
            Spawn spawn = GetComponentInChildren<Spawn>();
            if (spawn == null)
            {
                FilterLogger.LogWarning(this, $"The room {gameObject.name} does not have a spawn point. Every room should have at least one spawn point.");
            }
        }

        public LogLevel GetLogLevel()
        {
            return LogLevel.Error;
        }
    }
}