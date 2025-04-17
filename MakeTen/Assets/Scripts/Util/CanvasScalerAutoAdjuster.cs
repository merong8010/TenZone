using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasScaler))]
public class CanvasScalerAutoAdjuster : MonoBehaviour
{
    //[Tooltip("너비와 높이 중 어느 쪽을 더 중요하게 반영할지 자동 조절합니다.")]
    //[Range(0f, 1f)]
    //public float matchOnWide = 0f;   // 가로가 긴 화면일 때 match 값 (Width 중심)

    //[Range(0f, 1f)]
    //public float matchOnTall = 1f;   // 세로가 긴 화면일 때 match 값 (Height 중심)

    //private void Start()
    //{
    //    CanvasScaler scaler = GetComponent<CanvasScaler>();

    //    if (scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize)
    //    {
    //        Debug.LogWarning("CanvasScaler의 UI Scale Mode를 'Scale With Screen Size'로 설정해야 합니다.");
    //        return;
    //    }

    //    float screenRatio = (float)Screen.width / Screen.height;

    //    // 기준 비율 (예: 1080x1920 = 약 0.56)
    //    float referenceRatio = scaler.referenceResolution.x / scaler.referenceResolution.y;

    //    // 비율을 기준으로 자동 보간
    //    float t = Mathf.InverseLerp(referenceRatio, 1.0f, screenRatio);
    //    scaler.matchWidthOrHeight = Mathf.Lerp(matchOnTall, matchOnWide, t);

    //    Debug.Log($"CanvasScaler match 자동조정: {scaler.matchWidthOrHeight:F2}");
    //}

    private void Start()
    {
        if (safeAreas == null) safeAreas = GetComponentsInChildren<SafeArea>();
        if (scaler == null) scaler = GetComponent<CanvasScaler>();
    }
    private DeviceOrientation lastOrientation = DeviceOrientation.Unknown;
    private void Update()
    {
        if (lastOrientation != Util.GetDeviceOrientation())
        {
            Refresh();
            lastOrientation = Util.GetDeviceOrientation();
        }
    }
    private CanvasScaler scaler;
    private SafeArea[] safeAreas;
    private float width = 1920f;
    private float height = 1080f;
    public void Refresh()
    {
        if (Util.GetDeviceOrientation() == DeviceOrientation.Portrait)
        {
            scaler.referenceResolution = new Vector2(height, width);
        }
        else
        {
            scaler.referenceResolution = new Vector2(width, height);
        }
        for(int i = 0; i < safeAreas.Length; i++)
        {
            safeAreas[i].Refresh();
        }

        //if (UnityEngine.Device.SystemInfo.deviceType == DeviceType.Handheld)
        //{
        //    ApplySafeArea(Screen.safeArea, Util.GetScreenSize());
        //}
        //else
        //{
        //    ApplySafeArea(new Rect(0, 0, Screen.width, Screen.height), Util.GetScreenSize());
        //}

        //refreshAction?.Invoke();
    }
}
