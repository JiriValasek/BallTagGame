using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Reference to the player GameObject.
    public GameObject player;

    // The distance between the camera and the player.
    private Vector3 offset;

    [Tooltip( "Ratio used to smooth out changes\n" +
        "especially for small speeds.") ]
    [Range(0.01f,0.99f)]
    public float cameraChangeRatio = 0.2f;
    [Tooltip( "Camera distance during max speed.")]
    public float cameraClosest = 2f;
    [Tooltip("Camera distance during 0 speed.")]
    public float cameraFarthest = 3f;
    [Tooltip("Camera min FoV for 0 speed.")]
    public float cameraMinFoV = 60f;
    [Tooltip("Camera max FoV for max speed.")]
    public float cameraMaXFoV = 100f;
    [Tooltip("Ignore velocity bottom limit - lowpass filter,\n" +
        "removes 90deg camera flicking when stationary.")]
    public float ignoreVelocity = 1e-12f;


    // Start is called before the first frame update.
    void Start()
    {
        // Calculate the initial offset between the camera's position and the player's position.
        offset = transform.position - player.transform.position;
    }

    // LateUpdate is called once per frame after all Update functions have been completed.
    void LateUpdate()
    {
        // Maintain the same offset between the camera and player throughout the game.
        if (!player.IsDestroyed())
        {
            var rb = player.GetComponent<Rigidbody>();
            // Do not act on micro movement 
            if (rb.linearVelocity.magnitude < ignoreVelocity)
            {
                return;
            }
            // Get movement direction as y rotation
            var yRotation = (Mathf.Rad2Deg*Mathf.Atan2(rb.linearVelocity.z, rb.linearVelocity.x));
            // Update FoV and distance
            var relVelocity = rb.linearVelocity.magnitude / rb.maxLinearVelocity;
            offset.x = -Mathf.Lerp(cameraFarthest, cameraClosest, relVelocity);
            GetComponent<Camera>().fieldOfView = Mathf.Lerp(cameraMinFoV, cameraMaXFoV, relVelocity);
            // Update Camera position and rotation
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, -yRotation+90, transform.rotation.eulerAngles.z);
            transform.position = player.transform.position + Quaternion.AngleAxis(-yRotation,Vector3.up)*offset;
        }
    }
}