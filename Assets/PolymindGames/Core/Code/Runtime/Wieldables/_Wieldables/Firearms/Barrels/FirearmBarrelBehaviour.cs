namespace PolymindGames.WieldableSystem
{
    public abstract class FirearmBarrelBehaviour : FirearmAttachmentBehaviour, IFirearmBarrel
    {
        public abstract void DoFireEffect();
        public abstract void DoFireStopEffect();

        protected virtual void OnEnable()
        {
            if (Firearm != null)
                Firearm.Barrel = this;
        }
    }
}