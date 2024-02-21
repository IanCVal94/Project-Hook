using ASK.Helpers;
using UnityEngine;
using World;

namespace Spawning
{
    [RequireComponent(typeof(Room))]
    public class RoomSpawnSolver : MonoBehaviour, IFilterLoggerTarget
    {
        private Room _room;
        private PlayerSpawnManager _player;
        
        private Spawn[] _spawns;
        public Spawn[] Spawns { get; private set; }


        void Awake()
        {
            _player = FindObjectOfType<PlayerSpawnManager>();
            _room = GetComponent<Room>();
            FetchMechanics();
        }
        
        
        void Start()
        {
            if (this == _player.CurrentRoom)
            {
                
            }
            else
            {
                _room.SetRoomGridEnabled(false);
            }
        }
        
        private void OnTriggerStay2D(Collider2D other)
        {
            bool isPlayer = other.gameObject == _player.gameObject;
            bool needTransition = _player.CurrentRoom != _room;
            if (isPlayer && needTransition) 
            {
                /*
                 * This check ensures that the player can only ever be in one room at a time.
                 * It says that not only does the player need to collide, but the entire bounding box needs to be in the room.
                 */
                if (_room.ContainsCollider(other))
                {
                    _room.TransitionToThisRoom();
                }
            }
        }
        
        void FetchMechanics()
        {
            // _resettables = GetComponentsInChildren<IResettable>(includeInactive:true);
            _spawns = GetComponentsInChildren<Spawn>(includeInactive:true);
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