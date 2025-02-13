using System;
using TMPro;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

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
    public float chargingBoostRatio = 0.5f;
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

    // Hiding
    [Header("Hiding")]
    [Tooltip("Number of tries for finding a location where to hide.")]
    public int hideTries = 10;
    [Tooltip("Radius around the player where enemy shan't hide.")]
    public float playerAvoidRadius = 1f;
    [Tooltip("Factor by how much to drift away from the player.")]
    public float playerAvoidanceFactor = 0.5f;
    [Tooltip("Factor by how much to drift in the previous direction.")]
    public float momentumFactor = 0.5f;
    [Tooltip("Radius around the enemy where to look for hiding spot.")]
    public float hideInRadius = 0.5f;
    [Tooltip("Distance threshold for checking that enemy has reached hiding spot\n" +
        "and will search for next one.")]
    public float hidingSpotThreshold = 0.1f;
    [Tooltip("Distance traveled between frames which means that enemy is stuck.")]
    public float hidingStuckDistance = 1e-9f;
    [Tooltip("List of pickup objects to pick up when hiding.")]
    public GameObject[] pickups;
    /** Position of the hiding spot. */
    private Vector3 hidingSpot;
    /** Last position to avoid getting stuck. */
    private Vector3 lastPosition;


    // Rotationg
    private float xRotation = 0f;
    private float zRotation = 0f;
    private Renderer enemyRenderer;

    // Life related
    [Header("Life")]
    [Tooltip("Start life in seconds")]
    public float startLife = 60f;
    [Tooltip("Slider displaying remaining life.")]
    public Slider lifeIndicator;
    private float life;
    // Panel related
    [Header("Panel")]
    [Tooltip("Text where to show enemy state.")]
    public TextMeshProUGUI stateText;
    [Tooltip("Script controlling end menu.")]
    public EndMenu endMenu;

    // Reference to the NavMeshAgent component for pathfinding.
    [HideInInspector]
    public NavMeshAgent navMeshAgent;
    private bool started = false;
    private Vector3 oldPosition;

    private const string REACHED_IND_KEY = "ReachedIndex";
    private const string UNLOCKED_LEVEL_KEY = "UnlockedLevel";

    private void Awake()
    {
        // Get and store the NavMeshAgent component attached to this object.
        navMeshAgent = GetComponent<NavMeshAgent>();
        originalSpeed = navMeshAgent.speed;
        originalAngulraSpeed = navMeshAgent.angularSpeed;
        originalAcceleration = navMeshAgent.acceleration;
        oldPosition = transform.position;
        lastPosition = hidingSpot = transform.position;
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
    void Update()
    {
        // If there's a reference to the player...
        if (!started && !Input.anyKey)
        {
            stateText.text = "Waiting.";
            return;
        }
        started = true;

        // Seek state
        if (hasTag && !playerController.isProtected)
        {
            stateText.text = "Seeking.";
            Seek();
            hadTag = true;
            ProcessLife();
        }

        // Idle state
        if (hasTag && playerController.isProtected)
        {
            stateText.text = "Idling.";
        }

        // Hide state
        if(!hasTag)
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

    private void FixedUpdate()
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
    }

    void LateUpdate()
    {
        RotateBody();
    }

    private void Seek()
    {
        if (!player.IsDestroyed())
        {
            // Set the enemy's destination to the player's current position.
            navMeshAgent.SetDestination(player.position);
            navMeshAgent.isStopped = false;
        }
    }

    private void Hide()
    {
        if (!player.IsDestroyed())
        {
            if ((hidingSpot - transform.position).magnitude < hidingSpotThreshold ||
                (lastPosition - transform.position).magnitude < hidingStuckDistance) {

                // Try pickups 
                var picking = false;
                foreach (var p in pickups)
                {
                    if (p != null && p.GetComponent<BoxCollider>().enabled &&
                        NavMesh.SamplePosition(p.GetComponent<Bouncer>().basePosition, out _, 0f, 1 << NavMesh.GetAreaFromName("Walkable")))
                    {
                        Debug.Log("Pick");
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
                        if ((newHidingSpot - player.transform.transform.position).magnitude > playerAvoidRadius)
                        {
                            hidingSpot = newHidingSpot;
                            break;
                        }
                    }
                }
            }
            // Set the enemy's destination to the hiding spot
            lastPosition = transform.position;
            navMeshAgent.SetDestination(hidingSpot);
            navMeshAgent.isStopped = false;
        }

    }

    private void RotateBody()
    {
        // Rotationg 
        var newPosition = transform.position;
        var movement = Quaternion.AngleAxis(transform.rotation.eulerAngles.y, Vector3.up) * ( oldPosition - newPosition);
        oldPosition = newPosition;
        if (!enemyBody.IsDestroyed())
        {
            var oldRotation = enemyBody.transform.rotation.eulerAngles;
            enemyBody.transform.Rotate(-oldRotation.x, -oldRotation.y, -oldRotation.z);
            xRotation += Mathf.Rad2Deg * 2 * movement.z / (enemyBody.transform.localScale.z) + 360;
            zRotation += Mathf.Rad2Deg * 2 * movement.x / (enemyBody.transform.localScale.x) + 360;
            //enemyBody.transform.Rotate( xRotation, 0, zRotation, Space.World);
            xRotation %= 360;
            zRotation %= 360;
            enemyRenderer.material.mainTextureOffset  = new Vector2(zRotation / 360, xRotation / 360);
        }
    }

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
                navMeshAgent.speed =  navMeshAgent.speed / (1 + chargingBoostRatio);
                navMeshAgent.angularSpeed = navMeshAgent.angularSpeed / (1 + chargingBoostRatio);
                navMeshAgent.acceleration = navMeshAgent.acceleration / (1 + chargingBoostRatio);
                wasCharging = false;
            }
            chargingSign.SetActive(isInChargeRadius);
        }
    }

    public Vector3 RandomNavmeshLocation(float radius)
    {
        // Avoid direction towards player
        Vector3 normPlayerDir = Vector3.zero;
        if (!player.IsDestroyed())
        {
            normPlayerDir = (transform.position - player.transform.position).normalized;
        }
        // Have some momentum
        Vector3 moveDir = transform.position - lastPosition;
        // Generate random NavMesh point
        UnityEngine.Random.InitState(System.Environment.TickCount);
        Vector3 randomDirection = transform.position + 
            playerAvoidanceFactor * normPlayerDir +
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

    private void Carry(GameObject what)
    {
        what.transform.position = new Vector3(
            enemyBody.transform.position.x,
            enemyBody.transform.position.y + enemyBody.transform.localScale.y + tagOffset,
            enemyBody.transform.position.z
            );
    }

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

    private void UnlockLevel()
    {
        if (SceneManager.GetActiveScene().buildIndex >= PlayerPrefs.GetInt(REACHED_IND_KEY,0))
        {
            PlayerPrefs.SetInt(REACHED_IND_KEY, SceneManager.GetActiveScene().buildIndex + 1);
            PlayerPrefs.SetInt(UNLOCKED_LEVEL_KEY, PlayerPrefs.GetInt(UNLOCKED_LEVEL_KEY) + 1);
            PlayerPrefs.Save();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Apply power-ups
        if (other.gameObject.CompareTag("Jumping"))
        {
            Debug.Log("Enemy can jump");
            // Allow NavMeshLinks?
        }
        if (other.gameObject.CompareTag("Life"))
        {
            ProcessLife(5f);
            Debug.Log("Enemy got new life.");
        }
        if (other.gameObject.CompareTag("SpeedUp"))
        {
            Debug.Log("Enemy is faster?");
            // Change NavMeshAgent speeds?
        }

        // Deactivate the collided object (making it disappear).
        other.gameObject.GetComponent<MeshRenderer>().enabled = false;
        other.gameObject.GetComponent<BoxCollider>().enabled = false;
    }

}

