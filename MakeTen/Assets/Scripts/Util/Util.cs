using System.Collections.Generic;
using UnityEngine;

public static class Util
{
    //private static float mean = 5f;
    //private static float stdDev = 3f;

    //public static int GetRandomTen(this Random rand)
    //{
    //    double u1 = 1.0 - rand.NextDouble();
    //    double u2 = 1.0 - rand.NextDouble();
    //    double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
    //    double normalValue = mean + stdDev * randStdNormal;

    //    return (int)Math.Round(normalValue);
    //}

    public static int GenerateGaussianRandom(float mean, float stdDev)
    {
        float u1 = 1.0f - UnityEngine.Random.value; // 0~1 사이 난수 (0 포함 X)
        float u2 = 1.0f - UnityEngine.Random.value;

        float randStdNormal = UnityEngine.Mathf.Sqrt(-2.0f * UnityEngine.Mathf.Log(u1)) * UnityEngine.Mathf.Sin(2.0f * UnityEngine.Mathf.PI * u2);
        float randNormal = mean + stdDev * randStdNormal;

        // 1~9 범위를 벗어나지 않도록 보정
        return UnityEngine.Mathf.Clamp(UnityEngine.Mathf.RoundToInt(randNormal), 1, 9);
    }
    public static Vector2Int GetScreenSize()
    {
        if (UnityEngine.Device.SystemInfo.deviceType == DeviceType.Handheld)
        {
            return new Vector2Int(Screen.currentResolution.width, Screen.currentResolution.height);
        }
        else
        {
            return new Vector2Int(Screen.width, Screen.height);
        }
    }

    public static DeviceOrientation GetDeviceOrientation()
    {
#if UNITY_EDITOR
        // 에디터에서는 가로/세로 비율로 판단
        return Screen.width > Screen.height ? DeviceOrientation.Landscape : DeviceOrientation.Portrait;
#else
        // 실제 디바이스에서는 Screen.orientation 사용
        switch (Screen.orientation)
        {
            case ScreenOrientation.Portrait:
            case ScreenOrientation.PortraitUpsideDown:
                return DeviceOrientation.Portrait;

            case ScreenOrientation.LandscapeLeft:
            case ScreenOrientation.LandscapeRight:
                return DeviceOrientation.Landscape;

            default:
                // 미정 방향은 화면 비율로 판단
                return Screen.width > Screen.height ? DeviceOrientation.Landscape : DeviceOrientation.Portrait;
        }
#endif
    }
}
public enum DeviceOrientation
{
    Unknown,
    Portrait,
    Landscape
}

public static class Yielders
{

    static Dictionary<float, WaitForSeconds> _timeInterval = new Dictionary<float, WaitForSeconds>(100);

    static WaitForEndOfFrame _endOfFrame = new WaitForEndOfFrame();
    public static WaitForEndOfFrame EndOfFrame
    {
        get { return _endOfFrame; }
    }

    static WaitForFixedUpdate _fixedUpdate = new WaitForFixedUpdate();
    public static WaitForFixedUpdate FixedUpdate
    {
        get { return _fixedUpdate; }
    }

    public static WaitForSeconds Get(float seconds)
    {
        if (!_timeInterval.ContainsKey(seconds))
            _timeInterval.Add(seconds, new WaitForSeconds(seconds));
        return _timeInterval[seconds];
    }
}
