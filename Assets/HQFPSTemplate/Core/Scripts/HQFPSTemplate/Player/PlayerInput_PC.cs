//-=-=-=-=-=-=- Copyright (c) Polymind Games, All rights reserved. -=-=-=-=-=-=-//
using UnityEngine;

namespace HQFPSTemplate
{
	/// <summary>
	/// Simple component that handles the player input, and feeds it to the other components.
	/// </summary>
	public class PlayerInput_PC : PlayerComponent
	{
		private void Update()
		{
			// Un-paused game Input
			if (!Player.Pause.Active && !Player.ViewLocked.Is(true))
			{
				Check_InteractionInput();

				if(!Player.MovementLocked.Val)
					Check_MovementInput();

				if (!Player.ItemUsingLocked.Val)
					Check_EquipmentInput();
			}
			// Paused game Input
			else
			{
				Player.MoveInput.Set(Vector2.zero);
				Player.LookInput.Set(Vector2.zero);
			}

			var scrollWheelValue = Input.GetAxisRaw("Mouse ScrollWheel");
			Player.ScrollValue.Set(scrollWheelValue);
		}

		private void Check_InteractionInput()
		{
			// Interact
			Player.Interact.Set(Input.GetButton("Interact"));
		}

		private void Check_MovementInput() 
		{
			// Movement.
			Vector2 moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
			Player.MoveInput.Set(moveInput);

			// Look.
			Player.LookInput.Set(new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")));

			// Jump.
			if (Input.GetButtonDown("Jump"))
				Player.Jump.TryStart();

			bool sprintButtonHeld = Input.GetButton("Run");

			// Run.
			if (!Player.Run.Active)
			{
				bool canStartSprinting = sprintButtonHeld && moveInput.y > 0f;

				if (canStartSprinting && Player.Crouch.Active)
					canStartSprinting = Input.GetButtonDown("Run");

				if (canStartSprinting)
					Player.Run.TryStart();
			}
			else
			{
				if (Player.Run.Active && !sprintButtonHeld)
					Player.Run.ForceStop();
			}

			// Crouch.
			if (Input.GetButtonDown("Crouch"))
			{
				if (!Player.Crouch.Active)
					Player.Crouch.TryStart();
				else
					Player.Crouch.TryStop();
			}
			// Prone.
			else if (Input.GetButtonDown("Prone"))
			{
				if (!Player.Prone.Active)
					Player.Prone.TryStart();
				else
					Player.Prone.TryStop();
			}
		}

		private void Check_EquipmentInput()
		{
			if (Input.GetButtonDown("Drop") && !Player.EquippedItem.Is(null) && !Player.Reload.Active && !Player.Healing.Active)
			{
				Player.DropItem.Try(Player.EquippedItem.Get());
				return;
			}

			// Change use mode
			if (Input.GetButtonDown("ChangeUseMode"))
				Player.ChangeUseMode.Try();

			bool alternateUseButtonHeld = Input.GetButton("AlternateUse");

			// Use item
			if (Input.GetButtonDown("UseEquipment"))
				Player.UseItem.Try(false, alternateUseButtonHeld ? 1 : 0);
			else if (Input.GetButton("UseEquipment"))
				Player.UseItem.Try(true, alternateUseButtonHeld ? 1 : 0);

			if (Input.GetButtonDown("ReloadEquipment"))
				Player.Reload.TryStart();

			// Aim
			var aimButtonPressed = Input.GetButton("Aim");

			if (!Player.Aim.Active && aimButtonPressed)
				Player.Aim.TryStart();
			else if (Player.Aim.Active && !aimButtonPressed)
				Player.Aim.ForceStop();

			//Heal
			if (Input.GetButton("Heal") && !aimButtonPressed)
				Player.Healing.TryStart();
		}
	}
}
