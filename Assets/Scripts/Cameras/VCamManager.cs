using System;
using System.Collections.Generic;
using Cameras;
using Cinemachine;
using UnityEditor;
using UnityEngine;
using VFX;

namespace World
{
    public class VCamManager : MonoBehaviour
    {
        private Dictionary<int, CinemachineVirtualCamera> _vCams = new();
        protected Dictionary<int, CinemachineVirtualCamera> vcams {
            get
            {
                if (_vCams.Count == 0)
                {
                    var vcamArr = GetComponentsInChildren<CinemachineVirtualCamera>();
                    foreach (var vcam in vcamArr)
                    {
                        _vCams.Add(vcam.gameObject.layer, vcam);
                    }
                }
                return _vCams;
            }
        }

        private void Awake()
        {
            DefaultFollow f = FindObjectOfType<DefaultFollow>();
            SetFollow(f.transform);
        }

        public void SetFollow(Transform p)
        {
            foreach (var v in vcams) v.Value.Follow = p;
        }

        public void SetNoise(NoiseSettings dNoiseProfile)
        {
            foreach (var v in vcams)
                v.Value.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_NoiseProfile = dNoiseProfile;
        }

        public void SetConfiner(Collider2D boundingShape)
        {
            foreach (var v in vcams)
                v.Value.GetComponent<CinemachineConfiner2D>().m_BoundingShape2D = boundingShape;
        }

        public CinemachineVirtualCamera GetVCam(int layer) => vcams[layer];
        // public Vector2 GetVCamOffset(int layer) => GetVCam(layer).GetCinemachineComponent<CinemachinePixelTransposer>().Offset;
    }
}