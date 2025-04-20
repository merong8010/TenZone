using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class TextManager
{
    public static Dictionary<string, string> dic = new Dictionary<string, string>();

    public static void LoadDatas(string countryCode, GameData.Language[] data)
    {
        string[] keys = data.Select(x => x.key).ToArray();
        string[] vals = new string[keys.Length];
        switch (countryCode)
        {
            case "KR":
                vals = data.Select(x => x.KR).ToArray();
                break;
            case "JP":
                vals = data.Select(x => x.JP).ToArray();
                break;
            case "TW":
                vals = data.Select(x => x.TW).ToArray();
                break;
            default:
                vals = data.Select(x => x.US).ToArray();
                break;
        }

        dic.Clear();
        for (int i = 0; i < keys.Length; i++)
        {
            dic.Add(keys[i], vals[i]);
        }
    }

    public static string Get(string key)
    {
        //Debug.Log(dic.ContainsKey(key))
        if (!dic.ContainsKey(key) || string.IsNullOrEmpty(dic[key])) return key;
        return dic[key];
    }
}
