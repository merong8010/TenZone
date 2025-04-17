using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
//[ExecuteInEditMode]
public class SafeArea : MonoBehaviour
{
    [SerializeField]
    RectTransform Panel;

    public Action refreshAction;

    void Awake()
    {
        if (Panel == null) Panel = GetComponent<RectTransform>();
        //Refresh();
    }

    //private void OnEnable()
    //{
    //    Refresh();
    //}

    //private DeviceOrientation lastOrientation = DeviceOrientation.Unknown;
    //private void Update()
    //{
    //    if(lastOrientation != Util.GetDeviceOrientation())
    //    {
    //        Refresh();
    //        lastOrientation = Util.GetDeviceOrientation();
    //    }
    //}

    public void Refresh()
    {
        if(UnityEngine.Device.SystemInfo.deviceType == DeviceType.Handheld)
        {
            ApplySafeArea(Screen.safeArea, Util.GetScreenSize());
        }
        else
        {
            ApplySafeArea(new Rect(0, 0, Screen.width, Screen.height), Util.GetScreenSize());
        }

        refreshAction?.Invoke();
    }

    void ApplySafeArea(Rect r, Vector2Int size)
    {
        Debug.Log($"ApplySafeArea : {r} | {size}");
        Vector2 anchorMin = r.position;
        Vector2 anchorMax = r.position + r.size;

        anchorMin.x /= size.x;
        anchorMin.y /= size.y;
        anchorMax.x /= size.x;
        anchorMax.y /= size.y;
        Panel.anchorMin = anchorMin;
        Panel.anchorMax = anchorMax;
    }
}