using UnityEngine;
using UnityEngine.UI;

public class HUD : Singleton<HUD>
{
    [SerializeField]
    private Text heartCount;
    [SerializeField]
    private Text heartChargeRemainTime;

    public void UpdateHeart()
    {
        heartCount.text = DataManager.Instance.userData.Heart.ToString();


    }
}
