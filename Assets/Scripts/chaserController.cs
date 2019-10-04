using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZCameraShake;

public class chaserController : MonoBehaviour {

    public float speed = 0;
    public float finalSpeed;
    public float distanceWeight;
    public float increaseRate;
    public float attackPenalty;
    public float attackRange;

    public Transform player;
    public motionEnhancer motionEnhancer;
	private ExerciseProtocolState state = ExerciseProtocolState.lowIntensity;
	//public bool active;
    private float previousDistance;

    [Header("Distance Score Parameters")]
    public float maxPDist; //maximum distance before we cap out on points we can earn    50 cap max, 10 short
    public float minPDist; //minimum distance
	public int lowScore = 25; //The amount of base score we get every time interval for keeping distance from chaser
	public int highScore = 100; //Amount of base score each interval during high intensity

	[Header("Camera Shake Values")]
	public float minMagnitude;
	public float maxMagnitude;
	public float roughness;
	public float fadeInTime;
	public float fadeOutTime;

	[Header("Audio Parameters")]
	public AudioClip[] clips;
	private AudioSource source;

	public void Awake(){
		source = GetComponent<AudioSource> ();
	}

	/*-------------------*/
	// On Initialisation //
    public void Start()
    {
        StartCoroutine(UpdateSpeed());
    }


	/*--------------------------------------------------------------------------------------------*/
	// On Update -- Moves the chaser forwards a set distance and sets speed of running multiplier //
    void Update()//NOTE: CHANGE THIS FROM UPDATE TO FIXEDUPDATE
    {
		//if z-pos is outside attack range OR finalSpeed is less than 0
        if (transform.position.z < player.position.z - attackRange || finalSpeed<0) //astronaut cant get ahead of the player
        {
			transform.position += Vector3.forward * finalSpeed * Time.deltaTime;
        }
        
		//Sets animation multiplier
        GetComponent<Animator>().SetFloat("runMultiplier",Mathf.Min( finalSpeed/5 ,2f));
    }

	/*-------------------------------------------*/
	// Updates speed relative to player distance //
	IEnumerator UpdateSpeed()
	{
		//sets initial speed of chaser at 0.17% of player max speed
		speed = motionEnhancer.calculateBaseRunningSpeed(0.17f);

		while (true)
		{
			//Calculate current distance between chaser and player
			float distance = player.transform.position.z - this.transform.position.z;

			//Act according to intensity state
			switch (state) {

			case ExerciseProtocolState.lowIntensity: //-------------------------------------------------

                //keeps chaser following at low base player running speed to provide incentive to stay away
                finalSpeed = motionEnhancer.calculateBaseRunningSpeed(0.10f); 
                //Increment player points by distance we manage to maintain upto a cap to ensure people are moderately keeping away
				GameManager.Instance.adjustPoints(lowScore,6);

				break;

			case ExerciseProtocolState.highIntensity: //--------------------------------------------------
				
				//player is gaining distance on the astronaut
				if (distance > previousDistance)
				{
					//if distance from player is more than attackRange+7, astronaut gains on the player normally, if closer, gain lessens until we reach 
					//distance attack range+1 where there is no gain. At that point the player simply needs to maintain his speed
					speed += Mathf.InverseLerp(attackRange + 1, attackRange + 7, distance) * motionEnhancer.calculateBaseRunningSpeed(increaseRate);
				}

				//adding the contribution of distance to the final speed
				finalSpeed = speed + distanceWeight * distance;
				//Increment player points 
				GameManager.Instance.adjustPoints(highScore,6);
				//Debug.Log("High Intensity Speed = " + finalSpeed);
				break;

			}

			//Chaser within attack Range
			if (distance <= attackRange)
			{
				//Sets speed to a maximum of either (base running speed * 0.17) or (current speed - attack penalty)
				speed = Mathf.Max(speed - motionEnhancer.calculateBaseRunningSpeed(attackPenalty), motionEnhancer.calculateBaseRunningSpeed(0.17f));
				GameManager.Instance.adjustPoints(-100,5); //deduct 100 points for collision
			}
			yield return new WaitForSeconds(0.5f);

		}

	}


    /*-----------------------------*/
	// Shakes camera of the player //
	public void stomp(int clip)
    {
        CameraShaker.Instance.ShakeOnce(Mathf.Lerp(minMagnitude, maxMagnitude, Mathf.InverseLerp(50,0, player.transform.position.z - this.transform.position.z)), roughness, fadeInTime, fadeOutTime);
		source.PlayOneShot (clips [clip]); //Plays audio clip for stomp
	}
		

	/*------------------------------------------------------------------------------------*/
	//Adjusts the current state of the chaser (slow moving -- Low  || High speed -- High) //
    public void setState(ExerciseProtocolState newState)
    {
		state = newState;
    }


	/*-----------------------------------------------------------------------*/
	// Deactivates the chaser when the high-intensity exercise interval ends //
   // public void deactivate()
   // {
   //     active = false;
   //     finalSpeed = 0;
   // }
}
