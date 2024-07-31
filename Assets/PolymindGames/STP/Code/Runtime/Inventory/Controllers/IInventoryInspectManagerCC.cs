using UnityEngine.Events;

namespace PolymindGames
{
    /// <summary>
    /// Manages inventory inspection for a character.
    /// </summary>
    public interface IInventoryInspectManagerCC : ICharacterComponent
    {
        /// <summary> Gets a value indicating whether the character is currently inspecting. </summary>
        bool IsInspecting { get; }

        /// <summary> Gets the workstation being inspected, null for default inspection. </summary>
        IWorkstation Workstation { get; }

        /// <summary> Event triggered when inspection starts. </summary>
        event UnityAction InspectionStarted;

        /// <summary> Event triggered after inspection starts. </summary>
        event UnityAction AfterInspectionStarted;

        /// <summary> Event triggered when inspection ends. </summary>
        event UnityAction InspectionEnded;

        /// <summary> Starts inspection with the specified workstation. </summary>
        /// <param name="workstation">The workstation to inspect. Null for default inspection.</param>
        void StartInspection(IWorkstation workstation);

        /// <summary> Stops inspection. </summary>
        void StopInspection();
    }

}