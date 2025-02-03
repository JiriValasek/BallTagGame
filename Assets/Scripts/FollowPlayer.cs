using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{

    public GameObject player;

    private Vector3 offset = new Vector3(0, 6, -9);

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = player.transform.TransformPoint(offset);
        transform.rotation = Quaternion.Euler(15, player.transform.rotation.eulerAngles.y, 0);
    }
}
