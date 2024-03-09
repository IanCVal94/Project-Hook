using ASK.Core;
using ASK.Helpers;
using ASK.ScreenShake;
using Cameras;
using Cinemachine;
using Helpers;
using Player;
using UnityEditor;
using UnityEngine;

namespace Spawning
{
    [RequireComponent(typeof(PlayerSpawnManager))]
    [RequireComponent(typeof(PlayerDeathManager))]
    [RequireComponent(typeof(PlayerCore))]
    public class PlayerScreenShakeActivator : ScreenShakeActivator
    {
        private PlayerDeathManager _deathManager;
        
        public ScreenShakeDataBurst DeathData;
        public ScreenShakeDataBurst PunchData;
        public ScreenShakeDataContinuous CurShake;

        private void Awake()
        {
            _deathManager = GetComponent<PlayerDeathManager>();
        }

        private void OnEnable()
        {
            // Room.RoomTransitionEvent += SwitchRooms;
            _deathManager.OnDeath += DeathScreenShake;
        }
        
        private void OnDisable()
        {
            // Room.RoomTransitionEvent -= SwitchRooms;
            _deathManager.OnDeath -= DeathScreenShake;
        }
        
        private Coroutine _shakeRoutine;
        public void ScreenShakeBurst(ScreenShakeDataBurst d)
        {
            GetCamera().SetNoise(d.NoiseProfile);
            if (_shakeRoutine != null)
            {
                StopCoroutine(_shakeRoutine);
            }
        
            _shakeRoutine = StartCoroutine(Helper.DelayAction(d.Time, () =>
            {
                GetCamera().SetNoise(null);
            }));
        }

        public void ScreenShakePunch() => ScreenShakeBurst(PunchData);
        
        public void ScreenShakeContinuousOn(ScreenShakeDataContinuous d)
        {
            GetCamera().SetNoise(d.NoiseProfile);
            CurShake = d;
        }
        
        public void ScreenShakeContinuousOff(ScreenShakeDataContinuous d)
        {
            GetCamera().SetNoise(null);
            CurShake = null;
        }

        private void DeathScreenShake() => ScreenShakeBurst(DeathData);

        public override CinemachineVirtualCamera GetCamera()
        {
            return CameraProvider.Instance.MainVCamManager.GetVCam(LayerMask.NameToLayer("Interactable"));
        }
    }
}