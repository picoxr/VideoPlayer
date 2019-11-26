using UnityEngine;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;
using Pvr_UnitySDKAPI;

public class Pvr_UnitySDKEyeManager : MonoBehaviour
{
    private static Pvr_UnitySDKEyeManager instance;
    public static Pvr_UnitySDKEyeManager Instance
    {
        get
        {
            if (instance == null)
            {
                Debug.LogError("Pvr_UnitySDKEyeManager instance is not init yet...");
                UnityEngine.Object.FindObjectOfType<Pvr_UnitySDKEyeManager>();
            }
            return instance;
        }
    }

    /************************************    Properties  *************************************/
    #region Properties
    /// <summary>
    /// Eyebuffer Layers
    /// </summary>
    private Pvr_UnitySDKEye[] eyes = null;
    public Pvr_UnitySDKEye[] Eyes
    {
        get
        {
            if (eyes == null)
            {
                eyes = Pvr_UnitySDKEye.Instances.ToArray();
            }
            return eyes;
        }
    }

    /// <summary>
    /// Compositor Layers
    /// </summary>
    private Pvr_UnitySDKEyeOverlay[] overlays = null;
    public Pvr_UnitySDKEyeOverlay[] Overlays
    {
        get
        {
            if (overlays == null)
            {
                overlays = Pvr_UnitySDKEyeOverlay.Instances.ToArray();
            }
            return overlays;
        }
    }
	[HideInInspector]
    public Camera LeftEyeCamera;
	[HideInInspector]
    public Camera RightEyeCamera;
    /// <summary>
    /// Mono Camera(only enable when Monoscopic switch on)
    /// </summary>
	[HideInInspector]
    public Camera MonoEyeCamera;
    /// <summary>
    /// Mono Eye RTexture ID
    /// </summary>
    private int MonoEyeTextureID = 0;

    // wait for a number of frames, because custom splash screen(2D loading) need display time when first start-up.
    private readonly int WaitSplashScreenFrames = 3;
    public bool isFirstStartup = true;
    private int frameNum = 0;

    /// <summary>
    /// Max Compositor Layers
    /// </summary>
    private int MaxCompositorLayers = 15;

    [HideInInspector]
    public eFoveationLevel foveationLevel = eFoveationLevel.None;
    [HideInInspector]
    public Vector2 FoveationGainValue = Vector2.zero;
    [HideInInspector]
    public float FoveationAreaValue = 0.0f;
    [HideInInspector]
    public float FoveationMinimumValue = 0.0f;
    #endregion

    /************************************ Process Interface  *********************************/
    #region  Process Interface
    private void SetupMonoCamera()
    {
        transform.localPosition = Vector3.zero;
        MonoEyeCamera.aspect = 1.0f;
        MonoEyeCamera.rect = new Rect(0, 0, 1, 1);
    }

    private void SetupUpdate()
    {
        MonoEyeCamera.fieldOfView = Pvr_UnitySDKManager.SDK.EyeFov;
        MonoEyeTextureID = Pvr_UnitySDKManager.SDK.currEyeTextureIdx;
        MonoEyeCamera.enabled = true;
    }

    private void MonoEyeRender()
    {
        SetupUpdate();
        if (Pvr_UnitySDKManager.SDK.eyeTextures[MonoEyeTextureID] != null)
        {
            Pvr_UnitySDKManager.SDK.eyeTextures[MonoEyeTextureID].DiscardContents();
            MonoEyeCamera.targetTexture = Pvr_UnitySDKManager.SDK.eyeTextures[MonoEyeTextureID];
        }
    }
    #endregion

    /*************************************  Unity API ****************************************/
    #region Unity API
    private void Awake()
    {
        instance = this;
        if (this.MonoEyeCamera == null)
        {
            this.MonoEyeCamera = this.GetComponent<Camera>();
        }
        if (this.LeftEyeCamera == null)
        {
            this.LeftEyeCamera = this.gameObject.transform.Find("LeftEye").GetComponent<Camera>();
        }
        if (this.RightEyeCamera == null)
        {
            this.RightEyeCamera = this.gameObject.transform.Find("RightEye").GetComponent<Camera>();
        }

        Pvr_UnitySDKManager.eventEnterVRMode += SetEyeTrackingMode;
        SetFFRInfo();
    }

    private void OnPause()
    {
        Pvr_UnitySDKManager.eventEnterVRMode -= SetEyeTrackingMode;
    }

    void Start()
    {
#if !UNITY_EDITOR
        SetupMonoCamera();
        MonoEyeCamera.enabled = Pvr_UnitySDKManager.SDK.Monoscopic;
#endif
    }
    void OnEnable()
    {
        StartCoroutine("EndOfFrame");
    }

