using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.GraphicsBuffer;

public class EnemyMovement : MonoBehaviour
{
    // Reference to the player's transform.
    public Transform player;
    public float dangerZoneRadius;
    public GameObject enemy;
    public float blinkPeriod;
    public Color blinkColor;

    // Reference to the NavMeshAgent component for pathfinding.
    private NavMeshAgent navMeshAgent;
    private Material enemyMaterial;
    private Color originalColor;
    private float zoneTime;
    private float originalSpeed;
    private bool started = false;

    // Start is called before the first frame update.
    void Start()
    {
        // Get and store the NavMeshAgent component attached to this object.
        navMeshAgent = GetComponent<NavMeshAgent>();
        enemyMaterial = enemy.GetComponent<MeshRenderer>().material;
        originalColor = enemyMaterial.color;
        originalSpeed = navMeshAgent.speed;
    }

    // Update is called once per frame.
    void Update()
    {
        // If there's a reference to the player...
        if (player != null && (started || Input.anyKey))
        {
            started = true;
            // Set the enemy's destination to the player's current position.
            navMeshAgent.SetDestination(player.position);
            float distance = Vector3.Distance(player.transform.position, navMeshAgent.transform.position);
            if (distance <= dangerZoneRadius)
            {
                navMeshAgent.speed = 1.5f * originalSpeed;
            } else
            {
                navMeshAgent.speed = originalSpeed;
            }
        }
    }

    void LateUpdate()
    {
        // If there's a reference to the player...
        if (player != null)
        {
            float distance = Vector3.Distance(player.transform.position, navMeshAgent.transform.position);
            if (distance <= dangerZoneRadius)
            {
                zoneTime += Time.deltaTime;
                var blinkRatio = (zoneTime % (blinkPeriod / 2)) / (blinkPeriod / 2);
                if ((zoneTime % (blinkPeriod / 2)) == (zoneTime % blinkPeriod))
                {
                    enemyMaterial.color = (1 - blinkRatio) * originalColor + blinkRatio * blinkColor;
                }
                else
                {
                    enemyMaterial.color = (1 - blinkRatio) * blinkColor + blinkRatio * originalColor;
                }
            }
            else
            {
                zoneTime = 0;
                enemyMaterial.color = originalColor;
            }
        }
    }
}

