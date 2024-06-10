//-=-=-=-=-=-=- Copyright (c) Polymind Games, All rights reserved. -=-=-=-=-=-=-//
using Random = UnityEngine.Random;
using UnityEngine;
using System;

namespace HQFPSTemplate
{
	[Serializable]
	public class SoundPlayer : ICloneable
	{
		public int ClipCount { get { return m_Clips.Count; } }

		[SerializeField]
		[Reorderable]
		private AudioClipList m_Clips = null;

		[Space(5f)]

		[SerializeField]
		[SimpleMinMax(0,1)]
		private Vector2 m_VolumeRange = new Vector2(0.5f, 0.75f);

		[SerializeField]
		[SimpleMinMax(0.5f, 1.5f)]
		private Vector2 m_PitchRange = new Vector2(0.9f, 1.1f);

		[SerializeField]
		[Range(0f, 1f)]
		private float m_VolumeMultiplier = 1f;

		private int m_LastClipPlayed = -1;


		public object Clone() => MemberwiseClone();

		public void Play(AudioSource audioSource, float volume = 1f)
		{
			Play(ItemSelection.Method.RandomExcludeLast, audioSource, volume);
		}

		public void Play(ItemSelection.Method selectionMethod, AudioSource audioSource, float volume = 1f)
		{
			if(!audioSource || m_Clips.Count == 0)
				return;

			if(m_LastClipPlayed >= m_Clips.Count || m_LastClipPlayed <= -1)
				m_LastClipPlayed = m_Clips.Count - 1;

			AudioClip clipToPlay = m_Clips.List.Select(ref m_LastClipPlayed, selectionMethod);

			if (clipToPlay != null)
			{
				var finalVolume = GetVolume() * volume;
				audioSource.pitch = Random.Range(m_PitchRange.x, m_PitchRange.y);

				audioSource.PlayOneShot(clipToPlay, finalVolume);
			}
		}

		/// <summary>
		/// Will use the AudioSource.PlayClipAtPoint() method, which doesn't include pitch variation.
		/// </summary>
		public void PlayAtPosition(ItemSelection.Method selectionMethod, Vector3 position, float volume = 1f)
		{
			if(m_Clips.Count == 0)
				return;

			AudioClip clipToPlay = m_Clips.List.Select(ref m_LastClipPlayed, selectionMethod);

			AudioSource.PlayClipAtPoint(clipToPlay, position, GetVolume() * volume);
		}

		public void Play2D(ItemSelection.Method selectionMethod = ItemSelection.Method.RandomExcludeLast, float volume = 1f)
		{
			if(m_Clips.Count == 0)
				return;

			AudioClip clipToPlay = m_Clips.List.Select(ref m_LastClipPlayed, selectionMethod);

			AudioUtils.Instance.Play2D(clipToPlay, GetVolume() * volume);
		}

		private float GetVolume()
		{
			return Random.Range(m_VolumeRange.x, m_VolumeRange.y) * m_VolumeMultiplier;
		}
	}
}