using UnityEngine;
using UnityEngine.SceneManagement;

public class Buttons : MonoBehaviour
{
    GameObject menu;
    GameObject options;
    GameObject mic;
    GameObject scroll;
    void Start()
    {
        menu = transform.GetChild(1).gameObject;
        options = transform.GetChild(2).gameObject;
        mic = transform.GetChild(2).GetChild(0).gameObject;
        scroll = mic.transform.GetChild(1).gameObject;
    }
    public void QuitTheGame() => Application.Quit();
    public void Solo() => SceneManager.LoadScene("GameScene");
    public void Settings()
    {
        menu.SetActive(false);
        options.SetActive(true);
    }
    public void Back()
    {
        menu.SetActive(true);
        options.SetActive(false);
    }
    public void Mic()
    {
        if (scroll.activeSelf == false)
        {
            scroll.SetActive(true);
        }
        else
        {
            scroll.SetActive(false);
        }
    }
}
