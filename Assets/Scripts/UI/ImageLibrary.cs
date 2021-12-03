using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageLibrary : MonoBehaviour{

    public ImageGroup[] groups;

    Dictionary<string, Sprite[]> groupDict = new Dictionary<string, Sprite[]>();

    void Awake(){
        foreach (ImageGroup group in groups){
            groupDict.Add (group.groupID, group.group);
        }
    }

    public Sprite GetImageByName(string name){
        if(groupDict.ContainsKey(name)){
            Sprite[] sounds = groupDict[name];
            return sounds[Random.Range(0, sounds.Length)]; 
        }
        return null;
    }

    [System.Serializable]
    public class ImageGroup {
        public string groupID;
        public Sprite[] group;
    }
}