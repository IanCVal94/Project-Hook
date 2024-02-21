using System;
using System.Collections.Generic;
using Cameras;
using Cinemachine;
using UnityEngine;

namespace World
{
    public class VCamManager : MonoBehaviour
    {
        private CinemachineVirtualCamera[] _vCams;
        public CinemachineVirtualCamera[] VCams {
            get
            {
                if (_vCams == null) _vCams = GetComponentsInChildren<CinemachineVirtualCamera>();
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
            foreach (var v in VCams) v.Follow = p;
        }

        public void SetNoise(NoiseSettings dNoiseProfile)
        {
            foreach (var v in VCams)
                v.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_NoiseProfile = dNoiseProfile;
        }

        public void SetConfiner(Collider2D boundingShape)
        {
            foreach (var v in VCams)
                v.GetComponent<CinemachineConfiner2D>().m_BoundingShape2D = boundingShape;
        }
    }
}