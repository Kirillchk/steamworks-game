using UnityEngine;
using UnityEngine.SceneManagement;

public class Buttons : MonoBehaviour
{
    public void QuitTheGame()
    {
        Application.Quit();
    }
    public void Solo()
    {
        SceneManager.LoadScene("GameScene");
    }
}
