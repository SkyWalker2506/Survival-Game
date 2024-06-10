//-=-=-=-=-=-=- Copyright (c) Polymind Games, All rights reserved. -=-=-=-=-=-=-//
using UnityEngine;

namespace HQFPSTemplate
{
    public class MultilineCustom : PropertyAttribute
    {
        public readonly int Lines;


        public MultilineCustom(int lines = 3)
        {
            Lines = lines;
        }
    }
}