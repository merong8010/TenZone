using UnityEngine;
using Unity.VectorGraphics;
using System.IO;

public class SVGToJPGConverter : MonoBehaviour
{
    public string outputFolder = "ExportedSVGs";
    public int textureWidth = 320;
    public int textureHeight = 240;

    void Start()
    {
        ConvertAllSVGsInResources();
    }

    void ConvertAllSVGsInResources()
    {
        // Resources 폴더에서 모든 SVG(TextAsset) 불러오기
        TextAsset[] svgFiles = Resources.LoadAll<TextAsset>("SVG");

        if (svgFiles.Length == 0)
        {
            Debug.LogWarning("Resources 폴더에 SVG 파일이 없습니다!");
            return;
        }

        // 저장 폴더 생성
        string exportPath = Path.Combine(Application.dataPath, outputFolder);
        if (!Directory.Exists(exportPath))
        {
            Directory.CreateDirectory(exportPath);
        }

        foreach (TextAsset svgFile in svgFiles)
        {
            Debug.Log("변환 중: " + svgFile.name);
            ConvertSVGToJPG(svgFile, exportPath);
        }

        Debug.Log("SVG 일괄 변환이 완료되었습니다! 경로: " + exportPath);
    }

    void ConvertSVGToJPG(TextAsset svgFile, string exportPath)
    {
        // SVG 데이터 읽기
        var sceneInfo = SVGParser.ImportSVG(new StringReader(svgFile.text));

        // 렌더링 설정
        var tessOptions = new VectorUtils.TessellationOptions()
        {
            StepDistance = 0.1f,
            MaxCordDeviation = 0.5f,
            MaxTanAngleDeviation = 0.1f,
            SamplingStepSize = 0.01f
        };

        var geometry = VectorUtils.TessellateScene(sceneInfo.Scene, tessOptions);
        var sprite = VectorUtils.BuildSprite(geometry, 100.0f, VectorUtils.Alignment.Center, Vector2.zero, 128, true);

        var texture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
        var renderTexture = new RenderTexture(textureWidth, textureHeight, 24);
        RenderTexture.active = renderTexture;

        var tempGO = new GameObject("TempSVGRenderer");
        var spriteRenderer = tempGO.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;

        var cameraGO = new GameObject("TempCamera");
        var camera = cameraGO.AddComponent<Camera>();
        camera.targetTexture = renderTexture;
        camera.orthographic = true;
        camera.orthographicSize = 5;
        camera.clearFlags = CameraClearFlags.Color;
        camera.backgroundColor = Color.white;

        camera.Render();

        texture.ReadPixels(new Rect(0, 0, textureWidth, textureHeight), 0, 0);
        texture.Apply();

        byte[] bytes = texture.EncodeToJPG(90);
        string outputFilePath = Path.Combine(exportPath, svgFile.name + ".jpg");
        File.WriteAllBytes(outputFilePath, bytes);

        // 임시 객체 정리
        DestroyImmediate(tempGO);
        DestroyImmediate(cameraGO);
        renderTexture.Release();
        RenderTexture.active = null;

        Debug.Log("저장 완료: " + outputFilePath);
    }
}
