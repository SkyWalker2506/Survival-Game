using UnityEngine;

using Raymarcher.Objects.Fractals;

namespace Raymarcher.ExampleContent
{
    public sealed class RMSample_AudioWaveSampler : MonoBehaviour
    {
        [Space]
        [SerializeField] private AudioSource targetAudio;
        [SerializeField, Attributes.RMAttributes.Required] private RMSdfFractal_Apollonian targetFractalApollonian;
        [Space]
        [SerializeField, Range(8, 2048)] private int sampleDataLength = 1024;
        [Space]
        [SerializeField, Range(0.1f, 64f)] private float transitionSmooth = 8.0f;
        [SerializeField, Range(0.1f, 64f)] private float reactionAmplifier = 1.0f;
        [SerializeField] Vector2 minMax = new Vector2(0.8f, 0.98f);

        private float clipAmplify;
        private float[] clipSampleData;

        private void Awake()
        {
            clipSampleData = new float[sampleDataLength];
        }

        private void Update()
        {
            // We can sample audio waves to any output...

            targetAudio.clip.GetData(clipSampleData, targetAudio.timeSamples);
            clipAmplify = 0.0f;
            foreach (var samp in clipSampleData)
                clipAmplify += Mathf.Abs(samp);
            clipAmplify /= sampleDataLength;

            // Used for changing the fractalSpread field on Apollonian

            targetFractalApollonian.fractalSpread = Mathf.Lerp(targetFractalApollonian.fractalSpread, (clipAmplify * reactionAmplifier), transitionSmooth * Time.deltaTime);
            targetFractalApollonian.fractalSpread = Mathf.Clamp(targetFractalApollonian.fractalSpread, minMax.x, minMax.y);
        }
    }
}