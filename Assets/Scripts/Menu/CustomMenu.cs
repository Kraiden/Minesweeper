using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine;

public class CustomMenu : MonoBehaviour
{
    private static float MAX_MINES_PCT = 0.215f;
    private static float MAX_MINES = 300;

    public GameObject mainMenu;
    public GameObject noGuessWarning;

    public Slider mapX;
    public Slider mapY;
    public Slider mines;

    public void GoBack(){
       gameObject.SetActive(false);
       mainMenu.SetActive(true);
       //Hah
    }

    public void UpdateMinesAndWarning(){
        float x = mapX.value;
        float y = mapY.value;
        float mnCount = mines.value;

        float maxMines = (x * y) - 9;

        mines.maxValue = maxMines;

        bool noGuess = PlayerPrefs.GetInt("settings-no-guess", 1) == 1;

        if(noGuess && (mnCount / (x*y) > MAX_MINES_PCT || mnCount > MAX_MINES)){
            noGuessWarning.SetActive(true);
        } else {
            noGuessWarning.SetActive(false);
        }
    }

    public void PlayGame(){
        PlayerPrefs.SetInt("map-x", (int) mapX.value);
        PlayerPrefs.SetInt("map-y", (int) mapY.value);
        PlayerPrefs.SetInt("map-bombs", (int) mines.value);

        bool noGuessOverride = mines.value / (mapX.value * mapY.value) > MAX_MINES_PCT || mines.value > MAX_MINES;

        PlayerPrefs.SetInt("settings-no-guess-override", noGuessOverride ? 1 : 0 );

        PlayerPrefs.Save();

        SceneManager.LoadScene("Game");

    }
}
