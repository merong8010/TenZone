using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;


using System.Text;
using UnityEditor.Android;

#if UNITY_IOS
using UnityEditor.Callbacks;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.iOS.Xcode;

#endif

//using UnityEditor.iOS.Xcode.Custom;
//using UnityEditor.iOS.Xcode.Custom.Extensions;
//UNITY_IOS




public static class Unity3dBuilder
{
    static string TARGET_DIR = "_Build";
    static string APP_NAME
    {
        get
        {
            return Application.productName;
        }
    }

    private static string strKeyStorePath = "maketen.keystore";
    private static string strKeyStorePWD = "Wjdwlsals80!";
    private static string strKeyAliasName = "maketen";
    private static string strKeyAliasPWD = "Wjdwlsals80!";

    private static string[] BuildSelectedScene = { "" };

    private static string strTargetOS = "13.0";
    private static string strDeveloperTeamID = "J462Z4633P";

    [MenuItem("Custom/CI/Build Development Android")]
    public static void PerformAndroidDevelopmentBuild()
    {
        // Note: 글로벌 오픈 시 수정
        string strKeyStoreName = System.IO.Directory.GetCurrentDirectory() + "/" + strKeyStorePath;
        PlayerSettings.Android.keystoreName = strKeyStoreName;
        PlayerSettings.Android.keystorePass = strKeyStorePWD;
        PlayerSettings.Android.keyaliasName = strKeyAliasName;
        PlayerSettings.Android.keyaliasPass = strKeyAliasPWD;

        //PlayerSettings.bundleVersion = Project.version;
        //PlayerSettings.Android.bundleVersionCode = Project.versionCode;

        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, "DOTWEEN;DEV;ENABLE_UNITY_SERVICES_CORE_VERBOSE_LOGGING");

        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
        EditorUserBuildSettings.buildAppBundle = false;

        string BUILD_TARGET_PATH = System.IO.Directory.GetCurrentDirectory() + "/" + TARGET_DIR + "/Android/";
        Directory.CreateDirectory(BUILD_TARGET_PATH);