    void Update()
    {

        MonoEyeCamera.enabled = !Pvr_UnitySDKManager.SDK.VRModeEnabled || Pvr_UnitySDKManager.SDK.Monoscopic;

#if UNITY_EDITOR
        for (int i = 0; i < Eyes.Length; i++)
        {
            Eyes[i].eyecamera.enabled = Pvr_UnitySDKManager.SDK.VRModeEnabled;
        }
#else
        for (int i = 0; i < Eyes.Length; i++)
        {
            Eyes[i].eyecamera.enabled = !Pvr_UnitySDKManager.SDK.Monoscopic;
        }
#endif

        if (!Pvr_UnitySDKManager.SDK.IsViewerLogicFlow)
        {
            if (!Pvr_UnitySDKManager.SDK.Monoscopic)
            {
                // Open Stero Eye Render
                for (int i = 0; i < Eyes.Length; i++)
                {
                    if (Eyes[i].isActiveAndEnabled)
                    {
                        Eyes[i].EyeRender();
                    }
                }
            }
            else
            {
                // Open Mono Eye Render
                MonoEyeRender();
            }

        }
    }
    void OnDisable()
    {
        StopAllCoroutines();
    }

    private void OnPostRender()
    {
        int eyeTextureID = Pvr_UnitySDKManager.SDK.eyeTextureIds[Pvr_UnitySDKManager.SDK.currEyeTextureIdx];
        // eyebuffer
        Pvr_UnitySDKPluginEvent.IssueWithData(RenderEventType.LeftEyeEndFrame, eyeTextureID);
        Pvr_UnitySDKPluginEvent.IssueWithData(RenderEventType.RightEyeEndFrame, eyeTextureID);

        // boundary
        if (!Pvr_UnitySDKManager.SDK.HmdOnlyrot)
        {
            Pvr_UnitySDKPluginEvent.IssueWithData(RenderEventType.BoundaryRenderLeft, Pvr_UnitySDKManager.SDK.RenderviewNumber);
            Pvr_UnitySDKPluginEvent.IssueWithData(RenderEventType.BoundaryRenderRight, Pvr_UnitySDKManager.SDK.RenderviewNumber);
        }
    }

#if UNITY_EDITOR
    private void OnGUI()
    {
        if (Pvr_UnitySDKEyeOverlay.Instances.Count <= 0)
        {
            return;
        }
        Vector4 clipLowerLeft = new Vector4(-1, -1, 0, 1);
        Vector4 clipUpperRight = new Vector4(1, 1, 0, 1);

        Pvr_UnitySDKEyeOverlay.Instances.Sort();
        foreach (var eyeOverlay in Pvr_UnitySDKEyeOverlay.Instances)
        {
            if (!eyeOverlay.isActiveAndEnabled) continue;
            if (eyeOverlay.layerTextures[0] == null && eyeOverlay.layerTextures[1] == null) continue;
            if (eyeOverlay.layerTransform != null && !eyeOverlay.layerTransform.gameObject.activeSelf) continue;
            if (eyeOverlay.layerTransform != null && !eyeOverlay.layerTransform.IsChildOf(this.transform.parent)) continue;

            Rect textureRect = new Rect(0, 0, 1, 1);

            Vector2 leftCenter = new Vector2(Screen.width * 0.25f, Screen.height * 0.5f);
            Vector2 rightCenter = new Vector2(Screen.width * 0.75f, Screen.height * 0.5f);
            Vector2 eyeExtent = new Vector3(Screen.width * 0.25f, Screen.height * 0.5f);
            eyeExtent.x -= 100.0f;
            eyeExtent.y -= 100.0f;

            Rect leftScreen = Rect.MinMaxRect(
                leftCenter.x - eyeExtent.x,
                leftCenter.y - eyeExtent.y,
                leftCenter.x + eyeExtent.x,
                leftCenter.y + eyeExtent.y);
            Rect rightScreen = Rect.MinMaxRect(
                rightCenter.x - eyeExtent.x,
                rightCenter.y - eyeExtent.y,
                rightCenter.x + eyeExtent.x,
                rightCenter.y + eyeExtent.y);

            var eyeRectMin = clipLowerLeft; eyeRectMin /= eyeRectMin.w;
            var eyeRectMax = clipUpperRight; eyeRectMax /= eyeRectMax.w;


            leftScreen = Rect.MinMaxRect(
                        leftCenter.x + eyeExtent.x * eyeRectMin.x,
                        leftCenter.y + eyeExtent.y * eyeRectMin.y,
                        leftCenter.x + eyeExtent.x * eyeRectMax.x,
                        leftCenter.y + eyeExtent.y * eyeRectMax.y);

            Graphics.DrawTexture(leftScreen, eyeOverlay.layerTextures[0], textureRect, 0, 0, 0, 0);


            rightScreen = Rect.MinMaxRect(
                       rightCenter.x + eyeExtent.x * eyeRectMin.x,
                       rightCenter.y + eyeExtent.y * eyeRectMin.y,
                       rightCenter.x + eyeExtent.x * eyeRectMax.x,
                       rightCenter.y + eyeExtent.y * eyeRectMax.y);

            Graphics.DrawTexture(rightScreen, eyeOverlay.layerTextures[1], textureRect, 0, 0, 0, 0);
        }
    }
#endif
    #endregion

