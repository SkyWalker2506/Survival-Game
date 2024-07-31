using Random = UnityEngine.Random;
using UnityEngine;
using System;

namespace PolymindGames
{
    /// <summary>
    /// Contains extension methods for mathematical operations.
    /// </summary>
    public static class MathExtensions
    {
        /// <summary>
        /// Normalizes a value within a specified range.
        /// </summary>
        /// <param name="value">The value to normalize.</param>
        /// <param name="minValue">The minimum value of the range.</param>
        /// <param name="maxValue">The maximum value of the range.</param>
        /// <returns>The normalized value between 0 and 1.</returns>
        public static float Normalize(this float value, float minValue, float maxValue) =>
            (value - minValue) / (maxValue - minValue);

        /// <summary>
        /// Reverses a normalized value back to its original range.
        /// </summary>
        /// <param name="normalizedValue">The normalized value to reverse.</param>
        /// <param name="minValue">The minimum value of the original range.</param>
        /// <param name="maxValue">The maximum value of the original range.</param>
        /// <returns>The original value within the specified range.</returns>
        public static float ReverseNormalize(this float normalizedValue, float minValue, float maxValue) =>
            (normalizedValue * (maxValue - minValue)) + minValue;
        
        /// <summary>
        /// Adds jitter (random noise) to a value.
        /// </summary>
        /// <param name="value">The value to add jitter to.</param>
        /// <param name="jitter">The amount of jitter to add.</param>
        /// <returns>The original value with added jitter.</returns>
        public static float Jitter(this float value, float jitter) =>
            value + Random.Range(-jitter, jitter);

        /// <summary>
        /// Returns a random point within the given bounds.
        /// </summary>
        /// <param name="bounds">The bounds to generate the random point within.</param>
        /// <returns>A random point within the bounds.</returns>
        public static Vector3 GetRandomPoint(this Bounds bounds)
        {
            Vector3 min = bounds.min;
            Vector3 max = bounds.max;
            return new Vector3(
                Random.Range(min.x, max.x),
                Random.Range(min.y, max.y),
                Random.Range(min.z, max.z));
        }

        /// <summary>
        /// Checks if a point is inside a rotated bounding box.
        /// </summary>
        /// <param name="bounds">The bounds of the rotated box.</param>
        /// <param name="rotation">The rotation of the box.</param>
        /// <param name="rotationOrigin">The origin of rotation.</param>
        /// <param name="point">The point to check.</param>
        /// <returns>True if the point is inside the rotated bounds, otherwise false.</returns>
        public static bool IsPointInsideRotatedBounds(this Bounds bounds, Quaternion rotation, Vector3 rotationOrigin, Vector3 point)
        {
            // Translate so that rotationOrigin is the new origin
            Vector3 translatedPoint = point - rotationOrigin;
            Vector3 translatedBoundsCenter = bounds.center - rotationOrigin;

            // Counter-rotate the point
            Vector3 counterRotatedPoint = Quaternion.Inverse(rotation) * translatedPoint + rotationOrigin;

            // The bounds' center also needs to be translated back after the counter-rotation
            Vector3 counterRotatedBoundsCenter = Quaternion.Inverse(rotation) * translatedBoundsCenter + rotationOrigin;

            Bounds counterRotatedBounds = new Bounds(counterRotatedBoundsCenter, bounds.size);

            return counterRotatedBounds.Contains(counterRotatedPoint);
        }

        /// <summary>
        /// Jitters the components of a vector by random amounts.
        /// </summary>
        /// <param name="vector">The vector to jitter.</param>
        /// <param name="xJit">The maximum absolute amount to jitter in the x-direction.</param>
        /// <param name="yJit">The maximum absolute amount to jitter in the y-direction.</param>
        /// <param name="zJit">The maximum absolute amount to jitter in the z-direction.</param>
        /// <returns>The jittered vector.</returns>
        public static Vector3 Jitter(this Vector3 vector, float xJit, float yJit, float zJit)
        {
            vector.x -= Mathf.Abs(vector.x * Random.Range(0, xJit)) * 2f;
            vector.y -= Mathf.Abs(vector.y * Random.Range(0, yJit)) * 2f;
            vector.z -= Mathf.Abs(vector.z * Random.Range(0, zJit)) * 2f;

            return vector;
        }

        /// <summary>
        /// Rounds the components of a vector to a specified number of digits.
        /// </summary>
        /// <param name="vector">The vector to round.</param>
        /// <param name="digits">The number of digits to round to.</param>
        /// <returns>The rounded vector.</returns>
        public static Vector3 Round(this Vector3 vector, int digits)
        {
            vector.x = (float)Math.Round(vector.x, digits);
            if (Mathf.Approximately(vector.x, 0f))
                vector.x = 0f;

            vector.y = (float)Math.Round(vector.y, digits);
            if (Mathf.Approximately(vector.y, 0f))
                vector.y = 0f;

            vector.z = (float)Math.Round(vector.z, digits);
            if (Mathf.Approximately(vector.z, 0f))
                vector.z = 0f;

            return vector;
        }

        /// <summary>
        /// Returns a vector with its y-component set to zero.
        /// </summary>
        /// <param name="vector">The vector to modify.</param>
        /// <returns>The vector with the y-component set to zero.</returns>
        public static Vector3 GetHorizontal(this Vector3 vector) =>
            new Vector3(vector.x, 0f, vector.z);

        /// <summary>
        /// Returns a random float value within the range defined by the vector.
        /// </summary>
        /// <param name="vector">The vector defining the range.</param>
        /// <returns>A random float value within the specified range.</returns>
        public static float GetRandomFromRange(this Vector2 vector) =>
            Random.Range(vector.x, vector.y);

        /// <summary>
        /// Returns a random integer value within the range defined by the vector.
        /// </summary>
        /// <param name="vector">The vector defining the range.</param>
        /// <returns>A random integer value within the specified range.</returns>
        public static int GetRandomFromRange(this Vector2Int vector) =>
            Random.Range(vector.x, vector.y + 1);
    }
}