using System;
using UnityEngine;
using System.Collections;
using Pvr_UnitySDKAPI;


namespace Pvr_UnitySDKAPI
{
    public enum ControllerVariety
    {
        Controller0,
        Controller1,
    }
}   

public class Pvr_ControllerModuleInit : MonoBehaviour
{
    
    public ControllerVariety Variety;
    public bool IsCustomModel = false;
    [SerializeField]
    private GameObject dot;
    [SerializeField]
    private GameObject rayLine;
    [SerializeField]
    private GameObject controller;
    private int mainHand = 0;
    private bool moduleState = true;

    void Awake()
    {
        Pvr_ControllerManager.PvrServiceStartSuccessEvent += ServiceStartSuccess;
        Pvr_ControllerManager.SetControllerAbilityEvent += CheckControllerStateOfAbility;
        Pvr_ControllerManager.ControllerStatusChangeEvent += CheckControllerStateForGoblin;

        if(Pvr_ControllerManager.Instance.LengthAdaptiveRay)
        {           
            rayLine = transform.GetComponentInChildren<LineRenderer>(true).gameObject;
#if UNITY_2017_1_OR_NEWER
            rayLine.GetComponent<LineRenderer>().startWidth = 0.003f;
            rayLine.GetComponent<LineRenderer>().endWidth = 0.0015f;
#else
            rayLine.GetComponent<LineRenderer>().SetWidth(0.003f, 0.0015f);
#endif
        }
    }
    void OnDestroy()
    {
        Pvr_ControllerManager.PvrServiceStartSuccessEvent -= ServiceStartSuccess;
        Pvr_ControllerManager.SetControllerAbilityEvent -= CheckControllerStateOfAbility;
        Pvr_ControllerManager.ControllerStatusChangeEvent -= CheckControllerStateForGoblin;
    }

    private void ServiceStartSuccess()
    {
        mainHand = Controller.UPvr_GetMainHandNess();
        if (Variety == ControllerVariety.Controller0)
        {
            StartCoroutine(ShowAndHideRay(mainHand == 0 && Pvr_ControllerManager.controllerlink.controller0Connected));
            
        }
        if (Variety == ControllerVariety.Controller1)
        {
            StartCoroutine(ShowAndHideRay(mainHand == 1 && Pvr_ControllerManager.controllerlink.controller1Connected));
        }
    }

    private void CheckControllerStateForGoblin(string state)
    {
        if (Pvr_ControllerManager.controllerlink.controller0Connected)
        {
            moduleState = true;
            controller.transform.localScale = Vector3.one;
        }
        if (Variety == ControllerVariety.Controller0)
        {
            StartCoroutine(ShowAndHideRay(Convert.ToBoolean(Convert.ToInt16(state))));
        }
    }

    private void CheckControllerStateOfAbility(string data)
    {
        mainHand = Controller.UPvr_GetMainHandNess();
        if (Pvr_ControllerManager.controllerlink.controller0Connected ||
            Pvr_ControllerManager.controllerlink.controller1Connected)
        {
            moduleState = true;
            controller.transform.localScale = Vector3.one;
        }
        if (Variety == ControllerVariety.Controller0)
        {
            StartCoroutine(ShowAndHideRay(mainHand == 0 && Pvr_ControllerManager.controllerlink.controller0Connected));

        }
        if (Variety == ControllerVariety.Controller1)
        {
            StartCoroutine(ShowAndHideRay(mainHand == 1 && Pvr_ControllerManager.controllerlink.controller1Connected));
        }
    }
    
    private IEnumerator ShowAndHideRay(bool state)
    {
        yield return null;
        yield return null;
        if (moduleState)
        {
            dot.SetActive(state);
            rayLine.SetActive(state);
        }
    }

    public void ForceHideOrShow(bool state)
    {
        dot.SetActive(state);
        rayLine.SetActive(state);
        controller.transform.localScale = state ? Vector3.one : Vector3.zero;
        moduleState = state;
    }

    public void UpdateRay()
    {
        if (!Pvr_ControllerManager.Instance.LengthAdaptiveRay)
        {
            return;
        }
        bool isupdate = false;
        mainHand = Controller.UPvr_GetMainHandNess();
        if (Variety == ControllerVariety.Controller0)
        {
            if (mainHand == 0 && Pvr_ControllerManager.controllerlink.controller0Connected)
            {
                isupdate = true;
            }

        }
        if (Variety == ControllerVariety.Controller1)
        {
            if (mainHand == 1 && Pvr_ControllerManager.controllerlink.controller1Connected)
            {
                isupdate = true;
            }
        }

        if (isupdate && rayLine != null && rayLine.gameObject.activeSelf)
        {
            int type = Controller.UPvr_GetDeviceType();
            if (type == 1)
            {
                rayLine.GetComponent<LineRenderer>().SetPosition(0, transform.TransformPoint(0, 0, 0.058f));
            }
            else
            {
                rayLine.GetComponent<LineRenderer>().SetPosition(0, transform.TransformPoint(0, 0.009f, 0.055f));
            }
            rayLine.GetComponent<LineRenderer>().SetPosition(1, dot.transform.position);
        }
    }
}
