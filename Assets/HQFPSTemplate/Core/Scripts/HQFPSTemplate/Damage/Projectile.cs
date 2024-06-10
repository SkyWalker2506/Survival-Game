//-=-=-=-=-=-=- Copyright (c) Polymind Games, All rights reserved. -=-=-=-=-=-=-//
using UnityEngine;

namespace HQFPSTemplate
{
	public abstract class Projectile : MonoBehaviour
	{
		public abstract void Launch(Entity launcher);
	}
}