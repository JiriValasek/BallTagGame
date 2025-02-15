using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections;
using UnityEngine.UI;

namespace BallTag
{
    public class PlayerController : MonoBehaviour
    {

        // Movement
        /** Rigid body of the player. */
        private Rigidbody rb;
        /** Input for turning. */
        private float movementX;
        /** Input for acceleration/breaking. */
        private float movementY;
        [Header("Movement")]
        [Tooltip("Force for moving forward and breaking.")]
        public float lateralForce = 500;
        [Tooltip("Force for turning.")]
        public float turnForce = 500;
        [Tooltip("Speed at which breaking stops working\n" +
            "so we avoid camera flipping. It also limits turning.")]
        [Min(0.25f)]
        public float stopBrakeVelocity = 1f;
        [Tooltip("Maximum linear velocity for rigidbody,\n" +
            "camera field of view and distance.")]
        public float maxVelocity = 20;

        // Jumping
        [Header("Jumping")]
        [Tooltip("Force for jumping.")]
        public float jumpForce = 500;
        [Tooltip("Gravity modifier to have nicer jumps.")]
        public float gravityModifier = 1;
        /** Control variable for jumping based on being on the ground (or another object) */
        private bool isOnGround = true;

        // Tagging
        /** Tag variable marking who has the tag. */
        private bool hasTag = true;
        /** Player or the enemy is protected, tagging does not count. */
        private bool protectionActive = false;
        /** Player is protected */
        [HideInInspector]
        public bool isProtected = false;
        [Header("Tagging")]
        [Min(0f)]
        [Tooltip("Protection time after the tag has been transfered.")]
        public float protectionTime = 1f;
        [Tooltip("Text to tell user, that protection is active.")]
        public GameObject protectionText;
        /** Tag game object, showing who is seeking. */
        private GameObject tagObject;
        /** Protection game object, showing who is protected from being tagged. */
        private GameObject protectionObject;
        [Tooltip("Tag height offset from the player center.")]
        public float tagOffset = 0.0f;
        [Tooltip("Enemy controller script to pass the tag.")]
        public EnemyController enemyController;

        // PowerUps
        [Header("PowerUps")]
        [Min(0f)]
        [Tooltip("PowerUp period in seconds.")]
        public float powerUpPeriod = 5f;
        [Range(0f, 1f)]
        [Tooltip("Speed up ratio.")]
        public float speedUpRatio = 0.5f;
        /** Speed-up boost active. */
        private bool speedUpActive = false;
        /** Speed-up timer. */
        private float speedUpElapsed = 0f;
        [Tooltip("Icon to show when Speed-up activated.")]
        public GameObject SpeedUpIcon;
        /** Jumping active switch */
        private bool jumpingActive = false;
        /** Jumping timer. */
        private float jumpingElapsed = 0f;
        [Tooltip("Icon to show when jumping enabled.")]
        public GameObject jumpingIcon;

        // Life
        /** Remaining life. */
        private float life;
        /** Control variable to detect user started to play. */
        private bool started = false;
        [Min(0f)]
        [Tooltip("Life boost in seconds.")]
        public float lifeBoost = 3f;
        [Header("Life")]
        [Min(0f)]
        [Tooltip("Start life in seconds")]
        public float startLife = 60f;
        [Tooltip("Slider displaying remaining life.")]
        public Slider lifeIndicator;
        [Tooltip("Script controlling end menu.")]
        public EndMenu endMenu;

        // Audio
        /** Audio manager for playing sounds based on some events. */
        private AudioManager audioManager;

        // Touch
        private Vector2 touchStartPosition;

