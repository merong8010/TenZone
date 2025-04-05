using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;

public class FlagFetcher : MonoBehaviour
{
    private const string apiUrl = "https://www.countryflags.io/{0}/flat/64.png";  // ���� �ڵ�� �̹��� ��û

   // public string countryCode = "US";  // ���� �ڵ� (��: "US" = �̱�)

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
            // �ٿ�ε��� �̹����� UI�� ����
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
        // Texture2D�� ũ�⸦ ����Ͽ� Sprite�� ����
        Rect spriteRect = new Rect(0, 0, texture.width, texture.height);
        Vector2 pivot = new Vector2(0.5f, 0.5f);  // Sprite�� �߽����� (0.5, 0.5)�� ����
        return Sprite.Create(texture, spriteRect, pivot);
    }
}