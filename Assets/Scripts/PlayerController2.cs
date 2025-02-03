using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class PlayerController2 : MonoBehaviour
{
    /*
    public float speed = 20f;
    public float turnSpeed = 50f;
    private float horizontalInput;
    private float verticalInput;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        // Move the vehicle forward
        transform.Translate(Vector3.forward * Time.deltaTime * speed * verticalInput);
        // Turn only when moving
        if (verticalInput > 0)
        {
            // Forward turning
            transform.Rotate(Vector3.up, Time.deltaTime * turnSpeed * horizontalInput);
        }
        else if (verticalInput < 0)
        {
            // Reverse turning in the other direction
            transform.Rotate(Vector3.up, -Time.deltaTime * turnSpeed * horizontalInput);
        }
    }*/
}
