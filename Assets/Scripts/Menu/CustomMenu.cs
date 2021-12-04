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

    public Toggle settingsLock;

    void Awake(){
        SettingsUpdated();
    }

    public void SettingsUpdated(){
        int x = PlayerPrefs.GetInt("custom-x", 9);
        int y = PlayerPrefs.GetInt("custom-y", 9);
        int bombs = PlayerPrefs.GetInt("custom-bombs", 10);

        bool isLocked = PlayerPrefs.GetInt("custom-lock", 0) == 1;
        bool noGuess = PlayerPrefs.GetInt("settings-no-guess", 1) == 1;
        

        if(!noGuess){
            settingsLock.gameObject.SetActive(false);
            settingsLock.isOn = false;
        } else {
            settingsLock.gameObject.SetActive(true);
            settingsLock.isOn = isLocked;
        }

        mapX.value = x;
        mapY.value = y;
        mines.maxValue = bombs;
        mines.value = bombs;

        UpdateMinesAndWarning();
    }

    public void GoBack(){
       gameObject.SetActive(false);
       mainMenu.SetActive(true);
    }

    public void UpdateMinesAndWarning(){
        float x = mapX.value;
        float y = mapY.value;
        float mnCount = mines.value;

        bool isLocked = settingsLock.isOn;
        bool noGuess = PlayerPrefs.GetInt("settings-no-guess", 1) == 1;

        float maxMines;
        if(isLocked && noGuess){
            maxMines = (int) Mathf.Min((x * y) * MAX_MINES_PCT, MAX_MINES);
        } else {
            maxMines = (x * y) - 9;
        } 

        if(mnCount > maxMines){
            mnCount = maxMines;
        }

        mines.maxValue = maxMines;
        mines.value = mnCount;

        if(noGuess && (mnCount / (x*y) > MAX_MINES_PCT || mnCount > MAX_MINES)){
            noGuessWarning.SetActive(true);
        } else {
            noGuessWarning.SetActive(false);
        }
    }

    public void lockedToggled(bool isOn){
        float x = mapX.value;
        float y = mapY.value;
        float mnCount = mines.value;

        
        float maxMines;
        if(isOn){
            maxMines = (int) Mathf.Min((x * y) * MAX_MINES_PCT, MAX_MINES);
        } else {
            maxMines = (x * y) - 9;
        }

        if(mnCount > maxMines){
            mnCount = maxMines;
        }

        mines.maxValue = maxMines;
        mines.value = mnCount;
    }

    public void PlayGame(){
        int x = (int) mapX.value;
        int y = (int) mapY.value;
        int bombs= (int) mines.value;

        bool isLocked = settingsLock.isOn;

        bool noGuessOverride = bombs / (x * y) > MAX_MINES_PCT || bombs > MAX_MINES;
        
        PlayerPrefs.SetInt("map-x", x);
        PlayerPrefs.SetInt("map-y", y);
        PlayerPrefs.SetInt("map-bombs", bombs);

        PlayerPrefs.SetInt("custom-x", x);
        PlayerPrefs.SetInt("custom-y", y);
        PlayerPrefs.SetInt("custom-bombs", bombs);

        PlayerPrefs.SetInt("settings-no-guess-override", noGuessOverride ? 1 : 0 );

        PlayerPrefs.Save();

        SceneManager.LoadScene("Game");

    }
}
