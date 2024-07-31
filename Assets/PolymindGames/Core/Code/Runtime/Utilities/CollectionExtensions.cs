using System.Collections.Generic;
using UnityEngine;
using System;

namespace PolymindGames
{
    using UnityObject = UnityEngine.Object;
    using Random = UnityEngine.Random;

    /// <summary>
    /// Provides extension methods for collections.
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// Checks if the specified array contains any of the elements from another array.
        /// </summary>
        /// <typeparam name="T">The type of elements in the arrays.</typeparam>
        /// <param name="enumerable">The array to check.</param>
        /// <param name="elements">The array containing elements to check for.</param>
        /// <returns>True if any of the elements from the <paramref name="elements"/> array are found in the <paramref name="enumerable"/> array; otherwise, false.</returns>
        public static bool ContainsAny<T>(this T[] enumerable, T[] elements)
        {
            for (int i = 0; i < elements.Length; i++)
            {
                if (Array.IndexOf(enumerable, elements[i]) != -1)
                    return true;
            }

            return false;
        }
    
        /// <summary>
        /// Finds the index of the first occurrence of a specified value in the list.
        /// </summary>
        /// <typeparam name="T">The type of elements in the list.</typeparam>
        /// <param name="list">The list to search.</param>
        /// <param name="item">The value to locate in the list.</param>
        /// <returns>The index of the first occurrence of <paramref name="item"/> if found; otherwise, -1.</returns>
        public static int IndexOf<T>(this IReadOnlyList<T> list, T item)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (EqualityComparer<T>.Default.Equals(list[i], item))
                    return i;
            }
        
            return -1;
        }

        /// <summary>
        /// Selects an item from the array based on the specified selection method.
        /// </summary>
        /// <typeparam name="T">The type of items in the array.</typeparam>
        /// <param name="array">The array to select from.</param>
        /// <param name="lastIndex">The index of the last selected item.</param>
        /// <param name="selectionMethod">The method to use for selection.</param>
        /// <returns>The selected item.</returns>
        public static T Select<T>(this T[] array, ref int lastIndex, SelectionType selectionMethod = SelectionType.RandomExcludeLast)
        {
            return selectionMethod switch
            {
                SelectionType.Random => SelectRandom(array),
                SelectionType.RandomExcludeLast => SelectRandomExcludeLast(array, ref lastIndex),
                SelectionType.Sequence => SelectSequence(array, ref lastIndex),
                _ => default(T)
            };
        }

        /// <summary>
        /// Selects a random item from the list.
        /// </summary>
        /// <typeparam name="T">The type of items in the list.</typeparam>
        /// <param name="list">The list to select from.</param>
        /// <returns>The selected item, or the default value of type T if the list is empty.</returns>
        public static T SelectRandom<T>(this IReadOnlyList<T> list)
        {
            if (list == null || list.Count == 0)
                return default(T);

            return list[Random.Range(0, list.Count)];
        }

        /// <summary>
        /// Selects the next item in the list sequentially, looping back to the beginning if necessary.
        /// </summary>
        /// <typeparam name="T">The type of items in the list.</typeparam>
        /// <param name="list">The list to select from.</param>
        /// <param name="lastIndex">The index of the last selected item.</param>
        /// <returns>The selected item, or the default value of type T if the list is empty.</returns>
        public static T SelectSequence<T>(this IReadOnlyList<T> list, ref int lastIndex)
        {
            if (list == null || list.Count == 0)
                return default(T);

            lastIndex = (int)Mathf.Repeat(lastIndex + 1, list.Count);
            return list[lastIndex];
        }

        /// <summary>
        /// Selects a random item from the list, excluding the last selected item.
        /// </summary>
        /// <typeparam name="T">The type of items in the list.</typeparam>
        /// <param name="list">The list to select from.</param>
        /// <param name="lastIndex">The index of the last selected item.</param>
        /// <returns>The selected item, or the default value of type T if the list is empty or contains only one item.</returns>
        public static T SelectRandomExcludeLast<T>(this IReadOnlyList<T> list, ref int lastIndex)
        {
            if (list == null || list.Count == 0)
                return default(T);

            if (list.Count == 1)
                return list[0];
            
            T selected;
            T lastSelected = list[lastIndex];
            do
            {
                lastIndex = Random.Range(0, list.Count);
                selected = list[lastIndex];
            } while (EqualityComparer<T>.Default.Equals(selected, lastSelected));

            return selected;
        }


        public static void RemoveAllNull<T>(ref T[] array) where T : UnityObject
        {
            if (array == null)
                return;

            int index = 0;
            while (index < array.Length)
            {
                if (array[index] == null)
                    RemoveAtIndex(ref array, index);
                else
                    index++;
            }
        }

        public static void DistinctPreserveNull<T>(ref T[] array) where T : UnityObject
        {
            if (array == null)
                return;

            int index = 0;
            while (index < array.Length)
            {
                int itemCount = 0;
                int indexOfDuplicate = -1;
                for (int i = 0; i < array.Length; i++)
                {
                    if (array[index] == array[i])
                    {
                        itemCount++;
                        indexOfDuplicate = i;
                    }
                }

                if (array[indexOfDuplicate] != null && itemCount == 2)
                {
                    array[indexOfDuplicate] = null;
                    index++;
                    continue;
                }

                if (itemCount > 1)
                {
                    RemoveAtIndex(ref array, indexOfDuplicate);
                    continue;
                }

                index++;
            }
        }

        private static void RemoveAtIndex<T>(ref T[] array, int index) where T : UnityObject
        {
            var newArray = new T[array.Length - 1];

            for (int i = 0; i < index; i++)
                newArray[i] = array[i];

            for (int j = index; j < newArray.Length; j++)
                newArray[j] = array[j + 1];

            array = newArray;
        }
        
        public static void OrderArray<T>(this T[] arr) where T : IComparable<T>
        {
            int n = arr.Length;
            for (int i = 0; i < n - 1; i++)
            {
                for (int j = 0; j < n - i - 1; j++)
                {
                    if (arr[j].CompareTo(arr[j + 1]) > 0)
                    {
                        // Swap arr[j] and arr[j+1]
                        (arr[j], arr[j + 1]) = (arr[j + 1], arr[j]);
                    }
                }
            }
        }
        public static bool IsOrdered<T>(this T[] array) where T : IComparable<T>
        {
            if (array == null || array.Length <= 1)
            {
                // If the array is null or has only one element, it is considered ordered.
                return true;
            }

            for (int i = 0; i < array.Length - 1; i++)
            {
                if (array[i].CompareTo(array[i + 1]) > 0)
                {
                    // If any element is greater than the next one, the array is not ordered.
                    return false;
                }
            }

            // If the loop completes without returning false, the array is ordered.
            return true;
        }
    }

    public enum SelectionType
    {
        /// <summary>The item will be selected randomly.</summary>
        Random,

        /// <summary>The item will be selected randomly, but will exclude the last selected.</summary>
        RandomExcludeLast,

        /// <summary>The items will be selected in sequence.</summary>
        Sequence
    }
}