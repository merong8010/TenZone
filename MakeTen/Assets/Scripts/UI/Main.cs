using UnityEngine;
using UnityEngine.UI;

public class Main : MonoBehaviour
{
    [SerializeField]
    private Text loginStatusText;
    [SerializeField]
    private GameObject googleLoginButton;
    [SerializeField]
    private GameObject logoutButton;

    public void Refresh()
    {
        loginStatusText.text = DataManager.Instance.userData.Info.authType.ToString();

        googleLoginButton.SetActive(DataManager.Instance.userData.Info.authType == FirebaseManager.AuthenticatedType.None);
        logoutButton.SetActive(DataManager.Instance.userData.Info.authType != FirebaseManager.AuthenticatedType.None);
    }

    public void GameStart()
    {
        UIManager.Instance.Open<PopupLevelSelect>();
    }

    public void GoogleLogin()
    {
        FirebaseManager.Instance.StartGoogleLogin();
    }

    public void ClickLogOut()
    {
        FirebaseManager.Instance.LogOut();
    }
}
