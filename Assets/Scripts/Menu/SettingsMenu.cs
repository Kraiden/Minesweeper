using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsMenu : MonoBehaviour
{
   public GameObject mainMenu;

   public void GoBack(){
       gameObject.SetActive(false);
       mainMenu.SetActive(true);
   }
}
