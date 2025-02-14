using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class PickupGenerator : MonoBehaviour
{

    [Tooltip("List of pickup objects to randomly place.")]
    public GameObject[] pickups;
    [Tooltip("Generate period.")]
    public float generatePeriod;
    /** Elapsed time since last generation */
    private float elapsedTime = 0f;

    private void Awake()
    {
        foreach (var p in pickups)
        {
            p.GetComponent<MeshRenderer>().enabled = false;
            p.GetComponent<BoxCollider>().enabled = false;
        }
        NavMesh.AddNavMeshData(GameObject.FindWithTag("Ground").GetComponent<NavMeshSurface>().navMeshData);
    }

    private void OnDestroy()
    {
        NavMesh.RemoveAllNavMeshData();
    }

    private void Update()
    {
        elapsedTime += Time.deltaTime;
        if (elapsedTime > generatePeriod && pickups != null && pickups.Length > 0) {
            var pickupInd = UnityEngine.Random.Range(0, pickups.Length);
            var newPosition = GetRandomGameBoardLocation();
            if (NavMesh.SamplePosition(newPosition, out _, 0, 1 << NavMesh.GetAreaFromName("Walkable")))
            {
                pickups[pickupInd].GetComponent<Bouncer>().basePosition = newPosition;
                pickups[pickupInd].GetComponent<MeshRenderer>().enabled = true;
                pickups[pickupInd].GetComponent<BoxCollider>().enabled = true;
                elapsedTime = 0f;
            }
            
        }
    }


    /// Selects a random point on the NavMesh.
    /// </summary>
    /// <returns>Vector3 of the random location.</returns>
    private Vector3 GetRandomGameBoardLocation()
    {

        
        NavMeshTriangulation navMeshTriangulation = NavMesh.CalculateTriangulation();
        var maxTriangles = (navMeshTriangulation.indices.Length / 3);
        UnityEngine.Random.InitState(System.Environment.TickCount);
        var triangleInd = UnityEngine.Random.Range(0, maxTriangles);
        var w1 = UnityEngine.Random.Range(1e-3f,1f);
        var w2 = UnityEngine.Random.Range(1e-3f, 1f);
        var w3 = UnityEngine.Random.Range(1e-3f, 1f);
        var point = (
            navMeshTriangulation.vertices[navMeshTriangulation.indices[3 * triangleInd]] * w1 +
            navMeshTriangulation.vertices[navMeshTriangulation.indices[3 * triangleInd + 1]] * w2 +
            navMeshTriangulation.vertices[navMeshTriangulation.indices[3 * triangleInd + 2]] * w3) / 
            (w1 + w2 + w3);
        return point;
    }

}
