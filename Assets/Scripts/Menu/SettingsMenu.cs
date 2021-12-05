using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class SettingsMenu : MonoBehaviour
{
   public GameObject mainMenu;

   public CustomMenu customMenu;

   public Slider TouchSensitivity;
   public Slider MoveSensitivity;

   public Toggle ParticleEffects;
   public Toggle DefeatAnim;
   public Toggle NoGuess;

   void Awake(){
        float touchSensitivity = PlayerPrefs.GetFloat(PrefsConstants.SET_TOUCH_SENSITIVITY, 0.65f);
        float moveSensitivity = PlayerPrefs.GetFloat(PrefsConstants.SET_MOVE_SENSITIVITY, 3000);
        bool particleEffects = PlayerPrefs.GetInt(PrefsConstants.SET_PARTICLES_ENABLED, 1) == 1;
        bool defeatAnim = PlayerPrefs.GetInt(PrefsConstants.SET_DEFEAT_ANIM_ENABLED, 1) == 1;
        bool noGuess = PlayerPrefs.GetInt(PrefsConstants.SET_NO_GUESS_ENABLED, 1) == 1;

        SetAll(touchSensitivity, moveSensitivity, particleEffects, defeatAnim, noGuess);
   }

   public void TouchSensitivityChanged(float value){
       float calcVal = 1 - (value / 100);

       PlayerPrefs.SetFloat(PrefsConstants.SET_TOUCH_SENSITIVITY, calcVal);

       PlayerPrefs.Save();
   }

   public void MoveSensitivityChanged(float value){
       float calcVal = (100 - (value)) * 100;

       PlayerPrefs.SetFloat(PrefsConstants.SET_MOVE_SENSITIVITY, calcVal);

       PlayerPrefs.Save();
   }

   public void ParticleEffectsChanged(bool isOn){
       PlayerPrefs.SetInt(PrefsConstants.SET_PARTICLES_ENABLED, isOn ? 1 : 0);

       PlayerPrefs.Save();
   }

   public void DefeatAnimChanged(bool isOn){
       PlayerPrefs.SetInt(PrefsConstants.SET_DEFEAT_ANIM_ENABLED, isOn ? 1 : 0);

       PlayerPrefs.Save();
   }

   public void NoGuessChanged(bool isOn){
       PlayerPrefs.SetInt(PrefsConstants.SET_NO_GUESS_ENABLED, isOn ? 1 : 0);

       PlayerPrefs.Save();
       customMenu.SettingsUpdated();
   }

   public void ResetToDefault(){
        float touchSensitivity = 0.65f;
        float moveSensitivity = 3000;
        bool particleEffects = true;
        bool defeatAnim = true;
        bool noGuess = true;

        SetAll(touchSensitivity, moveSensitivity, particleEffects, defeatAnim, noGuess);
   }

   public void GoBack(){
       gameObject.SetActive(false);
       mainMenu.SetActive(true);
   }

   private void SetAll(float touchSensitivity, float moveSensitivity, bool particleEffects, bool defeatAnim, bool noGuess){
       int touchSetting = (int) ((1 - touchSensitivity) * 100);
       int moveSetting = (int) ((10000 - moveSensitivity) / 100);

       TouchSensitivity.value = touchSetting;
       MoveSensitivity.value = moveSetting;

       ParticleEffects.isOn = particleEffects;
       DefeatAnim.isOn = defeatAnim;
       NoGuess.isOn = noGuess;
   }
}
