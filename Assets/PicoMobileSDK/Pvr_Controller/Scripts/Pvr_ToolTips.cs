using UnityEngine;
using System.Collections;
using Pvr_UnitySDKAPI;
using UnityEngine.UI;

public class Pvr_ToolTips : MonoBehaviour
{
    public enum TipBtn
    {
        app,
        touchpad,
        home,
        volup,
        voldown,
        trigger
    }
    private ControllerDevice currentDevice;
    private float tipsAlpha;
    
    public void ChangeTipsText(TipBtn tip, string key)
    {
        switch (tip)
        {
            case TipBtn.app:
                {
                    transform.Find("apptip/btn/Text").GetComponent<Text>().text = key;
                }
                break;
            case TipBtn.touchpad:
                {
                    transform.Find("touchtip/btn/Text").GetComponent<Text>().text = key;
                }
                break;
            case TipBtn.home:
                {
                    transform.Find("hometip/btn/Text").GetComponent<Text>().text = key;
                }
                break;
            case TipBtn.volup:
                {
                    transform.Find("volup/btn/Text").GetComponent<Text>().text = key;
                }
                break;
            case TipBtn.voldown:
                {
                    transform.Find("voldown/btn/Text").GetComponent<Text>().text = key;
                }
                break;
            case TipBtn.trigger:
                {
                    transform.Find("triggertip/btn/Text").GetComponent<Text>().text = key;
                }
                break;
        }
    }

    void Start()
    {
        SystemLanguage localanguage = Application.systemLanguage;
        currentDevice = transform.GetComponentInParent<Pvr_ControllerVisual>().currentDevice;
        if (localanguage == SystemLanguage.Chinese || localanguage == SystemLanguage.ChineseSimplified || localanguage == SystemLanguage.ChineseTraditional)
        {
            transform.Find("apptip/btn/Text").GetComponent<Text>().text = "返回键";
            transform.Find("touchtip/btn/Text").GetComponent<Text>().text = "确认键";
            transform.Find("hometip/btn/Text").GetComponent<Text>().text = "Home键";

            var volup = transform.Find("volup/btn/Text");
            if (volup != null)
                volup.GetComponent<Text>().text = "音量+";
            var voldown = transform.Find("voldown/btn/Text");
            if(voldown !=null)
                voldown.GetComponent<Text>().text = "音量-";
            var trigtip = transform.Find("triggertip/btn/Text");
            if(trigtip != null)
                trigtip.GetComponent<Text>().text = "扳机键";
        }
        if(localanguage == SystemLanguage.English)
        {
            transform.Find("apptip/btn/Text").GetComponent<Text>().text = "Back";
            transform.Find("touchtip/btn/Text").GetComponent<Text>().text = "Confirm";
            transform.Find("hometip/btn/Text").GetComponent<Text>().text = "Home";
            var volup = transform.Find("volup/btn/Text");
            if (volup != null)
                volup.GetComponent<Text>().text = "Volume+";
            var voldown = transform.Find("voldown/btn/Text");
            if (voldown != null)
                voldown.GetComponent<Text>().text = "Volume-";
            var trigtip = transform.Find("triggertip/btn/Text");
            if (trigtip != null)
                trigtip.GetComponent<Text>().text = "Trigger";
        }
        if (localanguage == SystemLanguage.Korean)
        {
            transform.Find("apptip/btn/Text").GetComponent<Text>().text = "리턴 키";
            transform.Find("touchtip/btn/Text").GetComponent<Text>().text = "확인 키";
            transform.Find("hometip/btn/Text").GetComponent<Text>().text = "Home 키";
            var volup = transform.Find("volup/btn/Text");
            if (volup != null)
                volup.GetComponent<Text>().text = "볼륨+";
            var voldown = transform.Find("voldown/btn/Text");
            if (voldown != null)
                voldown.GetComponent<Text>().text = "볼륨-";
            var trigtip = transform.Find("triggertip/btn/Text");
            if (trigtip != null)
                trigtip.GetComponent<Text>().text = "트리거 키";
        }
        if (localanguage == SystemLanguage.Japanese)
        {
            transform.Find("apptip/btn/Text").GetComponent<Text>().text = "バック";
            transform.Find("touchtip/btn/Text").GetComponent<Text>().text = "確認";
            transform.Find("hometip/btn/Text").GetComponent<Text>().text = "Home";
            var volup = transform.Find("volup/btn/Text");
            if (volup != null)
                volup.GetComponent<Text>().text = "音量+";
            var voldown = transform.Find("voldown/btn/Text");
            if (voldown != null)
                voldown.GetComponent<Text>().text = "音量-";
            var trigtip = transform.Find("triggertip/btn/Text");
            if (trigtip != null)
                trigtip.GetComponent<Text>().text = "トリガー";
        }


    }
    void OnApplicationPause(bool pause)
    {
        
    }

    void Update()
    {

        switch (currentDevice)
        {
            case Pvr_UnitySDKAPI.ControllerDevice.Goblin1:
            case Pvr_UnitySDKAPI.ControllerDevice.G2:
                {
                    tipsAlpha = (330 - transform.parent.parent.parent.localRotation.eulerAngles.x) / 45.0f;
                    if (transform.parent.parent.parent.localRotation.eulerAngles.x >= 270 &&
                        transform.parent.parent.parent.localRotation.eulerAngles.x <= 330)
                    {
                        tipsAlpha = Mathf.Max(0.0f, tipsAlpha);
                        tipsAlpha = tipsAlpha > 1.0f ? 1.0f : tipsAlpha;
                    }
                    else
                    {
                        tipsAlpha = 0.0f;
                    }
                    GetComponent<CanvasGroup>().alpha = tipsAlpha;

                }
                break;
            case Pvr_UnitySDKAPI.ControllerDevice.Neo2:
                {
                    tipsAlpha = (330 - transform.parent.parent.parent.localRotation.eulerAngles.x) / 45.0f;
                    if (transform.parent.parent.parent.localRotation.eulerAngles.x >= 270 &&
                        transform.parent.parent.parent.localRotation.eulerAngles.x <= 330)
                    {
                        tipsAlpha = Mathf.Max(0.0f, tipsAlpha);
                        tipsAlpha = tipsAlpha > 1.0f ? 1.0f : tipsAlpha;
                    }
                    else
                    {
                        tipsAlpha = 0.0f;
                    }
                    GetComponent<CanvasGroup>().alpha = tipsAlpha;
                }
                break;
        }

    }
}


