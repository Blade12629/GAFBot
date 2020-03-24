using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerModule
{
    public static class Extensions
    {
        /// <summary>
        /// Inserts an array into another array
        /// </summary>
        /// <typeparam name="T">array type</typeparam>
        /// <param name="array">original</param>
        /// <param name="data">to insert</param>
        /// <param name="start">start index, inserts default values into created but skipped indexes</param>
        /// <returns>array with data inserted</returns>
        /// <exception cref="ArgumentOutOfRangeException">'start' cannot be less than 0</exception>
        public static T[] Insert<T>(this T[] array, T[] data, int start)
        {
            if (start < 0)
                throw new ArgumentOutOfRangeException("'start' cannot be less than 0");

            T[] newArray = new T[array.Length + data.Length];

            //Copy all until we reach start
            Array.Copy(array, 0, newArray, 0, start);

            //Add data to array
            for (int i = 0; i < data.Length; i++)
                newArray[start + i] = data[i];

            //insert default if we have skipped indexes
            if (start > array.Length)
            {
                int emptyLength = start - array.Length;

                for (int i = array.Length; i < array.Length + emptyLength; i++)
                    newArray[i] = default(T);
            }

            int toCopy = array.Length - (start + 1);

            //Copy rest of the array if there is something left
            if (toCopy != 0)
                Array.Copy(array, start + 1, newArray, start + data.Length, toCopy);

            return newArray;
        }

        public static T[] Replace<T>(this T[] array, T[] data, int start)
        {
            T[] result;
            if (start + data.Length >= array.Length)
                result = new T[start + data.Length];
            else
                result = new T[array.Length];

            Array.Copy(array, 0, result, 0, array.Length);
            Array.Copy(data, 0, result, start, data.Length);

            return result;
        }

        public static T[] GetRange<T>(this T[] array, int start, int length)
        {
            T[] result = new T[length];

            for (int i = start; i < start + length; i++)
                result[i - start] = array[i];

            return result;
        }
    }
}
