using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraMetadata : MonoBehaviour
{
    public HookCameraType CameraType => camType;
    [SerializeField] private HookCameraType camType;
}

[System.Flags]
public enum HookCameraType
{
    Final = 0,
    BgBack = 1 << 1,
    BgMid = 1 << 2,
    BgFore = 1 << 3,
    Main = 1 << 4,
    Mini = 1 << 5
}