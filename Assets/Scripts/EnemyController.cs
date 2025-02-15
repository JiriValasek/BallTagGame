using System;
using TMPro;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BallTag
{

    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyController : MonoBehaviour
    {
        // NavMeshAgent related
        [Header("NavMeshAgent")]
        [Tooltip("Reference to the player's transform component.")]
        public Transform player;
        [Tooltip("GameObject that presents a body of the enemy.")]
        public GameObject enemyBody;

        // Charge related
        [Header("Charging")]
        [Tooltip("Radius around the enemy where it switches to charge mode.")]
        public float chargeRadius = 3f;
        [Tooltip("GameObject that presents a charging mode sign.")]
        public GameObject chargingSign;
        [Tooltip("Charging sign height above the player (and tag).")]
        public float chargingSignOffset = 0.5f;
        [Range(0f, 1f)]
        [Tooltip("Charging boost ratio - how much is enemy faster when charging.")]
        public float chargingBoostRatio = 0.15f;
        /** Original NavMeshAgent speed to be used as charging base. */
        private float originalSpeed;
        /** Original NavMeshAgent angular speed to be used as charging base. */
        private float originalAngulraSpeed;
        /** Original NavMeshAgent acceleration to be used as charging base. */
        private float originalAcceleration;
        /** Old charging state variable to detect state change */
        private bool wasCharging = false;

        // Tag related
        [HideInInspector]
        /** Variable marking tag ownership. */
        public bool hasTag = false;
        [HideInInspector]
        /** Variable marking that enemy as protected. */
        public bool isProtected = false;
        /** Variable for detecting tag transfer. */
        private bool hadTag = false;
        /** Tag game object, showing who is seeking. */
        private GameObject tagObject;
        /** Protection game object, showing who is seeking. */
        private GameObject protectionObject;
        [Header("Tagging")]
        [Tooltip("Tag/Protection height offset from the player center.")]
        public float tagOffset = 0.5f;
        [Tooltip("Player controller script to read if it's protected.")]
        public PlayerController playerController;

        // Hiding related
        [Header("Hiding")]
        [Tooltip("Number of tries for finding a location where to hide.")]
        public int hideTries = 10;
        [Tooltip("Radius around the player where enemy shan't hide.")]
        public float playerAvoidRadius = 3f;
        [Tooltip("Factor by how much to drift away from the player.")]
        public float playerAvoidanceFactor = 0.3f;
        [Tooltip("Factor by how much to drift in the previous direction.")]
        public float momentumFactor = 0.5f;
        [Tooltip("Radius around the enemy where to look for hiding spot.")]
        public float hideInRadius = 4f;
        [Tooltip("Distance threshold for checking that enemy has reached hiding spot\n" +
            "and will search for next one.")]
        public float hidingSpotThreshold = 0.5f;
        [Tooltip("Distance traveled between frames which means that enemy is stuck.")]
        public float hidingStuckDistance = 1e-12f;
        [Tooltip("List of pickup objects to pick up when hiding.")]
        public GameObject[] pickups;
        /** Position of the hiding spot. */
        private Vector3 hidingSpot;
        /** Last position to avoid getting stuck. */
        private Vector3 lastPosition;


        // Material texture rotation
        /** Last rotation around the x axis. */
        private float xRotation = 0f;
        /** Last rotation around the z axis. */
        private float zRotation = 0f;
        /** Renderer of the enemy body to shift the texture. */
        private Renderer enemyRenderer;

        // Life related
        [Header("Life")]
        [Tooltip("Start life in seconds")]
        public float startLife = 60f;
        [Tooltip("Slider displaying remaining life.")]
        public Slider lifeIndicator;
        /** The amount of life remaining. */
        private float life;

        // Panel related
        [Header("Panel")]
        [Tooltip("Text where to show enemy state.")]
        public TextMeshProUGUI stateText;
        [Tooltip("Script controlling end menu.")]
        public EndMenu endMenu;

        /* Reference to the NavMeshAgent component for pathfinding. */
        [HideInInspector]
        public NavMeshAgent navMeshAgent;
        [HideInInspector]
        /** Control variable for initial waiting state. */
        public bool started = false;

        // Player preferences keys
        /** PlayerPrefs key for unlocked level in build indices. */
        private const string REACHED_IND_KEY = "ReachedIndex";
        /** PlayerPrefs key for unlocked level based on finished levels. */
        private const string UNLOCKED_LEVEL_KEY = "UnlockedLevel";

        private void Awake()
        {
            // Save current NavMesh agent configuration as a basis for changes.
            navMeshAgent = GetComponent<NavMeshAgent>();
            originalSpeed = navMeshAgent.speed;
            originalAngulraSpeed = navMeshAgent.angularSpeed;
            originalAcceleration = navMeshAgent.acceleration;
            // Init variables for hiding and rendering
            lastPosition = hidingSpot = transform.position;
            // Get
            enemyRenderer = enemyBody.GetComponent<Renderer>();
            // Get tag & protection object
            tagObject = GameObject.FindWithTag("Tag");
            protectionObject = GameObject.FindWithTag("Protection");
            life = startLife;
            NavMesh.AddNavMeshData(GameObject.FindWithTag("Ground").GetComponent<NavMeshSurface>().navMeshData);
        }

        private void OnDestroy()
        {
            NavMesh.RemoveAllNavMeshData();
        }

        // Update is called once per frame.
        private void Update()
        {
            ProcessStates();
        }

        private void LateUpdate()
        {
            // Update tag/protection
            if (hasTag)
            {
                Carry(tagObject);
            }
            if (isProtected)
            {
                Carry(protectionObject);
            }
            // Update material
            RotateBody();
            // update position for navigation and rendering
            lastPosition = transform.position;
        }

        private void OnTriggerEnter(Collider other)
        {
            // Apply power-ups
            if (other.gameObject.CompareTag("Jumping"))
            {
                // Debug.Log("Enemy can jump");
                // Allow NavMeshLinks?
            }
            if (other.gameObject.CompareTag("Life"))
            {
                ProcessLife(5f);
                // Debug.Log("Enemy got new life.");
            }
            if (other.gameObject.CompareTag("SpeedUp"))
            {
                // Debug.Log("Enemy is faster?");
                // Change NavMeshAgent speeds?
            }

            // Deactivate the collided object (making it disappear).
            other.gameObject.GetComponent<MeshRenderer>().enabled = false;
            other.gameObject.GetComponent<BoxCollider>().enabled = false;
        }

        /// <summary>
        /// Function processing the states of the enemy.
        /// </summary>
        private void ProcessStates()
        {
            // Wait for the player to start playing
            if (!started)
            {
                stateText.text = "Waiting.";
                return;
            }

            // Seek state after protection period
            if (hasTag && !playerController.isProtected)
            {
                stateText.text = "Seeking.";
                Seek();
                hadTag = true;
                ProcessLife();
            }

            // Idle state during protection period
            if (hasTag && playerController.isProtected)
            {
                stateText.text = "Idling.";
            }

            // Hide state
            if (!hasTag)
            {
                // force hiding spot search after tag transfered
                if (hadTag)
                {
                    stateText.text = "Transfer.";
                    hidingSpot = transform.position;
                }
                stateText.text = "Hiding.";
                Hide();
                hadTag = false;
            }

            // Charging
            Charging();
        }

        /// <summary>
        /// Process seek state, update NavMesh agent destination.
        /// </summary>
        private void Seek()
        {
            if (!player.IsDestroyed())
            {
                // Set the enemy's destination to the player's current position.
                navMeshAgent.SetDestination(player.position);
                navMeshAgent.isStopped = false;
            }
        }

        /// <summary>
        /// Proces hiding statate. Update NavMeshAgent destination if necessary, avoid the player.
        /// </summary>
        private void Hide()
        {
            if (!player.IsDestroyed())
            {
                // Get new enemy destination if necessary
                if ((hidingSpot - transform.position).magnitude < hidingSpotThreshold ||
                    (lastPosition - transform.position).magnitude < hidingStuckDistance)
                {

                    // Try pickups 
                    var picking = false;
                    foreach (var p in pickups)
                    {
                        if (p != null && p.GetComponent<BoxCollider>().enabled &&
                            NavMesh.SamplePosition(p.GetComponent<Bouncer>().basePosition, out _, 0f, 1 << NavMesh.GetAreaFromName("Walkable")))
                        {
                            hidingSpot = p.GetComponent<Bouncer>().basePosition;
                            stateText.text = "Picking.";
                            picking = true;
                            break;
                        }
                    }

                    // Try to find new random spot
                    if (!picking || (lastPosition - transform.position).magnitude < hidingStuckDistance)
                    {
                        var newHidingSpot = transform.position;
                        for (int i = 0; i < hideTries; i++)
                        {
                            newHidingSpot = RandomNavmeshLocation(hideInRadius);
                            // Ignore new hiding spots too close to current position or the player
                            if ((newHidingSpot - player.transform.transform.position).magnitude > playerAvoidRadius && 
                                (newHidingSpot - transform.position).magnitude > hidingSpotThreshold)
                            {
                                hidingSpot = newHidingSpot;
                                break;
                            }
                        }
                    }
                }

                // Set the enemy's destination to the hiding spot
                navMeshAgent.SetDestination(hidingSpot);
                navMeshAgent.isStopped = false;

                // Avoid Player if nearby
                var vectToPlayer = (player.transform.position - transform.position);
                if (vectToPlayer.magnitude < playerAvoidRadius)
                {
                    navMeshAgent.velocity = Vector3.Lerp(
                        navMeshAgent.desiredVelocity,
                        -vectToPlayer.normalized * navMeshAgent.speed * playerAvoidanceFactor,
                        Mathf.Clamp01((playerAvoidRadius - vectToPlayer.magnitude) / playerAvoidRadius)
                        );
                }
            }

        }

        /// <summary>
        /// Rotate body to have more persuasive motion.
        /// </summary>
        private void RotateBody()
        {
            // Rotationg 
            var newPosition = transform.position;
            var movement = Quaternion.AngleAxis(transform.rotation.eulerAngles.y, Vector3.up) * (lastPosition - newPosition);
            if (!enemyBody.IsDestroyed())
            {
                var oldRotation = enemyBody.transform.rotation.eulerAngles;
                enemyBody.transform.Rotate(-oldRotation.x, -oldRotation.y, -oldRotation.z);
                xRotation += Mathf.Rad2Deg * 2 * movement.z / (enemyBody.transform.localScale.z) + 360;
                zRotation += Mathf.Rad2Deg * 2 * movement.x / (enemyBody.transform.localScale.x) + 360;
                xRotation %= 360;
                zRotation %= 360;
                enemyRenderer.material.mainTextureOffset = new Vector2(zRotation / 360, xRotation / 360);
            }
        }

        /// <summary>
        /// Function to switch charging on and off based on player distance.
        /// </summary>
        private void Charging()
        {
            // Charging
            if (!player.IsDestroyed())
            {
                float distance = Vector3.Distance(player.transform.position, navMeshAgent.transform.position);
                var isInChargeRadius = distance < chargeRadius;
                if (isInChargeRadius && !wasCharging)
                {
                    navMeshAgent.speed = (1 + chargingBoostRatio) * navMeshAgent.speed;
                    navMeshAgent.angularSpeed = (1 + chargingBoostRatio) * navMeshAgent.angularSpeed;
                    navMeshAgent.acceleration = (1 + chargingBoostRatio) * navMeshAgent.acceleration;
                    wasCharging = true;
                }
                if (!isInChargeRadius && wasCharging)
                {
                    navMeshAgent.speed = navMeshAgent.speed / (1 + chargingBoostRatio);
                    navMeshAgent.angularSpeed = navMeshAgent.angularSpeed / (1 + chargingBoostRatio);
                    navMeshAgent.acceleration = navMeshAgent.acceleration / (1 + chargingBoostRatio);
                    wasCharging = false;
                }
                chargingSign.SetActive(isInChargeRadius);
            }
        }

        /// <summary>
        /// Function trying to find random point on NavMesh as a destination.
        /// </summary>
        /// <param name="radius">Radius around the enemy in which to search</param>
        /// <returns>Random point when succeded, current enemy position otherwise.</returns>
        public Vector3 RandomNavmeshLocation(float radius)
        {
            // Have some momentum
            Vector3 moveDir = transform.position - lastPosition;
            // Generate random NavMesh point based on current position and last direction of travel.
            UnityEngine.Random.InitState(System.Environment.TickCount);
            Vector3 randomDirection = transform.position +
                moveDir * momentumFactor +
                UnityEngine.Random.insideUnitSphere * radius;
            NavMeshHit hit;
            Vector3 finalPosition = transform.position;
            if (NavMesh.SamplePosition(randomDirection, out hit, radius, 1 << NavMesh.GetAreaFromName("Walkable")))
            {
                finalPosition = hit.position;
            }
            return finalPosition;
        }

        /// <summary>
        /// Move carried objects with the enemy.
        /// </summary>
        /// <param name="what">GameObject carried by the enemy.</param>
        private void Carry(GameObject what)
        {
            what.transform.position = new Vector3(
                enemyBody.transform.position.x,
                enemyBody.transform.position.y + enemyBody.transform.localScale.y + tagOffset,
                enemyBody.transform.position.z
                );
        }

        /// <summary>
        /// Function that updates amount of life and checks
        /// if enemy lost in which case it ends the level.
        /// </summary>
        /// <param name="boost">Level boost to add if obtained.</param>
        private void ProcessLife(float boost = 0f)
        {
            life = boost > 0 ? life + boost : life - Time.deltaTime;
            lifeIndicator.value = Mathf.Max(Mathf.Min(life / startLife, 1), 0);
            if (life < 0f)
            {
                UnlockLevel();
                endMenu.End(true);
            }
        }

        /// <summary>
        /// Function for updating new level if the player won.
        /// </summary>
        private void UnlockLevel()
        {
            if (SceneManager.GetActiveScene().buildIndex >= PlayerPrefs.GetInt(REACHED_IND_KEY, 0))
            {
                PlayerPrefs.SetInt(REACHED_IND_KEY, SceneManager.GetActiveScene().buildIndex + 1);
                PlayerPrefs.SetInt(UNLOCKED_LEVEL_KEY, PlayerPrefs.GetInt(UNLOCKED_LEVEL_KEY) + 1);
                PlayerPrefs.Save();
            }
        }
   
    }
}