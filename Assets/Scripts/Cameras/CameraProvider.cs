using Cinemachine;
using MyBox;
using UnityEngine;
using World;

namespace Cameras
{
    public class CameraProvider : Singleton<CameraProvider>
    {
        private VCamManager _mainVCamManager;

        public VCamManager MainVCamManager
        {
            get
            {
                if (_mainVCamManager == null) _mainVCamManager = GetComponentInChildren<VCamManager>();
                return _mainVCamManager;
            }
        }

        [Tooltip("Used for ScreenToWorldPoint conversions")]
        [MustBeAssigned] [SerializeField] private Camera finalCamera;

        public Vector3 ScreenToWorldPoint(Vector2 v) => finalCamera.ScreenToWorldPoint(v);
    }
}