using PolymindGames.WieldableSystem;
using UnityEditor;

namespace PolymindGamesEditor.WieldableSystem
{
    [CustomEditor(typeof(Firearm))]
    public sealed class FirearmEditor : WieldableEditor
    {
        private FirearmAttachmentDrawer[] _attachmentDrawers;
        private Firearm _firearm;

        private static bool s_AttachmentsFoldout;


        protected override void DrawChildInspector()
        {
            base.DrawChildInspector();

            EditorGUILayout.Space();
            s_AttachmentsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(s_AttachmentsFoldout, "Attachments");
            EditorGUILayout.EndFoldoutHeaderGroup();

            if (s_AttachmentsFoldout)
            {
                _attachmentDrawers ??= CreateAttachmentDrawers(_firearm);
                foreach (var drawer in _attachmentDrawers)
                {
                    GuiLayout.Separator();
                    drawer.Draw();
                }
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _firearm = (Firearm)target;
        }

        private static FirearmAttachmentDrawer[] CreateAttachmentDrawers(Firearm firearm)
        {
            return new FirearmAttachmentDrawer[]
            {
                new FirearmAttachmentDrawer<FirearmTriggerBehaviour>(firearm, "Trigger"), new FirearmAttachmentDrawer<FirearmSightBehaviour>(firearm, "Aimer (Sight)"), new FirearmAttachmentDrawer<FirearmFiringSystemBehaviour>(firearm, "Shooter (Firing System)"), new FirearmAttachmentDrawer<FirearmStorageAmmoBehaviour>(firearm, "Ammo (Storage)"), new FirearmAttachmentDrawer<FirearmMagazineBehaviour>(firearm, "Reloader (Magazine)"), new FirearmAttachmentDrawer<FirearmRecoilStockBehaviour>(firearm, "Recoil (Stock)"), new FirearmAttachmentDrawer<FirearmProjectileEffectBehaviour>(firearm, "Projectile Effect (Bullet)"), new FirearmAttachmentDrawer<FirearmCasingEjectorBehaviour>(firearm, "Casing Handler (Ejection System)"),
                new FirearmAttachmentDrawer<FirearmBarrelBehaviour>(firearm, "Muzzle Effect (Barrel)")
            };
        }
    }
}