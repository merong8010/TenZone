using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class PopupLevelSelect : Popup
{
    [SerializeField]
    private LevelList[] levelList;
    [SerializeField]
    private Toggle[] bonus10Seconds;

    public override void Open()
    {
        int lastLevelIdx = PlayerPrefs.GetInt("LastLevel", 0);
        base.Open();
        Initialize();
        for(int i = 0; i < levelList.Length; i++)
        {
            levelList[i].UpdateList(DataManager.Instance.Get<GameData.GameLevel>().ToList());
            levelList[i].Focus(lastLevelIdx);
        }
        
        currentLevel = levelList.FirstOrDefault().GetDatas()[lastLevelIdx];

        for(int i = 0; i < bonus10Seconds.Length; i++)
        {
            if (bonus10Seconds[i].isOn && !DataManager.Instance.userData.Has(GameData.GoodsType.Time_10s, 1))
            {
                bonus10Seconds[i].isOn = false;
            }
        }
        
    }

    private void Bonus10SecondsValueChanged(bool on)
    {
        if(on && !DataManager.Instance.userData.Has(GameData.GoodsType.Time_10s, 1))
        {
            for (int i = 0; i < bonus10Seconds.Length; i++)
            {
                bonus10Seconds[i].isOn = false;
            }
        }
    }

    private bool isInit = false;
    private GameData.GameLevel currentLevel;
    private void Initialize()
    {
        if (isInit) return;
        isInit = true;
        for (int i = 0; i < levelList.Length; i++)
        {
            levelList[i].SetEvent(ClickItem);
        }
        for (int i = 0; i < bonus10Seconds.Length; i++)
        {
            bonus10Seconds[i].onValueChanged.AddListener(Bonus10SecondsValueChanged);
        }
    }

    private void ClickItem(GameData.GameLevel data)
    {
        currentLevel = data;
        int levelIdx = levelList.FirstOrDefault().GetDatas().IndexOf(currentLevel);
        for (int i = 0; i < levelList.Length; i++)
        {
            levelList[i].Focus(levelIdx);
        }
    }

    public void ClickStart()
    {
        if (currentLevel == null)
        {
            UIManager.Instance.Message.Show(Message.Type.Simple, "Please Select Level");
            return;
        }
        PlayerPrefs.SetInt("LastLevel", levelList.FirstOrDefault().GetDatas().IndexOf(currentLevel));
        if (DataManager.Instance.userData.UseHeart())
        {
            GameManager.Instance.GoScene(GameManager.Scene.Puzzle, currentLevel, bonus10Seconds.First().isOn);
            Close();
        }
        
    }
    
}
