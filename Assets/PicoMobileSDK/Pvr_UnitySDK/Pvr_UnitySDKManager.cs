using System;
using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Pvr_UnitySDKAPI;
using UnityEngine.UI;

public class Pvr_UnitySDKManager : MonoBehaviour
{

    /************************************    Properties  *************************************/
    #region Properties
    public static PlatForm platform;
    bool BattEnable = false;

    private static Pvr_UnitySDKManager sdk = null;
    public static Pvr_UnitySDKManager SDK
    {
        get
        {
            if (sdk == null)
            {
                sdk = UnityEngine.Object.FindObjectOfType<Pvr_UnitySDKManager>();
            }
            if (sdk == null)
            {
                var go = new GameObject("Pvr_UnitySDKManager");
                sdk = go.AddComponent<Pvr_UnitySDKManager>();
                go.transform.localPosition = Vector3.zero;
            }
            return sdk;
        }
    }

    // Sensor
    [HideInInspector]
    public static Pvr_UnitySDKSensor pvr_UnitySDKSensor;
    [HideInInspector]
    public Pvr_UnitySDKPose HeadPose;
    [HideInInspector]
    public bool reStartHead = false;
    //render
    [HideInInspector]
    public static Pvr_UnitySDKRender pvr_UnitySDKRender;
    [SerializeField]
    private float eyeFov = 90.0f;
    [HideInInspector]
    public float EyeFov
    {
        get
        {
            return eyeFov;
        }
        set
        {
            if (value != eyeFov)
            {
                eyeFov = value;
            }
        }
    }
    [HideInInspector]
    public const int eyeTextureCount = 6;
    [HideInInspector]
    public RenderTexture[] eyeTextures;// = new RenderTexture[eyeTextureCount];
    [HideInInspector]
    public int[] eyeTextureIds = new int[eyeTextureCount] { 0, 0, 0, 0, 0, 0 };
    [HideInInspector]
    public int currEyeTextureIdx = 0;
    [HideInInspector]
    public int nextEyeTextureIdx = 1;
    [HideInInspector]
    public RenderTexture[] overlayTextures;// = new RenderTexture[eyeTextureCount];
    [HideInInspector]
    public int[] overlayTextureIds = new int[eyeTextureCount] { 0, 0, 0, 0, 0, 0 };
    [HideInInspector]
    public int overlayCamNum = 0;

    [HideInInspector]
    public int resetRot = 0;
    [HideInInspector]
    public int resetPos = 0;
    [HideInInspector]
    public int posStatus = 0;
    [HideInInspector]
    public bool isPUI;
    [HideInInspector]
    public Vector3 resetBasePos = new Vector3();
    [HideInInspector]
    public Vector3 resetCol0Pos = new Vector3();
    [HideInInspector]
    public Vector3 resetCol1Pos = new Vector3();
    [HideInInspector]
    public int trackingmode = -1;
    [HideInInspector]
    public int systemprop = -1;
    [HideInInspector]
    public bool systemFPS = false;

    [HideInInspector]
    public float[] headData = new float[7] { 0, 0, 0, 0, 0, 0, 0 };
    
    [SerializeField]
    private bool rotfoldout = false;

    public bool Rotfoldout
    {
        get { return rotfoldout; }
        set
        {
            if (value != rotfoldout)
                rotfoldout = value;
        }
    }

    [SerializeField]
    private bool hmdOnlyrot =false;

    public bool HmdOnlyrot
    {
        get { return hmdOnlyrot; }
        set
        {
            if (value != hmdOnlyrot)
                hmdOnlyrot = value;
        }
    }
    [SerializeField]
    private bool controllerOnlyrot = false;

    public bool ControllerOnlyrot
    {
        get { return controllerOnlyrot; }
        set
        {
            if (value != controllerOnlyrot)
                controllerOnlyrot = value;
        }
    }

    public TrackingOrigin TrackingOrigin = TrackingOrigin.EyeLevel;

    [SerializeField]
    private RenderTextureAntiAliasing rtAntiAlising = RenderTextureAntiAliasing.X_2;
    public RenderTextureAntiAliasing RtAntiAlising
    {
        get
        {
            return rtAntiAlising;
        }
        set
        {
            if (value != rtAntiAlising)
            {
                rtAntiAlising = value;

            }
        }
    }
    [SerializeField]
    private RenderTextureDepth rtBitDepth = RenderTextureDepth.BD_24;
    public RenderTextureDepth RtBitDepth
    {
        get
        {
            return rtBitDepth;
        }
        set
        {
            if (value != rtBitDepth)
                rtBitDepth = value;

        }
    }
    [SerializeField]
    private RenderTextureFormat rtFormat = RenderTextureFormat.Default;
    public RenderTextureFormat RtFormat
    {
        get
        {
            return rtFormat;
        }
        set
        {
            if (value != rtFormat)
                rtFormat = value;

        }
    }

    [SerializeField]
    private Vector2 rtSize = new Vector2(1024, 1024);
    public Vector2 RtSize
    {
        get
        {
            return rtSize;
        }
        set
        {
            if (value != rtSize)
            {
                rtSize = value;

                if (pvr_UnitySDKRender != null)
                {
                    pvr_UnitySDKRender.ReCreateEyeBuffer();
                }
            }
        }
    }

    [SerializeField]
    private float rtScaleFactor = 1;
    public float RtScaleFactor
    {
        get
        {
            return rtScaleFactor;
        }
        set
        {
            if (value != rtScaleFactor)
            {
                rtScaleFactor = value;

                if (pvr_UnitySDKRender != null)
                {
                    pvr_UnitySDKRender.ReCreateEyeBuffer();
                }
            }
        }
    }

    [SerializeField]
    private bool defaultRenderTexture;
    public bool DefaultRenderTexture
    {
        get
        {
            return defaultRenderTexture;
        }
        set
        {
            if (value != defaultRenderTexture)
            {
                defaultRenderTexture = value;
            }
        }
    }

    [HideInInspector]
    public int RenderviewNumber = 0;
    public Vector3 EyeOffset(Eye eye)
    {
        return eye == Eye.LeftEye ? leftEyeOffset : rightEyeOffset;
    }
    [HideInInspector]
    public Vector3 leftEyeOffset;
    [HideInInspector]
    public Vector3 rightEyeOffset;
    public Rect EyeRect(Eye eye)
    {
        return eye == Eye.LeftEye ? leftEyeRect : rightEyeRect;
    }
    [HideInInspector]
    public Rect leftEyeRect;
    [HideInInspector]
    public Rect rightEyeRect;
    [HideInInspector]
    public Matrix4x4 leftEyeView;
    [HideInInspector]
    public Matrix4x4 rightEyeView;

    // unity editor
    [HideInInspector]
    public Pvr_UnitySDKEditor pvr_UnitySDKEditor;
    [SerializeField]
    private bool vrModeEnabled = true;
    [HideInInspector]
    public bool VRModeEnabled
    {

        get
        {
            return vrModeEnabled;
        }
        set
        {
            if (value != vrModeEnabled)
                vrModeEnabled = value;

        }
    }
    [HideInInspector]
    public Material Eyematerial;
    [HideInInspector]
    public Material Middlematerial;
    [HideInInspector]
    public bool picovrTriggered { get; set; }
    [HideInInspector]
    public bool newPicovrTriggered = false;

