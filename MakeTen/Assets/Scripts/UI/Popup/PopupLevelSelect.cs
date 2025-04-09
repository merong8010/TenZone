using UnityEngine;
using System.Linq;

public class PopupLevelSelect : Popup
{
    [SerializeField]
    private LevelList levelList;


    public override void Open()
    {
        base.Open();
        Initialize();
        levelList.UpdateList(DataManager.Instance.gameLevel.ToList());
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

        if(DataManager.Instance.userData.UseHeart())
        {
            PuzzleManager.Instance.GameStart(currentLevel);
            Close();
        }
        
    }
}
