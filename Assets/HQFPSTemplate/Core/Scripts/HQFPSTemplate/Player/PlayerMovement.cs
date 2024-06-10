//-=-=-=-=-=-=- Copyright (c) Polymind Games, All rights reserved. -=-=-=-=-=-=-//
using UnityEngine;

namespace HQFPSTemplate
{
    public class PlayerMovement : PlayerComponent
    {
        public PlayerMovementProfile MovementProfile { get => m_MovementProfile; set { m_MovementProfile = value; } }
        public bool IsGrounded { get => m_Controller.isGrounded; }
        public Vector3 Velocity { get => m_Controller.velocity; }
        public Vector3 SurfaceNormal { get; private set; }
        public float SlopeLimit { get => m_Controller.slopeLimit; }
        public float DefaultHeight { get; private set; }
        public float CurrentStepLength { get; private set; }

        [SerializeField]
        private CharacterController m_Controller = null;

        [SerializeField]
        private LayerMask m_ObstacleCheckMask = ~0;

        [Space]

        [SerializeField]
        private PlayerMovementProfile m_MovementProfile;

        [BHeader("Injured Profile")]

        [SerializeField]
        [Range(-1f, 1000f)]
        private float m_InjuredHealthThreshold;

        [SerializeField]
        private PlayerMovementProfile m_InjuredMovementProfile;

        private PlayerMovementProfile.MovementStateModule m_MM; // Current movement state (e.g. run, crouch etc.)
        private PlayerMovementProfile m_MP; // Current movement profile (e.g. normal state, injured etc.)

        private Vector3 m_DesiredVelocityLocal;
        private Vector3 m_SlideVelocity;

        private CollisionFlags m_CollisionFlags;
        private bool m_PreviouslyGrounded;
        private float m_LastLandTime;

        private float m_NextTimeCanChangeHeight;

        private float m_DistMovedSinceLastCycleEnded;


        private void Awake()
        {
            SnapControllerToGround();
        }

        private void Start()
        {
            Player.Run.SetStartTryer(Try_Run);
            Player.Run.AddStopListener(StopRun);

            Player.Crouch.SetStartTryer(() => { return Try_ToggleCrouch(m_MP.CrouchState); });
            Player.Crouch.SetStopTryer(() => { return Try_ToggleCrouch(null); });
            Player.Sliding.SetStartTryer(Try_Slide);

            Player.Prone.SetStartTryer(() => { return Try_ToggleProne(m_MP.ProneState); });
            Player.Prone.SetStopTryer(() => { return Try_ToggleProne(null); });

            Player.Jump.SetStartTryer(Try_Jump);
            Player.IsGrounded.AddChangeListener(OnGroundingStateChanged);

            Player.Death.AddListener(OnDeath);
            Player.Respawn.AddListener(SnapControllerToGround);

            Player.Health.AddChangeListener(On_PlayerTakeDamage);

            DefaultHeight = m_Controller.height;

            On_PlayerTakeDamage(Player.Health.Get());
            SnapControllerToGround();
        }

        private void On_PlayerTakeDamage(float health) 
        {
            if (health <= m_InjuredHealthThreshold)
                m_MP = m_InjuredMovementProfile;
            else
                m_MP = m_MovementProfile; 
        }

        private void SnapControllerToGround()
        {
            // Snaps the Player's position to the ground.
            if (Physics.Raycast(transform.position + transform.up, -transform.up, out RaycastHit hitInfo, 4f, ~0, QueryTriggerInteraction.Ignore))
                transform.position = hitInfo.point + Vector3.up * 0.05f;
        }

