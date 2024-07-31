using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace PolymindGames.WieldableSystem
{
    [AddComponentMenu("Polymind Games/Wieldables/Firearms/Firearm")]
    public sealed class Firearm : Wieldable, IFirearm, IUseInputHandler, IAimInputHandler, IReloadInputHandler
    {
        [SerializeField, Range(0f, 1f), NewLabel("Hip Fire Accuracy")]
        [Tooltip("Modifier for accuracy when firing from the hip.")]
        private float _hipAccuracyMod = 0.9f;

        private bool _requiresEject;
        private bool _isShooting;

        private const float ACTION_COOLDOWN = 0.3f;
        
        
        protected override void OnCharacterChanged(ICharacter character)
        {
            if (character == null || !character.TryGetCC(out _characterAccuracy))
                _characterAccuracy = new NullAccuracyHandler();
        }

        private void Shoot(float triggerValue)
        {
            // Check if the firearm has enough ammo in the reloader/magazine.
            bool hasEnoughAmmo = GameplayOptions.Instance.InfiniteMagazineAmmo
                                 || _magazine.TryUseAmmo(_firingSystem.AmmoPerShot);

            if (!hasEnoughAmmo)
                return;

            _isShooting = true;
            _firingSystem.Shoot(Accuracy, _projectileEffect, triggerValue);
            _shootInaccuracy += _sight.IsAiming ? _recoilStock.AimAccuracyKick : _recoilStock.HipfireAccuracyKick;

            _heatUpdateTimer = Time.time + _recoilStock.RecoilHeatRecoverDelay;
            _heatValue = Mathf.Clamp01(_heatValue + 1 / (float)Mathf.Clamp(_magazine.MagazineSize, 0, 30));
            _recoilStock.DoRecoil(_sight.IsAiming, _heatValue, triggerValue);

            _barrel.DoFireEffect();

            // if (GameplayOptions.Instance.ManualCasingEjection && _casingEjector.EjectDuration > 0.01f)
            //     _requiresEject = true;
            // else
                _casingEjector.Eject();

            ReloadBlocker.AddDurationBlocker(ACTION_COOLDOWN);
        }

        private void Start()
        {
            AimBlocker.OnBlocked += ForceEndAiming;
            UseBlocker.OnBlocked += ForceReleaseTrigger;
            ReloadBlocker.OnBlocked += ForceCancelReload;

            return;

            void ForceReleaseTrigger() => _trigger.ReleaseTrigger();
            void ForceEndAiming() => EndAim();
            void ForceCancelReload() => EndReload();
        }

        private void FixedUpdate()
        {
            float deltaTime = Time.fixedDeltaTime;
            UpdateAccuracy(deltaTime);
            UpdateHeatValue(deltaTime);

            if (_heatUpdateTimer < Time.fixedTime && _isShooting)
            {
                _barrel.DoFireStopEffect();
                _isShooting = false;
            }
        }

        #region Editor
#if UNITY_EDITOR
        protected override void DrawDebugGUI()
        {
            using (new UnityEditor.EditorGUI.IndentLevelScope())
            {
                GUILayout.Label($"Is Aiming: {Sight.IsAiming}");
                GUILayout.Label($"Is Reloading: {Magazine.IsReloading}");
                GUILayout.Label($"Ammo In Magazine: {Magazine.AmmoInMagazine}");
                GUILayout.Label($"Is Magazine Empty: {Magazine.IsMagazineEmpty()}");
                GUILayout.Label($"Is Magazine Full: {Magazine.IsMagazineFull()}");
                GUILayout.Label($"Is Trigger Held: {Trigger.IsTriggerHeld}");
                GUILayout.Label($"Current Accuracy: {Math.Round(Accuracy, 2)}");
                GUILayout.Label($"Current Heat Value: {_heatValue}");
                GUILayout.Label($"Current Speed Multiplier: {Math.Round(SpeedModifier.EvaluateValue(), 2)}");
                GUILayout.Label($"Is Use Input Blocked: {UseBlocker.IsBlocked}");
                GUILayout.Label($"Is Aim Input Blocked: {AimBlocker.IsBlocked}");
                GUILayout.Label($"Is Reload Input Blocked: {ReloadBlocker.IsBlocked}");
            }
        }
#endif
        #endregion

        #region Attachments
        private UnityAction[] _attachmentChangedCallbacks;

        private static readonly int s_AttachmentTypeCount = Enum.GetValues(typeof(AttachmentType)).Cast<AttachmentType>().ToArray().Length;
        
        
        public void AddChangedListener(AttachmentType type, UnityAction callback)
        {
            _attachmentChangedCallbacks ??= new UnityAction[s_AttachmentTypeCount];

            int index = (int)type;
            _attachmentChangedCallbacks[index] += callback;
        }

        public void RemoveChangedListener(AttachmentType type, UnityAction callback)
        {
            _attachmentChangedCallbacks ??= new UnityAction[s_AttachmentTypeCount];

            int index = (int)type;
            _attachmentChangedCallbacks[index] -= callback;
        }

        private void RaiseAttachmentChangedEvent(AttachmentType type)
        {
            if (_attachmentChangedCallbacks != null)
            {
                int index = (int)type;
                _attachmentChangedCallbacks[index]?.Invoke();
            }
        }

        private ISight _sight = DefaultFirearmSight.Instance;
        public ISight Sight
        {
            get => _sight;
            set
            {
                if (value != _sight)
                {
                    if (_sight.IsAiming)
                        _sight.EndAim();

                    _sight.Detach();

                    _sight = value ?? DefaultFirearmSight.Instance;
                    _sight.Attach();

                    RaiseAttachmentChangedEvent(AttachmentType.Sight);
                }
            }
        }

        private IFirearmTrigger _trigger = DefaultFirearmTrigger.Instance;
        public IFirearmTrigger Trigger
        {
            get => _trigger;
            set
            {
                if (_trigger != value)
                {
                    bool wasHeld = _trigger.IsTriggerHeld;
                    _trigger.Shoot -= Shoot;
                    _trigger.Detach();

                    _trigger = value ?? DefaultFirearmTrigger.Instance;

                    _trigger.Shoot += Shoot;
                    _trigger.Attach();

                    if (wasHeld)
                        _trigger.HoldTrigger();

                    RaiseAttachmentChangedEvent(AttachmentType.Trigger);
                }
            }
        }

        private IFirearmFiringSystem _firingSystem = DefaultFirearmFiringSystem.Instance;
        public IFirearmFiringSystem FiringSystem
        {
            get => _firingSystem;
            set
            {
                if (value != _firingSystem)
                {
                    _firingSystem.Detach();
                    _firingSystem = value ?? DefaultFirearmFiringSystem.Instance;
                    _firingSystem.Attach();

                    RaiseAttachmentChangedEvent(AttachmentType.Shooter);
                }
            }
        }

        private IFirearmStorageAmmo _storageAmmo = DefaultFirearmStorageAmmo.Instance;
        public IFirearmStorageAmmo StorageAmmo
        {
            get => _storageAmmo;
            set
            {
                if (value != _storageAmmo)
                {
                    _storageAmmo.Detach();
                    _storageAmmo = value ?? DefaultFirearmStorageAmmo.Instance;
                    _storageAmmo.Attach();

                    RaiseAttachmentChangedEvent(AttachmentType.Ammo);
                }
            }
        }

        private IFirearmMagazine _magazine = DefaultFirearmMagazine.Instance;
        public IFirearmMagazine Magazine
        {
            get => _magazine;
            set
            {
                if (value != _magazine)
                {
                    int prevAmmoCount = Magazine.AmmoInMagazine;
                    _magazine.Detach();
                    _magazine = value ?? DefaultFirearmMagazine.Instance;
                    _magazine.ForceSetAmmo(prevAmmoCount);
                    _magazine.Attach();

                    RaiseAttachmentChangedEvent(AttachmentType.Reloader);
                }
            }
        }

        private IFirearmRecoilStock _recoilStock = DefaultFirearmRecoilStock.Instance;
        public IFirearmRecoilStock RecoilStock
        {
            get => _recoilStock;
            set
            {
                if (value != _recoilStock)
                {
                    _recoilStock.Detach();
                    _recoilStock = value ?? DefaultFirearmRecoilStock.Instance;
                    _recoilStock.Attach();

                    RaiseAttachmentChangedEvent(AttachmentType.Recoil);
                }
            }
        }

        private IFirearmProjectileEffect _projectileEffect = DefaultFirearmProjectileEffect.Instance;
        public IFirearmProjectileEffect ProjectileEffect
        {
            get => _projectileEffect;
            set
            {
                if (value != _projectileEffect)
                {
                    _projectileEffect.Detach();
                    _projectileEffect = value ?? DefaultFirearmProjectileEffect.Instance;
                    _projectileEffect.Attach();

                    RaiseAttachmentChangedEvent(AttachmentType.ProjectileEffect);
                }
            }
        }

        private IFirearmCasingEjector _casingEjector = DefaultFirearmCasingEjector.Instance;
        public IFirearmCasingEjector CasingEjector
        {
            get => _casingEjector;
            set
            {
                if (value != _casingEjector)
                {
                    _casingEjector.Detach();
                    _casingEjector = value ?? DefaultFirearmCasingEjector.Instance;
                    _casingEjector.Attach();

                    RaiseAttachmentChangedEvent(AttachmentType.CasingEjector);
                }
            }
        }

        private IFirearmBarrel _barrel = DefaultFirearmBarrel.Instance;
        public IFirearmBarrel Barrel
        {
            get => _barrel;
            set
            {
                if (value != _barrel)
                {
                    _barrel.Detach();
                    _barrel = value ?? DefaultFirearmBarrel.Instance;
                    _barrel.Attach();

                    RaiseAttachmentChangedEvent(AttachmentType.MuzzleEffect);
                }
            }
        }
        #endregion

        #region Input Handling
        public ActionBlockHandler UseBlocker { get; } = new();
        public ActionBlockHandler AimBlocker { get; } = new();
        public ActionBlockHandler ReloadBlocker { get; } = new();
        public bool IsReloading => _magazine.IsReloading;
        public bool IsUsing => _trigger.IsTriggerHeld;
        public bool IsAiming => _sight.IsAiming;

        // Try to use the input to do some custom actions before pressing the trigger.
        //     if (inputPhase == WieldableInputPhase.Start && HandlePreTriggerPressActions())
        //     return;
        //
        // bool canPressTrigger = inputPhase != WieldableInputPhase.End
        //                        && !UseBlocker.IsBlocked
        //                        && !IsReloading;
        //
        //     // Use the input to press or release the trigger.
        //     if (canPressTrigger)
        //     _trigger.HoldTrigger();
        //     else
        // _trigger.ReleaseTrigger();
        
        public bool Use(WieldableInputPhase inputPhase)
        {
            return inputPhase switch
            {
                WieldableInputPhase.Start => StartUse(),
                WieldableInputPhase.Hold => HoldUse(),
                WieldableInputPhase.End => EndUse(),
                _ => false
            };
        }
        
        public bool Aim(WieldableInputPhase inputPhase) => inputPhase switch
        {
            WieldableInputPhase.Start => StartAim(),
            WieldableInputPhase.End => EndAim(),
            _ => false
        };

        public bool Reload(WieldableInputPhase inputPhase) => inputPhase switch
        {
            WieldableInputPhase.Start => StartReload(),
            WieldableInputPhase.End => EndReload(),
            _ => false
        };

        private bool StartUse()
        {
            // Use the input to manually eject the casing (only if manual eject is required).
            if (_requiresEject)
            {
                _casingEjector.Eject();
                UseBlocker.AddDurationBlocker(_casingEjector.EjectDuration);
                _requiresEject = false;
                return true;
            }

            // Use the input to cancel the reload if active.
            if (IsReloading && EndReload())
                return true;

            // If the magazine is empty, try to reload or dry fire.
            if (_magazine.IsMagazineEmpty() && !IsReloading)
            {
                if (GameplayOptions.Instance.AutoReloadOnDry && StartReload())
                    return true;

                _firingSystem.DryFire();
                return true;
            }

            return HoldUse();
        }

        private bool HoldUse()
        {
            if (!UseBlocker.IsBlocked && !IsReloading)
            {
                Trigger.HoldTrigger();
                return true;
            }
                
            Trigger.ReleaseTrigger();
            return false;
        }

        private bool EndUse()
        {
            Trigger.ReleaseTrigger();
            return true;
        }

        private bool StartAim()
        {
            if (AimBlocker.IsBlocked)
                return false;

            if (IsReloading && !GameplayOptions.Instance.CanAimWhileReloading)
                return false;

            return _sight.StartAim();
        }

        private bool EndAim()
        {
            if (_sight.EndAim())
            {
                AimBlocker.AddDurationBlocker(ACTION_COOLDOWN);
                return true;
            }

            return false;
        }

        private bool StartReload()
        {
            if (ReloadBlocker.IsBlocked)
                return false;
            
            var ammo = GameplayOptions.Instance.InfiniteStorageAmmo ? DefaultFirearmStorageAmmo.Instance : StorageAmmo;
            if (_magazine.TryStartReload(ammo))
            {
                UseBlocker.AddDurationBlocker(ACTION_COOLDOWN);

                if (IsAiming && !GameplayOptions.Instance.CanAimWhileReloading)
                    EndAim();

                return true;
            }

            return false;
        }

        private bool EndReload()
        {
            if (GameplayOptions.Instance.CancelReloadOnShoot && !_magazine.IsMagazineEmpty())
            {
                if (_magazine.TryCancelReload(StorageAmmo, out var endDuration))
                {
                    UseBlocker.AddDurationBlocker(endDuration + 0.05f);
                    return true;
                }
            }

            return false;
        }
        #endregion

        #region Accuracy
        private IAccuracyHandlerCC _characterAccuracy;
        private float _shootInaccuracy;
        private float _baseAccuracy = 1f;
        private float _heatUpdateTimer;
        private float _heatValue;


        public override bool IsCrosshairActive() => !UseBlocker.IsBlocked && !Magazine.IsMagazineEmpty();

        private void UpdateAccuracy(float deltaTime)
        {
            _baseAccuracy = _characterAccuracy.GetAccuracyMod() * (Sight.IsAiming ? Sight.AimAccuracyMod : _hipAccuracyMod);
            float targetAccuracy = Mathf.Clamp01(_baseAccuracy - _shootInaccuracy);

            float accuracyRecoverDelta = deltaTime * (Sight.IsAiming ? _recoilStock.AimAccuracyRecover : _recoilStock.HipfireAccuracyRecover);
            _shootInaccuracy = Mathf.Clamp01(_shootInaccuracy - accuracyRecoverDelta);

            Accuracy = targetAccuracy;
        }

        private void UpdateHeatValue(float deltaTime)
        {
            if (_heatUpdateTimer > Time.time)
                deltaTime *= 0.1f;

            float heatRecoverDelta = deltaTime * _recoilStock.RecoilHeatRecover;
            _heatValue = Mathf.Clamp01(_heatValue - Mathf.Max(heatRecoverDelta * _heatValue * 5f, heatRecoverDelta));
        }
        #endregion
    }
}