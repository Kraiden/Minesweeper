using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof (Screenshotter))]
public class Screenshot : Editor {

    public int width = 1024;
    public int height = 500;
   
   public override void OnInspectorGUI () {
        Screenshotter scene = target as Screenshotter;
        if (DrawDefaultInspector()){
            //map.GenerateMap();
        }
    
        if(GUILayout.Button("Screenshot")){
            TakeScreenshot(scene.subcameras);
        }
   }

   private string fileName(int width, int height){
        return string.Format("screen_{0}x{1}_{2}.png",
                              width, height,
                              System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
     }

   private void TakeScreenshot(Camera[] subcameras){
        Camera camera = Camera.main; 

        RenderTexture rt = new RenderTexture(width, height, 24);

        foreach(Camera sub in subcameras) sub.targetTexture = rt;
        camera.targetTexture = rt;

        Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGB24, false);

        
        camera.Render();
        foreach(Camera sub in subcameras) sub.Render();

        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);

        foreach(Camera sub in subcameras) sub.targetTexture = null;
        camera.targetTexture = null;

        RenderTexture.active = null; // JC: added to avoid errors
        Destroy(rt);
        byte[] bytes = screenShot.EncodeToPNG();
        string filename = fileName(width, height);
        System.IO.File.WriteAllBytes(filename, bytes);
        Debug.Log(string.Format("Took screenshot to: {0}", filename));
   }
}