        #region Moving
        private void Update()
        {
            if (m_MP == null)
                Debug.LogError("Movement Profile is unasigned, the Player can't move without one");
            else
            {
                float deltaTime = Time.deltaTime;

                Vector3 translation;

                if (IsGrounded)
                {
                    translation = transform.TransformVector(m_DesiredVelocityLocal) * deltaTime;

                    if (!Player.Jump.Active)
                        translation.y = -m_MP.CoreMovement.AntiBumpFactor;
                }
                else
                    translation = transform.TransformVector(m_DesiredVelocityLocal * deltaTime);

                m_CollisionFlags = m_Controller.Move(translation);

                if ((m_CollisionFlags & CollisionFlags.Below) == CollisionFlags.Below && !m_PreviouslyGrounded)
                {
                    bool wasJumping = Player.Jump.Active;

                    if (Player.Jump.Active)
                        Player.Jump.ForceStop();

                    Player.FallImpact.Send(Mathf.Abs(m_DesiredVelocityLocal.y));

                    m_LastLandTime = Time.time;

                    if (wasJumping)
                        m_DesiredVelocityLocal = Vector3.ClampMagnitude(m_DesiredVelocityLocal, 1f);
                }

                // Check if the top of the controller collided with anything,
                // If it did then add a counter force
                if (((m_CollisionFlags & CollisionFlags.Above) == CollisionFlags.Above && !m_Controller.isGrounded) && m_DesiredVelocityLocal.y > 0)
                    m_DesiredVelocityLocal.y *= -m_MP.CoreMovement.HeadBounceFactor;

                Vector3 targetVelocity = CalcTargetVelocity(Player.MoveInput.Get());

                if (!IsGrounded)
                    UpdateAirborneMovement(deltaTime, targetVelocity, ref m_DesiredVelocityLocal);
                else if (!Player.Jump.Active)
                    UpdateGroundedMovement(deltaTime, targetVelocity, ref m_DesiredVelocityLocal);

                Player.IsGrounded.Set(IsGrounded);
                Player.Velocity.Set(Velocity);

                m_PreviouslyGrounded = IsGrounded;
            }
        }

        private void UpdateGroundedMovement(float deltaTime, Vector3 targetVelocity, ref Vector3 velocity)
        {
            // Make sure to lower the speed when moving on steep surfaces.
            float surfaceAngle = Vector3.Angle(Vector3.up, SurfaceNormal);
            targetVelocity *= m_MP.CoreMovement.SlopeSpeedMult.Evaluate(surfaceAngle / SlopeLimit);

            // Calculate the rate at which the current speed should increase / decrease. 
            // If the player doesn't press any movement button, use the "m_Damping" value, otherwise use "m_Acceleration".
            float targetAccel = targetVelocity.sqrMagnitude > 0f ? m_MP.CoreMovement.Acceleration : m_MP.CoreMovement.Damping;

            velocity = Vector3.Lerp(velocity, targetVelocity, targetAccel * deltaTime);

            // If we're moving and not running, start the "Walk" activity.
            if (!Player.Walk.Active && targetVelocity.sqrMagnitude > 0.05f && !Player.Run.Active && !Player.Crouch.Active)
                Player.Walk.ForceStart();
            // If we're running, or not moving, stop the "Walk" activity.
            else if (Player.Walk.Active && (targetVelocity.sqrMagnitude < 0.05f || Player.Run.Active || Player.Crouch.Active || Player.Prone.Active))
                Player.Walk.ForceStop();

            if (Player.Run.Active)
            {
                bool wantsToMoveBackwards = Player.MoveInput.Get().y < 0f;
                bool runShouldStop = wantsToMoveBackwards || targetVelocity.sqrMagnitude == 0f || Player.Stamina.Is(0f);

                if (runShouldStop)
                    Player.Run.ForceStop();
            }

            if (Player.Sliding.Active || m_MP.Sliding.SlopeSlide)
            {
                // Sliding...
                if (surfaceAngle > m_MP.Sliding.SlopeSlideTreeshold && Player.MoveInput.Get().sqrMagnitude == 0f)
                {
                    Vector3 slideDirection = (SurfaceNormal + Vector3.down);
                    m_SlideVelocity += slideDirection * m_MP.Sliding.SlopeSlideSpeed * deltaTime;
                }
                else
                    m_SlideVelocity = Vector3.Lerp(m_SlideVelocity, Vector3.zero, deltaTime * 10f);

                m_SlideVelocity = new Vector3(m_SlideVelocity.x, 0f, m_SlideVelocity.z);

                velocity += transform.InverseTransformVector(m_SlideVelocity);
            }

            // Advance step
            m_DistMovedSinceLastCycleEnded += m_DesiredVelocityLocal.magnitude * deltaTime;

            // Which step length should be used?
            float targetStepLength = m_MP.CoreMovement.StepLength;

            if (m_MM != null)
                targetStepLength = m_MM.StepLength;

            CurrentStepLength = Mathf.MoveTowards(CurrentStepLength, targetStepLength, deltaTime);

            // If the step cycle is complete, reset it, and send a notification.
            if (m_DistMovedSinceLastCycleEnded > CurrentStepLength)
            {
                m_DistMovedSinceLastCycleEnded -= CurrentStepLength;
                Player.MoveCycleEnded.Send();
            }

            Player.MoveCycle.Set(m_DistMovedSinceLastCycleEnded / CurrentStepLength);
        }

