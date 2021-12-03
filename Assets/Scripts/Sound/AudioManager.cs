using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{

    public static AudioManager instance;

    AudioSource sound2dSource;
    SoundLibrary library;

    private Dictionary<string, int> soundsPlaying;

    void Awake(){
        if(instance != null){
            Destroy(gameObject);
        } else {
            instance = this;
            DontDestroyOnLoad(gameObject);

            library = GetComponent<SoundLibrary>();

            soundsPlaying = new Dictionary<string, int>();

            GameObject sound2d = new GameObject("2D source");
            sound2dSource = sound2d.AddComponent<AudioSource> ();
            sound2d.transform.parent = transform;
        }
    }

    public void PlaySound2d(string name, float vol = 1f){
        if(PlayerPrefs.GetInt("settings-sound", 1) == 1){
            AudioClip clip = GetClipByName(name);

            if(clip != null){
                sound2dSource.PlayOneShot(clip, vol);
            }
        }
    }

    private AudioClip GetClipByName(string name){
        SoundLibrary.SoundGroup group = library.GetGroupByName(name);
        if(group == null) return null;

        if(group.simultaneosCount >= 0){
            if(soundsPlaying.ContainsKey(name)){
                int count = soundsPlaying[name];
                Debug.Log(name + " inc " + count);
                if(count >= group.simultaneosCount) return null;
            } else {
                soundsPlaying[name] = 0;
            }

            soundsPlaying[name]++;

            AudioClip clip = group.GetClip();
            float length = clip.length;

            StartCoroutine(soundEnded(name, length));
            return clip;
        } else {
            return group.GetClip();
        }
    }

    private IEnumerator soundEnded(string name, float clipLength){
        yield return new WaitForSeconds(clipLength);
        soundsPlaying[name]--;
        Debug.Log(name + " dec " + soundsPlaying[name]);
    }

}