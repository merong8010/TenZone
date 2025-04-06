using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;

public class FlagFetcher : MonoBehaviour
{
    private const string apiUrl = "https://flagcdn.com/w320/{0}.png";  // ���� �ڵ�� �̹��� ��û

   // public string countryCode = "US";  // ���� �ڵ� (��: "US" = �̱�)

    void Start()
    {
        //StartCoroutine(DownloadFlag(countryCode));
    }

    public void GetFlag(string countryCode, Action<Sprite> callback)
    {
        StartCoroutine(DownloadFlag(countryCode, callback));
    }

    IEnumerator DownloadFlag(string countryCode, Action<Sprite> callback)
    {
        string url = string.Format(apiUrl, countryCode.ToLower());
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();
        Debug.Log("DownloadFlag : " + request.result);
        if (request.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            callback.Invoke(SpriteFromTexture(texture));
            // �ٿ�ε��� �̹����� UI�� ����
            //GetComponent<Renderer>().material.mainTexture = texture;
        }
        else
        {
            Debug.LogError("Flag download failed: " + request.error);
            callback.Invoke(null);
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