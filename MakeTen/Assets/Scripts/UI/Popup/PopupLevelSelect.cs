using UnityEngine;
using System.Linq;

public class PopupLevelSelect : Popup
{
    [SerializeField]
    private LevelList[] levelList;
    

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
            GameManager.Instance.GoScene(GameManager.Scene.Puzzle, currentLevel);
            Close();
        }
        
    }
}
