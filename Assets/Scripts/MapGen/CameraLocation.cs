using UnityEngine;

public struct CameraLocation{
    public Vector3 position;
    public float zoom;

    public CameraLocation(Vector3 pos, float zoom){
        this.position = pos;
        this.zoom = zoom;
    }
}