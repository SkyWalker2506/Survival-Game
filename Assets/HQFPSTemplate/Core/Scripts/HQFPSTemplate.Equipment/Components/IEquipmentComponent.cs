//-=-=-=-=-=-=- Copyright (c) Polymind Games, All rights reserved. -=-=-=-=-=-=-//
namespace HQFPSTemplate.Equipment
{
    public interface IEquipmentComponent
    {
        void Initialize(EquipmentItem equipmentItem);
        void OnSelected();
    }
}
