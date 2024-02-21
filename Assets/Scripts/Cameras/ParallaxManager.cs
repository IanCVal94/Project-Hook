using System;
using UnityEngine;
using UnityEngine.Rendering;
// using Mathf;
using UnityEngine.Experimental.Rendering;
using System.Reflection;
using System.Linq;
using Cinemachine;
using UnityEngine.Rendering.Universal;
using VFX;

public class ParallaxManager : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private Transform quad;

    // [SerializeField] private CinemachinePixelTransposer transposer;
    // [SerializeField] private Transform layer;

    [SerializeField] private bool pixelate = true;

    private CinemachineBrain _brain;

    private void Awake()
    {
        _brain = cam.GetComponent<CinemachineBrain>();
    }

    // Unity calls this method automatically when it enables this component
    private void OnEnable()
    {
        // Add WriteLogMessage as a delegate of the RenderPipelineManager.beginCameraRendering event
        RenderPipelineManager.beginCameraRendering += ComputeBGOffset;
    }

    // Unity calls this method automatically when it disables this component
    private void OnDisable()
    {
        // Remove WriteLogMessage as a delegate of the  RenderPipelineManager.beginCameraRendering event
        RenderPipelineManager.beginCameraRendering -= ComputeBGOffset;
    }

    // When this method is a delegate of RenderPipeline.beginCameraRendering event, Unity calls this method every time it raises the beginCameraRendering event
    void ComputeBGOffset(ScriptableRenderContext context, Camera camera)
    {
        if (camera == cam)
        {
            float quadOldZ = quad.localPosition.z;
            CinemachineVirtualCamera vcam = (CinemachineVirtualCamera)_brain.ActiveVirtualCamera;
            Vector3 quadPos = -vcam.GetCinemachineComponent<CinemachinePixelTransposer>().Offset;
            quadPos.z = quadOldZ;
            quad.localPosition = quadPos;
        }
    }
    
    /*private void ExtractScriptableRendererData()
    {
        var pipeline = ((UniversalRenderPipelineAsset)GraphicsSettings.renderPipelineAsset);
        FieldInfo propertyInfo = pipeline.GetType(  ).GetField( "m_RendererDataList", BindingFlags.Instance | BindingFlags.NonPublic );
        var _scriptableRendererData = ((ScriptableRendererData[]) propertyInfo?.GetValue( pipeline ))?[0];
        
        foreach ( var renderObjSetting in _scriptableRendererData.rendererFeatures.OfType<UnityEngine.Experimental.Rendering.Universal.RenderObjects>( ) )
        {
            // renderObjSetting.settings.cameraSettings.cameraFieldOfView    = _currentFPSFov;
            // renderObjSetting.settings.cameraSettings.offset                = _currentFPSOffset;

            renderObjSetting.settings.cameraSettings.offset = new Vector3(10, 10, 0);
        }
    }*/
}