using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundLibrary : MonoBehaviour{

    public SoundGroup[] groups;

    Dictionary<string, SoundGroup> groupDict = new Dictionary<string, SoundGroup>();

    void Awake(){
        foreach (SoundGroup group in groups){
            groupDict.Add (group.groupID, group);
        }
    }

    public SoundGroup GetGroupByName(string name){
        if(groupDict.ContainsKey(name)){
            return groupDict[name];
        }
        return null;
    }

    [System.Serializable]
    public class SoundGroup {
        public string groupID;
        public int simultaneosCount = -1;
        public AudioClip[] group;

        public bool randomize = true;

        private int lastPlayedIndex = -1;

        public AudioClip GetClip(){
            if(randomize){
                return group[Random.Range(0, group.Length)];
            } else {
                int nextIndex = ++lastPlayedIndex % group.Length;
                if(nextIndex == 0) lastPlayedIndex = 0;
                return group[nextIndex];
            } 
        }
    }
}