        string target_filename = APP_NAME + ".apk";
        GenericBuild(BUILD_TARGET_PATH + target_filename, BuildTargetGroup.Android, BuildTarget.Android, BuildOptions.None);
    }

    [MenuItem("Custom/CI/Build Development Android And Run")]
    public static void PerformAndroidDevelopmentBuildAndRun()
    {
        // Note: 글로벌 오픈 시 수정
        string strKeyStoreName = System.IO.Directory.GetCurrentDirectory() + "/" + strKeyStorePath;
        PlayerSettings.Android.keystoreName = strKeyStoreName;
        PlayerSettings.Android.keystorePass = strKeyStorePWD;
        PlayerSettings.Android.keyaliasName = strKeyAliasName;
        PlayerSettings.Android.keyaliasPass = strKeyAliasPWD;

        //PlayerSettings.bundleVersion = Project.version;
        //PlayerSettings.Android.bundleVersionCode = Project.versionCode;

        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, "DOTWEEN;DEV;ENABLE_UNITY_SERVICES_CORE_VERBOSE_LOGGING");

        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
        EditorUserBuildSettings.buildAppBundle = false;

        string BUILD_TARGET_PATH = System.IO.Directory.GetCurrentDirectory() + "/" + TARGET_DIR + "/Android/";
        Directory.CreateDirectory(BUILD_TARGET_PATH);

        string target_filename = APP_NAME + ".apk";
        GenericBuild(BUILD_TARGET_PATH + target_filename, BuildTargetGroup.Android, BuildTarget.Android, BuildOptions.AutoRunPlayer);
    }

    [MenuItem("Custom/CI/Build Release Android")]
    public static void PerformAndroidReleaseBuild()
    {
        // Note: 글로벌 오픈 시 수정
        string strKeyStoreName = System.IO.Directory.GetCurrentDirectory() + "/" + strKeyStorePath;
        PlayerSettings.Android.keystoreName = strKeyStoreName;
        PlayerSettings.Android.keystorePass = strKeyStorePWD;
        PlayerSettings.Android.keyaliasName = strKeyAliasName;
        PlayerSettings.Android.keyaliasPass = strKeyAliasPWD;

        //PlayerSettings.bundleVersion = Project.version;
        //PlayerSettings.Android.bundleVersionCode = Project.versionCode;

        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, "DOTWEEN;RELEASE");

        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
        EditorUserBuildSettings.buildAppBundle = true;

        string BUILD_TARGET_PATH = System.IO.Directory.GetCurrentDirectory() + "/" + TARGET_DIR + "/Android/";
        Directory.CreateDirectory(BUILD_TARGET_PATH);

        string target_filename = APP_NAME + ".aab";
        GenericBuild(BUILD_TARGET_PATH + target_filename, BuildTargetGroup.Android, BuildTarget.Android, BuildOptions.None);
    }

    [MenuItem("Custom/CI/Build Development iOS")]
    static void PerformiOSDevelopmentBuild()
    {
        BuildOptions opt = BuildOptions.None;

        PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;
        PlayerSettings.iOS.targetOSVersionString = strTargetOS;
        PlayerSettings.iOS.appleEnableAutomaticSigning = true;
        PlayerSettings.iOS.appleDeveloperTeamID = strDeveloperTeamID;
        PlayerSettings.statusBarHidden = true;
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, "DOTWEEN;DEV");

        //PlayerSettings.bundleVersion = Project.version;
        //PlayerSettings.iOS.buildNumber = Project.versionCode.ToString();

        char sep = Path.DirectorySeparatorChar;
        string buildDirectory = Path.GetFullPath(".") + sep + TARGET_DIR;
        Directory.CreateDirectory(buildDirectory);

        string BUILD_TARGET_PATH = buildDirectory + "/iOS";
        Directory.CreateDirectory(BUILD_TARGET_PATH);

        GenericBuild(BUILD_TARGET_PATH, BuildTargetGroup.iOS, BuildTarget.iOS, opt);
    }


    [MenuItem("Custom/CI/Build Release iOS")]
    public static void PerformiOSReleaseBuild()
    {
        BuildOptions opt = BuildOptions.None;

        PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;
        PlayerSettings.iOS.targetOSVersionString = strTargetOS;
        PlayerSettings.iOS.appleEnableAutomaticSigning = true;
        PlayerSettings.iOS.appleDeveloperTeamID = strDeveloperTeamID;
        PlayerSettings.statusBarHidden = true;
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, "DOTWEEN;RELEASE");

        //PlayerSettings.bundleVersion = Project.version;
        //PlayerSettings.iOS.buildNumber = Project.versionCode.ToString();

        char sep = Path.DirectorySeparatorChar;
        string buildDirectory = Path.GetFullPath(".") + sep + TARGET_DIR;
        Directory.CreateDirectory(buildDirectory);

        string BUILD_TARGET_PATH = buildDirectory + sep + "iOS";
        Directory.CreateDirectory(BUILD_TARGET_PATH);

        GenericBuild(BUILD_TARGET_PATH, BuildTargetGroup.iOS, BuildTarget.iOS, opt);
    }

    static void GenericBuild(string target_filename, BuildTargetGroup build_group, BuildTarget build_target, BuildOptions build_options)
    {
        var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        EditorUserBuildSettings.SwitchActiveBuildTarget(build_group, build_target);

#if UNITY_2018_2_OR_NEWER
        //UnityEditor.Build.Reporting.BuildReport report = BuildPipeline.BuildPlayer(scenes, target_filename, build_target, build_options);
        UnityEditor.Build.Reporting.BuildReport report = BuildPipeline.BuildPlayer(scenes, target_filename, build_target, build_options);
        if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            throw new Exception("BuildPlayer failure: " + report.summary.result);
        }
#else
        string res = BuildPipeline.BuildPlayer(sceneList.ToArray(), target_filename, build_target, build_options);
        if (res.Length > 0)
        {
            throw new Exception("BuildPlayer failure: " + res);
        }
#endif 
    }
}



#if UNITY_IOS
public class AddFakeUploadTokenPostprocessor : IPostprocessBuildWithReport
{
    public int callbackOrder => 100;

