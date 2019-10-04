using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZCameraShake;

public class astronautController : MonoBehaviour {

    public float speed;
    public float finalSpeed;
    public float distanceWeight;
    public float increaseRate;
    public float attackPenalty;
    public float attackRange;

    public Transform player;
    public motionEnhancer motionEnhancer;
    public Animator hitPanelAnimator;
    public bool active;
    private float previousDistance;

	[Header("Camera Shake Values")]
	public float minMagnitude;
	public float maxMagnitude;
	public float roughness;
	public float fadeInTime;
	public float fadeOutTime;

	/*-------------------*/
	// On Initialisation //
    private void Start()
    {
        speed = 0;
        //StartCoroutine(CalculateSpeed());
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
        GetComponent<Animator>().SetFloat("runMultiplier",Mathf.Min( finalSpeed/5,3f));
    }

	/*-------------------------------------------*/
	// Updates speed relative to player distance //
	IEnumerator UpdateSpeed()
	{
		//sets initial speed of chaser at 0.17% of player max speed
		speed = motionEnhancer.calculateBaseRunningSpeed(0.17f);

		//While chaser is active in a High-intensity phase
		while (active)
		{
			//Calculate current distance between chaser and player
			float distance = player.transform.position.z - this.transform.position.z;

			//player is gaining distance on the astronaut
			if (distance > previousDistance)
			{
				//if distance from player is more than attackRange+7, astronaut gains on the player normally, if closer, gain lessens until we reach 
				//distance attack range+1 where there is no gain. At that point the player simply needs to maintain his speed
				speed += Mathf.InverseLerp(attackRange + 1, attackRange + 7, distance) * motionEnhancer.calculateBaseRunningSpeed(increaseRate);
			}

			//Chaser within attack Range // -----------------
			if (distance <= attackRange)
			{
				//Sets speed to a maximum of either (base running speed * 0.17) or (current speed - attack penalty)
				speed = Mathf.Max(speed - motionEnhancer.calculateBaseRunningSpeed(attackPenalty), motionEnhancer.calculateBaseRunningSpeed(0.17f));
				GameManager.Instance.adjustPoints(-100,5); //deduct 100 points for collision
			}

			//adding the contribution of distance to the final speed
			finalSpeed = speed + distanceWeight * distance;
			yield return new WaitForSeconds(0.5f);
		}

	}


    /*-----------------------------*/
	// Shakes camera of the player //
    public void stomp()
    {
        CameraShaker.Instance.ShakeOnce(Mathf.Lerp(minMagnitude, maxMagnitude, Mathf.InverseLerp(50,0, player.transform.position.z - this.transform.position.z)), roughness, fadeInTime, fadeOutTime);
    }

 

	/*---------------------------------------------------------------------------------*/
	//Called by the High-intensity exercise protocol controller, activating the chaser //
    public void activate()
    {
        active = true;
        StartCoroutine(UpdateSpeed());
    }


	/*-----------------------------------------------------------------------*/
	// Deactivates the chaser when the high-intensity exercise interval ends //
    public void deactivate()
    {
        active = false;
        finalSpeed = 0;
    }
}
