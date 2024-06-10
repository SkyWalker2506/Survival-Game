//-=-=-=-=-=-=- Copyright (c) Polymind Games, All rights reserved. -=-=-=-=-=-=-//

namespace HQFPSTemplate
{
	public abstract class CloneableObject<T>
	{
		public T GetMemberwiseClone()
		{
			return (T)MemberwiseClone();
		}
	}
}
