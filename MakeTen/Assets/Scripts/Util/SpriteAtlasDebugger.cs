using UnityEngine;
using UnityEngine.U2D;

public class SpriteAtlasDebugger : MonoBehaviour
{
    public SpriteAtlas atlas;

    void Start()
    {
        if (atlas == null)
        {
            Debug.LogWarning("SpriteAtlas가 할당되지 않았습니다.");
            return;
        }

        // Sprite 배열 생성
        Sprite[] sprites = new Sprite[atlas.spriteCount];
        atlas.GetSprites(sprites);

        // 이름 출력
        foreach (var sprite in sprites)
        {
            Debug.Log("Sprite name: " + sprite.name);
        }
    }
}
