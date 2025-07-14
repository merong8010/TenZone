using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[AddComponentMenu("UI/Effects/Advanced Gradient")]
public class UIGradient : BaseMeshEffect
{
    [Tooltip("그라데이션 색상 설정. 여러 색상을 지정할 수 있습니다.")]
    public Gradient gradient = new Gradient();

    [Tooltip("그라데이션 방향")]
    public GradientDirection direction = GradientDirection.Vertical;

    [Tooltip("수평 방향으로 메시를 얼마나 나눌지 결정합니다. 높을수록 부드럽습니다.")]
    [Range(1, 100)]
    public int horizontalSegments = 1;

    [Tooltip("수직 방향으로 메시를 얼마나 나눌지 결정합니다. 높을수록 부드럽습니다.")]
    [Range(1, 100)]
    public int verticalSegments = 1;

    public enum GradientDirection
    {
        Vertical,
        Horizontal,
        BottomToTop,
        RightToLeft
    }

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive())
        {
            return;
        }

        List<UIVertex> vertexList = new List<UIVertex>();
        vh.GetUIVertexStream(vertexList);

        int initialVertexCount = vertexList.Count;
        if (initialVertexCount == 0) return;

        // 경계 찾기
        float topY = vertexList[0].position.y;
        float bottomY = vertexList[0].position.y;
        float leftX = vertexList[0].position.x;
        float rightX = vertexList[0].position.x;

        for (int i = 1; i < initialVertexCount; i++)
        {
            float y = vertexList[i].position.y;
            if (y > topY) topY = y;
            else if (y < bottomY) bottomY = y;

            float x = vertexList[i].position.x;
            if (x > rightX) rightX = x;
            else if (x < leftX) leftX = x;
        }

        float uiElementHeight = topY - bottomY;
        float uiElementWidth = rightX - leftX;

        vh.Clear(); // 기존 메시를 지우고 새로 만듭니다.

        int horizontalSteps = Mathf.Max(1, horizontalSegments);
        int verticalSteps = Mathf.Max(1, verticalSegments);

        // 메시를 잘게 나누어 다시 생성
        for (int i = 0; i < horizontalSteps; i++)
        {
            float x0 = leftX + (uiElementWidth * i / horizontalSteps);
            float x1 = leftX + (uiElementWidth * (i + 1) / horizontalSteps);

            for (int j = 0; j < verticalSteps; j++)
            {
                float y0 = bottomY + (uiElementHeight * j / verticalSteps);
                float y1 = bottomY + (uiElementHeight * (j + 1) / verticalSteps);

                UIVertex[] quad = new UIVertex[4];
                quad[0] = CreateVertex(new Vector2(x0, y0), new Vector2(0, 0), uiElementWidth, uiElementHeight, leftX, bottomY); // Bottom-Left
                quad[1] = CreateVertex(new Vector2(x0, y1), new Vector2(0, 1), uiElementWidth, uiElementHeight, leftX, bottomY); // Top-Left
                quad[2] = CreateVertex(new Vector2(x1, y1), new Vector2(1, 1), uiElementWidth, uiElementHeight, leftX, bottomY); // Top-Right
                quad[3] = CreateVertex(new Vector2(x1, y0), new Vector2(1, 0), uiElementWidth, uiElementHeight, leftX, bottomY); // Bottom-Right

                vh.AddUIVertexQuad(quad);
            }
        }
    }

    private UIVertex CreateVertex(Vector2 pos, Vector2 uv, float width, float height, float leftX, float bottomY)
    {
        UIVertex vertex = new UIVertex();
        vertex.position = pos;
        vertex.uv0 = uv; // UV 좌표 설정도 필요할 수 있으므로 추가

        float normalizedPos = GetNormalizedPosition(pos, width, height, leftX, bottomY);
        vertex.color = gradient.Evaluate(normalizedPos);

        return vertex;
    }

    private float GetNormalizedPosition(Vector2 pos, float width, float height, float leftX, float bottomY)
    {
        switch (direction)
        {
            case GradientDirection.Vertical:
                return height > 0 ? (pos.y - bottomY) / height : 0f;
            case GradientDirection.Horizontal:
                return width > 0 ? (pos.x - leftX) / width : 0f;
            case GradientDirection.BottomToTop:
                return height > 0 ? 1 - ((pos.y - bottomY) / height) : 0f;
            case GradientDirection.RightToLeft:
                return width > 0 ? 1 - ((pos.x - leftX) / width) : 0f;
            default:
                return 0;
        }
    }
}