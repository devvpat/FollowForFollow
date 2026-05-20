using System;
using System.Collections.Generic;

// https://stackoverflow.com/questions/273313/randomize-a-listt
public static class Utilities
{
    private static Random rnd = new Random();

    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1) {
            n--;
            int k = rnd.Next(n + 1);
            (list[n], list[k]) = (list[k], list[n]);
        }
    }
}