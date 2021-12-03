using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class UIManager : MonoBehaviour
{
    public Image imageButton;
    public Image soundButton;
    public Image vibrateButton;

    public Sprite soundOn;
    public Sprite soundOff;

    public Sprite vibrateOn;
    public Sprite vibrateOff;
    
    public TextMeshProUGUI flagCount;
    public TextMeshProUGUI timer;

    public Generator board;
    public Controller controller;

    public ParticleSystem winEffect;

    ImageLibrary library;

    private float gameDuration = 0;
    private float gameStart = 0;
    private bool inGame = false;

    private int flagged = 0;
    private int bombs = 0;

    private bool isGenerating = false;
    
    void Awake()
    {
        library = GetComponent<ImageLibrary>();

        imageButton.sprite = library.GetImageByName("playing");

        controller.OnTouchDown = OnTouchDown;
        controller.OnTouchUp = OnTouchUp;
        board.OnNewGame += OnNewGame;
        board.OnGeneration += OnMapGen;
        board.OnGameOver = OnGameOver;
        board.OnFlagChange = OnFlagChange;
        
        bombs = PlayerPrefs.GetInt("map-bombs", 99);
        flagCount.text = bombs.ToString();

        inGame = false;

        var sound = PlayerPrefs.GetInt("settings-sound", 1);
        var vib = PlayerPrefs.GetInt("settings-vib", 1);

        if(sound == 1){
            soundButton.sprite = soundOn;
        } else {
            soundButton.sprite = soundOff;
        }

        if(vib == 1){
            vibrateButton.sprite = vibrateOn;
        } else {
            vibrateButton.sprite = vibrateOff;
        }

        InvokeRepeating("UpdateTime", 0, 1f);

    }

    private void UpdateTime(){
        if(inGame){
            gameDuration = Time.time - gameStart;
            timer.text = FormatTime(gameDuration);
        }
    }

    public void OnImageClick(){
        imageButton.sprite = library.GetImageByName("playing");
        board.GenerateMap();
        gameDuration = 0;
        inGame = false;

        bombs = board.bombCount;
        flagged = 0;

        flagCount.text = bombs.ToString();
    }

    public void OnBackClick() {
        SceneManager.LoadScene("Menu");
    }

    public void OnSoundClick(){
        var sound = PlayerPrefs.GetInt("settings-sound", 1);
        sound = 1 - sound;
        PlayerPrefs.SetInt("settings-sound", sound);
        PlayerPrefs.Save();

        if(sound == 1){
            soundButton.sprite = soundOn;
        } else {
            soundButton.sprite = soundOff;
        }
    }

    public void OnVibClick(){
        var vib = PlayerPrefs.GetInt("settings-vib", 1);
        vib = 1 - vib;
        PlayerPrefs.SetInt("settings-vib", vib);
        PlayerPrefs.Save();

        if(vib == 1){
            vibrateButton.sprite = vibrateOn;
        } else {
            vibrateButton.sprite = vibrateOff;
        }
    }

    private void OnTouchDown(){
        if(inGame || isGenerating){
            imageButton.sprite = library.GetImageByName("down");
        }
    }

    private void OnTouchUp(){
        if(inGame){
            imageButton.sprite = library.GetImageByName("playing");
        } else if(isGenerating){
            imageButton.sprite = library.GetImageByName("thinking");
        }
    }

    private void OnNewGame(){
        inGame = true;
        timer.text = "00:00";
        gameStart = Time.time;
        isGenerating = false;
        imageButton.sprite = library.GetImageByName("playing");
    }

    private void OnMapGen(){
        isGenerating = true;
        imageButton.sprite = library.GetImageByName("thinking");
    }

    private void OnGameOver(bool win){
        inGame = false;
        if(win){
            imageButton.sprite = library.GetImageByName("win");
            AudioManager.instance.PlaySound2d("win");
            flagCount.text = "0";
            Vector3 position = Vector3.zero + (Vector3.back * 6f);
            Destroy(Instantiate(winEffect.gameObject, position, Quaternion.identity), winEffect.main.startLifetime.constant);
        } else {
            imageButton.sprite = library.GetImageByName("lose");
        }
    }

    private void OnFlagChange(bool added){
        if(added){
            flagged++;
        } else {
            flagged--;
        }

        flagCount.text = (bombs - flagged).ToString();
    }

    private string FormatTime(float time){
        int minutes = (int) time / 60 ;
        int seconds = (int) time / 1 - 60 * minutes;
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
