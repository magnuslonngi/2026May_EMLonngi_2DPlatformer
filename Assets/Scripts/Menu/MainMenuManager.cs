using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject _creditsUI;
    [SerializeField] private GameObject _winUI;
    [SerializeField] private GameObject _loseUI;

    public void GoToGameScene()
    {
        SceneManager.LoadScene(1);
    }

    public void GoToMenuScene()
    {
        SceneManager.LoadScene(0);
    }

    public void CloseGame()
    {
        Application.Quit();
    }

    public void ShowCredits()
    {
        _creditsUI.SetActive(true);
    }

    public void HideCredits()
    {
        _creditsUI.SetActive(false);
    }

    public void ShowWin()
    {
        _winUI.SetActive(true);
    }

    public void ShowLose()
    {
        _loseUI.SetActive(true);
    }
}
