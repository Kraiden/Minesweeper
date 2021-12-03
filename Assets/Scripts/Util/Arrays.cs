using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Arrays {

    public static T[] Shuffle<T>(T[] array){
        return Shuffle(array, Random.Range(0, int.MaxValue));
    }
    
    public static T[] Shuffle<T>(T[] array, int seed){
        System.Random prng = new System.Random(seed);

        for (int i = 0; i < array.Length -1; i++) {
            int randomIndex = prng.Next (i, array.Length);
            T tempItem = array[randomIndex];
            array[randomIndex] = array[i];
            array[i] = tempItem;
        }

        return array;
    }
}
