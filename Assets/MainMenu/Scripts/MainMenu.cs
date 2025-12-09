using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("RollBallGame");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void RestartGame(){
        SceneManager.LoadScene("MainMenu");
    }
}
