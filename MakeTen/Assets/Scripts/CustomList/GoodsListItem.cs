using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;

public class GoodsListItem : ListItem<GoodsList.Data>
{
    [SerializeField]
    private GoodsDisplay goods;
    //[SerializeField]
    //private Image image;
    ////[SerializeField]
    ////private Text type;
    //[SerializeField]
    //private TMPro.TextMeshProUGUI amount;

    public override void SetData(GoodsList.Data data)
    {
        base.SetData(data);
        goods.SetStaticValue(data.type, data.amount);
        //image.sprite = Resources.Load<SpriteAtlas>("Graphics/Goods").GetSprite(data.type.ToString());
        //image.SetNativeSize();
        ////type.text = data.type.ToString();
        //amount.text = data.amount.ToString("n0");
    }
}
