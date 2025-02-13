using Unity.VisualScripting;
using UnityEngine;

public class Bouncer : MonoBehaviour
{

    [Tooltip("Rotation Rate deg/s around Y axis.")]
    public float yRotationRate = 90f;
    [Tooltip("Upper limit of bouncing")]
    public float bounceUpperLimit = 1.1f;
    [Tooltip("Lower limit of bouncing")]
    public float bounceLowerLimit = 0.6f;
    [Tooltip("Bounce speed.")]
    public float bounceSpeed = 0.4f;
    /** Base position to which bounce is added. */
    [HideInInspector]
    public Vector3 basePosition;
    /** Bounce direction. */
    private Vector3 bounceDirection;
    /** Current bounce. */
    private Vector3 currBounce;


    private void Awake()
    {
        basePosition = transform.position;
        bounceDirection = Vector3.up;
    }

    void Update()
    {
        // Rotate the object on X, Y, and Z axes by specified amounts, adjusted for frame rate.
        transform.Rotate(new Vector3(0, yRotationRate, 0) * Time.deltaTime);
        currBounce += bounceDirection * bounceSpeed * Time.deltaTime;
        transform.position = basePosition + currBounce;
        bounceDirection = transform.position.y > basePosition.y + bounceUpperLimit ? Vector3.down : bounceDirection;
        bounceDirection = transform.position.y < basePosition.y + bounceLowerLimit ? Vector3.up : bounceDirection;
    }

}