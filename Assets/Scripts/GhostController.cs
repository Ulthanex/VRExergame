//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;

//public class GhostController: MonoBehaviour {

//    [SerializeField]
//    private GameObject player;

//    [SerializeField]
//    GameObject tilt;

//    [SerializeField]
//    GameManager GM;

//    [SerializeField]
//    Text seperation;

//    private float[] ghostSpeedRecording;
//    private float[] ghostLeanRecording;
//    private int recordingIterator = 0;

//    // Update is called once per frame
//    private Rigidbody ghostRigidbody;
    
//    private float forwardSpeed, horizontalSpeed;

//    private float forwardProduct;

//    private Vector3 forwardVelocity, horizontalVelocity, rotatePoint;

//    private bool rotateLeft, rotateRight;

//    private float degreesLeft;
//    private float degreesRight;
//    private float totalRotation = 0.0f;
//    private float rotationStep;
//    private float maxLean;

//    void Awake()
//    {
//        ghostRigidbody = GetComponent<Rigidbody>();

//        //Setting the ghosts specs to be the same as those defined in the player inspector.
//        PlayerController PC = player.GetComponent<PlayerController>();
//        forwardSpeed = PC.forwardSpeed;
//        horizontalSpeed = PC.horizontalSpeed;
//        maxLean = PC.maxLean;
//    }

//    // Update is called once per frame
//    void FixedUpdate()
//    {
//        if(ghostSpeedRecording != null)
//        {
//            if(recordingIterator!= ghostSpeedRecording.Length)
//            {
//                float v = ghostSpeedRecording[recordingIterator];
//                float h = ghostLeanRecording[recordingIterator];
//                recordingIterator++;
//                Move(v, h);
//                calculateSeperation();
//            }
//            else
//            {
//                GM.ghostExists = false;
//                Destroy(this.gameObject);
//            }
//        }
//        else
//        {
//            Destroy(this.gameObject);
//        }

//    }

//    void calculateSeperation()
//    {
//        int distance = Mathf.RoundToInt(player.transform.position.z - transform.position.z);
//        string tempSep;
//        if(distance > 0)
//        {
//            tempSep = "+" + distance.ToString() + " m";
//        }
//        else
//        {
//            tempSep = distance.ToString() + " m";
//        }
//        seperation.text = tempSep;
//    }

//    void Move(float v, float h)
//    { //Handles movement forwards and backwards

//        if (rotateLeft)
//        {
//            TurnLeft(v);
//        }
//        else if (rotateRight)
//        {
//            TurnRight(v);
//        }
//        else
//        {
//            forwardProduct = forwardSpeed * v;
//            forwardVelocity = ghostRigidbody.transform.forward * forwardProduct;
//        }

//        horizontalVelocity = ghostRigidbody.transform.right * horizontalSpeed * h;

//        //Setting the bike rotation
//        Vector3 angle = tilt.transform.eulerAngles;
//        angle.z = h * maxLean / 3.0f;
//        tilt.transform.eulerAngles = angle;

//        ghostRigidbody.velocity = forwardVelocity + horizontalVelocity;
//    }

//    void TurnLeft(float v)
//    {
//        //Float difference between position and rotation point
//        float seperation = Vector3.Distance(transform.position, rotatePoint);
//        forwardProduct = v * forwardSpeed;

//        degreesLeft = (-90 * forwardProduct) / (0.5f * Mathf.PI * seperation);

//        rotationStep = degreesLeft * Time.fixedDeltaTime;
//        if (rotationStep < -90.0f - totalRotation)
//        {
//            rotationStep = -90.0f - totalRotation;
//            totalRotation = 0.0f;
//            rotateLeft = false;
//        }
//        else
//        {
//            totalRotation += rotationStep;
//        }
//        transform.RotateAround(rotatePoint, Vector3.up, rotationStep);
//    }

//    void TurnRight(float v)
//    {
//        float seperation = Vector3.Distance(transform.position, rotatePoint);
//        forwardProduct = v * forwardSpeed;

//        degreesRight = (90 * forwardProduct) / (0.5f * Mathf.PI * seperation);

//        rotationStep = degreesRight * Time.fixedDeltaTime;
//        if (rotationStep > 90.0f - totalRotation)
//        {
//            rotationStep = 90.0f - totalRotation;
//            totalRotation = 0.0f;
//            rotateRight = false;
//        }
//        else
//        {
//            totalRotation += rotationStep;
//        }
//        transform.RotateAround(rotatePoint, Vector3.up, rotationStep);
//    }

//    void OnTriggerEnter(Collider other)
//    {
//        if (other.tag == "LeftTurn")
//        {
//            forwardVelocity = Vector3.zero;
//            rotatePoint = other.transform.Find("RotationPoint").transform.position;
//            rotateLeft = true;
//        }
//        else if (other.tag == "RightTurn")
//        {
//            forwardVelocity = Vector3.zero;
//            rotatePoint = other.transform.Find("RotationPoint").transform.position;
//            rotateRight = true;
//        }
//    }

//    public void initialiseArrays(float[] parsedSpeedRecording, float[] parsedLeanRecording)
//    {
//        ghostSpeedRecording = parsedSpeedRecording;
//        ghostLeanRecording = parsedLeanRecording;
//    }
//}
