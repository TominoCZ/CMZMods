using System;

namespace ResourcePacks
{
    public static class ArrayExtensions
    {
        public static T[] Shrink<T>(this T[] original, int n)
        {
            if (original == null || n <= 1)
                return original;

            var result = new T[original.Length / (n * n)];
            var widthIn = (int)Math.Sqrt(original.Length);
            var widthOut = (int)Math.Sqrt(result.Length);

            for (int y = 0; y < widthOut; y++)
            {
                for (int x = 0; x < widthOut; x++)
                {
                    var indexIn = (y * widthIn + x) * n;
                    var indexOut = y * widthOut + x;

                    result[indexOut] = original[indexIn];
                }
            }

            return result;
        }

        public static T[] Rotate<T>(this T[] input, int rotations = 1)
        {
            rotations %= 4;

            var result = new T[input.Length];
            var width = (int)Math.Sqrt(input.Length);

            for (int _ = 0; _ < rotations; _++)
            {
                for (int y = 0; y < width; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int indexIn = y * width + x;
                        int indexOut = x * width + (width - y - 1);

                        result[indexOut] = input[indexIn];
                    }
                }
            }

            return result;
        }

        public static T[] MirrorY<T>(this T[] input)
        {
            var result = new T[input.Length];
            var width = (int)Math.Sqrt(input.Length);

            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int indexIn = y * width + x;
                    int indexOut = (width - y - 1) * width + x;

                    result[indexOut] = input[indexIn];
                }
            }

            return result;
        }

        public static T[] MirrorX<T>(this T[] input)
        {
            var result = new T[input.Length];
            var width = (int)Math.Sqrt(input.Length);

            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int indexIn = y * width + x;
                    int indexOut = y * width + (width - x - 1);

                    result[indexOut] = input[indexIn];
                }
            }

            return result;
        }

        public static T[] MirrorXY<T>(this T[] input)
        {
            var result = new T[input.Length];
            var width = (int)Math.Sqrt(input.Length);

            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int indexIn = y * width + x;
                    int indexOut = (width - y - 1) * width + (width - x - 1);

                    result[indexOut] = input[indexIn];
                }
            }

            return result;
        }
    }
}
