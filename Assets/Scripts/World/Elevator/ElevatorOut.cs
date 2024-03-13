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
        
        private void OnTriggerEnter2D(Collider2D other) 
        {
            // Add logic here to check if the player has eliminated all entities!!!!!!!!!!!!!!
            if (_unlocked && other.GetComponent<OnElevatorEnter>() is { } e)
            {
                _animator.Play("Close");
                e.OnEnter(this);
                _sr.sortingLayerName = "VFX";
                walls.SetActive(true);
            }
        }

        public void SetDestination(Room nextRoom)
        {
            Destination = nextRoom.ElevatorIn;
        }
    }
}