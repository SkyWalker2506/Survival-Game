using System;
using PolymindGames.ResourceGathering;
using UnityEngine;

namespace PolymindGames.UserInterface
{
    [DefaultExecutionOrder(ExecutionOrderConstants.SCENE_SINGLETON)]
    public sealed class TreeHealthIndicatorUI : MonoBehaviour
    {
        [SerializeField, BeginGroup("References")]
        [Tooltip("The renderer used for displaying the health indicator.")]
        private Renderer _renderer;

        [SerializeField, EndGroup]
        [Tooltip("The animator responsible for animation control.")]
        private Animator _animator;

        [SerializeField, Range(0f, 30f), BeginGroup("Settings")]
        [Tooltip("The minimum distance at which the health indicator is visible.")]
        private float _minShowDistance = 1.5f;

        [SerializeField, Range(0f, 30f)]
        [Tooltip("The maximum distance at which the health indicator is visible.")]
        private float _maxShowDistance = 5.5f;

        [SerializeField, Range(0f, 120f)]
        [Tooltip("The angle within which the health indicator is visible.")]
        private float _showAngle = 60f;

        [SerializeField, Range(0f, 30f)]
        [Tooltip("The speed at which the opacity of the health indicator changes.")]
        private float _opacityLerpSpeed = 6f;

        [SerializeField, SpaceArea]
        [Tooltip("Determines if the health indicator rotates with the player.")]
        private bool _rotateWithPlayer = false;

        [SerializeField, EndGroup]
        [Tooltip("The gradient representing the colors indicating health status.")]
        private Gradient _healthColorGradient;

        private static readonly int s_ShowAnimHash = Animator.StringToHash("Show");
        private static readonly int s_DamageAnimHash = Animator.StringToHash("Damage");
        private static readonly int s_AlphaShaderId = Shader.PropertyToID("_Alpha");
        private static readonly int s_HealthAmountShaderId = Shader.PropertyToID("_Health_Amount");
        private static readonly int s_HealthColorShaderId = Shader.PropertyToID("_Health_Color");
        private const float DETECTION_COOLDOWN = 0.5f;
        
        private GatherableDefinition[] _definitions;
        private IGatherable _activeTree;
        private float _detectionTimer;
        private float _lastHealth;
        private float _opacity;
        
        
        public static TreeHealthIndicatorUI Instance { get; private set; }

        // Method to show the indicator for given gatherable definitions
        public void ShowIndicator(GatherableDefinition[] definitions)
        {
            // Store gatherable definitions and enable indicator
            _definitions = definitions;
            enabled = true;
        }

        // Method to hide the indicator
        public void HideIndicator()
        {
            // Clear reference to closest tree, set opacity to 0, and disable indicator
            _activeTree = null;
            SetOpacity(0f);
            enabled = false;
        }

        private void Awake()
        {
            // Disable script and initialize shader properties
            enabled = false;

            // Initialize indicator properties
            _renderer.material.SetFloat(s_HealthAmountShaderId, 1f);
            _renderer.material.SetColor(s_HealthColorShaderId, _healthColorGradient.Evaluate(1));

            SetOpacity(0f);
            SetHealth(0f);

            // Ensure only one instance of TreeIndicator exists
            if (Instance == null)
                Instance = this;
        }

        private void OnDestroy()
        {
            // Clear singleton instance when destroyed
            if (Instance == this)
                Instance = null;
        }

        private void FixedUpdate()
        {
            // Update tree detection periodically
            if (Time.fixedTime > _detectionTimer)
            {
                _detectionTimer = Time.fixedTime + DETECTION_COOLDOWN;
                var tree = FindTree(Player.LocalPlayer.transform);
                SetTree(tree);
            }

            UpdateOpacity();
            UpdateHealth();

            // Rotate indicator with player if enabled
            if (_rotateWithPlayer)
                RotateWithPlayer();
        }

