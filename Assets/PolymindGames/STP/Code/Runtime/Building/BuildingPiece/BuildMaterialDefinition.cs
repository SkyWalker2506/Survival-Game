using UnityEngine;

namespace PolymindGames.BuildingSystem
{
    /// <summary>
    /// Definition for a build material used in construction.
    /// </summary>
    [CreateAssetMenu(menuName = "Polymind Games/Building/Build Material Definition", fileName = "BuildMaterial_")]
    public sealed class BuildMaterialDefinition : DataDefinition<BuildMaterialDefinition>
    {
        [SerializeField, NewLabel("Name"), BeginGroup]
        [Tooltip("The name of the build material")]
        private string _buildMaterialName; 

        [SerializeField]
        [Tooltip("The icon representing the build material")]
        private Sprite _icon; 

        [SerializeField, EndGroup]
        [Tooltip("The audio data for using the build material")]
        private AudioDataSO _useAudio;
        

        /// <summary>
        /// Gets or sets the name of the build material.
        /// </summary>
        public override string Name
        {
            get => _buildMaterialName;
            protected set => _buildMaterialName = value;
        }

        /// <summary>
        /// Gets the icon representing the build material.
        /// </summary>
        public override Sprite Icon => _icon;

        /// <summary>
        /// Gets the audio data for using the build material.
        /// </summary>
        public AudioDataSO UseAudio => _useAudio;
    }

}