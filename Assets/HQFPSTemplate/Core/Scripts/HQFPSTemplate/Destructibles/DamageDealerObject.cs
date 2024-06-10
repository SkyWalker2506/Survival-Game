//-=-=-=-=-=-=- Copyright (c) Polymind Games, All rights reserved. -=-=-=-=-=-=-//
using UnityEngine;

namespace HQFPSTemplate
{
    public abstract class DamageDealerObject : MonoBehaviour
    {
        public virtual void ActivateDamage(Entity source) { }
    }
}
