//-=-=-=-=-=-=- Copyright (c) Polymind Games, All rights reserved. -=-=-=-=-=-=-//
using System;

namespace HQFPSTemplate
{
	[Serializable]
	public struct EasingOptions
	{
		public Easings.Function Function;

		public float Duration;
	}
}