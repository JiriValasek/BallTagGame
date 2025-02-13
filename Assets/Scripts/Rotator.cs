using UnityEngine;

public class Rotator : MonoBehaviour
{

    [Tooltip("Rotation Rate deg/s around X axis.")]
    public float xRotationRate = 15f;
    [Tooltip("Rotation Rate deg/s around Y axis.")]
    public float yRotationRate = 30f;
    [Tooltip("Rotation Rate deg/s around Z axis.")]
    public float zRotationRate = 45f;

    void Update()
    {
        // Rotate the object on X, Y, and Z axes by specified amounts, adjusted for frame rate.
        transform.Rotate(new Vector3(xRotationRate, yRotationRate, zRotationRate) * Time.deltaTime);
    }

}