    /************************************  End Of Per Frame  *************************************/
    // for eyebuffer params
    private int eyeTextureId = 0;
    private RenderEventType eventType = RenderEventType.LeftEyeEndFrame;
    private RenderEventType boundaryEventType = RenderEventType.BoundaryRenderLeft;

    private int overlayLayerDepth = 1;
    private int underlayLayerDepth = 0;
    private bool isHeadLocked = false;

    IEnumerator EndOfFrame()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
#if !UNITY_EDITOR
            if (!Pvr_UnitySDKManager.SDK.isEnterVRMode)
            {
                // Call GL.clear before Enter VRMode to avoid unexpected graph breaking.
                GL.Clear(false, true, Color.black);
            }
#endif           
            if (isFirstStartup && frameNum == this.WaitSplashScreenFrames)
            {
                Pvr_UnitySDKAPI.System.UPvr_RemovePlatformLogo();
                Pvr_UnitySDKAPI.System.UPvr_StartVRModel();
                isFirstStartup = false;
            }
            else if (isFirstStartup && frameNum < this.WaitSplashScreenFrames)
            {
                Debug.Log("+++++++++++++++++++++++++++++++" + frameNum);
                frameNum++;
            }

            #region Eyebuffer
#if UNITY_2018_1_OR_NEWER
            if (UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset != null)
            {
                for (int i = 0; i < Eyes.Length; i++)
                {
                    if (!Eyes[i].isActiveAndEnabled)
                    {
                        continue;
                    }

                    switch (Eyes[i].eyeSide)
                    {
                        case Pvr_UnitySDKAPI.Eye.LeftEye:
                            eyeTextureId = Pvr_UnitySDKManager.SDK.eyeTextureIds[Pvr_UnitySDKManager.SDK.currEyeTextureIdx];
                            eventType = RenderEventType.LeftEyeEndFrame;
                            boundaryEventType = RenderEventType.BoundaryRenderLeft;
                            break;
                        case Pvr_UnitySDKAPI.Eye.RightEye:
                            if (!Pvr_UnitySDKManager.SDK.Monoscopic)
                            {
                                eyeTextureId = Pvr_UnitySDKManager.SDK.eyeTextureIds[Pvr_UnitySDKManager.SDK.currEyeTextureIdx + 3];
                            }
                            else
                            {
                                eyeTextureId = Pvr_UnitySDKManager.SDK.eyeTextureIds[Pvr_UnitySDKManager.SDK.currEyeTextureIdx];
                            }
                            eventType = RenderEventType.RightEyeEndFrame;
                            boundaryEventType = RenderEventType.BoundaryRenderRight;
                            break;
                        default:
                            break;
                    }

                    // eyebuffer
                    Pvr_UnitySDKPluginEvent.IssueWithData(eventType, eyeTextureId);
                    // boundary
                    if (!Pvr_UnitySDKManager.SDK.HmdOnlyrot)
                    {
                        Pvr_UnitySDKPluginEvent.IssueWithData(boundaryEventType, Pvr_UnitySDKManager.SDK.RenderviewNumber);
                    }
                    Pvr_UnitySDKPluginEvent.Issue(RenderEventType.EndEye);
                }
            }
#endif
            #endregion