    // FPS
    [SerializeField]
    private bool showFPS;
    public bool ShowFPS
    {
        get
        {
            return showFPS;
        }
        set
        {
            if (value != showFPS)
            {
                showFPS = value;
            }
        }
    }
    //6dof recenter
    [SerializeField]
    private bool sixDofPosReset;
    public bool SixDofPosReset
    {
        get
        {
            return sixDofPosReset;
        }
        set
        {
            if (value != sixDofPosReset)
            {
                sixDofPosReset = value;
            }
        }
    }

    //show safe panel
    [SerializeField]
    private bool showSafePanel;
    public bool ShowSafePanel
    {
        get
        {
            return showSafePanel;
        }
        set
        {
            if (value != showSafePanel)
            {
                showSafePanel = value;
            }
        }
    }
    //use default fps 
    [SerializeField]
    private bool defaultFPS;
    public bool DefaultFPS
    {
        get
        {
            return defaultFPS;
        }
        set
        {
            if (value != defaultFPS)
            {
                defaultFPS = value;
            }
        }
    }
    //custom fps
    [SerializeField]
    private int customFPS = 60;
    public int CustomFPS
    {
        get
        {
            return customFPS;
        }
        set
        {
            if (value != customFPS)
            {
                customFPS = value;
            }
        }
    }
    //use default range 0.8m
    [SerializeField]
    private bool defaultRange;
    public bool DefaultRange
    {
        get
        {
            return defaultRange;
        }
        set
        {
            if (value != defaultRange)
            {
                defaultRange = value;
            }
        }
    }
    //custom range
    [SerializeField]
    private float customRange = 0.8f;
    public float CustomRange
    {
        get
        {
            return customRange;
        }
        set
        {
            if (value != customRange)
            {
                customRange = value;
            }
        }
    }
    //Moving Ratios
    [SerializeField]
    private float movingRatios;
    public float MovingRatios
    {
        get
        {
            return movingRatios;
        }
        set
        {
            if (value != movingRatios)
            {
                movingRatios = value;
            }
        }
    }
    // screenFade
    [SerializeField]
    private bool screenFade = false;
    public bool ScreenFade
    {
        get
        {
            return screenFade;
        }
        set
        {
            if (value != screenFade)
            {
                screenFade = value;
            }
        }
    }
    //Neck model
    [HideInInspector]
    public Vector3 neckOffset = new Vector3(0, 0.075f, 0.0805f);

    [HideInInspector]
    public bool PVRNeck = true;
    [HideInInspector]
    public bool UseCustomNeckPara = false;

    // life
    [HideInInspector]
    public bool onResume = false;
    [HideInInspector]
    public bool isEnterVRMode = false;

    private GameObject safeArea;
    [HideInInspector]
    public GameObject safeToast;
    [HideInInspector]
    public GameObject resetPanel;
    private GameObject safePanel1;
    public bool isHasController = false;
    public GameObject ViewerToast;
    public Pvr_UnitySDKConfigProfile pvr_UnitySDKConfig;

    private GameObject calltoast;
    private GameObject msgtoast;
    private GameObject lowhmdBatterytoast;
    private GameObject lowphoneBatterytoast;
    private GameObject LowPhoneHealthtoast;
    private GameObject LowcontrollerBatterytoast;
    private bool lowControllerpowerstate = false;
    private float controllerpowershowtime = 0f;
    private bool UseToast = true;
    private int iPhoneHMDModeEnabled;
    [SerializeField]
    private bool isViewerLogicFlow = true;
    public bool IsViewerLogicFlow
    {
        get
        {
            return isViewerLogicFlow;
        }
        set
        {
            if (value != isViewerLogicFlow)
            {
                isViewerLogicFlow = value;
            }
        }
    }
    [SerializeField]
    private bool monoscopic = false;

    [HideInInspector]
    public bool Monoscopic
    {
        get { return monoscopic; }
        set
        {
            if (value != monoscopic)
            {
                monoscopic = value;
            }
        }
    }

    [SerializeField]
    private bool copyrightprotection = false;

    [HideInInspector]
    public bool Copyrightprotection
    {
        get { return copyrightprotection; }
        set
        {
            if (value != copyrightprotection)
            {
                copyrightprotection = value;
            }
        }
    }

    private bool mIsAndroid7 = false;
    public static Func<bool> eventEnterVRMode;
    #endregion

    /************************************ Public Interfaces  *********************************/
    #region Public Interfaces  
    public bool setBatteryLow(string s)
    {
        if (isViewerLogicFlow)
        {
            Debug.Log("BatteryLow 1: " + s.ToString());
            if (Convert.ToInt16(s) == 15 || Convert.ToInt16(s) == 10)
            {
                string showtext = "电量不足 ，请及时给设备充电";
                if (Application.systemLanguage == SystemLanguage.Chinese || Application.systemLanguage == SystemLanguage.ChineseSimplified)
                {
                    showtext = "电量不足" + s + "%，请及时给设备充电";
                }
                if (Application.systemLanguage == SystemLanguage.English)
                {
                    showtext = "Power is less than " + s + "%, please charge your device";
                }
                if (Application.systemLanguage == SystemLanguage.Japanese)
                {
                    showtext = "バッテリー残量が" + s + "% 以下になりました。充電してください";
                }
                ViewerToast.SetActive(true);
                ViewerToast.transform.Find("Panel").GetComponentInChildren<Text>().text = showtext;

                Invoke("disableViewerToast", 2.0f);
                Debug.Log("BatteryLow 2: " + s.ToString());
            }
            return true;
        }
        else
            return false;

    }

    public void disableViewerToast()
    {
        ViewerToast.SetActive(false);
    }


    #endregion

    /************************************ Private Interfaces  *********************************/
    #region Private Interfaces
    private AndroidJavaClass javaSysActivityClass;
    private UnityEngine.AndroidJavaClass batteryjavaVrActivityClass;
    private bool InitViewerBatteryVolClass()
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        try
        {  
            if (pvr_UnitySDKRender.activity != null)
                {  
                    batteryjavaVrActivityClass = new UnityEngine.AndroidJavaClass("com.psmart.aosoperation.BatteryReceiver");
                    return true;
                }
                else
                    return false;
            }
            catch (Exception e)
            {
                Debug.LogError("startReceiver Error :" + e.ToString());
                return false;
            }
#endif
        return true;
    }
    private bool StartViewerBatteryReceiver(string startreceivre)
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        try
        {
            // string startreceivre = PicoVRManager.SDK.gameObject.name;  
            Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(batteryjavaVrActivityClass, "Pvr_StartReceiver", Pvr_UnitySDKManager.pvr_UnitySDKRender.activity, startreceivre);
            BattEnable = true;
            return BattEnable;
        }
        catch (Exception e)
        {
            Debug.LogError("startReceiver Error :" + e.ToString());
            BattEnable = false;
            return BattEnable;
        }