        // Find the nearest valid tree within the detection radius and angle
        private IGatherable FindTree(Transform targetTransform)
        {
            Vector3 targetPosition = targetTransform.position;

            // Find colliders within detection radius
            int count = PhysicsUtils.OverlapSphereOptimized(targetPosition, _maxShowDistance, out var colliders, 1 << LayerConstants.DYNAMIC_OBJECTS, QueryTriggerInteraction.Ignore);

            IGatherable closestTree = null;
            float closestDistance = float.MaxValue;
            
            // Iterate through colliders to find valid trees
            for (int i = 0; i < count; i++)
            {
                var col = colliders[i];
                if (col.TryGetComponent(out IGatherable tree) && Array.IndexOf(_definitions, tree.Definition) != -1 && tree.IsAlive)
                {
                    Vector3 gatherPosition = tree.GetGatherPosition();
                    float treeDistance = Vector3.Distance(targetPosition, gatherPosition);
                    float treeAngle = Vector3.Angle(gatherPosition - targetPosition, targetTransform.forward);

                    // Check if tree is within distance and angle limits
                    if (treeDistance < closestDistance && treeDistance < _maxShowDistance && treeAngle < _showAngle && tree.Health > Mathf.Epsilon)
                    {
                        closestTree = tree;
                        closestDistance = treeDistance;
                    }
                }
            }

            return closestTree;
        }

        // Set the active tree and update indicator position and health
        private void SetTree(IGatherable tree)
        {
            if (tree == null)
            {
                _activeTree = null;
                return;
            }

            // Trigger show animation if a new tree is selected
            if (tree != _activeTree)
                _animator.SetTrigger(s_ShowAnimHash);

            // Set position to the tree's gather position
            transform.position = tree.GetGatherPosition();

            // Set active tree reference
            _activeTree = tree;
        }

        // Rotate the indicator to face the player
        private void RotateWithPlayer()
        {
            Quaternion rot = Quaternion.LookRotation(transform.position - Player.LocalPlayer.transform.position, Vector3.up);
            rot.eulerAngles = new Vector3(0, rot.eulerAngles.y, 0);
            transform.rotation = rot;
        }
        
        // Update the opacity of the indicator based on the distance to the active tree
        private void UpdateOpacity()
        {
            float opacity;

            // Check if an active tree exists and it is alive
            if (_activeTree != null && _activeTree.IsAlive)
            {
                // Calculate the distance between the player and the active tree
                float distance = Vector3.Distance(Player.LocalPlayer.transform.position, _activeTree.GetGatherPosition());
                
                // Calculate opacity based on the distance
                opacity = (_maxShowDistance - distance) / (_maxShowDistance - _minShowDistance);
            }
            else
            {
                // Fade out the opacity if there is no active tree or it's not alive
                opacity = Mathf.Lerp(_opacity, 0f, Time.deltaTime * _opacityLerpSpeed);
            }

            // Apply the calculated opacity to the indicator
            SetOpacity(opacity);
        }

        // Set the opacity of the indicator
        private void SetOpacity(float opacity)
        {
            _opacity = opacity;
            _renderer.material.SetFloat(s_AlphaShaderId, _opacity);
        }

        // Update the health display of the indicator based on the active tree's health
        private void UpdateHealth()
        {
            if (_activeTree == null)
                return;
            
            // Calculate the health value normalized to [0, 1]
            float health = _activeTree.Health / Gatherable.MAX_HEALTH;

            // Check if the active tree is alive and its health has changed
            if (!_activeTree.IsAlive || Mathf.Approximately(health, _lastHealth))
                return;

            // Set the health value and color on the indicator material
            SetHealth(health);

            // Trigger the damage animation to provide visual feedback
            _animator.SetTrigger(s_DamageAnimHash);

            // Update the last known health value
            _lastHealth = health;
        }

        // Set the health value and color on the indicator material
        private void SetHealth(float health)
        {
            // Calculate the color based on the health value using a gradient
            Color healthColor = _healthColorGradient.Evaluate(health);

            // Set the health amount and color on the indicator material
            _renderer.material.SetFloat(s_HealthAmountShaderId, health);
            _renderer.material.SetColor(s_HealthColorShaderId, healthColor);
        }
    }
}