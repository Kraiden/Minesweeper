using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderValue : MonoBehaviour{
    public void SetValue(float value){
        GetComponent<Text>().text = value.ToString();
    }
}
