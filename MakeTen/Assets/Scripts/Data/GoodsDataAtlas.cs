// GoodsDataAtlas.cs
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// ������Ʈ ��򰡿� GameData.GoodsType enum�� ���ǵǾ� �־�� �մϴ�.
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

    // ��ųʸ��� �ʿ��� �� �� ���� �ʱ�ȭ�˴ϴ�.
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
        Debug.LogWarning($"GoodsDataAtlas���� '{type}' Ÿ���� ��������Ʈ�� ã�� �� �����ϴ�.");
        return null;
    }
}