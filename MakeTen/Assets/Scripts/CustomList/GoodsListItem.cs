using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;

public class GoodsListItem : ListItem<GoodsList.Data>
{
    [SerializeField]
    private Image image;
    [SerializeField]
    private Text amount;

    public override void SetData(GoodsList.Data data)
    {
        base.SetData(data);

        image.sprite = Resources.Load<SpriteAtlas>("Graphics/Goods").GetSprite(data.type.ToString());
    }
}
