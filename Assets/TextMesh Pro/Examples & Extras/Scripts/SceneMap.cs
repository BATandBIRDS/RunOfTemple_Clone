using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneMap : MonoBehaviour
{
    public Text bestDistanceText;
    public Text maxCoinText;
    

    // Start is called before the first frame update
    void Start()
    {
        bestDistanceText.text = "Max Distance: " + PlayerPrefs.GetInt("highscoreD") + "M";
        maxCoinText.text = "Max Coin: " + PlayerPrefs.GetInt("highscoreC");
    }
    public void ToGame()
    {
        SceneManager.LoadScene("Game");
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
