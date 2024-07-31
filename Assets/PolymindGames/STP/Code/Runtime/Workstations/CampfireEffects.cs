using UnityEngine;

namespace PolymindGames.BuildingSystem
{
    [RequireComponent(typeof(CookingStation))]
    public sealed class CampfireEffects : MonoBehaviour
    {
        [SerializeField, NotNull, BeginGroup("Material")]
        private GameObject _wood;
        
        [SerializeField, NotNull]
        private Material _woodMaterial;

        [SerializeField, Range(0f, 600f), EndGroup]
        private float _woodBurnDuration = 30f;
        
        [SerializeField, NotNull, BeginGroup("Audio")]
        private AudioEffect _audioEffect;

        [SerializeField, Range(0f, 1f), EndGroup]
        private float _minFireVolume = 0.5f;

        [SerializeField, NotNull, BeginGroup("Light")]
        private LightEffect _lightEffect;

        [SerializeField, Range(0f, 1f), EndGroup]
        private float _minLightIntensity = 0.5f;
        
        [SpaceArea, BeginGroup("Particles"), EndGroup]
        [SerializeField, ReorderableList(ListStyle.Lined, HasLabels = false, HasHeader = false)]
        private ParticleSystem[] _particleEffects;

        private static readonly int s_BurnedAmountShaderId = Shader.PropertyToID("_Burned_Amount");
        private CookingStation _station;
        private float _lastFuelAddTime;


        private void Awake()
        {
            CreateWoodMaterial();

            _station = GetComponent<CookingStation>();
            _station.CookingStarted += OnFireStarted;
            _station.CookingStopped += OnFireStopped;
            _station.FuelAdded += OnFuelAdded;

            enabled = false;
        }

        private void OnDestroy()
        {
            _station.CookingStarted -= OnFireStarted;
            _station.CookingStopped -= OnFireStopped;
            _station.FuelAdded -= OnFuelAdded;
        }

        /// <summary>
        /// Create and assign a new material instance to the wood.
        /// </summary>
        private void CreateWoodMaterial()
        {
            _woodMaterial = new Material(_woodMaterial);
            _woodMaterial.SetFloat(s_BurnedAmountShaderId, 0f);

            var renderers = _wood.GetComponentsInChildren<Renderer>(true);
            foreach (var rend in renderers)
                rend.material = _woodMaterial;

            _wood.SetActive(false);
        }

        private void OnFireStarted()
        {
            foreach (var effect in _particleEffects)
                effect.Play(false);

            _wood.SetActive(true);
            _lightEffect.Play();
            _audioEffect.Play();

            enabled = true;
        }

        private void OnFireStopped()
        {
            foreach (var effect in _particleEffects)
                effect.Stop(false);

            _lightEffect.Stop();
            _audioEffect.Stop();

            enabled = false;
        }

        private void OnFuelAdded(int fuelDuration) => _lastFuelAddTime = Time.fixedTime;

        private void FixedUpdate()
        {
            float cookingStrength = _station.CookingStrength;
            
            _audioEffect.VolumeMultiplier = Mathf.Lerp(_minFireVolume, Mathf.Max(cookingStrength, _minFireVolume), cookingStrength);
            _lightEffect.Multiplier = Mathf.Lerp(_minLightIntensity, Mathf.Max(cookingStrength, _minLightIntensity), cookingStrength);

            float burnedAmount = Mathf.Clamp01((Time.fixedTime - _lastFuelAddTime) / _woodBurnDuration);
            _woodMaterial.SetFloat(s_BurnedAmountShaderId, burnedAmount);
        }
    }
}