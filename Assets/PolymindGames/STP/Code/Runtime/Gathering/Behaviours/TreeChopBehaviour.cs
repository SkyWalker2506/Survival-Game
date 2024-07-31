using UnityEngine;

namespace PolymindGames.ResourceGathering
{
    public sealed class TreeChopBehaviour : MonoBehaviour, ISaveableComponent
    {
        [SerializeField, NotNull, BeginGroup, EndGroup]
        private Transform _segmentsRoot;
        
        private ChoppingSegment[] _choppingSegments;
        private int _choppedSegmentsCount;
        private IGatherable _gatherable;


        private void Awake()
        {
            // Initialize chopping segments array
            _choppingSegments = new ChoppingSegment[_segmentsRoot.childCount];

            // Iterate through child segments
            for (int i = 0; i < _segmentsRoot.childCount; i++)
            {
                Transform segmentTransform = _segmentsRoot.GetChild(i);
                Vector3 segmentNormal = (_segmentsRoot.position - segmentTransform.position).normalized;
                _choppingSegments[i] = new ChoppingSegment(segmentTransform.gameObject, segmentNormal); // Store segment data
            }
        }

        private void OnEnable()
        {
            // Ensure every segment is active
            _choppedSegmentsCount = 0;
            foreach (var segmentObject in _choppingSegments)
                segmentObject.Object.SetActive(true); 
            
            // Get reference to gatherable component
            _gatherable = gameObject.GetComponentInParent<IGatherable>();

            // Subscribe to damage received event
            if (_gatherable != null)
                _gatherable.DamageReceived += OnDamage;
        }

        private void OnDisable()
        {
            // Unsubscribe from damage received event
            if (_gatherable != null)
                _gatherable.DamageReceived -= OnDamage;
        }

        // Event handler for damage received
        private void OnDamage(float damage, in DamageArgs args)
        {
            // Check if tree is still alive
            if (_gatherable.IsAlive)
                UpdateChoppingSegments(args.HitPoint); // Update chopping segments based on hit point
            else
                DisableAllChoppingSegments(); // Disable all chopping segments if the tree is dead
        }

        // Update chopping segments based on hit point
        private void UpdateChoppingSegments(Vector3 hitPoint)
        {
            // Calculate target number of segments to chop
            int targetSegmentsCount = _choppingSegments.Length - (int)(_gatherable.Health / 100f * _choppingSegments.Length);
            int amountToChop = targetSegmentsCount - _choppedSegmentsCount;

            if (amountToChop <= 0)
                return;

            for (int i = 0; i < amountToChop; i++)
            {
                int indexToSelect = 0;

                float largestAngle = 0f;
                Vector3 chopPointNormal = (hitPoint - _segmentsRoot.position).normalized;

                // Find segment with largest angle between chop point normal and segment normal
                for (int j = 0; j < _choppingSegments.Length; j++)
                {
                    if (!_choppingSegments[j].Object.activeSelf)
                        continue;

                    float angle = Vector3.Angle(chopPointNormal, _choppingSegments[j].Normal);

                    if (angle > largestAngle)
                    {
                        largestAngle = angle;
                        indexToSelect = j;
                    }
                }

                _choppingSegments[indexToSelect].Object.SetActive(false); // Disable selected segment
            }

            _choppedSegmentsCount += amountToChop; // Update chopped segments count
        }

        // Disable all chopping segments
        private void DisableAllChoppingSegments()
        {
            foreach (var segment in _choppingSegments)
                segment.Object.SetActive(false);
        }

        #region Internal
        private readonly struct ChoppingSegment
        {
            public readonly GameObject Object;
            public readonly Vector3 Normal;

            public ChoppingSegment(GameObject gameObject, Vector3 normal)
            {
                Object = gameObject;
                Normal = normal;
            }
        }
        #endregion

        #region Save & Load
        private sealed class SaveData
        {
            public int ChoppedSegmentsCount;
            public bool[] ChoppedInfo;
        }

        void ISaveableComponent.LoadMembers(object data)
        {
            if (data == null)
                return;

            var saveData = (SaveData)data;
            _choppedSegmentsCount = saveData.ChoppedSegmentsCount;
            var choppedInfo = saveData.ChoppedInfo;

            for (int i = 0; i < choppedInfo.Length; i++)
            {
                if (!choppedInfo[i])
                    _choppingSegments[i].Object.SetActive(false);
            }
        }

        object ISaveableComponent.SaveMembers()
        {
            if (_choppedSegmentsCount == 0)
                return null;
            
            var choppedInfo = new bool[_choppingSegments.Length];

            for (int i = 0; i < choppedInfo.Length; i++)
                choppedInfo[i] = _choppingSegments[i].Object.activeSelf;

            return new SaveData
            {
                ChoppedSegmentsCount = _choppedSegmentsCount,
                ChoppedInfo = choppedInfo
            };
        }
        #endregion
    }
}