        private void Awake()
        {
            Time.timeScale = 1f;
            // Get and store the Rigidbody component attached to the player.
            rb = GetComponent<Rigidbody>();
            rb.maxLinearVelocity = maxVelocity;
            // Get tag & protection object
            tagObject = GameObject.FindWithTag("Tag");
            protectionObject = GameObject.FindWithTag("Protection");

            audioManager = GameObject.FindWithTag("Audio").GetComponent<AudioManager>();
            life = startLife; // init life amount
            jumpingElapsed = powerUpPeriod; // disable by setting to expired
            speedUpElapsed = powerUpPeriod; // disable by setting to expired
        }

        private void Start()
        {
            // Update Gravity to have slower/faster falls
            Physics.gravity *= gravityModifier;
        }

        private void OnMove(InputValue movementValue)
        {
            // Convert the input value into a Vector2 for movement.
            Vector2 movementVector = movementValue.Get<Vector2>();
            // Store the X and Y components of the movement.
            movementX = movementVector.x;
            movementY = movementVector.y;
            Debug.Log(String.Format("key {0}:{1}",movementX,movementY));
            enemyController.started = true;
            started = true;
        }

        private void OnPrimaryStartPosition(InputValue positionValue)
        {
            // Convert the input value into a Vector2 for movement.
            touchStartPosition = positionValue.Get<Vector2>();
            enemyController.started = true;
            started = true;
        }

        private void OnPrimaryPosition(InputValue positionValue)
        {
            // Convert the input value into a Vector2 for movement.
            Vector2 movementVector = positionValue.Get<Vector2>() - touchStartPosition;
            // Store the X and Y components of the movement.
            movementX = movementVector.normalized.x;
            movementY = movementVector.normalized.y;
        }
        private void OnPrimaryPhase(InputValue phaseValue)
        {
            if (phaseValue.Get<UnityEngine.InputSystem.TouchPhase>() == UnityEngine.InputSystem.TouchPhase.Ended ||
                phaseValue.Get<UnityEngine.InputSystem.TouchPhase>() == UnityEngine.InputSystem.TouchPhase.Canceled)
            {
                touchStartPosition = Vector2.zero;
                movementX = 0;
                movementY = 0;
            }
        }

        private void OnJump(InputValue jumpValue)
        {
            // Jumping, only when on the ground (or in contact with other ground subobject)
            if (isOnGround && jumpingActive)
            {
                audioManager.playEffect(audioManager.jump);
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                isOnGround = false;
            }
        }

        private void Update()
        {
            ProcessJumping();
            if (hasTag && !enemyController.isProtected)
            {
                ProcessLife();
            }
        }

        private void FixedUpdate()
        {
            // Create a 3D movement
            // Make arrow down only for breaking, it does not work for reverse driving.
            // Camera is oriented in the direction of movement, so it just keeps fliping and does not move anyway.
            if (rb.linearVelocity.magnitude < stopBrakeVelocity)
            {
                movementY = movementY > 0 ? movementY : 0;
            }

            // Disable forward when enemy is protected
            if (enemyController.isProtected)
            {
                movementY = movementY > 0 ? 0 : movementY;
            }
            var currLateralForce = (speedUpActive ? (1 + speedUpRatio) : 1) * lateralForce;
            ProcessSpeedUp(false);

            // Transform input to a force changing player's movement
            var yRotation = Mathf.Rad2Deg * Mathf.Atan2(rb.linearVelocity.z, rb.linearVelocity.x);
            Vector3 movement = Quaternion.AngleAxis(-yRotation, Vector3.up) * (new Vector3(currLateralForce * movementY, 0f, -turnForce * movementX));
            // Apply the force to the Rigidbody to move the player
            if (rb != null)
            {
                rb.AddForce(movement * Time.fixedDeltaTime);
            }
            // Update tag/protection
            if (hasTag)
            {
                Carry(tagObject);
            }
            if (isProtected)
            {
                Carry(protectionObject);
            }
        }

