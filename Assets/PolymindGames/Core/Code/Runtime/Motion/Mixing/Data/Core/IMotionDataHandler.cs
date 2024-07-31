namespace PolymindGames.ProceduralMotion
{
    public interface IMotionDataHandler
    {
        void SetPreset(MotionProfile profile);
        void RegisterBehaviour<T>(MotionDataChangedDelegate changedCallback);
        void UnregisterBehaviour<T>(MotionDataChangedDelegate changedCallback);
        void SetDataOverride<T>(T data) where T : IMotionData;
        void SetDataOverride(IMotionData data, bool enable);
        T GetData<T>() where T : IMotionData;
        bool TryGetData<T>(out T data) where T : IMotionData;
    }

    public delegate void MotionDataChangedDelegate(IMotionDataHandler dataHandler, bool forceUpdate);
}