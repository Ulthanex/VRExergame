using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoliceBehaviour : MonoBehaviour
{

    private Rigidbody vehicleRigidbody;

    public float forwardRate = 0.0f;
    public float addedIntensitySpeed = 0.0f;

    private float degreesPerSecondLeft;
    private float degreesPerSecondRight;
    private float totalRotation = 0.0f;
    private float rotationStep;

    private Vector3 rotatePoint;
    private float laneVal = 8.0f;
    bool rotateLeft = false;
    bool rotateRight = false;

    void Awake()
    {
        vehicleRigidbody = GetComponent<Rigidbody>();
    }
    // Update is called once per physics update
    void FixedUpdate()

    {
        if (rotateLeft)
        {
            TurnLeft();
        }
        else if (rotateRight)
        {
            TurnRight();
        }
        else
        {
            Move();
        }
    }

    void Move()
    { //Handles movement forwards and backwards
        Vector3 forwardMoveAmount = vehicleRigidbody.transform.forward * forwardRate;
        vehicleRigidbody.velocity = forwardMoveAmount;
    }

    public void setPoliceSpeed(float speed)
    {
        forwardRate = speed + addedIntensitySpeed;
}

    void TurnLeft()
    {
        degreesPerSecondLeft = (-90 * forwardRate) / (0.5f * Mathf.PI * laneVal);
        rotationStep = degreesPerSecondLeft * Time.fixedDeltaTime;
        if (rotationStep < -90.0f - totalRotation)
        {
            rotationStep = -90.0f - totalRotation;
            totalRotation = 0.0f;
            rotateLeft = false;
        }
        else
        {
            totalRotation += rotationStep;
        }
        transform.RotateAround(rotatePoint, Vector3.up, rotationStep);
    }

    void TurnRight()
    {
        degreesPerSecondRight = (90 * forwardRate) / (0.5f * Mathf.PI * (16.0f - laneVal));
        rotationStep = degreesPerSecondRight * Time.fixedDeltaTime;
        if (rotationStep > 90.0f - totalRotation)
        {
            rotationStep = 90.0f - totalRotation;
            totalRotation = 0.0f;
            rotateRight = false;
        }
        else
        {
            totalRotation += rotationStep;
        }
        transform.RotateAround(rotatePoint, Vector3.up, rotationStep);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "LeftTurn")
        {
            vehicleRigidbody.velocity = Vector3.zero;
            rotatePoint = transform.position + (-transform.right * laneVal);
            rotateLeft = true;
        }
        else if (other.tag == "RightTurn")
        {
            vehicleRigidbody.velocity = Vector3.zero;
            rotatePoint = transform.position + (transform.right * (16.0f - laneVal));
            rotateRight = true;
        }
        else if (other.tag == "Vehicle")
        {
            //Destroy truck we will ahve to then in the GM remove the null vehicle gameobjects.
            Destroy(other.transform.parent.gameObject);
        }
    }
}
