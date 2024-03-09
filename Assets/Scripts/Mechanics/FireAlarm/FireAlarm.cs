using UnityEngine.Events;
using UnityEngine;
using Combat;

namespace Mechanics
{
    public class FireAlarm : MonoBehaviour, IPunchable
    {
        [SerializeField] private GameObject waterContainer;
        private bool _usedOnce = false;
        private GameObject _waterContainerDoor;

        void Start()
        {
            Transform childTransform = waterContainer.transform.Find("ContainerDoor");
            if (childTransform != null)
            {
                _waterContainerDoor = childTransform.gameObject;
            }
        }
        public bool ReceivePunch(Vector2 v)
        {
            if (!_usedOnce) {
                Debug.Log("Alarm Used");
                _waterContainerDoor.SetActive(false);
                _usedOnce = true;
            }
            return false;
        }
    }
}
