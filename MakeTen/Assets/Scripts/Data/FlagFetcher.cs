using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;

public class FlagFetcher : MonoBehaviour
{
    private const string apiUrl = "https://www.countryflags.io/{0}/flat/64.png";  // 국가 코드로 이미지 요청

   // public string countryCode = "US";  // 국가 코드 (예: "US" = 미국)

    void Start()
    {
        //StartCoroutine(DownloadFlag(countryCode));
    }

    IEnumerator DownloadFlag(string countryCode, Action<Sprite> callback)
    {
        string url = string.Format(apiUrl, countryCode);
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            flagsDic.Add(countryCode, SpriteFromTexture(texture));
            callback.Invoke(flagsDic[countryCode]);
            // 다운로드한 이미지를 UI에 적용
            //GetComponent<Renderer>().material.mainTexture = texture;
        }
        else
        {
            Debug.LogError("Flag download failed: " + request.error);
            callback.Invoke(null);
        }
    }

    private Dictionary<string, Sprite> flagsDic = new Dictionary<string, Sprite>();
    public void GetFlags(string countryCode, Action<Sprite> callback)
    {
        if(flagsDic.ContainsKey(countryCode))
        {
            callback.Invoke(flagsDic[countryCode]);
        }
        else
        {
            StartCoroutine(DownloadFlag(countryCode, callback));
        }
    }

    Sprite SpriteFromTexture(Texture2D texture)
    {
        // Texture2D의 크기를 사용하여 Sprite를 만듬
        Rect spriteRect = new Rect(0, 0, texture.width, texture.height);
        Vector2 pivot = new Vector2(0.5f, 0.5f);  // Sprite의 중심점을 (0.5, 0.5)로 설정
        return Sprite.Create(texture, spriteRect, pivot);
    }
}