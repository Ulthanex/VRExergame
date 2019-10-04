using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleBehaviour : MonoBehaviour
{

    private Rigidbody vehicleRigidbody;
    public float forwardRate;

    void Awake()
    {
        vehicleRigidbody = GetComponent<Rigidbody>();
    }
    // Update is called once per physics update
    void FixedUpdate()
    {
        vehicleRigidbody.velocity = new Vector3(vehicleRigidbody.velocity.x, vehicleRigidbody.velocity.y, forwardRate);
    }

}