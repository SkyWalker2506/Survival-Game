using UnityEngine;
// ReSharper disable InconsistentNaming

namespace PolymindGames.WieldableSystem
{
    /// <summary>
    /// You can extend this by creating another partial class with the same name.
    /// </summary>
    public static partial class WieldableAnimationConstants
    {
        // General Params.
        public static readonly int EQUIP = Animator.StringToHash("Equip");
        public static readonly int HOLSTER = Animator.StringToHash("Holster");
        public static readonly int EQUIP_SPEED = Animator.StringToHash("Equip Speed");
        public static readonly int HOLSTER_SPEED = Animator.StringToHash("Holster Speed");
        public static readonly int IS_VISIBLE = Animator.StringToHash("Is Visible");
        public static readonly int USE = Animator.StringToHash("Use");
        
        // Firearm Params.
        public static readonly int SHOOT = Animator.StringToHash("Shoot");
        public static readonly int SHOOT_SPEED = Animator.StringToHash("Shoot Speed");
        public static readonly int DRY_FIRE = Animator.StringToHash("Dry Fire");
        public static readonly int IS_AIMING = Animator.StringToHash("Is Aiming");
        public static readonly int RELOAD = Animator.StringToHash("Reload");
        public static readonly int RELOAD_START = Animator.StringToHash("Start Reload");
        public static readonly int RELOAD_END = Animator.StringToHash("End Reload");
        public static readonly int RELOAD_SPEED = Animator.StringToHash("Reload Speed");
        public static readonly int EMPTY_RELOAD = Animator.StringToHash("Empty Reload");
        public static readonly int IS_CHARGING = Animator.StringToHash("Is Charging");
        public static readonly int FULL_CHARGE = Animator.StringToHash("Full Charge");
        public static readonly int EJECT = Animator.StringToHash("Eject");
        public static readonly int CHANGE_MODE = Animator.StringToHash("Change Mode");

        // Melee Params.
        public static readonly int ATTACK = Animator.StringToHash("Attack");
        public static readonly int ATTACK_HIT = Animator.StringToHash("Attack Hit");
        public static readonly int ATTACK_INDEX = Animator.StringToHash("Attack Index");
        public static readonly int ATTACK_SPEED = Animator.StringToHash("Attack Speed");
        public static readonly int IS_THROWN = Animator.StringToHash("Is Thrown");
    }
}