        void OnTriggerEnter(Collider other)
        {
            // Apply power-ups
            if (other.gameObject.CompareTag("Jumping"))
            {
                ProcessJumping(true);
            }
            if (other.gameObject.CompareTag("Life"))
            {
                ProcessLife(lifeBoost);
            }
            if (other.gameObject.CompareTag("SpeedUp"))
            {
                ProcessSpeedUp(true);

            }

            // Deactivate the collided object (making it disappear).
            other.gameObject.GetComponent<MeshRenderer>().enabled = false;
            other.gameObject.GetComponent<BoxCollider>().enabled = false;
        }
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Enemy"))
            {
                // Do tagging
                if (enemyController != null && !protectionActive)
                {
                    // Transfer hasTag state
                    enemyController.navMeshAgent.isStopped = true;
                    enemyController.hasTag = hasTag;
                    // Invert player's has tag state
                    hasTag = !hasTag;
                    // Do protection
                    StartCoroutine(Protect(hasTag ? enemyController : this));
                }

            }
            // enable jumping
            isOnGround = true;
        }

        /// <summary>
        /// Move carried objects with the enemy.
        /// </summary>
        /// <param name="what">GameObject carried by the enemy.</param>
        private void Carry(GameObject what)
        {
            what.transform.position = new Vector3(
                transform.position.x,
                transform.position.y + transform.localScale.y + tagOffset,
                transform.position.z
                );
        }

        /// <summary>
        /// Function conrolling the ability to jump.
        /// </summary>
        /// <param name="start">
        /// True to start jumping, false to just update timeout and stop jumping if expired.
        /// </param>
        private void ProcessJumping(bool start = false)
        {
            if (start)
            {
                jumpingActive = true;
                jumpingElapsed = 0f;
                audioManager.playEffect(audioManager.pickUp);
            }
            else
            {
                jumpingElapsed += Time.deltaTime;
            }
            jumpingActive = jumpingElapsed < powerUpPeriod;
            jumpingIcon.SetActive(jumpingActive);
        }

        /// <summary>
        /// Function conrolling the temporary speed boost.
        /// </summary>
        /// <param name="start">
        /// True to start boosting, false to just update timeout and stop boosting if expired.
        /// </param>
        private void ProcessSpeedUp(bool start = false)
        {
            if (start)
            {
                speedUpActive = true;
                speedUpElapsed = 0f;
                audioManager.playEffect(audioManager.pickUp);
                audioManager.playEffect(audioManager.speedUp);
            }
            else
            {
                speedUpElapsed += Time.deltaTime;
            }
            speedUpActive = speedUpElapsed < powerUpPeriod;
            SpeedUpIcon.SetActive(speedUpActive);
        }

        /// <summary>
        /// Function that updates amount of life and checks
        /// if the player lost in which case it ends the level.
        /// </summary>
        /// <param name="boost">Level boost to add if obtained.</param>
        private void ProcessLife(float boost = 0f)
        {
            if (boost > 0f)
            {
                life += boost;
                audioManager.playEffect(audioManager.pickUp);
            }
            else
            {
                life -= started ? Time.deltaTime : 0;
            }
            lifeIndicator.value = Mathf.Max(Mathf.Min(life / startLife, 1), 0);
            if (life < 0f)
            {
                endMenu.End(false);
            }
        }

        /// <summary>
        /// Coroutine controlling protection state of player/enemy.
        /// </summary>
        /// <param name="who"></param>
        /// <returns>Timeout to run again only after it expired.</returns>
        IEnumerator Protect(MonoBehaviour who)
        {
            // Start protection
            protectionActive = true;
            protectionText.SetActive(who == enemyController);
            this.isProtected = who == this;
            enemyController.isProtected = who == enemyController;
            protectionObject.GetComponent<MeshRenderer>().enabled = true;
            // Wait
            yield return new WaitForSeconds(protectionTime);
            // Stop protection
            protectionActive = false;
            this.isProtected = false;
            enemyController.isProtected = false;
            protectionText.SetActive(false);
            protectionObject.GetComponent<MeshRenderer>().enabled = false;
        }

    }

}