using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof (Generator))]
public class MapEditor : Editor {
   
   public override void OnInspectorGUI () {
        Generator map = target as Generator;
        if (DrawDefaultInspector()){
            //map.GenerateMap();
        }
    
        if(GUILayout.Button("Regenerate Map")){
            map.GenerateMap();
        }
   }
}