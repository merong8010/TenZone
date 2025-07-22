using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;

public class PopupShop : Popup
{
    //[SerializeField]
    //private TabGroup tab;
    private bool isInit = false;

    //private int currentTabIdx;

    [SerializeField]
    private ShopCategoryList categoryList;
    [SerializeField]
    private ShopList shopList;

    private void Init()
    {
        if (isInit) return;
        isInit = true;

        //tab.Init(0, idx =>
        //{
        //    currentTabIdx = idx;
        //    Refresh();
        //});

        
    }

    public override void Open()
    {
        Init();
        base.Open();
    }

    public override void Refresh()
    {
        base.Refresh();

        //GameData.ShopCategory category = (GameData.ShopCategory)currentTabIdx;
        //shopList.UpdateList(DataManager.Instance.Get<GameData.Shop>().Where(x => x.category == category).ToArray());
        GameData.ShopCategory[] categories = (GameData.ShopCategory[])System.Enum.GetValues(typeof(GameData.ShopCategory));
        List<GameData.Shop>[] datas = new List<GameData.Shop>[categories.Length];
        for (int i = 0; i < categories.Length; i++) 
        {
            datas[i] = DataManager.Instance.Get<GameData.Shop>().Where(x => x.category == categories[i]).ToList();
        }
        categoryList.UpdateList(datas);
    }

}
