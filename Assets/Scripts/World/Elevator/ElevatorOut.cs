using UnityEngine;

namespace World
{
    public class ElevatorOut : Elevator
    {
        [SerializeField] private GameObject walls;
        private Animator _animator;
        private SpriteRenderer _sr;
        public Elevator Destination;

        private bool _unlocked = false;

        [SerializeField] private Sprite openSprite;
        
        public void Unlock()
        {
            if (CheckPlayerInside() is OnElevatorEnter e)
            {
                Teleport(e);
            }
            _unlocked = true;
            _sr.sprite = openSprite;
        }
        
        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _sr = GetComponent<SpriteRenderer>();
            _sr.sortingLayerName = "VFX";
        }

        void Start()
        {
            walls.SetActive(false);
        }

        private OnElevatorEnter CheckPlayerInside()
        {
            Collider2D[] colliders = Physics2D.OverlapBoxAll(transform.position, transform.localScale, 0);
            foreach (Collider2D collider in colliders)
            {
                if (collider.GetComponent<OnElevatorEnter>() is { } e) return e;
            }

            return null;
        }

        private void Teleport(OnElevatorEnter e)
        {
            _animator.Play("Close");
            e.OnEnter(this);
            _sr.sortingLayerName = "VFX";
            walls.SetActive(true);
        }
        
        private void OnTriggerEnter2D(Collider2D other) 
        {
            if (_unlocked && other.GetComponent<OnElevatorEnter>() is { } e) Teleport(e);
        }

        public void SetDestination(Room nextRoom)
        {
            Destination = nextRoom.ElevatorIn;
        }
    }
}