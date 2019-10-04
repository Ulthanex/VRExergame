using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float hitForce;
    public float hitRange;

    //[SerializeField]
    //SerialPortCommunicator SPC;

    [SerializeField]
    GameObject headset, tilt;

	// Recording of players speed and leaning for ghost recreation?
    private List<float> playerSpeedRecording = new List<float>();
    private List<float> playerLeanRecording = new List<float>();

	//Rigidbody of the player avatar
    private Rigidbody playerRigidbody;
    
    public float forwardSpeed = 10.0f, horizontalSpeed = 5.0f, maxLean = 35.0f, rpmDividor = 100.0f;

    private float forwardProduct;

    private Vector3 forwardVelocity, horizontalVelocity, rotatePoint;

    private bool rotateLeft, rotateRight;

    private float degreesLeft;
    private float degreesRight;
    private float totalRotation = 0.0f;
    private float rotationStep;

    private bool noLeft = false, noRight = false, noBuffer = false;

    //public bool freezeMovement = false;
    public PlayerBodyController playerBodyController;
    public Animator hitAnimator;

    //[SerializeField]
    //PoliceBehaviour police;

	//Singleton value -- For obtaining player transform indirectly
	private static PlayerController _instance;

	/*------------------------*/
	//Singleton Getter Method //
	public static PlayerController Instance { get { return _instance; } }


	/*----------*/
	// On Awake //
    void Awake()
    {
		//Initialises singleton instance
		if (_instance != null && _instance != this) {
			Destroy (this.gameObject);
		}else{
			_instance = this;
		}
		//Initialises rigidbody component
		playerRigidbody = GetComponent<Rigidbody>(); 
	}


	/*--------------------------------------*/
    // Fixed Update is called once per frame//
    void FixedUpdate()
    {
        //This is where we will put a function to sort out 
        //float v = calculateSpeed();
        //float h = calculateLean();
        //Move(v,h);
    }


	/*-----------------------------------------------*/
	// Give a value between 0 and one based on bike. // -- Currently Defunct
//    float calculateSpeed() 
//    {
//        float speed;
//        if (freezeMovement)
//        {
//            speed = 0.0f;
//        }
//        else
//        {
//            //speed = SPC.rpm / rpmDividor;
//            speed = Input.GetAxisRaw("Vertical");
//            //speed = playerBodyController.currentMovement;
//        }
//        
//        playerSpeedRecording.Add(speed);
//        return speed;
//    }


	/*------------------------------------------*/
	// Calculates player lean from vive headset // -- Currently defunct
//    float calculateLean()
//    {
//        float lean;
//        if (freezeMovement)
//        {
//            lean = 0.0f;
//        }
//        else
//        {
//            //Vive headset
//            
//			//Attempts to obtain Vive headset euler rotation along the z axis
//            float rotation = headset.transform.eulerAngles.z;
//            //offsets rotations above 90 degrees
//			if(rotation > 90)
//            {
//                rotation -= 360;
//            }
//
//            lean = -rotation / maxLean;
//      
//            //lean = Input.GetAxisRaw("Horizontal");
//            if (noLeft && lean < 0.0f)
//            {
//                lean = 0.0f;
//            }
//            else if (noRight && lean > 0.0f)
//            {
//                lean = 0.0f;
//            }
//        }
//
//        playerLeanRecording.Add(lean);
//        return lean;
//    }


	/*--------------------------------------*/
	//Handles Forwards and Backwards motion // -- defunct?
