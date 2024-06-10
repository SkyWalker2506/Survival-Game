using UnityEngine;

using Raymarcher.Objects.Modifiers;
using Raymarcher.Objects.Primitives;

namespace Raymarcher.ExampleContent
{
    public sealed class RMSample_Operations : MonoBehaviour
    {
        [SerializeField] private RMSdf_Torus sub;
        [SerializeField] private RMModifier_Morph morph;
        [SerializeField] private RMModifier_Intersect inter;

        public void ChangeSubtraction(float val)
        {
            sub.torusRadius = val;
        }

        public void ChangeMorph(float val)
        {
            morph.morphValue = val;
        }

        public void ChangeInter(float val)
        {
            inter.intersectSmoothness = val;
        }
    }
}