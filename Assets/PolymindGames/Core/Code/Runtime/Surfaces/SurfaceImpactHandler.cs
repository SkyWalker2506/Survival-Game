using UnityEngine;
using System;

namespace PolymindGames.SurfaceSystem
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Collider))]
    public sealed class SurfaceImpactHandler : MonoBehaviour
    {
	    [Flags]
	    private enum ImpactPlayFlags : byte
	    {
		    PlayAudio = 1,
		    PlayParticles = 2
	    }

		[SerializeField, BeginGroup]
		private ImpactPlayFlags _impactFlags = ImpactPlayFlags.PlayAudio;

		[SerializeField, Range(0f, 1f), EndGroup]
		private float _volumeMultiplier = 1f;

		private SurfaceDefinition.EffectPair _impactEffect;
		private float _playEffectsTimer;

		private const float PLAY_COOLDOWN = 0.4f;
		private const float MIN_SPEED_THRESHOLD = 2.5f;
		
		
		private void OnCollisionEnter(Collision col)
		{
			if (Time.time < _playEffectsTimer)
				return;
			
			float relativeVelocityMagnitude = col.relativeVelocity.magnitude;
			if (relativeVelocityMagnitude < MIN_SPEED_THRESHOLD)
				return;
			
			_playEffectsTimer = Time.time + PLAY_COOLDOWN;

			if ((_impactFlags & ImpactPlayFlags.PlayAudio) == ImpactPlayFlags.PlayAudio)
			{
				float volumeMod = Mathf.Clamp(relativeVelocityMagnitude / MIN_SPEED_THRESHOLD / 10, 0.2f, 1f);
				volumeMod *= _volumeMultiplier;

				var audioData = _impactEffect.AudioData;
				if (audioData.IsPlayable)
					AudioManager.Instance.PlayClipAtPoint(audioData.Clip, transform.position, audioData.Volume * volumeMod, audioData.Pitch);
			}

			if ((_impactFlags & ImpactPlayFlags.PlayParticles) == ImpactPlayFlags.PlayParticles &&
			    _impactEffect.VisualEffect != null)
			{
				Vector3 contactPoint = col.contactCount > 0 ? col.GetContact(0).point : transform.position;
				Instantiate(_impactEffect.VisualEffect, contactPoint, Quaternion.identity);
			}
		}

        private void Awake()
        {
			if (!TryGetImpactEffect(out _impactEffect))
			{
				Debug.LogWarning("No corresponding surface definition found for this object, removing this component.", gameObject);
				Destroy(this);
			}
        }

        private bool TryGetImpactEffect(out SurfaceDefinition.EffectPair effect)
        {
			var colMaterial = GetComponent<Collider>().sharedMaterial;
			var surface = SurfaceManager.Instance.GetSurfaceFromMaterial(colMaterial);

			if (surface != null)
			{
				effect = surface.ImpactEffect;
				return true;
			}

			effect = null; 
			return false;
        }
	}
}