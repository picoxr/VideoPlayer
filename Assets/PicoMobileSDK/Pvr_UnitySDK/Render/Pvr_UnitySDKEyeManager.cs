using UnityEngine;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;

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
                eyes = GetComponentsInChildren<Pvr_UnitySDKEye>(true).ToArray();
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
                    Eyes[i].EyeRender();
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
        Pvr_UnitySDKPluginEvent.IssueWithData(RenderEventType.LeftEyeEndFrame, eyeTextureID);
        Pvr_UnitySDKPluginEvent.IssueWithData(RenderEventType.RightEyeEndFrame, eyeTextureID);
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

    private int layerDepth = 0;

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
                switch (Eyes[i].eyeSide)
                {
                    case Pvr_UnitySDKAPI.Eye.LeftEye:
                        eyeTextureId = Pvr_UnitySDKManager.SDK.eyeTextureIds[Pvr_UnitySDKManager.SDK.currEyeTextureIdx];
                        eventType = RenderEventType.LeftEyeEndFrame;
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
                        break;
                    default:
                        break;
                }

                Pvr_UnitySDKPluginEvent.IssueWithData(eventType, eyeTextureId);
            }
        }
#endif
            #endregion



            // Compositor Layers: if find Overlay then Open Compositor Layers feature
            #region Compositor Layers
            if (Pvr_UnitySDKEyeOverlay.Instances.Count > 0)
            {
                layerDepth = 0;
                Pvr_UnitySDKEyeOverlay.Instances.Sort();
                for (int i = 0; i < Overlays.Length; i++)
                {
                    if (!Overlays[i].isActiveAndEnabled) continue;
                    if (Overlays[i].layerTextures[0] == null && Overlays[i].layerTextures[1] == null) continue;
                    if (Overlays[i].layerTransform != null && !Overlays[i].layerTransform.gameObject.activeSelf) continue;

                    if (Overlays[i].layerType == Pvr_UnitySDKEyeOverlay.ImageType.StandardTexture)
                    {
                        // 2D Overlay Standard Texture
                        layerDepth++;
                        Pvr_UnitySDKAPI.Render.UPvr_SetOverlayModelViewMatrix(Overlays[i].layerTextureIds[0], (int)Pvr_UnitySDKAPI.Eye.LeftEye, layerDepth, Overlays[i].MVMatrixs[0]);
                        Pvr_UnitySDKAPI.Render.UPvr_SetOverlayModelViewMatrix(Overlays[i].layerTextureIds[1], (int)Pvr_UnitySDKAPI.Eye.RightEye, layerDepth, Overlays[i].MVMatrixs[1]);
                    }
                    else if (Overlays[i].layerType == Pvr_UnitySDKEyeOverlay.ImageType.EquirectangularTexture)
                    {
                        // 360 Overlay Equirectangular Texture
                        Pvr_UnitySDKAPI.Render.UPvr_SetupLayerData(0, (int)Pvr_UnitySDKAPI.Eye.LeftEye, Overlays[i].layerTextureIds[0], (int)Overlays[i].layerType, 0);
                        Pvr_UnitySDKAPI.Render.UPvr_SetupLayerData(0, (int)Pvr_UnitySDKAPI.Eye.RightEye, Overlays[i].layerTextureIds[1], (int)Overlays[i].layerType, 0);
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
}