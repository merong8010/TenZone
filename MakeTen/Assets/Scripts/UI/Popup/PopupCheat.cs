using UnityEngine;
using System.Collections;
using TMPro;
using GameData;
using System;
using System.Linq;

public class PopupCheat : Popup
{
    [SerializeField]
    private TMP_Dropdown itemTypes;
    [SerializeField]
    private TMP_InputField itemAmount;

    private bool isInit = false;
    private GameData.GoodsType currentGoodsType;

    public override void Open()
    {
        base.Open();
        Init();
    }
    private void Init()
    {
        if (isInit) return;
        isInit = true;
        var enumNames = Enum.GetNames(typeof(GameData.GoodsType)).ToList();
        itemTypes.ClearOptions();
        itemTypes.AddOptions(enumNames);

        // 선택 시 이벤트 등록
        itemTypes.onValueChanged.AddListener(OnGoodsTypeChanged);
    }
    private void OnGoodsTypeChanged(int idx)
    {
        currentGoodsType = (GameData.GoodsType)idx;
    }

    public void ChargeGoods()
    {
        if (currentGoodsType == GameData.GoodsType.None) return;
        if (!int.TryParse(itemAmount.text, out int amount)) return;

        DataManager.Instance.userData.Charge(currentGoodsType, amount);
    }
}
