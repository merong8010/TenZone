// GoodsDataAtlas.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// 프로젝트 어딘가에 GameData.GoodsType enum이 정의되어 있어야 합니다.
// namespace GameData { public enum GoodsType { None, Gold, Gem, ... } }

[CreateAssetMenu(fileName = "GoodsDataAtlas", menuName = "Game Data/Goods Data Atlas")]
public class GoodsDataAtlas : ScriptableObject
{
    [System.Serializable]
    public struct GoodsSprite
    {
        public GameData.GoodsType type;
        public Sprite sprite;
    }

    [SerializeField]
    private List<GoodsSprite> goodsSprites;

    private Dictionary<GameData.GoodsType, Sprite> _spriteDictionary;

    // 딕셔너리는 필요할 때 한 번만 초기화됩니다.
    private Dictionary<GameData.GoodsType, Sprite> SpriteDictionary
    {
        get
        {
            if (_spriteDictionary == null)
            {
                _spriteDictionary = new Dictionary<GameData.GoodsType, Sprite>();
                foreach (var item in goodsSprites)
                {
                    if (!_spriteDictionary.ContainsKey(item.type))
                    {
                        _spriteDictionary.Add(item.type, item.sprite);
                    }
                }
            }
            return _spriteDictionary;
        }
    }

    public Sprite GetSprite(GameData.GoodsType type)
    {
        if (SpriteDictionary.TryGetValue(type, out Sprite sprite))
        {
            return sprite;
        }
        Debug.LogWarning($"GoodsDataAtlas에서 '{type}' 타입의 스프라이트를 찾을 수 없습니다.");
        return null;
    }
}