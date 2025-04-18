using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasScaler))]
public class CanvasScalerAutoAdjuster : MonoBehaviour
{
    [SerializeField]
    private CanvasScaler scaler;
    [SerializeField]
    private SafeArea[] safeAreas;
    [SerializeField]
    private Vector2Int refSize = new Vector2Int(1920, 1080);

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
    
    public void Refresh()
    {
        if (Util.GetDeviceOrientation() == DeviceOrientation.Portrait)
        {
            scaler.referenceResolution = new Vector2(refSize.y, refSize.x);
        }
        else
        {
            scaler.referenceResolution = new Vector2(refSize.x, refSize.y);
        }

        Vector2Int screenSize = Util.GetScreenSize();
        float screenRatio = (float)screenSize.x / screenSize.y;

        // 기준 비율 (예: 1080x1920 = 약 0.56)
        float referenceRatio = scaler.referenceResolution.x / scaler.referenceResolution.y;

        // 비율을 기준으로 자동 보간
        //float t = Mathf.InverseLerp(referenceRatio, 1.0f, screenRatio);
        //scaler.matchWidthOrHeight = Mathf.Lerp(1f, 0f, t);
        scaler.matchWidthOrHeight = screenRatio > referenceRatio ? 1f : 0f;

        for (int i = 0; i < safeAreas.Length; i++)
        {
            safeAreas[i].Refresh();
        }
    }
}
