using UnityEngine;
using System.Linq;

public class PopupLevelSelect : Popup
{
    [SerializeField]
    private LevelList levelList;


    public override void Open()
    {
        int lastLevelIdx = PlayerPrefs.GetInt("LastLevel", 0);
        base.Open();
        Initialize();
        levelList.UpdateList(DataManager.Instance.Get<GameData.GameLevel>().ToList());
        currentLevel = levelList.GetDatas()[lastLevelIdx];
        levelList.Focus(lastLevelIdx);
    }

    private bool isInit = false;
    private GameData.GameLevel currentLevel;
    private void Initialize()
    {
        if (isInit) return;
        isInit = true;

        levelList.SetEvent(ClickItem);
    }

    private void ClickItem(GameData.GameLevel data)
    {
        currentLevel = data;
    }

    public void ClickStart()
    {
        if (currentLevel == null)
        {
            UIManager.Instance.Message.Show(Message.Type.Simple, "Please Select Level");
            return;
        }
        PlayerPrefs.SetInt("LastLevel", levelList.GetDatas().IndexOf(currentLevel));
        if (DataManager.Instance.userData.UseHeart())
        {
            GameManager.Instance.GoScene(GameManager.Scene.Puzzle, currentLevel);
            Close();
        }
        
    }
}
