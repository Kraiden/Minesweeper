using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Coord {
    public int x;
    public int y;

    public Coord(int _x, int _y){
        x = _x;
        y = _y;
    }

    public override bool Equals(object obj)
    {
        return obj is Coord coord &&
               x == coord.x &&
               y == coord.y;
    }

    public override int GetHashCode()
    {
        int hashCode = 1502939027;
        hashCode = hashCode * -1521134295 + x;
        hashCode = hashCode * -1521134295 + y;
        return hashCode;
    }

    public static bool operator == (Coord c1, Coord c2){
        if((object)c1 == null && (object)c2 == null) return true;
        if((object)c1 != null ^ (object)c2 != null) return false;
        return c1.x == c2.x && c1.y == c2.y;
    }

    public static bool operator !=(Coord c1, Coord c2){
        return !(c1 == c2);
    }
}
