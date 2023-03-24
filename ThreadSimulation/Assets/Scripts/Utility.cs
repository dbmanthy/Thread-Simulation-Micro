using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utility
{
    public static T[] FisherYatesArrayShuffle<T>(T[] array, int seed = -1)
    {
        //psuedo random number generator
        System.Random prng = seed == -1 ? new System.Random() : new System.Random(seed);

        for (int i = 0; i < array.Length - 1; i++)
        {
            int randomIndex = prng.Next(i, array.Length);
            T tempItem = array[randomIndex];
            array[randomIndex] = array[i];
            array[i] = tempItem;
        }
        return array;
    }

    public static T[] ShuffleArray<T>(T[] array, System.Random prng)
    {

        int elementsRemainingToShuffle = array.Length;
        int randomIndex = 0;

        while (elementsRemainingToShuffle > 1)
        {

            // Choose a random element from array
            randomIndex = prng.Next(0, elementsRemainingToShuffle);
            T chosenElement = array[randomIndex];

            // Swap the randomly chosen element with the last unshuffled element in the array
            elementsRemainingToShuffle--;
            array[randomIndex] = array[elementsRemainingToShuffle];
            array[elementsRemainingToShuffle] = chosenElement;
        }

        return array;
    }
}