        private void UpdateAirborneMovement(float deltaTime, Vector3 targetVelocity, ref Vector3 velocity)
        {
            if (m_PreviouslyGrounded && !Player.Jump.Active)
                velocity.y = 0f;

            // Modify the current velocity by taking into account how well we can change direction when not grounded (see "m_AirControl" tooltip).
            velocity += targetVelocity * m_MP.CoreMovement.Acceleration * m_MP.CoreMovement.AirborneControl * deltaTime;

            // Apply gravity.
            velocity.y -= m_MP.CoreMovement.Gravity * deltaTime;
        }

        private Vector3 CalcTargetVelocity(Vector2 moveInput)
        {
            moveInput = Vector2.ClampMagnitude(moveInput, 1f);

            bool wantsToMove = moveInput.sqrMagnitude > 0f;

            // Calculate the direction (relative to the us), in which the player wants to move.
            Vector3 targetDirection = (wantsToMove ? new Vector3(moveInput.x, 0f, moveInput.y) : m_DesiredVelocityLocal.normalized);

            float desiredSpeed = 0f;

            if (wantsToMove)
            {
                // Set the default speed.
                desiredSpeed = m_MP.CoreMovement.ForwardSpeed;

                // If the player wants to move sideways...
                if (Mathf.Abs(moveInput.x) > 0f)
                    desiredSpeed = m_MP.CoreMovement.SideSpeed;

                // If the player wants to move backwards...
                if (moveInput.y < 0f)
                    desiredSpeed = m_MP.CoreMovement.BackSpeed;

                // Little Hack until a cleaner solution will be found
                if (Player.Aim.Active && Player.Prone.Active)
                    desiredSpeed = 0f;
                // If we're currently running...
                else if (Player.Run.Active)
                {
                    // If the player wants to move forward or sideways, apply the run speed multiplier.
                    if (desiredSpeed == m_MP.CoreMovement.ForwardSpeed || desiredSpeed == m_MP.CoreMovement.SideSpeed)
                        desiredSpeed = m_MM.SpeedMultiplier;
                }
                else
                {
                    // If we're crouching / proning...
                    if (m_MM != null)
                        desiredSpeed *= m_MM.SpeedMultiplier;
                }
            }

            return targetDirection * (desiredSpeed * Player.MovementSpeedFactor.Val);
        }

        private bool DoCollisionCheck(bool checkAbove, float maxDistance)
        {
            Vector3 rayOrigin = transform.position + (checkAbove ? Vector3.up * m_Controller.height : Vector3.zero);
            Vector3 rayDirection = checkAbove ? Vector3.up : Vector3.down;

            return Physics.Raycast(rayOrigin, rayDirection, maxDistance, m_ObstacleCheckMask, QueryTriggerInteraction.Ignore);
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            SurfaceNormal = hit.normal;
        }
        #endregion

        #region Movement States
        private bool Try_Run()
        {
            if (!m_MP.RunState.Enabled || Player.Stamina.Get() < 15f)
                return false;

            bool wantsToMoveBack = Player.MoveInput.Get().y < 0f;
            bool canRun = Player.IsGrounded.Get() && !wantsToMoveBack && !Player.Aim.Active && !Player.Prone.Active &&
                 !(Player.Crouch.Active && !Player.Crouch.TryStop()); // Stop crouching if the run activity can be started

            if (canRun)
                m_MM = m_MP.RunState;

            return canRun;
        }