#endif

        return BattEnable;
    }

    private bool StopViewerBatteryReceiver()
    {
#if  !UNITY_EDITOR   && UNITY_ANDROID
            try
            {
              Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(batteryjavaVrActivityClass, "Pvr_StopReceiver", Pvr_UnitySDKManager.pvr_UnitySDKRender.activity);
              BattEnable = false;
              return true;
            }
            catch (Exception e)
            {
                Debug.LogError("startReceiver Error :" + e.ToString());
                return false;
            }
#endif
        return true;
    }

    private bool SDKManagerInit()
    {
        if (SDKManagerInitConfigProfile())
        {
            mIsAndroid7 = SystemInfo.operatingSystem.Contains("Android OS 7.");
            Debug.Log("Android 7 = " + mIsAndroid7);
#if UNITY_EDITOR
            if (SDKManagerInitEditor())
                return true;
            else
                return false;
#else

            if (SDKManagerInitCoreAbility())

                return true;
            else
                return false;
#endif
        }
        else
            return false;
    }

    private bool SDKManagerInitCoreAbility()
    {
        if (pvr_UnitySDKRender == null)
        {
            Debug.Log("pvr_UnitySDKRender  init");
            pvr_UnitySDKRender = new Pvr_UnitySDKRender();
        }
        else
        {
            pvr_UnitySDKRender.Init();
        }

        if (pvr_UnitySDKSensor == null)
        {
            Debug.Log("pvr_UnitySDKSensor init");
            HeadPose = new Pvr_UnitySDKPose(Vector3.zero, Quaternion.identity);
            pvr_UnitySDKSensor = new Pvr_UnitySDKSensor();
        }
        Pvr_UnitySDKAPI.System.UPvr_StartHomeKeyReceiver(this.gameObject.name);

        return true;
    }

    public void smsReceivedCallback(string msg)
    {
        PLOG.I("PvrLog MSG" + msg);

        var Jdmsg = LitJson.JsonMapper.ToObject(msg);

        string name = "";
        if (msg.Contains("messageSender"))
        {
            name = (string)Jdmsg["messageSender"];
        }

        string number = "";
        if (msg.Contains("messageAdr"))
        {
            number = (string)Jdmsg["messageAdr"];
            if (number.Substring(0, 3) == "+82")
            {
                number = "0" + number.Remove(0, 3);
                number = TransformNumber(number);
            }
            else
            {
                if (number.Substring(0, 1) != "+")
                {
                    number = TransformNumber(number);
                }
            }
        }
        string body = "";
        if (msg.Contains("messageBody"))
        {
            body = (string)Jdmsg["messageBody"];
        }
        //DateTime dt = DateTime.Parse("1970-01-01 00:00:00").AddMilliseconds(Convert.ToInt64((Int64)Jdmsg["messageTime"]));
        //string time = dt.ToString("yyyy-MM-dd HH:mm:ss");
        if (UseToast)
        {
            msgtoast.transform.Find("number").GetComponent<Text>().text = number;
            msgtoast.transform.Find("name").GetComponent<Text>().text = name;
            if (name.Length == 0)
            {
                msgtoast.transform.Find("number").transform.localPosition = new Vector3(0, 0, 0);
            }
            else
            {
                msgtoast.transform.Find("number").transform.localPosition = new Vector3(60, 0, 0);
            }

            StartCoroutine(ToastManager(2, true, 0f));
            StartCoroutine(ToastManager(2, false, 5.0f));
        }
    }

    public void phoneStateCallback(string state)
    {
        PLOG.I("PvrLog phone" + state);

        var Jdstate = LitJson.JsonMapper.ToObject(state);

        string number = "";
        if (state.Contains("phoneNumber"))
        {
            number = (string)Jdstate["phoneNumber"];
            
            if (number.Substring(0, 3) == "+82")
            {
                number = "0" + number.Remove(0, 3);
                number =TransformNumber(number);
            }
            else
            {
                if (number.Substring(0, 1) != "+")
                {
                    number = TransformNumber(number);
                }
            }
        }
        string name = "";
        if (state.Contains("contactName"))
        {
           name = (string)Jdstate["contactName"];
        }
        
        if (UseToast)
        {
            calltoast.transform.Find("number").GetComponent<Text>().text = number;
            calltoast.transform.Find("name").GetComponent<Text>().text = name;
            if (name.Length == 0)
            {
                calltoast.transform.Find("number").transform.localPosition = new Vector3(0, 0, 0);
            }
            else
            {
                calltoast.transform.Find("number").transform.localPosition = new Vector3(60, 0, 0);
            }
            
            StartCoroutine(ToastManager(1, true, 0f));
            StartCoroutine(ToastManager(1, false, 5.0f));
        }
    }

    public void phoneBatteryStateCallback(string state)
    {
        PLOG.I("PvrLog phoneBatteryState" + state);

        var Jdstate = LitJson.JsonMapper.ToObject(state);

        string level = "";
        if (state.Contains("phoneBatteryLevel"))
        {
            level = (string)Jdstate["phoneBatteryLevel"];
        }
        string health = "";
        if (state.Contains("phoneBatteryHealth"))
        {
            health = (string)Jdstate["phoneBatteryHealth"];
        }
        
        if (UseToast)
        {
            if (Convert.ToInt16(level) <= 5)
            {
                if (lowhmdBatterytoast.activeSelf == false)
                {
                    StartCoroutine(ToastManager(4, true, 0f));
                    StartCoroutine(ToastManager(4, false, 3.0f));
                }
                else
                {
                    StartCoroutine(ToastManager(4, true, 5.0f));
                    StartCoroutine(ToastManager(4, false, 8.0f));
                }
                
            }
            if (Convert.ToInt16(health) == 3)
            {
                StartCoroutine(ToastManager(5, true, 0f));
                StartCoroutine(ToastManager(5, false, 5.0f));
            }
        }
    }
    public void hmdLowBatteryCallback(string level)
    {
        PLOG.I("PvrLog hmdLowBatteryCallback" + level);

        if (UseToast)
        {
            if (lowphoneBatterytoast.activeSelf == false)
            {
                StartCoroutine(ToastManager(3, true, 0f));
                StartCoroutine(ToastManager(3, false, 3.0f));
            }
            else
            {
                StartCoroutine(ToastManager(3, true, 5.0f));
                StartCoroutine(ToastManager(3, false, 8.0f));
            }
            
        }
    }
    private string TransformNumber(string number)
    {
        if (number.Length == 11)
        {
            //0xy-1234-1234
            //x = 3,4,5,6
            //y = 1,2,3,4,5

            //01x-1234-1234
            //x=0,1,6,7,8...
            var part1 = number.Substring(0, 3);
            var part2 = number.Substring(3, 4);
            var part3 = number.Substring(7, 4);

            number = part1 + "-" + part2 + "-" + part3;
        }
        else if (number.Length == 10)
        {
            //01x-123-1234
            if (number.Substring(1, 1) == "1")
            {
                var part1 = number.Substring(0, 3);
                var part2 = number.Substring(3, 3);
                var part3 = number.Substring(6, 4);

                number = part1 + "-" + part2 + "-" + part3;
            }
            //02-1234-1234
            else
            {
                var part1 = number.Substring(0, 2);
                var part2 = number.Substring(2, 4);
                var part3 = number.Substring(6, 4);

                number = part1 + "-" + part2 + "-" + part3;
            }
        }
        //02-123-1234
        else if (number.Length == 9)
        {
            if (number.Substring(1, 1) == "2")
            {
                var part1 = number.Substring(0, 2);
                var part2 = number.Substring(2, 3);
                var part3 = number.Substring(5, 4);

                number = part1 + "-" + part2 + "-" + part3;
            }
            else
            {
                number = "+82" + number.Remove(0, 1);
            }
        }
        return number;
    }
    //Head reset is complete
    public void onHmdOrientationReseted()
    {

    }

    private IEnumerator ToastManager(int type,bool state,float time)
    {
        yield return new WaitForSeconds(time);

        switch (type)
        {
            //call toast
            case 1:
                {
                    calltoast.SetActive(state);
                    break;
                }
            //msg toast
            case 2:
                {
                    msgtoast.SetActive(state);
                    break;
                }
            //low hmd battery toast
            case 3:
                {
                    lowhmdBatterytoast.SetActive(state);
                    break;
                }
            //low phone battery toast
            case 4:
                {
                    lowphoneBatterytoast.SetActive(state);
                    break;
                }
            //low phone health toast
            case 5:
                {
                    LowPhoneHealthtoast.SetActive(state);
                    break;
                }
            //low controller battery toast
            case 6:
                {
                    LowcontrollerBatterytoast.SetActive(state);
                    break;
                }
        }

    }

    private void CheckControllerStateForG2(string state)
    {
        if (iPhoneHMDModeEnabled == 1)
        {
            if (Convert.ToBoolean(Convert.ToInt16(state)) && Controller.UPvr_GetControllerPower(0) == 0 && Pvr_ControllerManager.controllerlink.Controller0.Rotation.eulerAngles != Vector3.zero)
            {
                StartCoroutine(ToastManager(6, true, 0f));
                StartCoroutine(ToastManager(6, false, 3.0f));
            }
        }
    }
    private bool SDKManagerInitFPS()
    {
        Transform[] father;
        father = GetComponentsInChildren<Transform>(true);
        GameObject FPS = null;
        foreach (Transform child in father)
        {
            if (child.gameObject.name == "FPS")
            {
                FPS = child.gameObject;
            }
        }
        if (FPS != null)
        {
            if (systemFPS)
            {
                FPS.SetActive(true);
                return true;
            }
            int fps = 0;
#if !UNITY_EDITOR
            int rate = (int)GlobalIntConfigs.iShowFPS;
            Render.UPvr_GetIntConfig(rate, ref fps);
#endif
            if (Convert.ToBoolean(fps))
            {
                FPS.SetActive(true);
                return true;
            }
            if (ShowFPS)
            {
                FPS.SetActive(true);
                return true;
            }
            return false;
        }
        return false;
    }

    private bool SDKManagerInitConfigProfile()
    {
        pvr_UnitySDKConfig = Pvr_UnitySDKConfigProfile.Default;
        return true;
    }

    private bool SDKManagerInitEditor()
    {
        if (pvr_UnitySDKEditor == null)
        {
            HeadPose = new Pvr_UnitySDKPose(Vector3.zero, Quaternion.identity);
            pvr_UnitySDKEditor = this.gameObject.AddComponent<Pvr_UnitySDKEditor>();
        }
        return true;
    }

    private bool SDKManagerInitPara()
    {
        return true;
    }

    public void SDKManagerLongHomeKey()
    {
        //closepanel
        if (resetPanel.activeSelf)
        {
            resetPanel.SetActive(false);
            resetPanel.transform.Find("Panel").GetComponent<Canvas>().sortingOrder = 10001;
        }
        if (pvr_UnitySDKSensor != null)
        {
            if (isHasController)
            {
                if (Controller.UPvr_GetControllerState(0) == ControllerState.Connected ||
                    Controller.UPvr_GetControllerState(1) == ControllerState.Connected)
                {
                    pvr_UnitySDKSensor.OptionalResetUnitySDKSensor(0, 1);
                }
                else
                {
                    pvr_UnitySDKSensor.OptionalResetUnitySDKSensor(1, 1);
                }
            }
            else
            {
                pvr_UnitySDKSensor.OptionalResetUnitySDKSensor(1, 1);
            }

        }
    }

    private void setLongHomeKey()
    {
        if (sdk.HmdOnlyrot)
        {
            if (pvr_UnitySDKSensor != null)
            {
                if (isViewerLogicFlow)
                {
                    Debug.Log(pvr_UnitySDKSensor.ResetUnitySDKSensorAll()
                       ? "Long Home Key to Reset Sensor ALL Success!"
                       : "Long Home Key to Reset Sensor ALL Failed!");
                }
                else
                {
                    Debug.Log(pvr_UnitySDKSensor.ResetUnitySDKSensor()
                        ? "Long Home Key to Reset Sensor Success!"
                        : "Long Home Key to Reset Sensor Failed!");
                }
            }
        }
        else
        {
            if (trackingmode == 4)
            {
                pvr_UnitySDKSensor.OptionalResetUnitySDKSensor(1, 1);

            }
            else
            {
                if (safeToast.activeSelf)
                {
                    if (isHasController && (Controller.UPvr_GetControllerState(0) == ControllerState.Connected || Controller.UPvr_GetControllerState(1) == ControllerState.Connected))
                    {
                        pvr_UnitySDKSensor.OptionalResetUnitySDKSensor(0, 1);
                    }
                    else
                    {
                        pvr_UnitySDKSensor.OptionalResetUnitySDKSensor(1, 1);
                    }
                }
                else
                {
                    if (trackingmode == 0 || trackingmode == 1)
                    {
                        if (isViewerLogicFlow)
                        {
                            pvr_UnitySDKSensor.ResetUnitySDKSensorAll();
                        }
                        else
                        {
                            pvr_UnitySDKSensor.ResetUnitySDKSensor();
                        }
                    }
                    else
                    {
                        resetPanel.SetActive(true);
                    }
                }
            }
            
        }
    }

    public bool ViewerLogicFlow()
    {
        bool enable = false;
        try
        {
            int enumindex = (int)Pvr_UnitySDKAPI.GlobalIntConfigs.LOGICFLOW;
            int viewer = 0;
            int temp = Pvr_UnitySDKAPI.Render.UPvr_GetIntConfig(enumindex, ref viewer);
            PLOG.D("viewer  = " + viewer.ToString());
            if (temp == 0)
            {
                if (viewer == 1)
                {
                    enable = true;
                }
            }

        }
        catch (System.Exception e)
        {
            Debug.LogError("ViewerLogicFlow Get ERROR! " + e.Message);
            throw;
        }
        return enable;
    }
    #endregion

    /*************************************  Unity API ****************************************/
    #region Unity API

    void Awake()
    {

#if !UNITY_EDITOR && UNITY_ANDROID
        var javaVrActivityClass = new AndroidJavaClass("com.psmart.vrlib.VrActivity");
        var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        Pvr_UnitySDKAPI.System.UPvr_CallStaticMethod(javaVrActivityClass, "SetSecure", activity,SDK.Copyrightprotection);
#endif
        var controllermanager = FindObjectOfType<Pvr_ControllerManager>();
        isHasController = controllermanager != null;
        PLOG.getConfigTraceLevel();
#if !UNITY_EDITOR && UNITY_ANDROID
        isViewerLogicFlow = ViewerLogicFlow();
        Debug.Log("viewer :" + isViewerLogicFlow.ToString());
        if (isViewerLogicFlow)
        {
            SDK.HmdOnlyrot = true;
            SDK.ControllerOnlyrot = true;
        }
        else
        {
            int enumindex = (int)GlobalIntConfigs.TRACKING_MODE;
            Render.UPvr_GetIntConfig(enumindex, ref trackingmode);
            LoadIsPUIValue();
            if (isPUI)
            {
                if (trackingmode == 1 || trackingmode == 0)
                {
                    SDK.HmdOnlyrot = true;
                    SDK.ControllerOnlyrot = true;
                } 
            }
        }
#endif
        Application.targetFrameRate = 61;
#if !UNITY_EDITOR && UNITY_ANDROID
        int fps = -1;
        int rate = (int) GlobalIntConfigs.TARGET_FRAME_RATE;
        Render.UPvr_GetIntConfig(rate, ref fps);
        float ffps = 0.0f;
        int frame = (int) GlobalFloatConfigs.DISPLAY_REFRESH_RATE;
        Render.UPvr_GetFloatConfig(frame, ref ffps);
        Application.targetFrameRate = fps > 0 ? fps : (int)ffps;
#endif
        if (!DefaultFPS)
        {
            Application.targetFrameRate = CustomFPS;
        }

#if !UNITY_EDITOR && UNITY_ANDROID
        if (!UseCustomNeckPara)
        {
            float neckx = 0.0f;
            float necky = 0.0f;
            float neckz = 0.0f;
            int modelx = (int) GlobalFloatConfigs.NECK_MODEL_X;
            int modely = (int) GlobalFloatConfigs.NECK_MODEL_Y;
            int modelz = (int) GlobalFloatConfigs.NECK_MODEL_Z;
            Render.UPvr_GetFloatConfig(modelx, ref neckx);
            Render.UPvr_GetFloatConfig(modely, ref necky);
            Render.UPvr_GetFloatConfig(modelz, ref neckz);
            if (neckx != 0.0f || necky != 0.0f || neckz != 0.0f)
            {
                neckOffset = new Vector3(neckx, necky, neckz);
            }
        }
#endif
        Render.UPvr_GetIntConfig((int)GlobalIntConfigs.iPhoneHMDModeEnabled, ref iPhoneHMDModeEnabled);
        if (sdk == null)
        {
            sdk = this;
        }
        if (sdk != this)
        {
            Debug.LogError("SDK object should be a singleton.");
            enabled = false;
            return;
        }
        if (SDKManagerInit())
        {
            Debug.Log("SDK Init success.");
        }
        else
        {
            Debug.LogError("SDK Init Failed.");
            Application.Quit();
        }

        SDKManagerInitFPS();

        safeArea = transform.Find("SafeArea2").gameObject;
        safeToast = transform.Find("SafeToast").gameObject;
        if (trackingmode == 2 || trackingmode == 3)
        {
            safeArea.transform.localScale = Vector3.one;
            safeToast.transform.localScale = Vector3.one;
        }
        else
        {
            safeArea.transform.localScale = Vector3.zero;
            safeToast.transform.localScale = Vector3.zero;
        }
        resetPanel = transform.Find("ResetPanel").gameObject;
        safePanel1 = transform.Find("SafePanel1").gameObject;

        calltoast = transform.Find("Head/Canvas/Call").gameObject;
        msgtoast = transform.Find("Head/Canvas/Msg").gameObject;
        lowhmdBatterytoast = transform.Find("Head/Canvas/LowHmdBattery").gameObject;
        lowphoneBatterytoast = transform.Find("Head/Canvas/LowPhoneBattery").gameObject;
        LowPhoneHealthtoast = transform.Find("Head/Canvas/LowPhoneHealth").gameObject;
        LowcontrollerBatterytoast = transform.Find("Head/Canvas/LowControllerBattery").gameObject;

        Pvr_ControllerManager.ControllerStatusChangeEvent += CheckControllerStateForG2;

        if (isViewerLogicFlow)
        {
            ViewerToast = transform.Find("Head").Find("Viewertoast").gameObject;
            if (ViewerToast == null)
            {
                Debug.Log("WHT");
            }
            InitViewerBatteryVolClass();

#if !UNITY_EDITOR && UNITY_ANDROID
            if (safeArea != null)
            {
                DestroyObject(safeArea);
            }
            if (safeToast != null)
            {
                DestroyObject(safeToast);
            }
            if (resetPanel != null)
            {
                DestroyObject(resetPanel);
            }
            if (safePanel1 != null)
            {
                DestroyObject(safePanel1);
            }
#endif
        }
        else
        {
            if (Application.systemLanguage != SystemLanguage.Chinese && Application.systemLanguage != SystemLanguage.ChineseSimplified)
            {
                safeToast.transform.Find("Panel").GetComponent<RectTransform>().sizeDelta = new Vector2(470, 470);
                safeToast.transform.Find("Panel/title").localPosition = new Vector3(0, 173, 0);
                safeToast.transform.Find("Panel/title").GetComponent<Text>().text = "Please back into the safe zone";
                safeToast.transform.Find("Panel/Image").localPosition = new Vector3(0, -108, 0);
                safeToast.transform.Find("Panel/Text").GetComponent<RectTransform>().sizeDelta = new Vector2(440, 180);
                safeToast.transform.Find("Panel/Text").localPosition = new Vector3(10, 55, 0);
                safePanel1.transform.Find("Panel").GetComponent<RectTransform>().sizeDelta = new Vector2(470, 470);
                safePanel1.transform.Find("Panel/toast1").GetComponent<RectTransform>().sizeDelta = new Vector2(425, 200);
                resetPanel.transform.Find("Panel").GetComponent<RectTransform>().sizeDelta = new Vector2(470, 470);
                resetPanel.transform.Find("Panel/toast").GetComponent<RectTransform>().sizeDelta = new Vector2(440, 180);

                if (Application.systemLanguage == SystemLanguage.English)
                {
                    if (DefaultRange)
                    {
                        resetPanel.transform.Find("Panel/toast").GetComponent<Text>().text =
                            "Please take off the headset，insure there have no obstacles in the radius of 0.8 meters，then press on 【confirm button】 again";
                        safePanel1.transform.Find("Panel/toast1").GetComponent<Text>().text =
                            "Safe zone has reset successfully, please insure there have no obstacles in the radius of 0.8 meters，then press on 【confirm button】";
                    }
                    else
                    {
                        resetPanel.transform.Find("Panel/toast").GetComponent<Text>().text =
                            "Please take off the headset，insure there have no obstacles in the radius of " + CustomRange + " meters，then press on 【confirm button】 again";
                        safePanel1.transform.Find("Panel/toast1").GetComponent<Text>().text =
                            "Safe zone has reset successfully, please insure there have no obstacles in the radius of " + CustomRange + " meters，then press on 【confirm button】";
                    }
                }

                if (Application.systemLanguage == SystemLanguage.Korean)
                {
                    if (DefaultRange)
                    {
                        resetPanel.transform.Find("Panel/toast").GetComponent<Text>().text =
                            "헤드셋을 벗고 주변 반경 0.8m 범위 내에 장애물이 없도록 확보해 주신 후 다시 【확인 키】를 눌러 위치를 리셋해 주십시오";
                        safePanel1.transform.Find("Panel/toast1").GetComponent<Text>().text =
                            "안전 구역이 리셋되었으니 헤드셋을 벗고 주변 반경 0.8m 범위 내에 장애물이 없도록 확보해 주신 후 【확인 키】를 눌러 주십시오.";
                    }
                    else
                    {
                        resetPanel.transform.Find("Panel/toast").GetComponent<Text>().text =
                            "헤드셋을 벗고 주변 반경 " + CustomRange + " 범위 내에 장애물이 없도록 확보해 주신 후 다시 【확인 키】를 눌러 위치를 리셋해 주십시오";
                        safePanel1.transform.Find("Panel/toast1").GetComponent<Text>().text =
                            "안전 구역이 리셋되었으니 헤드셋을 벗고 주변 반경 " + CustomRange + " 범위 내에 장애물이 없도록 확보해 주신 후 【확인 키】를 눌러 주십시오.";
                    }
                }

                if (Application.systemLanguage == SystemLanguage.Japanese)
                {
                    if (DefaultRange)
                    {
                        resetPanel.transform.Find("Panel/toast").GetComponent<Text>().text =
                            "ヘッドセットを取り外して、半径0.8メルトに障害物がないように確認して、また確認ボタンを押してください";
                        safePanel1.transform.Find("Panel/toast1").GetComponent<Text>().text =
                            "安全域がリエットしました、ヘッドセットを取り外して、半径0.8メルトに障害物がないように確認して、確認ボタンを押してください";
                    }
                    else
                    {
                        resetPanel.transform.Find("Panel/toast").GetComponent<Text>().text =
                            "ヘッドセットを取り外して、半径 " + CustomRange + " メルトに障害物がないように確認して、また確認ボタンを押してください";
                        safePanel1.transform.Find("Panel/toast1").GetComponent<Text>().text =
                            "安全域がリエットしました、ヘッドセットを取り外して、半径" + CustomRange + "メルトに障害物がないように確認して、確認ボタンを押してください";
                    }
                }
                
            }
#if !UNITY_EDITOR && UNITY_ANDROID
            if (!SDK.HmdOnlyrot)
            {
                if (Sensor.Pvr_IsHead6dofReset() && ShowSafePanel)
                {
                    safePanel1.SetActive(true);
                }
            }
#endif
        }


        //korean toast
        if (Application.systemLanguage == SystemLanguage.English)
        {
            msgtoast.transform.Find("Text").GetComponent<Text>().text = "Receive a message";
            msgtoast.transform.Find("string").GetComponent<Text>().text = "To check MMS，please take off HMD and plug out HMD from smart phone";
            calltoast.transform.Find("Text").GetComponent<Text>().text = "Incoming Call";
            calltoast.transform.Find("Text").GetComponent<Text>().text = "To check phone call or MMS，please take off HMD and plug out HMD from smart phone";
            lowhmdBatterytoast.transform.Find("Text").GetComponent<Text>().text = "VR device low battery，5% remaining，please charge";
            lowphoneBatterytoast.transform.Find("Text").GetComponent<Text>().text = "Smart phone low battery，5% remaining，please charge";
            LowPhoneHealthtoast.transform.Find("Text").GetComponent<Text>().text = "Heat warning，please take off HMD and plug out HMD from smart phone";
            LowcontrollerBatterytoast.transform.Find("Text").GetComponent<Text>().text = "Low battery of controller";
        }
        if (Application.systemLanguage == SystemLanguage.Japanese)
        {
            msgtoast.transform.Find("Text").GetComponent<Text>().text = "新着メッセージがあります";
            msgtoast.transform.Find("string").GetComponent<Text>().text = "メッセージを見るには、ヘッドセットを取り外して、スマートホンからヘッドセットを外しでください";
            calltoast.transform.Find("Text").GetComponent<Text>().text = "お電話です";
            calltoast.transform.Find("Text").GetComponent<Text>().text = "応答するには、ヘッドセットを取り外して、スマートホンからヘッドセットを外しでください";
            lowhmdBatterytoast.transform.Find("Text").GetComponent<Text>().text = "ヘッドセットのバッテリー残量が5％になっています";
            lowphoneBatterytoast.transform.Find("Text").GetComponent<Text>().text = "スマートホンのバッテリー残量が5％になっています";
            LowPhoneHealthtoast.transform.Find("Text").GetComponent<Text>().text = "スマートホンが過熱になっています、今すぐにヘッドセットを取り外して、スマートホンからヘッドセットを外しでください";
            LowcontrollerBatterytoast.transform.Find("Text").GetComponent<Text>().text = "コントローラーのバッテリー残量が低いです";
        }
        if (Application.systemLanguage == SystemLanguage.Chinese || Application.systemLanguage == SystemLanguage.ChineseSimplified)
        {
            msgtoast.transform.Find("Text").GetComponent<Text>().text = "收到短信";
            msgtoast.transform.Find("string").GetComponent<Text>().text = "如需确认,请摘下VR设备并断开连接";
            calltoast.transform.Find("Text").GetComponent<Text>().text = "收到来电";
            calltoast.transform.Find("Text").GetComponent<Text>().text = "如接听来电,请摘下VR设备并断开连接";
            lowhmdBatterytoast.transform.Find("Text").GetComponent<Text>().text = "VR设备电量低于5%";
            lowphoneBatterytoast.transform.Find("Text").GetComponent<Text>().text = "手机电量低于5%";
            LowPhoneHealthtoast.transform.Find("Text").GetComponent<Text>().text = "手机过热，请摘下头戴并拔掉USB线，暂停使用该设备";
            LowcontrollerBatterytoast.transform.Find("Text").GetComponent<Text>().text = "手柄电量不足";
        }

    }


    //wait for unity to start rendering
    IEnumerator Start()
    {
#if UNITY_EDITOR
        yield break;
#else
        yield return StartCoroutine(InitRenderThreadRoutine());
#endif
    }

    IEnumerator InitRenderThreadRoutine()
    {
        Debug.Log("InitRenderThreadRoutine begin");
        for (int i = 0; i < 2; ++i)
        {
            yield return null;
        }
        Debug.Log("InitRenderThreadRoutine after a wait");


        if (pvr_UnitySDKRender != null)
        {
            pvr_UnitySDKRender.IssueRenderThread();
        }
        else
        {
            Debug.Log("InitRenderThreadRoutine pvr_UnitySDKRender == null");
        }

        Debug.Log("InitRenderThreadRoutine end");
        yield break;
    }


    void Update()
    {
        if (isHasController  && iPhoneHMDModeEnabled ==1)
        {
            if (Controller.UPvr_GetControllerPower(0) == 0 && Pvr_ControllerManager.controllerlink.controller0Connected && Pvr_ControllerManager.controllerlink.Controller0.Rotation.eulerAngles != Vector3.zero)
            {
                if (!lowControllerpowerstate)
                {
                    StartCoroutine(ToastManager(6, true, 0f));
                    StartCoroutine(ToastManager(6, false, 3.0f));
                    lowControllerpowerstate = true;
                }

                controllerpowershowtime += Time.deltaTime;
                if (controllerpowershowtime >= 3600f)
                {
                    lowControllerpowerstate = false;
                    controllerpowershowtime = 0f;
                }
            }
        }
        if (Input.touchCount == 1)
        {
            if (Input.touches[0].phase == TouchPhase.Began)
            {
                newPicovrTriggered = true;
            }
        }
        else
         if (Input.GetKeyDown(KeyCode.JoystickButton0))
        {
            newPicovrTriggered = true;
        }

        if (pvr_UnitySDKSensor != null)
        {
            pvr_UnitySDKSensor.SensorUpdate();
        }
        if (!IsViewerLogicFlow)
        {
#if !UNITY_EDITOR && UNITY_ANDROID
            if (isHasController && (Controller.UPvr_GetControllerState(0) == ControllerState.Connected || Controller.UPvr_GetControllerState(1) == ControllerState.Connected))
            {
                if (DefaultRange)
                {
                    if (Application.systemLanguage == SystemLanguage.Chinese ||
                        Application.systemLanguage == SystemLanguage.ChineseSimplified)
                    {
                        safeToast.transform.Find("Panel/Text").GetComponent<Text>().text =
                            "若需重置位置，请确保周围半径0.8米范围内没有障碍物，将手柄指向前方，长按【Home键】";
                    }
                    if(Application.systemLanguage == SystemLanguage.English)
                    {

                        safeToast.transform.Find("Panel/Text").GetComponent<Text>().text =
                            "To reset safe zone，please insure there have no obstacles in the radius of 0.8 meters，then point the controller forward，long press on the 【home button】";
                    }

                    if (Application.systemLanguage == SystemLanguage.Korean)
                    {
                        safeToast.transform.Find("Panel/Text").GetComponent<Text>().text =
                            "위치를 리셋해야 할 경우, 주변 반경 0.8m 범위 내에 장애물이 없도록 확보해 주시고 컨트롤러를 앞쪽으로 가리키며 【Home 키】를 길게 눌러 주십시오";
                    }

                    if (Application.systemLanguage == SystemLanguage.Japanese)
                    {
                        safeToast.transform.Find("Panel/Text").GetComponent<Text>().text =
                            "安全域をリセットしたい時は、半径0.8メルトに障害物がないように確認して、Homeボタンを長押ししてください";
                    }
                }
                else
                {
                    if (Application.systemLanguage == SystemLanguage.Chinese ||
                        Application.systemLanguage == SystemLanguage.ChineseSimplified)
                    {
                        safeToast.transform.Find("Panel/Text").GetComponent<Text>().text =
                            "若需重置位置，请确保周围半径" + CustomRange + "米范围内没有障碍物，将手柄指向前方，长按【Home键】";
                    }
                    if (Application.systemLanguage == SystemLanguage.English)
                    {

                        safeToast.transform.Find("Panel/Text").GetComponent<Text>().text =
                            "To reset safe zone，please insure there have no obstacles in the radius of " + CustomRange + " meters，then point the controller forward，long press on the 【home button】";
                    }

                    if (Application.systemLanguage == SystemLanguage.Korean)
                    {
                        safeToast.transform.Find("Panel/Text").GetComponent<Text>().text =
                            "위치를 리셋해야 할 경우, 주변 반경 " + CustomRange + " 범위 내에 장애물이 없도록 확보해 주시고 컨트롤러를 앞쪽으로 가리키며 【Home 키】를 길게 눌러 주십시오";
                    }

                    if (Application.systemLanguage == SystemLanguage.Japanese)
                    {
                        safeToast.transform.Find("Panel/Text").GetComponent<Text>().text =
                            "安全域をリセットしたい時は、半径" + CustomRange + "メルトに障害物がないように確認して、Homeボタンを長押ししてください";
                    }

                }

                if (Input.GetKeyDown(KeyCode.JoystickButton0) || Controller.UPvr_GetKeyDown(0, Pvr_KeyCode.TOUCHPAD) || Controller.UPvr_GetKeyDown(1, Pvr_KeyCode.TOUCHPAD))
                {
                    if (safePanel1.activeSelf)
                        safePanel1.SetActive(false);
                    if (resetPanel.activeSelf)
                    {
                        resetPanel.SetActive(false);
                        pvr_UnitySDKSensor.OptionalResetUnitySDKSensor(0, 1);
                    }
                }
            }
            else
            {
                if (DefaultRange)
                {
                    if (Application.systemLanguage == SystemLanguage.Chinese ||
                        Application.systemLanguage == SystemLanguage.ChineseSimplified)
                    {
                        safeToast.transform.Find("Panel/Text").GetComponent<Text>().text =
                            "若需重置位置，请确保周围半径0.8米范围内没有障碍物，长按头戴【Home键】";
                    }
                    else
                    {

                        safeToast.transform.Find("Panel/Text").GetComponent<Text>().text =
                            "To reset safe zone，please insure there have no obstacles in the radius of 0.8 meters，then long press on the 【home button】 on headset";
                    }
                }
                else
                {
                    if (Application.systemLanguage == SystemLanguage.Chinese ||
                        Application.systemLanguage == SystemLanguage.ChineseSimplified)
                    {
                        safeToast.transform.Find("Panel/Text").GetComponent<Text>().text =
                            "若需重置位置，请确保周围半径" + CustomRange + "米范围内没有障碍物，长按头戴【Home键】";
                    }
                    else
                    {

                        safeToast.transform.Find("Panel/Text").GetComponent<Text>().text =
                            "To reset safe zone，please insure there have no obstacles in the radius of " + CustomRange + " meters，then long press on the 【home button】 on headset";
                    }

                }
                if (Input.GetKeyDown(KeyCode.JoystickButton0))
                {
                    if (safePanel1.activeSelf)
                    {
                        safePanel1.SetActive(false);
                    }
                    if (resetPanel.activeSelf)
                    {
                        resetPanel.SetActive(false);
                        pvr_UnitySDKSensor.OptionalResetUnitySDKSensor(1, 1);
                    }
                }
            }
#endif
        }

        picovrTriggered = newPicovrTriggered;
        newPicovrTriggered = false;
        if (!isViewerLogicFlow)
        {
#if !UNITY_EDITOR && UNITY_ANDROID
            
        if (safeToast.activeSelf)
        {
            safeToast.transform.localPosition = SDK.HeadPose.Position;
            safeToast.transform.localRotation = Quaternion.Euler(0, SDK.HeadPose.Orientation.eulerAngles.y, 0);
        }

        if (resetPanel.activeSelf)
        {
            resetPanel.transform.localPosition = SDK.HeadPose.Position;
            resetPanel.transform.localRotation = Quaternion.Euler(0, SDK.HeadPose.Orientation.eulerAngles.y, 0);
        }
  
        if (safePanel1.activeSelf)
        {
            safePanel1.transform.localPosition = SDK.HeadPose.Position;
            safePanel1.transform.localRotation = Quaternion.Euler(0, SDK.HeadPose.Orientation.eulerAngles.y, 0);
        }

            if (trackingmode == 2 || trackingmode == 3)
            {
                if (!SDK.HmdOnlyrot)
                {
                    //default 0.8m
                    if (DefaultRange)
                    {
                        if (isHasController)
                        {
                            if (Mathf.Sqrt(Mathf.Pow(HeadPose.Position.x, 2.0f) + Mathf.Pow(HeadPose.Position.z, 2.0f)) > 0.56f || Mathf.Sqrt(Mathf.Pow(Controller.UPvr_GetControllerPOS(0).x, 2.0f) + Mathf.Pow(Controller.UPvr_GetControllerPOS(0).z, 2.0f)) > 0.8f || Mathf.Sqrt(Mathf.Pow(Controller.UPvr_GetControllerPOS(1).x, 2.0f) + Mathf.Pow(Controller.UPvr_GetControllerPOS(1).z, 2.0f)) > 0.8f)
                            {
                                safeArea.transform.localScale = new Vector3(1.6f, 1.6f, 1.6f);
                                safeArea.SetActive(true);
                            }
                            else
                            {
                                safeArea.SetActive(false);
                            }
                        }
                        else
                        {
                            if (Mathf.Sqrt(Mathf.Pow(HeadPose.Position.x, 2.0f) + Mathf.Pow(HeadPose.Position.z, 2.0f)) > 0.56f)
                            {
                                safeArea.transform.localScale = new Vector3(1.6f, 1.6f, 1.6f);
                                safeArea.SetActive(true);
                            }
                            else
                            {
                                safeArea.SetActive(false);
                            }
                        }

                        if (Mathf.Sqrt(Mathf.Pow(HeadPose.Position.x, 2.0f) + Mathf.Pow(HeadPose.Position.z, 2.0f)) > 0.8f)
                        {
                            if (!safeToast.activeSelf)
                            {
                                safeToast.transform.Find("Panel").GetComponent<Canvas>().sortingOrder = resetPanel.transform.Find("Panel").GetComponent<Canvas>().sortingOrder + 1;
                                safeToast.SetActive(true);
                            }
                        }
                        else
                        {
                            if (safeToast.activeSelf)
                            {
                                safeToast.SetActive(false);
                                safeToast.transform.Find("Panel").GetComponent<Canvas>().sortingOrder = 10001;
                            }
                        }
                    }
                    else
                    {
                        if (isHasController)
                        {
                            if (Mathf.Sqrt(Mathf.Pow(HeadPose.Position.x, 2.0f) + Mathf.Pow(HeadPose.Position.z, 2.0f)) > (0.7f * CustomRange) || Mathf.Sqrt(Mathf.Pow(Controller.UPvr_GetControllerPOS(0).x, 2.0f) + Mathf.Pow(Controller.UPvr_GetControllerPOS(0).z, 2.0f)) > CustomRange || Mathf.Sqrt(Mathf.Pow(Controller.UPvr_GetControllerPOS(1).x, 2.0f) + Mathf.Pow(Controller.UPvr_GetControllerPOS(1).z, 2.0f)) > CustomRange)
                            {
                                safeArea.transform.localScale = new Vector3(CustomRange / 0.5f, CustomRange / 0.5f, CustomRange / 0.5f);
                                safeArea.SetActive(true);
                            }
                            else
                            {
                                safeArea.SetActive(false);
                            }
                        }
                        else
                        {
                            if (Mathf.Sqrt(Mathf.Pow(HeadPose.Position.x, 2.0f) + Mathf.Pow(HeadPose.Position.z, 2.0f)) > (0.7f * CustomRange))
                            {
                                safeArea.transform.localScale =
                                    new Vector3(CustomRange / 0.5f, CustomRange / 0.5f, CustomRange / 0.5f);
                                safeArea.SetActive(true);
                            }
                            else
                            {
                                safeArea.SetActive(false);
                            }
                        }
                        if (Mathf.Sqrt(Mathf.Pow(HeadPose.Position.x, 2.0f) + Mathf.Pow(HeadPose.Position.z, 2.0f)) > CustomRange)
                        {
                            if (!safeToast.activeSelf)
                            {
                                safeToast.transform.Find("Panel").GetComponent<Canvas>().sortingOrder = resetPanel.transform.Find("Panel").GetComponent<Canvas>().sortingOrder + 1;
                                safeToast.SetActive(true);
                            }
                        }
                        else
                        {
                            if (safeToast.activeSelf)
                            {
                                safeToast.SetActive(false);
                                safeToast.transform.Find("Panel").GetComponent<Canvas>().sortingOrder = 10001;
                            }
                        }
                    }
                }
            }
#endif
        }
    }
    void OnDestroy()
    {

        if (sdk == this)
        {
            sdk = null;
        }
        RenderTexture.active = null;
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
        Pvr_ControllerManager.ControllerStatusChangeEvent -= CheckControllerStateForG2;
    }

    public void OnApplicationQuit()
    {

#if UNITY_ANDROID && !UNITY_EDITOR
        /*
               if (pvr_UnitySDKSensor != null)
                 {
                pvr_UnitySDKSensor.StopUnitySDKSensor();
                  }
                try{
                    Debug.Log("OnApplicationQuit  1  -------------------------");
                    Pvr_UnitySDKPluginEvent.Issue( RenderEventType.ShutdownRenderThread );
                }
                catch (Exception e)
                {
                    Debug.Log("ShutdownRenderThread Error" + e.Message);
                }
        */
#endif
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    private void OnPause()
    {
        Pvr_UnitySDKAPI.System.UPvr_StopHomeKeyReceiver();
        this.LeaveVRMode();
        if (pvr_UnitySDKSensor != null)
        {
            pvr_UnitySDKSensor.StopUnitySDKSensor();
        }
    }

    private void OnApplicationPause(bool pause)
    {
        Debug.Log("OnApplicationPause-------------------------" + (pause ? "true" : "false"));
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!isViewerLogicFlow)
        {
            if (Pvr_UnitySDKAPI.System.UPvr_IsPicoActivity())
            {
                bool state = Pvr_UnitySDKAPI.System.UPvr_GetMainActivityPauseStatus();
                Debug.Log("Current Activity Pause State:" + state);
                pause = state;
            }
        }

        if (pause)
        { 
            onResume = false;
           if (BattEnable && IsViewerLogicFlow)
            {
                StopViewerBatteryReceiver();
            }
            OnPause();
        }
        else
        {             
            onResume = true;
            GL.InvalidateState();
            StartCoroutine(OnResume());
        }
#endif
    }

    void OnApplicationFocus(bool focus)
    {
        Debug.Log("OnApplicationFocus-------------------------" + (focus ? "true" : "false"));
        if (focus)
        {
            if (IsViewerLogicFlow)
            {
                string gameobjName = this.gameObject.name;
                StartViewerBatteryReceiver(gameobjName);
            }
        }
    }

    public void EnterVRMode()
    {
        Pvr_UnitySDKPluginEvent.Issue(RenderEventType.Resume);
        this.isEnterVRMode = true;
        if (eventEnterVRMode != null)
        {
            eventEnterVRMode();
        }
    }

    public void LeaveVRMode()
    {
        Pvr_UnitySDKPluginEvent.Issue(RenderEventType.Pause);
        this.isEnterVRMode = false;
    }

    public void SixDofForceQuit()
    {
        Application.Quit();
    }

    private void LoadIsPUIValue()
    {
        AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject packageManagerObj = jo.Call<AndroidJavaObject>("getPackageManager");
        string packageName = jo.Call<string>("getPackageName");
        AndroidJavaObject applicationInfoObj = packageManagerObj.Call<AndroidJavaObject>("getApplicationInfo", packageName, 128);
        AndroidJavaObject bundleObj = applicationInfoObj.Get<AndroidJavaObject>("metaData");
        isPUI = Convert.ToBoolean(bundleObj.Call<int>("getInt", "isPUI"));
    }
    #endregion

    /************************************    IEnumerator  *************************************/
    private IEnumerator OnResume()
    {
        if (!isViewerLogicFlow)
        {
            if (pvr_UnitySDKSensor != null)
            {
                pvr_UnitySDKSensor.StartUnitySDKSensor();

                int iEnable6Dof = -1;
#if !UNITY_EDITOR && UNITY_ANDROID
            int iEnable6DofGlobalTracking = (int) GlobalIntConfigs.ENBLE_6DOF_GLOBAL_TRACKING;
            Render.UPvr_GetIntConfig(iEnable6DofGlobalTracking, ref iEnable6Dof);
#endif
                if (iEnable6Dof != 1)
                {
                    int sensormode = -1;
#if !UNITY_EDITOR && UNITY_ANDROID
            int isensormode = (int) GlobalIntConfigs.SensorMode;
            Render.UPvr_GetIntConfig(isensormode, ref sensormode);
#endif

                    if (sensormode != 8)
                    {
                        pvr_UnitySDKSensor.ResetUnitySDKSensor();
                    }
                }
                if (!SDK.HmdOnlyrot)
                {
                    if (Sensor.Pvr_IsHead6dofReset() && ShowSafePanel)
                    {
                        safePanel1.SetActive(true);
                    }
                }

            }
        }

        if (Pvr_UnitySDKAPI.System.UPvr_IsPicoActivity())
        {
            bool setMonoPresentation = Pvr_UnitySDKAPI.System.UPvr_SetMonoPresentation();
            Debug.Log("onresume set monoPresentation success ?-------------" + setMonoPresentation.ToString());

            bool isPresentationExisted = Pvr_UnitySDKAPI.System.UPvr_IsPresentationExisted();
            Debug.Log("onresume presentation existed ?-------------" + isPresentationExisted.ToString());
        }

        yield return new WaitForSeconds(1.0f);

        this.EnterVRMode();
        Pvr_UnitySDKAPI.System.UPvr_StartHomeKeyReceiver(this.gameObject.name);
        Pvr_UnitySDKEye.setLevel = false;
    }
}
