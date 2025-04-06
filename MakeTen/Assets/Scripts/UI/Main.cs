using UnityEngine;

public class Main : MonoBehaviour
{
    public void GameStart()
    {
        Debug.Log("Main.GameStart");
        PuzzleManager.Instance.GameStart();
    }
}