//    void Move(float v, float h)
//    { 
//        
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
//            forwardVelocity = playerRigidbody.transform.forward * forwardProduct;
//            //police.setPoliceSpeed(forwardProduct);
//        }
//
//        horizontalVelocity = playerRigidbody.transform.right * horizontalSpeed * h;
//        //Setting the bike rotation
//        Vector3 angle = tilt.transform.eulerAngles;
//        angle.z = h * maxLean / 3.0f;
//        tilt.transform.eulerAngles = angle;
//
//        playerRigidbody.velocity = forwardVelocity + horizontalVelocity;
//    }




    public float[] getPlayerSpeedRecording()
    {
        return playerSpeedRecording.ToArray();
    }

    public float[] getPlayerLeanRecording()
    {
        return playerLeanRecording.ToArray();
    }

	/*----------------------------------------------------------------------*/
    //called by the player body controller, to notify of a trigger collision//
    public void onBodyTrigger(Collider other)
    {
		switch (other.tag) {

		case "Straight":
            GameManager.Instance.spawnTile();
			break;

		//case "Vehicle":
			//Destroy truck we will ahve to then in the GM remove the null vehicle gameobjects.
			//Destroy(other.transform.parent.gameObject);
			//Increase resitance or slow user / minus points.
			//SPC.crashResistance = 1.15f;
		//	other.transform.parent.GetComponent<VehicleBehaviour> ().enabled = false;
		//	Rigidbody truckBody = other.transform.parent.GetComponent<Rigidbody> ();
		//	truckBody.freezeRotation = false;
		//	motionEnhancer motionEnhancer = GetComponent<motionEnhancer> ();
			//truckBody.AddExplosionForce(hitForce, transform.position , hitRange);
		//	truckBody.AddRelativeForce (Random.Range (-100 * Mathf.Pow (motionEnhancer.avatarSpeed, 1.5f), 100 * Mathf.Pow (motionEnhancer.avatarSpeed, 1.5f)), Random.Range (0, 500 * Mathf.Pow (motionEnhancer.avatarSpeed, 1.5f)), Random.Range (Mathf.Pow (100 * motionEnhancer.avatarSpeed, 1.5f), Mathf.Pow (200 * motionEnhancer.avatarSpeed, 1.5f)));
		//	truckBody.AddTorque (Random.Range (-motionEnhancer.avatarSpeed * 5000, motionEnhancer.avatarSpeed * 5000), Random.Range (-motionEnhancer.avatarSpeed * 5000, motionEnhancer.avatarSpeed * 5000), Random.Range (-motionEnhancer.avatarSpeed * 5000, motionEnhancer.avatarSpeed * 5000));
		//	break;

		case "LeftBox":
			if (noLeft == true) {
				noBuffer = true;
			} else {
				noLeft = true;
			}
			break;

		case "RightBox":
			if (noRight == true) {
				noBuffer = true;
			} else {
				noRight = true;
			}
			break;


		case "Ghost": //Collide with conqueror type ghost
			//switch (GameManager.Instance.gameCondition) {

			//case personalityType.Conqueror:
				GameManager.Instance.adjustPoints(25,0);
				//break;

			//case personalityType.Survivor:
			//	GameManager.Instance.adjustPoints(-25);
			//	break;
			//}
			break;

		case "GhostS": //Collide with Survivor type ghost
			GameManager.Instance.adjustPoints(-25,2);
			break;

		case "SkeletonLich": //Collide with the skeleton lich (Conqueror only)
			GameManager.Instance.adjustPoints(500,4);

			break;

		case "Skeleton": //Collide with conqueror type skeleton
			//switch (GameManager.Instance.gameCondition) {

			//case personalityType.Conqueror:
				GameManager.Instance.adjustPoints(50,1);
			//	break;

			//case personalityType.Survivor:
			//	GameManager.Instance.adjustPoints(-50);
			//	break;
			//}
			break;

		case "SkeletonS": //Collide with conqueror type skeleton
			GameManager.Instance.adjustPoints(-50,3);
			break;

		case "SkeletonSwipe": //Collide with sword swipe from skeleton
			GameManager.Instance.adjustPoints(-50,3);
			break;

		case "SkullProjectile": //collide with either falling skull/thrown skull projectile
			GameManager.Instance.adjustPoints(-50,6);
			break;

		}

    }

	/*-----------------------------------*/
	//When Player exits trigger collider //
    void OnTriggerExit(Collider other)
    {
        if(other.tag == "LeftBox")
        {
            if (noBuffer)
            {
                noBuffer = false;
            }
            else
            {
                noLeft = false;
            }
        }
        else if (other.tag == "RightBox")
        {
            if (noBuffer)
            {
                noBuffer = false;
            }
            else
            {
                noRight = false;
            }
        }
    }
}