    public void OnPostprocessBuild(BuildReport report)
    {
//        var pathToBuiltProject = report.summary.outputPath;
//        var target = report.summary.platform;
//        if (target != BuildTarget.iOS)
//        {
//            return;
//        }

//        //Debug.LogFormat("Postprocessing build at \"{0}\" for target {1}", pathToBuiltProject, target);
//        PBXProject project = new PBXProject();
//        string pbxFilename = pathToBuiltProject + "/Unity-iPhone.xcodeproj/project.pbxproj";
//        project.ReadFromFile(pbxFilename);


//        //Undefined symbol: __swift_FORCE_LOAD_$_swiftCompatibilityDynamicReplacements 관련 xcode 에러 해결책
//        // Update the Library Search Paths of the whole Xcode project
//        project.AddBuildProperty(project.ProjectGuid(), "LIBRARY_SEARCH_PATHS", "$(SDKROOT)/usr/lib/swift");

//        project.SetBuildProperty(project.GetUnityFrameworkTargetGuid(), "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "NO");


//        ////추가
//        //project.AddBuildProperty(project.ProjectGuid(), "LIBRARY_SEARCH_PATHS", "$(TOOLCHAIN_DIR)/usr/lib/swift/$(PLATFORM_NAME)");
//        ////추가
//        //project.AddBuildProperty(project.ProjectGuid(), "LIBRARY_SEARCH_PATHS", "$(TOOLCHAIN_DIR)/usr/lib/swift-5.2/$(PLATFORM_NAME)");

//        // Get the UnityFramework target and exclude the unwanted architectures
//        //var unityFrameworkGuid = project.TargetGuidByName("UnityFramework");
//        //project.SetBuildProperty(unityFrameworkGuid, "EXCLUDED_ARCHS", "i386");
//        //project.AddBuildProperty(unityFrameworkGuid, "EXCLUDED_ARCHS", "x86_64");




//#if UNITY_2019_3_OR_NEWER
//        string targetGUID = project.GetUnityMainTargetGuid();
//#else
//        string targetName = PBXProject.GetUnityTargetName();
//        string targetGUID = project.TargetGuidByName(targetName);
//#endif

//        var token = project.GetBuildPropertyForAnyConfig(targetGUID, "USYM_UPLOAD_AUTH_TOKEN");
//        if (string.IsNullOrEmpty(token))
//        {
//            token = "FakeToken";
//        }

//        string targetGUID2 = project.TargetGuidByName("UnityFramework");
//        project.SetBuildProperty(targetGUID, "USYM_UPLOAD_AUTH_TOKEN", token);
//        project.SetBuildProperty(targetGUID2, "USYM_UPLOAD_AUTH_TOKEN", token);
//        project.SetBuildProperty(project.ProjectGuid(), "USYM_UPLOAD_AUTH_TOKEN", token);

//        project.WriteToFile(pbxFilename);
    }

#if UNITY_IOS
    [PostProcessBuildAttribute]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {

        if (target == BuildTarget.iOS)
        {
            PBXProject project = new PBXProject();
            string pbxFilename = pathToBuiltProject + "/Unity-iPhone.xcodeproj/project.pbxproj";
            project.ReadFromFile(pbxFilename);
            project.SetBuildProperty(project.GetUnityFrameworkTargetGuid(), "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "NO");
            project.WriteToFile(pbxFilename);


            string plistPath = pathToBuiltProject + "/Info.plist";
            PlistDocument plist = new PlistDocument();
            plist.ReadFromString(File.ReadAllText(plistPath));
            PlistElementDict rootDict = plist.root;
            rootDict.SetString("NSAdvertisingAttributionReportEndpoint", "https://appsflyer-skadnetwork.com/");
            rootDict.SetString("NSUserTrackingUsageDescription", "This identifier will be used to deliver personalized ads to you.");
            rootDict.SetBoolean("ITSAppUsesNonExemptEncryption", false);

            //Todo MaxPostProcessBuildiOS 클래스에 ("ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES") 검색하면 원본 외에 내가 직접 추가한 코드 한 줄 있음.

            //project.SetBuildProperty(project.GetUnityFrameworkTargetGuid(), "ALWAYS_EMBED_SWIFT_STANDARD_LIBRARIES", "NO");


            /*** To add more keys :
            ** rootDict.SetString("<your key>", "<your value>");
            ***/

            File.WriteAllText(plistPath, plist.WriteToString());

            Debug.Log("Info.plist updated with NSAdvertisingAttributionReportEndpoint");
        }
    }


    //public static void OnProstProcessBuildIOS(string pathToBuiltProject)
    //{
    //    //This is the default path to the default pbxproj file. Yours might be different
    //    string projectPath = "/Unity-iPhone.xcodeproj/project.pbxproj";
    //    //Default target name. Yours might be different
    //    string targetName = "Unity-iPhone";
    //    //Set the entitlements file name to what you want but make sure it has this extension
    //    string entitlementsFileName = "my_app.entitlements";

    //    var entitlements = new ProjectCapabilityManager(pathToBuiltProject + projectPath, entitlementsFileName, targetName);
    //    //entitlements.AddAssociatedDomains(new string[] { "applinks:myurl.com" });
    //    entitlements.AddAssociatedDomains(new string[] { "applinks:hunteridlerpg.onelink.me/RDtu?af_xp=app&pid=Deeplink&c=test&is_retargeting=true&af_reengagement_window=61d&deep_link_value=retargeting_open" });
    //    //Apply
    //    entitlements.WriteToFile();
    //}


#endif
}
#endif