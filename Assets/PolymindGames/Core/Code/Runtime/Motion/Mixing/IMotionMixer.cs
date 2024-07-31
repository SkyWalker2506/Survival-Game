using UnityEngine;

namespace PolymindGames.ProceduralMotion
{
    public interface IMotionMixer : IMonoBehaviour
    {
        Transform TargetTransform { get; }
        float WeightMultiplier { get; set; }

        void ResetMixer(Transform targetTransform, Vector3 pivotOffset, Vector3 positionOffset, Vector3 rotationOffset);

        T GetMotionOfType<T>() where T : class, IMixedMotion;
        bool TryGetMotionOfType<T>(out T motion) where T : class, IMixedMotion;

        void AddMixedMotion(IMixedMotion mixedMotion);
        void RemoveMixedMotion(IMixedMotion mixedMotion);
    }
}