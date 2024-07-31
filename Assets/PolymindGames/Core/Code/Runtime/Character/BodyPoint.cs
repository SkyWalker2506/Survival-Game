using System.Collections.Generic;
using System.Linq;
using System;

namespace PolymindGames
{
    public enum BodyPoint
    {
        Head = 0,
        Torso = 1,
        Feet = 2,
        Legs = 4,
        Hands = 8
    }

    // [Flags]
    // public enum BodyPointFlags
    // {
    //     Head = 0,
    //     Torso = 1,
    //     Feet = 2,
    //     Legs = 4,
    //     Hands = 8
    // }

    public static class BodyPointUtils
    {
        public static readonly BodyPoint[] BodyPoints = Enum.GetValues(typeof(BodyPoint)).Cast<BodyPoint>().ToArray();
        public static readonly int BodyPointsCount = BodyPoints.Length;

        private static readonly Dictionary<BodyPoint, int> s_BodyPointIndexMap = GetBodyPointsDictionary();


        // public static bool ContainsFlag(this BodyPointFlags thisFlags, BodyPointFlags flags) =>
        //     (thisFlags & flags) == flags;

        public static int GetIndex(this BodyPoint bodyPoint) => s_BodyPointIndexMap[bodyPoint];

        private static Dictionary<BodyPoint, int> GetBodyPointsDictionary()
        {
            var dict = new Dictionary<BodyPoint, int>(BodyPoints.Length);
            for (int i = 0; i < BodyPoints.Length; i++)
                dict[(BodyPoint)BodyPoints.GetValue(i)] = i;
            return dict;
        }
    }
}
