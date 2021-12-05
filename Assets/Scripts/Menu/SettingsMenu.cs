using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsMenu : MonoBehaviour
{
   public GameObject mainMenu;

   public CustomMenu customMenu;

   void Awake(){
    //    float touchSensitivity =
    //    float moveSensitivity =
    //    bool touchSensitivity =
   }

   public void TouchSensitivityChanged(float value){
       // 0.65f
       float calcVal = 1 - (value / 100);

       PlayerPrefs.SetFloat("settings-touch-sensitivity", calcVal);

       PlayerPrefs.Save();
   }

   public void MoveSensitivityChanged(float value){
       //3500

       float calcVal = value;

       PlayerPrefs.SetFloat("settings-move-sensitivity", calcVal);

       PlayerPrefs.Save();
   }

   public void ParticleEffectsChanged(bool isOn){
       PlayerPrefs.SetInt("settings-play-particles", isOn ? 1 : 0);

       PlayerPrefs.Save();
   }

   public void DefeatAnimChanged(bool isOn){
       PlayerPrefs.SetInt("settings-defeat-anim", isOn ? 1 : 0);

       PlayerPrefs.Save();
   }

   public void NoGuessChanged(bool isOn){
       PlayerPrefs.SetInt("settings-no-guess", isOn ? 1 : 0);

       PlayerPrefs.Save();
       customMenu.SettingsUpdated();
   }

   public void ResetToDefault(){

   }

   public void GoBack(){
       gameObject.SetActive(false);
       mainMenu.SetActive(true);
   }
}
