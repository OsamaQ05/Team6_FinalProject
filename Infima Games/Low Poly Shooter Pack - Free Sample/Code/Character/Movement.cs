// Copyright 2021, Infima Games. All Rights Reserved.

using System.Linq;
using UnityEngine;

namespace InfimaGames.LowPolyShooterPack
{
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public class Movement : MovementBehaviour
    {
        #region FIELDS SERIALIZED

        [Header("Audio Clips")]
        
        [Tooltip("The audio clip that is played while walking.")]
        [SerializeField]
        private AudioClip audioClipWalking;

        [Tooltip("The audio clip that is played while running.")]
        [SerializeField]
        private AudioClip audioClipRunning;

        [Tooltip("The audio clip that is played while crouching.")]
        [SerializeField]
        private AudioClip audioClipCrouching;

        [Tooltip("The audio clip that is played when jumping.")]
        [SerializeField]
        private AudioClip audioClipJumping;

        [Tooltip("The audio clip that is played when landing.")]
        [SerializeField]
        private AudioClip audioClipLanding;

        [Header("Speeds")]

        [SerializeField]
        private float speedWalking = 5.0f;

        [Tooltip("How fast the player moves while running."), SerializeField]
        private float speedRunning = 9.0f;

        [Tooltip("How fast the player moves while crouching."), SerializeField]
        private float speedCrouching = 3.0f;

        [Header("Jumping")]

        [Tooltip("Force applied when jumping."), SerializeField]
        private float jumpForce = 5.0f;

        [Tooltip("Cooldown between jumps in seconds."), SerializeField]
        private float jumpCooldown = 0.5f;

        [Header("Crouching")]

        [Tooltip("Height of the capsule collider when crouching."), SerializeField]
        private float crouchHeight = 1.0f;

        [Tooltip("Height of the capsule collider when standing."), SerializeField]
        private float standingHeight = 2.0f;

        [Tooltip("Smooth time for crouching transition."), SerializeField]
        private float crouchSmoothTime = 0.2f;

        #endregion

        #region PROPERTIES

        //Velocity.
        private Vector3 Velocity
        {
            //Getter.
            get => rigidBody.velocity;
            //Setter.
            set => rigidBody.velocity = value;
        }

        #endregion

        #region FIELDS

        /// <summary>
        /// Attached Rigidbody.
        /// </summary>
        private Rigidbody rigidBody;
        /// <summary>
        /// Attached CapsuleCollider.
        /// </summary>
        private CapsuleCollider capsule;
        /// <summary>
        /// Attached AudioSource.
        /// </summary>
        private AudioSource audioSource;
        
        /// <summary>
        /// True if the character is currently grounded.
        /// </summary>
        private bool grounded;

        /// <summary>
        /// True if the character was grounded in the previous frame.
        /// </summary>
        private bool wasGrounded;

        /// <summary>
        /// Player Character.
        /// </summary>
        private CharacterBehaviour playerCharacter;
        /// <summary>
        /// The player character's equipped weapon.
        /// </summary>
        private WeaponBehaviour equippedWeapon;
        
        /// <summary>
        /// Array of RaycastHits used for ground checking.
        /// </summary>
        private readonly RaycastHit[] groundHits = new RaycastHit[8];

        /// <summary>
        /// Current jump cooldown timer.
        /// </summary>
        private float jumpCooldownTimer;

        /// <summary>
        /// True if the character is currently crouching.
        /// </summary>
        private bool isCrouching;

        /// <summary>
        /// Current height of the capsule collider.
        /// </summary>
        private float currentHeight;

        /// <summary>
        /// Velocity for smooth height transition.
        /// </summary>
        private float heightVelocity;

        #endregion

        #region UNITY FUNCTIONS

        /// <summary>
        /// Awake.
        /// </summary>
        protected override void Awake()
        {
            //Get Player Character.
            playerCharacter = ServiceLocator.Current.Get<IGameModeService>().GetPlayerCharacter();
        }

        /// Initializes the FpsController on start.
        protected override void Start()
        {
            //Rigidbody Setup.
            rigidBody = GetComponent<Rigidbody>();
            rigidBody.constraints = RigidbodyConstraints.FreezeRotation;
            //Cache the CapsuleCollider.
            capsule = GetComponent<CapsuleCollider>();
            currentHeight = capsule.height;

            //Audio Source Setup.
            audioSource = GetComponent<AudioSource>();
            audioSource.clip = audioClipWalking;
            audioSource.loop = true;
        }

        /// Checks if the character is on the ground.
        private void OnCollisionStay()
        {
            //Bounds.
            Bounds bounds = capsule.bounds;
            //Extents.
            Vector3 extents = bounds.extents;
            //Radius.
            float radius = extents.x - 0.01f;
            
            //Cast. This checks whether there is indeed ground, or not.
            Physics.SphereCastNonAlloc(bounds.center, radius, Vector3.down,
                groundHits, extents.y - radius * 0.5f, ~0, QueryTriggerInteraction.Ignore);
            
            //We can ignore the rest if we don't have any proper hits.
            if (!groundHits.Any(hit => hit.collider != null && hit.collider != capsule)) 
                return;
            
            //Store RaycastHits.
            for (var i = 0; i < groundHits.Length; i++)
                groundHits[i] = new RaycastHit();

            //Set grounded. Now we know for sure that we're grounded.
            grounded = true;
        }
			
        protected override void FixedUpdate()
        {
            //Move.
            MoveCharacter();
            
            //Handle jumping.
            HandleJumping();

            //Handle crouching.
            HandleCrouching();

            //Check if we just landed.
            if (!wasGrounded && grounded)
                OnLanded();

            //Update wasGrounded for next frame.
            wasGrounded = grounded;

            //Update jump cooldown timer.
            if (jumpCooldownTimer > 0)
                jumpCooldownTimer -= Time.fixedDeltaTime;

            //Unground.
            grounded = false;
        }

        /// Moves the camera to the character, processes jumping and plays sounds every frame.
        protected override void Update()
        {
            //Get the equipped weapon!
            equippedWeapon = playerCharacter.GetInventory().GetEquipped();
            
            //Play Sounds!
            PlayFootstepSounds();
        }

        #endregion

        #region METHODS

        private void MoveCharacter()
        {
            #region Calculate Movement Velocity

            //Get Movement Input!
            Vector2 frameInput = playerCharacter.GetInputMovement();
            //Calculate local-space direction by using the player's input.
            var movement = new Vector3(frameInput.x, 0.0f, frameInput.y);
            
            //Movement speed calculation based on state.
            if (isCrouching)
            {
                //Use crouching speed.
                movement *= speedCrouching;
            }
            else if (playerCharacter.IsRunning())
            {
                //Use running speed.
                movement *= speedRunning;
            }
            else
            {
                //Use walking speed.
                movement *= speedWalking;
            }

            //World space velocity calculation. This allows us to add it to the rigidbody's velocity properly.
            movement = transform.TransformDirection(movement);

            #endregion
            
            //Update horizontal velocity, preserving vertical velocity.
            Velocity = new Vector3(movement.x, rigidBody.velocity.y, movement.z);
        }

        /// <summary>
        /// Handles the jumping logic.
        /// </summary>
        private void HandleJumping()
        {
            //Check for jump input.
            if (IsJumpKeyPressed() && grounded && jumpCooldownTimer <= 0 && !isCrouching)
            {
                //Apply jump force.
                rigidBody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                //Reset jump cooldown.
                jumpCooldownTimer = jumpCooldown;
                //Play jump sound.
                if (audioClipJumping != null)
                {
                    audioSource.clip = audioClipJumping;
                    audioSource.loop = false;
                    audioSource.Play();
                }
            }
        }

        /// <summary>
        /// Handles crouching logic.
        /// </summary>
        private void HandleCrouching()
        {
            //Check for crouch input.
            bool crouchInput = IsCrouchKeyPressed();
            
            //Toggle crouch state.
            if (crouchInput && grounded)
                isCrouching = !isCrouching;

            //Calculate target height.
            float targetHeight = isCrouching ? crouchHeight : standingHeight;
            
            //Smoothly transition between heights.
            currentHeight = Mathf.SmoothDamp(currentHeight, targetHeight, ref heightVelocity, crouchSmoothTime);
            
            //Update capsule collider height.
            capsule.height = currentHeight;
            
            //Update capsule center position to keep feet at the same position.
            capsule.center = new Vector3(0, currentHeight / 2f, 0);
        }

        /// <summary>
        /// Called when the character lands after being in the air.
        /// </summary>
        private void OnLanded()
        {
            //Play landing sound.
            if (audioClipLanding != null)
            {
                audioSource.clip = audioClipLanding;
                audioSource.loop = false;
                audioSource.Play();
            }
        }

        /// <summary>
        /// Checks if the jump key (Space) is pressed.
        /// </summary>
        private bool IsJumpKeyPressed()
        {
            return Input.GetKeyDown(KeyCode.Space);
        }

        /// <summary>
        /// Checks if the crouch key (Left Control) is pressed.
        /// </summary>
        private bool IsCrouchKeyPressed()
        {
            return Input.GetKeyDown(KeyCode.LeftControl);
        }

        /// <summary>
        /// Plays Footstep Sounds. This code is slightly old, so may not be great, but it functions alright-y!
        /// </summary>
        private void PlayFootstepSounds()
        {
            //Check if we're moving on the ground. We don't need footsteps in the air.
            if (grounded && rigidBody.velocity.sqrMagnitude > 0.1f)
            {
                //Select the correct audio clip to play based on movement state.
                if (isCrouching && audioClipCrouching != null)
                    audioSource.clip = audioClipCrouching;
                else if (playerCharacter.IsRunning())
                    audioSource.clip = audioClipRunning;
                else
                    audioSource.clip = audioClipWalking;

                //Play it!
                if (!audioSource.isPlaying)
                    audioSource.Play();
            }
            //Pause it if we're doing something like flying, or not moving!
            else if (audioSource.isPlaying && audioSource.clip != audioClipJumping && audioSource.clip != audioClipLanding)
                audioSource.Pause();
        }

        #endregion
    }
}