            // Compositor Layers: if find Overlay then Open Compositor Layers feature
            #region Compositor Layers
            if (Pvr_UnitySDKEyeOverlay.Instances.Count > 0)
            {
                overlayLayerDepth = 1;
                underlayLayerDepth = 0;

                Pvr_UnitySDKEyeOverlay.Instances.Sort();
                for (int i = 0; i < Overlays.Length; i++)
                {
                    if (!Overlays[i].isActiveAndEnabled) continue;
                    if (Overlays[i].layerTextures[0] == null && Overlays[i].layerTextures[1] == null) continue;
                    if (Overlays[i].layerTransform != null && !Overlays[i].layerTransform.gameObject.activeSelf) continue;

                    if (Overlays[i].imageType == Pvr_UnitySDKEyeOverlay.ImageType.StandardTexture)
                    {
                        if (Overlays[i].overlayType == Pvr_UnitySDKEyeOverlay.OverlayType.Overlay)
                        {
                            isHeadLocked = false;
                            if (Overlays[i].layerTransform != null && Overlays[i].layerTransform.parent == this.transform)
                            {
                                isHeadLocked = true;
                            }

                            Pvr_UnitySDKAPI.Render.UPvr_SetOverlayModelViewMatrix((int)Overlays[i].overlayType, Overlays[i].layerTextureIds[0], (int)Pvr_UnitySDKAPI.Eye.LeftEye, overlayLayerDepth, isHeadLocked, Overlays[i].MVMatrixs[0]);
                            Pvr_UnitySDKAPI.Render.UPvr_SetOverlayModelViewMatrix((int)Overlays[i].overlayType, Overlays[i].layerTextureIds[1], (int)Pvr_UnitySDKAPI.Eye.RightEye, overlayLayerDepth, isHeadLocked, Overlays[i].MVMatrixs[1]);

                            overlayLayerDepth++;
                        }
                        else if (Overlays[i].overlayType == Pvr_UnitySDKEyeOverlay.OverlayType.Underlay)
                        {
                            Pvr_UnitySDKAPI.Render.UPvr_SetOverlayModelViewMatrix((int)Overlays[i].overlayType, Overlays[i].layerTextureIds[0], (int)Pvr_UnitySDKAPI.Eye.LeftEye, underlayLayerDepth, false, Overlays[i].MVMatrixs[0]);
                            Pvr_UnitySDKAPI.Render.UPvr_SetOverlayModelViewMatrix((int)Overlays[i].overlayType, Overlays[i].layerTextureIds[1], (int)Pvr_UnitySDKAPI.Eye.RightEye, underlayLayerDepth, false, Overlays[i].MVMatrixs[1]);

                            underlayLayerDepth++;
                        }
                    }
                    else if (Overlays[i].imageType == Pvr_UnitySDKEyeOverlay.ImageType.EquirectangularTexture)
                    {
                        // 360 Overlay Equirectangular Texture
                        Pvr_UnitySDKAPI.Render.UPvr_SetupLayerData(0, (int)Pvr_UnitySDKAPI.Eye.LeftEye, Overlays[i].layerTextureIds[0], (int)Overlays[i].imageType, 0);
                        Pvr_UnitySDKAPI.Render.UPvr_SetupLayerData(0, (int)Pvr_UnitySDKAPI.Eye.RightEye, Overlays[i].layerTextureIds[1], (int)Overlays[i].imageType, 0);
                    }
                }
                #endregion
            }


