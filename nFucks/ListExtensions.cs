using System.Collections.Generic;

static class ListExtensions {
    public static void Swap<T> (this IList<T> list, int indexA, int indexB) {
        T tmp = list[indexA];
        list[indexA] = list[indexB];
        list[indexB] = tmp;
    }
}