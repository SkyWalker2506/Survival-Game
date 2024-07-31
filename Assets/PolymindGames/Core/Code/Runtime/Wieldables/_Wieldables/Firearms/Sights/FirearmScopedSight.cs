using UnityEngine;
using UnityEngine.Events;

namespace PolymindGames.WieldableSystem
{
    [AddComponentMenu("Polymind Games/Wieldables/Firearms/Aimers/Scoped Sight")]
    public sealed class FirearmScopedSight : FirearmBasicSight, IScopeHandler
    {
        [SerializeField, Range(0f, 10f), BeginGroup("Scope")]
        private float _scopeEnableDelay = 0.3f;

        [SerializeField, Range(-1, 24), EndGroup]
        private int _scopeIndex = 0;

        // [SerializeField, Range(0, 8)]
        // private int _maxZoomLevel;

        private Coroutine _aimCoroutine;


        public int ZoomLevel { get => 0; set { } }
        public bool IsScopeEnabled => _aimCoroutine != null;
        public int ScopeIndex => _scopeIndex;
        public int MaxZoomLevel => 0;

        public event UnityAction<bool> ScopeEnabled;

        public override bool StartAim()
        {
            if (!base.StartAim())
                return false;

            _aimCoroutine = CoroutineUtils.InvokeDelayed(this, EnableScope, _scopeEnableDelay);
            return true;
        }

        public override bool EndAim()
        {
            if (!base.EndAim())
                return false;

            CoroutineUtils.StopCoroutine(this, ref _aimCoroutine);
            DisableScope();
            return true;
        }

        protected override FieldOfViewParams GetFieldOfViewParams(bool aim)
        {
            return !aim ?
                new FieldOfViewParams(0f, 0f, 1f, 0f, 1f)
                : base.GetFieldOfViewParams(true);
        }

        private void EnableScope()
        {
            Wieldable.IsGeometryVisible = false;
            ScopeEnabled?.Invoke(true);
        }
        
        private void DisableScope()
        {
            Wieldable.IsGeometryVisible = true;
            ScopeEnabled?.Invoke(false);
        }
    }
}