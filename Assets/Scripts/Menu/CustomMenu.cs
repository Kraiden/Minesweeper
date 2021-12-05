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
        int x = PlayerPrefs.GetInt(PrefsConstants.CUST_X, 9);
        int y = PlayerPrefs.GetInt(PrefsConstants.CUST_Y, 9);
        int bombs = PlayerPrefs.GetInt(PrefsConstants.CUST_BOMBS, 10);

        bool isLocked = PlayerPrefs.GetInt(PrefsConstants.CUST_LOCK, 0) == 1;
        bool noGuess = PlayerPrefs.GetInt(PrefsConstants.SET_NO_GUESS_ENABLED, 1) == 1;
        

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
        bool noGuess = PlayerPrefs.GetInt(PrefsConstants.SET_NO_GUESS_ENABLED, 1) == 1;

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
        
        PlayerPrefs.SetInt(PrefsConstants.MAP_X, x);
        PlayerPrefs.SetInt(PrefsConstants.MAP_Y, y);
        PlayerPrefs.SetInt(PrefsConstants.MAP_BOMBS, bombs);

        PlayerPrefs.SetInt(PrefsConstants.CUST_X, x);
        PlayerPrefs.SetInt(PrefsConstants.CUST_Y, y);
        PlayerPrefs.SetInt(PrefsConstants.CUST_BOMBS, bombs);

        PlayerPrefs.SetInt(PrefsConstants.CUST_NO_GUESS_OVERRIDE, noGuessOverride ? 1 : 0 );

        PlayerPrefs.Save();

        SceneManager.LoadScene("Game");

    }
}
