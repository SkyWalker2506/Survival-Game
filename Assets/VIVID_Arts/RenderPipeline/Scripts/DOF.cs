using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class DOF : MonoBehaviour
{
    public VolumeProfile Profile;

    public Transform Reference;

    public LayerMask Mask;

    public float MaxDistance;

    public float AdjustmentSpeed;
    
    private DepthOfField DepthOfField { get; set; }
    
    private float Distance { get; set; }

    private void Start()
    {
        DepthOfField = Profile.components.OfType<DepthOfField>().First();
    }

    private void FixedUpdate()
    {
        var reference = Reference ? Reference : transform;

        var position = reference.position;
        
        Physics.Raycast(position, reference.forward, out var hit, float.MaxValue, Mask);

        var distance = hit.point == Vector3.zero ? MaxDistance : Vector3.Distance(position, hit.point);

        if (distance > Distance)
        {
            Distance += AdjustmentSpeed;

            if (Distance > distance)
            {
                Distance = distance;
            }
        }

        if (distance < Distance)
        {
            Distance -= AdjustmentSpeed;

            if (Distance < distance)
            {
                Distance = distance;
            }
        }

        if (Distance > MaxDistance)
        {
            Distance = MaxDistance;
        }

        DepthOfField.focusDistance.value = Distance;
    }
}
