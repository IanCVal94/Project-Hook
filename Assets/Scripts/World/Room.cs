using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

using ASK.Helpers;
using UnityEditor;

namespace World {
    public class Room : MonoBehaviour {
        private Collider2D _roomCollider;
        private Collider2D roomCollider
        {
            get
            {
                if (_roomCollider == null) _roomCollider = GetComponentInChildren<Collider2D>();
                return _roomCollider;
            }
        }

        private ElevatorIn _elevatorIn;

        public ElevatorIn ElevatorIn
        {
            get
            {
                if (_elevatorIn == null) _elevatorIn = GetComponentInChildren<ElevatorIn>(true);
                return _elevatorIn;
            }
        }
        
        private ElevatorOut _elevatorOut;

        public ElevatorOut ElevatorOut
        {
            get
            {
                if (_elevatorOut == null) _elevatorOut = GetComponentInChildren<ElevatorOut>(true);
                return _elevatorOut;
            }
        }

        private CinemachineBrain _cmBrain;
        
        private GameObject _grid;
        private static Coroutine _transitionRoutine;

        //Invoked whenever child objects change. Mostly used for player spawns
        public event Action RecalculateChildren;

        // private int _numEnables;
        // [SerializeField] private int _maxEnables = 4;

        public delegate void OnRoomTransition(Room roomEntering);
        public static event OnRoomTransition RoomTransitionEvent;

        public Room[] AdjacentRooms;
        // private EndCutsceneManager _endCutsceneManager;

        private void Awake()
        {
            _cmBrain = FindObjectOfType<CinemachineBrain>(true);

            // _endCutsceneManager = FindObjectOfType<EndCutsceneManager>();
            RecalculateChildren?.Invoke();
            
            _grid = transform.GetChild(0).gameObject;
        }

        /*private void OnEnable()
        {
            EndCutsceneManager.BeegBounceStartEvent += TurnOffStopTime;
        }

        private void OnDisable()
        {
            EndCutsceneManager.BeegBounceStartEvent -= TurnOffStopTime;
        }*/

        /*private void Update()
        {
            float dist2CameraToRoomCenter = Vector3.SqrMagnitude(Camera.main.transform.position - _roomCollider.transform.position);
            bool shouldEnable = dist2CameraToRoomCenter < _roomCollider.bounds.size.sqrMagnitude * 1.1;
            if (_player.CurrentRoom == this) SetRoomGridEnabled(true);
            if (shouldEnable != _grid.gameObject.activeSelf)
            {
                SetRoomGridEnabled(shouldEnable);
            }
        }*/

        /**
         * Does this room fully contain the bounds of other?
         */
        public bool ContainsCollider(Collider2D other) => roomCollider.bounds.Contains(other.bounds.min) && roomCollider.bounds.Contains(other.bounds.max);

        public float GetRoomArea()
        {
            Vector3 dims = roomCollider.bounds.size;
            return dims.x * dims.y;
        }

        public Vector2 GetExtents() => roomCollider.bounds.extents;

        public virtual void TransitionToThisRoom()
        {
            SetRoomGridEnabled(true);
            Reset();
            // FilterLogger.Log(this, $"Transitioned to room: {gameObject.name}");
            if (_transitionRoutine != null)
            {
                StopCoroutine(_transitionRoutine);
            }
            _transitionRoutine = StartCoroutine(TransitionRoutine());
        }

        private IEnumerator TransitionRoutine()
        {
            RoomTransitionEvent?.Invoke(this);
            // Time.timeScale = 0f;
            /*
             * This is kinda "cheating". Instead of waiting for the camera to be done switching,
             * we're just waiting for the same amount of time as the blend time between cameras.
             */
            yield return new WaitForSecondsRealtime(_cmBrain.m_DefaultBlend.BlendTime);
            // Time.timeScale = 1f;
            _transitionRoutine = null;
        }
        
        public void Reset()
        {
            /*foreach (var r in _resettables)
            {
                if (r != null && r.CanReset()) r.Reset();
            }*/
        }
        
        public void SetRoomGridEnabled(bool setActive)
        {
            _grid.SetActive(setActive);
        }

        private void DestroyAndRecreateGrid()
        {
            GameObject gridObj = _grid.gameObject; 
            var newGrid = Instantiate(
                gridObj,
                gridObj.transform.position,
                Quaternion.identity,
                gridObj.transform.parent
            );

            gridObj.transform.parent = null; //This is so the mechanics in _grid aren't counted in FetchMechanics.
            Destroy(gridObj);
            _grid = newGrid;
            RecalculateChildren?.Invoke();
        }

        public void RoomSetEnable(bool enable)
        {
            SetRoomGridEnabled(enable);
            Reset();
        }

        public LogLevel GetLogLevel()
        {
            return LogLevel.Error;
        }

        public Room[] CalcAdjacentRooms(Vector2 roomAdjacencyTolerance, LayerMask roomLayerMask)
        {
            var bounds = roomCollider.bounds;
            Vector2 pointB = (Vector2)bounds.max + roomAdjacencyTolerance;
            Vector2 pointA = (Vector2)bounds.min - roomAdjacencyTolerance;

            var hits = Physics2D.OverlapAreaAll(pointA, pointB, roomLayerMask);
            List<Room> ret = new();
            foreach (var hit in hits)
            {
                Room r = hit.GetComponent<Room>();
                if (r != null && r != this)
                {
                    ret.Add(r);
                }
            }

            return ret.ToArray();
        }

        public void SetNextRoom(Room nextRoom)
        {
            // ElevatorIn elevatorIn = curRoom.GetComponentInChildren<ElevatorIn>();
            #if UNITY_EDITOR
            ElevatorOut.SetDestination(nextRoom);
            EditorUtility.SetDirty(ElevatorOut);
            #endif
        }

        public Vector3 GetCenter() => transform.position + new Vector3(GetExtents().x, -GetExtents().y);
        
        #if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            foreach (Room r in AdjacentRooms)
            {
                BoxDrawer.DrawBox(r.GetCenter(), r.GetExtents(), Quaternion.identity, Color.red);
            }
        }
        #endif
    }
}