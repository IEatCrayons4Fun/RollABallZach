using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathScreenUI : MonoBehaviour
{
    public void RestartGame(){
        SceneManager.LoadScene("RollBallGame");
    }

    public void QuitToMainMenu(){
        SceneManager.LoadScene("MainMenu");
    }



}
