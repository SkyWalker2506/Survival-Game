//-=-=-=-=-=-=- Copyright (c) Polymind Games, All rights reserved. -=-=-=-=-=-=-//
using UnityEngine;

namespace HQFPSTemplate
{
	public class DisableAtStartBehaviour : MonoBehaviour
	{
		[SerializeField]
		private GameObject[] m_Objects = null;


		private void Start()
		{
			foreach (var obj in m_Objects)
				obj.SetActive(false);
		}
	}
}
