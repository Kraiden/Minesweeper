using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject customMenu;
    public GameObject settingsMenu;

    void Start(){
        // Initialize Firebase
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                // Crashlytics will use the DefaultInstance, as well;
                // this ensures that Crashlytics is initialized.
                Firebase.FirebaseApp app = Firebase.FirebaseApp.DefaultInstance;

                // Set a flag here for indicating that your project is ready to use Firebase.
            }
            else
            {
                UnityEngine.Debug.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}",dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });
    }

    public void EasyClick() {
        StartGame(9,9,10);
    }

    public void NormalClick() {
        StartGame(15,17,40);
    }

    public void HardClick() {
        StartGame(16,30,99);
    }

    public void CustomClick() {
        customMenu.SetActive(true);
        mainMenu.SetActive(false);
    }

    public void SettingsClick() {
        settingsMenu.SetActive(true);
        mainMenu.SetActive(false);
    }

    public void QuitClick() {
        Application.Quit();
    }

    private void StartGame(int x, int y, int bombs){
        PlayerPrefs.SetInt(PrefsConstants.MAP_X, x);
        PlayerPrefs.SetInt(PrefsConstants.MAP_Y, y);
        PlayerPrefs.SetInt(PrefsConstants.MAP_BOMBS, bombs);

        PlayerPrefs.SetInt(PrefsConstants.CUST_NO_GUESS_OVERRIDE, 0);

        PlayerPrefs.Save();

        SceneManager.LoadScene("Game");
    }
}
