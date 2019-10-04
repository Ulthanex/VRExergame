using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvasiveEnemy : MonoBehaviour {

	[System.Serializable]
	public enum movementType{ //Only root motion currently used to match animcations
		Root,
		Speed,
		Duration
	}

	[System.Serializable]
	public enum pathingType{
		ZigZag,
		Evade,
		Random,
		Forward
	}

	[Header("Player Avoidance")]
	/*Game Objects*/
	public Transform player; //The players transform from which to flee from

	[Header("Active Play Area")]
	/*Active Map Width*/
	public float mapWidth = 2.5f; //The width of the pathway that the gameobject can walk into +- the value

	[Header("Pathfinding Parameters")]
	public movementType moveType; //Type of movement, by speed / duration / root motion
	public pathingType pathType; //Type of movement path taken
	public float movementSpeed; //The speed at which the character moves
	public float rotationSpeed; //The speed at which the character rotates
	public float HostileRange = 13f; //The Distance at which the enemy becomes hostile to the player
	public int movementDuration = 5;//The length of time one will maintain the path
	public float boundaryAngleClamp; //The angle of direction the character is clamped to, adjusting as one gets closer to boundary
	public float evasionAngleOffset; //The random range that is applied to the current walking direction for that interval

	[Header("Animation")]
	public GameObject body; //Physical body of character
	public GameObject deathEffect; //Animation effect played during death


	/* Private Parameters */
	public Vector3 currentDestination;
	private personalityType gameCondition;
	private bool activeHostility = false;
	Animator _animator;
	//CleanUp
	private float cleanUpProgress = 0f;
	private float cleanUpTimer = 10f;



	/*----------------*/
	// Initialization //
	void Start () {
		_animator = gameObject.GetComponent<Animator> (); //Get Animator
		player = PlayerController.Instance.transform; //Get Player Transform
		pathType = (pathingType)Random.Range(0, 4);

		StartCoroutine (movement ()); 
	}
		

	/*---------------------------------------------*/
	// Coroutine to run from OR towards the player //
	public IEnumerator movement ()
	{
		//Only Act if player target has been set
		if (player == null) {
			this.gameObject.SetActive (false);
			Debug.Log ("Player Target has not been set");
		}

		while (true) {
			//Begins inactive, acting on their behaviour once a player gets close enough //---------------------------
			if (!activeHostility) {
				//Debug.Log ("Distance from player = " + Vector3.Distance (transform.position, player.position));
				if (Vector3.Distance (transform.position, player.position) < HostileRange) {
					activeHostility = true;
					_animator.SetTrigger ("active");
					yield return null;
				}
				yield return null;
			} else {
				//Obtain next destination to move towards 
				findMovementPath ();
		
				//Get Target rotation to lerp towards
				Vector3 targetRot = currentDestination - transform.position;
             
                //If Travelling by root motion of animation //---------------------------------------
                if (moveType == movementType.Root) {
					while (Vector3.Distance (transform.position, currentDestination) > 0.5f) { //&& transform.position.z < currentDestination.z ){
						Vector3 newRot = Vector3.RotateTowards (transform.forward, targetRot, rotationSpeed * Time.deltaTime, 0.0f);
						transform.rotation = Quaternion.LookRotation (newRot);
						cleanupCheck ();
						yield return null;
					}
				}

				//If travelling by speed // UNUSED -----------------------------------
				else if (moveType == movementType.Speed) {
				
					// speed should be 1 unit per second
					while (transform.position != currentDestination) {
						//transform.position = Vector3.MoveTowards(transform.position, currentDestination, movementSpeed * Time.deltaTime);
						Vector3 newRot = Vector3.RotateTowards (transform.forward, targetRot, rotationSpeed * Time.deltaTime, 0.0f);
						transform.rotation = Quaternion.LookRotation (newRot);
						cleanupCheck ();
						yield return null;
					}

				//If travelling by set duration //  UNUSED--------------------------------------
				} else if (moveType == movementType.Duration) {
					float elapsedTime = 0;
					Vector3 startingPos = transform.position;
					while (elapsedTime < movementDuration) {
						transform.position = Vector3.Lerp (startingPos, currentDestination, (elapsedTime / movementDuration));
						elapsedTime += Time.deltaTime;
						cleanupCheck ();
						yield return null;
					}
					transform.position = currentDestination;
				}
			}
		}
	}
		
	/*-----------------------------------------------------------------------------*/
	// Takes position of player and position on map to derive direction to flee in //
	public void findMovementPath (){

		/* Move Forward Evasion Type *///------------------------------------------------------------------------------------------- 
		if (pathType == pathingType.Forward) {
			forwardMovement();
		
		/* Random evasion Type *///----------------------------------------------------------------------------------------------------
		} else if(pathType == pathingType.Random) {
			randomMovement ();
		/* Evasive & ZigZag evasion Type *///---------------------------------------------------------------------------------------
		}else{
		//Determine whether diagonally evading or aiming towards player
			if (tag == "Ghost") { //Conqueror - variant
				diagonalMovementEvade ();
			} else { //Survivor-Variant
				diagonalMovementPursuit ();
			}
		}
	}

	/*---------------------------------------*/
	//Movement in a single forward direction //
	private void forwardMovement(){
		Vector3 forwardPosition;
		forwardPosition = transform.position + transform.forward * 3; //Sets target destination set amount of steps forwards
		currentDestination = new Vector3 (Mathf.Clamp (forwardPosition.x, -mapWidth, mapWidth), forwardPosition.y, forwardPosition.z);
	}


	/*------------------------------------*/
	//Movement without a strict direction //
	private void randomMovement(){
		Vector3 forwardPosition; //Forward displacement on target position
		Vector3 startRotation;
		float mapPositionPerc; //The percentage position on horizontal axis
		float currentAngle = 0; //The Angle direction to head in to avoid the player

		//Calculation position on horizontal axis to ensure that it doesnt continuously gutter ball itself
		mapPositionPerc = Mathf.InverseLerp (-mapWidth, mapWidth, transform.position.x);
	
		//Change Angle of walking dependent on game condition (Conqueror -away, evade -towards)
		if (tag == "Ghost") {	
			if (mapPositionPerc < 0.25f) {currentAngle = 45;} 
			else if (mapPositionPerc > 0.75f) {currentAngle = -45;} 
			else {currentAngle = Random.Range (-45, 45);}
		} else{
			if (mapPositionPerc < 0.25f) {currentAngle = 135;} 
			else if (mapPositionPerc > 0.75f) {currentAngle = 225;} 
			else {currentAngle = Random.Range (135, 225);}
		}

		//Find Destination
		startRotation = this.transform.eulerAngles;
		transform.eulerAngles = new Vector3 (0, currentAngle, 0);
		forwardPosition = transform.position + transform.forward * 3;
		transform.eulerAngles = startRotation;
		currentDestination = new Vector3 (Mathf.Clamp (forwardPosition.x, -mapWidth, mapWidth), forwardPosition.y, forwardPosition.z);
	}

	/*--------------------------------------------------------------*/
	//Movement that either moves away/towards the player at an angle//
	private void diagonalMovementEvade(){
		Vector3 forwardPosition; //Forward displacement on target position
		Vector3 startRotation;
		float mapPositionPerc; //The percentage position on horizontal axis
		float currentAngle = 0; //The Angle direction to head in to avoid the player

		//If Infront of the player -- Determine angle from player to transform
		if (transform.position.z >= player.position.z) {
			Vector3 playerDir = transform.position - player.position;
			currentAngle = Vector3.Angle (playerDir, Vector3.forward);
		
		//If behind the player, run in a straightfoward direction
		} else { 
			currentAngle = 0;
		}

		//Inverse the angle of direction dependent on relative x-axis position to player 
		// (or leave un-inversed to have them criss-cross the player)

		/* Normal Evasion */
		if (pathType == pathingType.Evade) {
			if (player.position.x > transform.position.x) {
				currentAngle = currentAngle*-1; //evasive
			}
		/* Criss-cross evasion */
		} else {
			if (player.position.x < transform.position.x) {
				currentAngle = currentAngle * -1; //criss cross
			}
		}

		//Clamps running angle into a forward motion (clamped within -30 +30 degree range)
		float clampedAngle = Mathf.Clamp (currentAngle, -30, 30); //get clamped foward angle from player

		//Calculate character position on map as a percentage to identify the clamped rotation of the character
		//to prevent them over steering into a wall / also calculate a random offset to potential correct guttering
		mapPositionPerc = Mathf.InverseLerp (-mapWidth, mapWidth, this.transform.position.x);
		float negativeAngleBoundary = (boundaryAngleClamp * mapPositionPerc) * -1;
		float positiveAngleBoundary = boundaryAngleClamp - Mathf.Abs (negativeAngleBoundary);
		float lowerBoundEvasion = evasionAngleOffset * mapPositionPerc;
		float upperBoundEvasion = evasionAngleOffset - lowerBoundEvasion;

		//Create a random offset to the direction for unexpected evasion
		float rOffset = Random.Range (-lowerBoundEvasion, upperBoundEvasion);

		//Find Destination
		startRotation = this.transform.eulerAngles;
		transform.eulerAngles = new Vector3 (0, Mathf.Clamp (clampedAngle + rOffset, negativeAngleBoundary, positiveAngleBoundary), 0);
		forwardPosition = transform.position + transform.forward * 3;
		transform.eulerAngles = startRotation;
		currentDestination = new Vector3 (Mathf.Clamp (forwardPosition.x, -mapWidth, mapWidth), forwardPosition.y, forwardPosition.z);
	}


	/*--------------------------------------------------------------*/
	//Movement that either moves away/towards the player at an angle//
	private void diagonalMovementPursuit(){
		Vector3 forwardPosition; //Forward displacement on target position
		Vector3 startRotation;
		float mapPositionPerc; //The percentage position on horizontal axis
		float currentAngle = 0; //The Angle direction to head in to avoid the player

		//Determine angle of pursuit from transform to player
		if (transform.position.z >= player.position.z) {
			Vector3 playerDir = transform.position - player.position;
			if (transform.position.x <= player.position.x) {
				currentAngle = 180 - Vector3.Angle (playerDir, Vector3.forward);
			} else{
				currentAngle = 180 + Vector3.Angle (playerDir, Vector3.forward);
			}
		} else { 
			currentAngle = 180;
		}

		//Clamps running angle into a forward motion (clamped within -30 +30 degree range)
		float clampedAngle = Mathf.Clamp (currentAngle, 150, 210); //get clamped foward angle from player

		//Calculate character position on map as a percentage to identify the clamped rotation of the character
		//to prevent them over steering into a wall / also calculate a random offset to potential correct guttering
		mapPositionPerc = Mathf.InverseLerp (-mapWidth, mapWidth, this.transform.position.x);

		float positiveAngleBoundary = (boundaryAngleClamp * mapPositionPerc) + 180;
		float negativeAngleBoundary = (boundaryAngleClamp - Mathf.Abs ((boundaryAngleClamp * mapPositionPerc))) * -1 + 180;
		float lowerBoundEvasion = evasionAngleOffset * mapPositionPerc;
		float upperBoundEvasion = evasionAngleOffset - lowerBoundEvasion;

		//Create a random offset to the direction for unexpected evasion
		float rOffset = Random.Range (-upperBoundEvasion, lowerBoundEvasion);

		//Find Destination
		startRotation = this.transform.eulerAngles;
		transform.eulerAngles = new Vector3 (0, Mathf.Clamp (clampedAngle + rOffset, negativeAngleBoundary, positiveAngleBoundary), 0);
		//Debug.Log ("New angle to player = " +  Mathf.Clamp (clampedAngle + rOffset, negativeAngleBoundary, positiveAngleBoundary));
		forwardPosition = transform.position + transform.forward * 3;
		transform.eulerAngles = startRotation;
		currentDestination = new Vector3 (Mathf.Clamp (forwardPosition.x, -mapWidth, mapWidth), forwardPosition.y, forwardPosition.z);
	}

	/*---------------------------------------------------------------------------------*/
	// Detect Collisions with our personal collider -- for player stampede recognition //
	void OnTriggerEnter(Collider col){

		//If Player enters hitbox trigger -- transition to dieing
		if(col.tag == "Player") {
			body.SetActive (false); //Disables physical body on collision
			deathEffect.SetActive(true); 
				destroyThis (4);
		}
	}

	/*---------------------------------*/
	//Cleans up enemies, removing them //
	private void cleanupCheck(){
		//Clean up if the player has gone past the enemy (+minium offset range) and a set duration has passed
		if (transform.position.z < player.transform.position.z - 1) {
			cleanUpProgress += Time.deltaTime;
			if (cleanUpProgress > cleanUpTimer) {
				destroyThis (4);
			}
		} else {
			cleanUpProgress = 0f;
		}
	}

	/*-------------------------------------------------*/
	//Removes any references and destroy's game object //
	private void destroyThis(int count = -1){
		if(GameManager.Instance.enemyList.Contains(this.gameObject)){
			GameManager.Instance.enemyList.Remove(this.gameObject); //Remove it from game manager list
			if (count != -1) {
				Destroy (this.gameObject, count);
			} else {
				Destroy (this.gameObject);
			}
		}
	}
}
