using UnityEngine;

public static class Haptic
{
    private class HapticFeedbackManager
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        private int HapticFeedbackConstantsKey;
        private AndroidJavaObject UnityPlayer;
#endif

        public HapticFeedbackManager()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            HapticFeedbackConstantsKey=new AndroidJavaClass("android.view.HapticFeedbackConstants").GetStatic<int>("VIRTUAL_KEY");
            UnityPlayer=new AndroidJavaClass ("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity").Get<AndroidJavaObject>("mUnityPlayer");
#endif
        }

        public bool Execute()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return UnityPlayer.Call<bool> ("performHapticFeedback",HapticFeedbackConstantsKey);
#endif
            return false;
        }
    }

    //Cache the Manager for performance
    private static HapticFeedbackManager mHapticFeedbackManager;

    public static bool HapticFeedback()
    {
        if (mHapticFeedbackManager == null)
        {
            mHapticFeedbackManager = new HapticFeedbackManager();
        }
        return mHapticFeedbackManager.Execute();
    }

    public static void Execute()
    {
#if UNITY_ANDROID
        HapticFeedback();
#elif UNITY_IOS
        IOSNative.StartHapticFeedback(HapticFeedbackTypes.HEAVY);
#endif
    }
}

//public static class Vibration
//{

//#if UNITY_ANDROID && !UNITY_EDITOR
//    public static AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
//    public static AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
//    public static AndroidJavaObject vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
//#else
//    public static AndroidJavaClass unityPlayer;
//    public static AndroidJavaObject currentActivity;
//    public static AndroidJavaObject vibrator;
//#endif
//    public static void Vibrate()
//    {
//        if (isAndroid())
//            vibrator.Call("vibrate");
//        else
//            Handheld.Vibrate();
//    }

//#if UNITY_ANDROID
//    public static void Vibrate(long milliseconds)
//    {
//        if (isAndroid())
//            vibrator.Call("vibrate", milliseconds);
//        else
//            Handheld.Vibrate();
//    }
//#elif UNITY_IPHONE
//    [DllImport("__Internal")]
//    public static extern void Vibrate(int _n);
//#endif

//    public static void Vibrate(long[] pattern, int repeat)
//    {
//        if (isAndroid())
//            vibrator.Call("vibrate", pattern, repeat);
//        else
//            Handheld.Vibrate();
//    }

//    public static bool HasVibrator()
//    {
//        return isAndroid();
//    }

//    public static void Cancel()
//    {
//        if (isAndroid())
//            vibrator.Call("cancel");
//    }

//    private static bool isAndroid()
//    {
//#if UNITY_ANDROID && !UNITY_EDITOR
//	return true;
//#else
//        return false;
//#endif
//    }
//}