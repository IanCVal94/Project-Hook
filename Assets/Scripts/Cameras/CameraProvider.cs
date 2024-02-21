using Cinemachine;
using MyBox;
using UnityEngine;
using World;

namespace Cameras
{
    [RequireComponent(typeof(VCamManager))]
    public class CameraProvider : Singleton<CameraProvider>
    {
        private VCamManager _mainVCamManager;

        public VCamManager MainVCamManager
        {
            get
            {
                if (_mainVCamManager == null) _mainVCamManager = GetComponent<VCamManager>();
                return _mainVCamManager;
            }
        }
    }
}