        private void StopRun()
        {
            if (m_MM == m_MP.RunState)
                m_MM = null;
        }

        private bool Try_Jump()
        {
            // If crouched, stop crouching first
            if (Player.Crouch.Active)
            {
                Player.Crouch.TryStop();
                return false;
            }
            else if (Player.Prone.Active)
            {
                if (!Player.Prone.TryStop())
                    Player.Crouch.TryStart();

                return false;
            }

            bool canJump = m_MP.Jumping.Enabled &&
                IsGrounded &&
                !Player.Crouch.Active &&
                Time.time > m_LastLandTime + m_MP.Jumping.JumpTimer;

            if (!canJump)
                return false;

            float jumpSpeed = Mathf.Sqrt(2 * m_MP.CoreMovement.Gravity * m_MP.Jumping.JumpHeight);
            m_DesiredVelocityLocal = new Vector3(m_DesiredVelocityLocal.x, jumpSpeed, m_DesiredVelocityLocal.z);

            return true;
        }

        private bool Try_ToggleCrouch(PlayerMovementProfile.LowerHeightStateModule lowerHeightState)
        {
            if (!m_MP.CrouchState.Enabled)
                return false;

            bool toggledSuccesfully;

            if (!Player.Crouch.Active)
                toggledSuccesfully = Try_ChangeControllerHeight(lowerHeightState);
            else
                toggledSuccesfully = Try_ChangeControllerHeight(null);

            if (toggledSuccesfully)
            {
                // Stop the run activity
                if (Player.Run.Active)
                    Player.Run.ForceStop();

                // Stop the prone activity
                if (Player.Prone.Active)
                    Player.Prone.ForceStop();

                // Start the sliding activity if possible
                Player.Sliding.TryStart();
            }

            return toggledSuccesfully;
        }

        private bool Try_Slide()
        {
            if (m_DesiredVelocityLocal.magnitude < m_MP.Sliding.CrouchSlideSpeedThreshold)
                return false;

            return true;
        }

        private bool Try_ToggleProne(PlayerMovementProfile.LowerHeightStateModule lowerHeightState)
        {
            if (!m_MP.ProneState.Enabled)
                return false;

            bool toggledSuccesfully;

            if (!Player.Prone.Active)
                toggledSuccesfully = Try_ChangeControllerHeight(lowerHeightState);
            else
                toggledSuccesfully = Try_ChangeControllerHeight(null);

            //Stop the crouch state if the prone state is enabled
            if (toggledSuccesfully)
            {
                if (Player.Crouch.Active)
                    Player.Crouch.ForceStop();

                if (Player.Run.Active)
                    Player.Run.ForceStop();
            }

            return toggledSuccesfully;
        }

        private bool Try_ChangeControllerHeight(PlayerMovementProfile.LowerHeightStateModule lowerHeightState)
        {
            bool canChangeHeight =
                (Time.time > m_NextTimeCanChangeHeight || m_NextTimeCanChangeHeight == 0f) &&
                Player.IsGrounded.Get();// &&
                                        //!Player.Run.Active;

            if (canChangeHeight)
            {
                float height = (lowerHeightState == null) ? DefaultHeight : lowerHeightState.ControllerHeight;

                //If the "lowerHeightState" height is bigger than the current one check if there's anything over the Player's head
                if (height > m_Controller.height)
                {
                    if (DoCollisionCheck(true, Mathf.Abs(height - m_Controller.height)))
                        return false;
                }

                if (lowerHeightState != null)
                    m_NextTimeCanChangeHeight = Time.time + lowerHeightState.TransitionDuration;

                SetHeight(height);

                m_MM = lowerHeightState;
            }

            return canChangeHeight;
        }

        private void SetHeight(float height)
        {
            m_Controller.height = height;
            m_Controller.center = Vector3.up * height * 0.5f;
        }

        private void OnGroundingStateChanged(bool isGrounded)
        {
            if (!isGrounded)
            {
                Player.Walk.ForceStop();
                Player.Run.ForceStop();
            }
        }

        private void OnDeath()
        {
            m_DesiredVelocityLocal = Vector3.zero;
        }
        #endregion
    }
}