            // Begin TimeWarp
            Pvr_UnitySDKPluginEvent.IssueWithData(RenderEventType.TimeWarp, Pvr_UnitySDKManager.SDK.RenderviewNumber);
            Pvr_UnitySDKManager.SDK.currEyeTextureIdx = Pvr_UnitySDKManager.SDK.nextEyeTextureIdx;
            Pvr_UnitySDKManager.SDK.nextEyeTextureIdx = (Pvr_UnitySDKManager.SDK.nextEyeTextureIdx + 1) % 3;
        }
    }

    #region EyeTrack  
    [HideInInspector]
    public bool trackEyes = false;
    [HideInInspector]
    public Vector3 eyePoint;
    private EyeTrackingData eyePoseData;

    public bool SetEyeTrackingMode()
    {
        bool result = false;
        if (trackEyes)
        {
            result = Pvr_UnitySDKAPI.System.UPvr_setTrackingMode((int)Pvr_UnitySDKAPI.TrackingMode.PVR_TRACKING_MODE_POSITION | (int)Pvr_UnitySDKAPI.TrackingMode.PVR_TRACKING_MODE_EYE);
        }
        Debug.Log("SetTrackingMode trackEyes " + trackEyes + " result " + result);
        return result;
    }

    public Vector3 GetEyeTrackingPos()
    {
        if (!Pvr_UnitySDKEyeManager.Instance.trackEyes)
            return Vector3.zero;

        EyeDeviceInfo info = GetDeviceInfo();

        Vector3 frustumSize = Vector3.zero;
        frustumSize.x = 0.5f * (info.targetFrustumLeft.right - info.targetFrustumLeft.left);
        frustumSize.y = 0.5f * (info.targetFrustumLeft.top - info.targetFrustumLeft.bottom);
        frustumSize.z = info.targetFrustumLeft.near;

        bool result = Pvr_UnitySDKAPI.System.UPvr_getEyeTrackingData(ref eyePoseData);
        if (!result)
        {
            Debug.LogError("UPvr_getEyeTrackingData failed " + result);
            return Vector3.zero;
        }

        //Debug.LogFormat("eyePoseData.combinedEyePoseStatus: {0}", eyePoseData.combinedEyePoseStatus.ToString());
        //Debug.LogFormat("eyePoseData.leftEyeGazePoint: {0}, Vector: {1}", eyePoseData.leftEyeGazePoint.ToString("f6"), eyePoseData.leftEyeGazeVector.ToString("f6"));
        //Debug.LogFormat("eyePoseData.rightEyeGazePoint: {0}, Vector: {1}", eyePoseData.rightEyeGazePoint.ToString("f6"), eyePoseData.rightEyeGazeVector.ToString("f6"));
        //Debug.LogFormat("eyePoseData.combinedEyeGazePoint: {0}, Vector: {1}", eyePoseData.combinedEyeGazePoint.ToString("f6"), eyePoseData.combinedEyeGazeVector.ToString("f6"));
        var combinedDirection = Vector3.zero;
        if ((eyePoseData.combinedEyePoseStatus & (int)pvrEyePoseStatus.kGazeVectorValid) != 0)
            combinedDirection = eyePoseData.combinedEyeGazeVector;

        if (combinedDirection.sqrMagnitude > 0f)
        {
            combinedDirection.Normalize();
            //Debug.LogFormat("Eye Direction: ({0}, {1}, {2})", combinedDirection.x.ToString("f6"), combinedDirection.y.ToString("f6"), combinedDirection.z.ToString("f6"));
            float denominator = Vector3.Dot(combinedDirection, Vector3.forward);
            if (denominator > float.Epsilon)
            {
                eyePoint = combinedDirection * frustumSize.z / denominator;
                eyePoint.x /= frustumSize.x; // [-1..1]
                eyePoint.y /= frustumSize.y; // [-1..1]
                //Debug.LogFormat("Eye Point: ({0}, {1})", eyePoint.x.ToString("f6"), eyePoint.y.ToString("f6"));
            }
        }
        return eyePoint;

    }

    private EyeDeviceInfo GetDeviceInfo()
    {
        EyeDeviceInfo info;
        info.targetFrustumLeft.left = -0.0428f;
        info.targetFrustumLeft.right = 0.0428f;
        info.targetFrustumLeft.top = 0.0428f;
        info.targetFrustumLeft.bottom = -0.0428f;
        info.targetFrustumLeft.near = 0.0508f;
        info.targetFrustumLeft.far = 100f;
        info.targetFrustumRight.left = -0.0428f;
        info.targetFrustumRight.right = 0.0428f;
        info.targetFrustumRight.top = 0.0428f;
        info.targetFrustumRight.bottom = -0.0428f;
        info.targetFrustumRight.near = 0.0508f;
        info.targetFrustumRight.far = 100f;

        return info;
    }

    void SetFFRInfo()
    {
        Vector2 tempFoveationGainValue = Vector2.zero;
        float tempFoveationAreaValue = 0.0f;
        float tempFoveationMinimumValue = 0.0f;
        switch (foveationLevel)
        {
            case eFoveationLevel.None:
                tempFoveationGainValue = Vector2.zero;
                tempFoveationAreaValue = 0.0f;
                tempFoveationMinimumValue = 0.0f;
                break;
            case eFoveationLevel.Low:
                tempFoveationGainValue = new Vector2(2.0f, 2.0f);
                tempFoveationAreaValue = 0.0f;
                tempFoveationMinimumValue = 0.125f;
                break;
            case eFoveationLevel.Med:
                tempFoveationGainValue = new Vector2(3.0f, 3.0f);
                tempFoveationAreaValue = 1.0f;
                tempFoveationMinimumValue = 0.125f;
                break;
            case eFoveationLevel.High:
                tempFoveationGainValue = new Vector2(4.0f, 4.0f);
                tempFoveationAreaValue = 2.0f;
                tempFoveationMinimumValue = 0.125f;
                break;
        }
        if (FoveationGainValue == Vector2.zero)
        {
            FoveationGainValue = tempFoveationGainValue;
        }
        if (FoveationAreaValue <= 0.0f)
        {
            FoveationAreaValue = tempFoveationAreaValue;
        }
        if (FoveationMinimumValue <= 0.0f)
        {
            FoveationMinimumValue = tempFoveationMinimumValue;
        }
    }

    #endregion
}