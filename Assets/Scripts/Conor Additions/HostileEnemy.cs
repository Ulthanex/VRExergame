using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum hostileType{
	Pursuit,
	Block
}

public enum blockingState{
	Idle,
	Moving,
	Blocking,
	Attacking,
	Dead
}

public class HostileEnemy : MonoBehaviour{

	[Header("Player Target")]
	public Transform player; //The players transform from which to flee from

	[Header("Enemy Parameters")]
	public hostileType behaviour; //Type of behaviour the enemy commits too
	public float movementSpeed; //Movement speed of the enemy
	public float rotationSpeed; //rotation speed of the enemy
	public float hostileRange = 13f; //The Distance at which the enemy becomes hostile to the player 
	public float attackRange = 1f; //The Distance at which the enemy attempts to attack the player
	private bool activeHostility = false;

	[Header("Blocking Parameters")]
	//Blocking ---------------------------------
	private Vector3 currentBlockPos = new Vector3 (-9999, -9999, -9999); 
	private blockingState blockState = blockingState.Idle;
	private float blockingProgress = 0f;// Running timer for blocking until, moving to next position
	public float blockDuration = 5f;//The length of time an enemy stays blocking a point

	//Attacking -----------------------------------
	Animator _animator;
	public BoxCollider attackHitBox;
	private Rigidbody rBody;
	private CapsuleCollider capCollider;
    private SkinnedMeshRenderer playerRenderer = null;

    //CleanUp
    private float cleanUpProgress = 0f;
	private float cleanUpTimer = 4f;



	/*----------------------------*/
	// Initialization on start up //
	void Start()
	{
		_animator = gameObject.GetComponent<Animator> (); //Get Animator
		rBody = gameObject.GetComponent<Rigidbody>();
		player = PlayerController.Instance.transform; //Get Player Transform
		capCollider = gameObject.GetComponent<CapsuleCollider>();
        playerRenderer = player.GetComponentInChildren<SkinnedMeshRenderer>(); //Attempts to grab skinned mesh renderer of player
    }


	/*--------*/
	// Update //
	void Update() {

		// Hostile enemy begins inactive, acting on their behaviour once a player gets close enough //---------------------------
		if (!activeHostility) {
			if (Vector3.Distance (transform.position, player.position) < hostileRange) {
				activeHostility = true;
			}
		
		//Active, begin acting on identified behaviour //---------------------------------------------
		} else {
			
			//Attempt to block the player//
			if (behaviour == hostileType.Block) {
				BlockPlayer ();

			//Attempt to pursue the player
			} else {
				//PursuePlayer ();
			}
		}

		//Clean up if the player has gone past the enemy (+minium offset range) and a set duration has passed
		if (transform.position.z < player.transform.position.z - 5) {
			cleanUpProgress += Time.deltaTime;
			if (cleanUpProgress > cleanUpTimer) {
				destroyThis ();
			}
		} else {
			cleanUpProgress = 0f;
		}
	}

	/*--------------------------------------------------------------*/
	// Function to Pursue the Player and try to defend against them //  
	public void BlockPlayer() {
		Vector3 targetRot;
		Quaternion newRot;
		Vector2 minBlockBoundary;
		Vector2 maxBlockBoundary;

		switch (blockState) {

		/*-------------------------------------*/
		// Idle State -- Default Initial state //
		case blockingState.Idle:
			
			//Grab first location to move too
			acquirePosition();
			blockState = blockingState.Moving;
			break;

		/*--------------------------------------------------*/
		// Moving state -- movement between block locations // 
		case blockingState.Moving:
			_animator.SetBool ("isWalking", true);
			//Get Target rotation to lerp towards
			targetRot = currentBlockPos - transform.position;
			newRot = Quaternion.Lerp (transform.rotation, Quaternion.LookRotation (targetRot), rotationSpeed * Time.deltaTime);
			transform.rotation = newRot;
			transform.eulerAngles = new Vector3 (0, transform.eulerAngles.y, 0);

			//Begin movement towards blocking location
			transform.position = Vector3.MoveTowards (transform.position, currentBlockPos, movementSpeed * Time.deltaTime);
			if (transform.position == currentBlockPos) {
				blockingProgress = 0; 
				blockState = blockingState.Blocking;
				_animator.SetBool ("isWalking", false);
			}
			break;

		/*---------------------------------------------------------------------------------------*/
		// Blocking state -- when stationary, rotate to face player and move on after a set time //
		case blockingState.Blocking:
			
			// Calculate the vector from player to position
			targetRot = player.position - transform.position;

			//Calculate the Quaternion for the rotiation
			newRot = Quaternion.Lerp (transform.rotation, Quaternion.LookRotation (targetRot), rotationSpeed * Time.deltaTime);

			//Apply rotation and 0 out axis apart from y-rotation
			transform.rotation = newRot;
			transform.eulerAngles = new Vector3 (0, transform.eulerAngles.y, 0);

			//Increment blocking timer
			if (blockingProgress < blockDuration) {
				blockingProgress += Time.deltaTime;
			} else {
				acquirePosition ();
				blockState = blockingState.Moving;
			}

			//If player passes by them, immediately attempt to block a new position
			if (transform.position.z < player.transform.position.z - 0.2f) {
				acquirePosition ();
				blockState = blockingState.Moving;
			}

			break;

		/*-----------------------------------------------------------------------*/
		// Attacking state -- When the Player gets too close, attempts to attack //
		case blockingState.Attacking:
			_animator.SetTrigger ("proximityAttack");
			_animator.SetBool ("isWalking", false);
			break;

		/*-----------------------------------------------------------------------------*/
		// Dead State -- When the player chrashes into it, destroys object after delay //
		case blockingState.Dead:
			destroyThis(5); //Cleans up object after 5 seconds
			break;
		}
	}

	/*------------------------------------*/
	// Searches for new blocking position //
	public void acquirePosition(){
		Vector2 minBlockBoundary = new Vector2 (Mathf.Clamp (player.position.x - 1.3f, -2.7f, 2.7f), player.position.z + 8);
		Vector2 maxBlockBoundary = new Vector2 (Mathf.Clamp (player.position.x + 1.3f, -2.7f, 2.7f), player.position.z + 10);
		currentBlockPos = EnemyManager.Instance.getBlockingPosition (currentBlockPos, minBlockBoundary, maxBlockBoundary, 0.4f);
	}


	/*-------------------------------------------------------*/
	// Coroutine to Pursue the Player and try to attack them //   TO BE DONE
	public void PursuePlayer() {

		// Hostile enemy begins inactive, pursuing player once they get close enough //---------------------------
		if (!activeHostility) {
			Debug.Log("Distance from player = " + Vector3.Distance(transform.position,player.position));
			if (Vector3.Distance (transform.position, player.position) < hostileRange) {
				activeHostility = true;
			}

		//Active, begin moving towards player within attack range //---------------------------------------------
		} else {
			//Get Target rotation to lerp towards
			Vector3 targetRot = player.position - transform.position;
			Vector3 newRot = Vector3.RotateTowards (transform.forward, targetRot, rotationSpeed * Time.deltaTime, 0.0f);
			transform.rotation = Quaternion.LookRotation (newRot);

			//Begin movement towards player
			transform.position = Vector3.MoveTowards(transform.position, player.position, movementSpeed * Time.deltaTime);
		}
	
	}

	/*---------------------------------------------------------------------------------*/
	// Detect Collisions with our personal collider -- for player stampede recognition //
	void OnTriggerEnter(Collider col){

		//If Player enters hitbox trigger && conqueror-type skeleton -- transition to dieing
		if(col.tag == "Player" && blockState != blockingState.Dead && tag == "Skeleton") {

			Vector3 targetDir = col.gameObject.transform.position - transform.position;
			float angle = Vector3.Angle (targetDir, transform.forward);
			if (angle < 90) { //facing towards player
				_animator.SetTrigger ("dieBackwards");
			} else {
				_animator.SetTrigger ("dieForwards");
			}
				
			//Over the top collision effect
			capCollider.isTrigger = false; //Disables trigger collider enabling detection of ground
			rBody.isKinematic = false; //Disables kinematic control allowing physic based effects
			rBody.useGravity = true; //Enables gravity to manage bringing npc down to ground
           
           // rBody.AddExplosionForce (20.0f, playerRenderer.bounds.center, 3f,0.2f,ForceMode.Impulse); //Applies explosive force
            rBody.AddExplosionForce(20.0f, new Vector3(transform.position.x, playerRenderer.bounds.center.y, playerRenderer.bounds.center.z), 6f, 0.25f, ForceMode.Impulse); //Applies explosive force

            //Set to dead state
            blockState = blockingState.Dead;

			//Deactivates attack hitbox incase we were mid attack animation
			attackHitBox.gameObject.SetActive (false);
		}

	}

	/*------------------------------------------*/
	// Detect Collision with our hitbox trigger //
	public void playerProximityEnter(){
		//If Player enters hitbox trigger -- transition to attack
		if(behaviour == hostileType.Block && blockState != blockingState.Dead) {
			blockState = blockingState.Attacking;
		}
	}

	/*-----------------------*/
	// Detect Collision exit //
	public void playerProximityExit(){
		if(behaviour == hostileType.Block && blockState != blockingState.Dead) {
			_animator.ResetTrigger ("proximityAttack");
			acquirePosition ();
			blockState = blockingState.Moving;
		}
	}


//	/*------------------------------------------*/
//	// Detect Collision with our hitbox trigger //
//	void OnTriggerEnter(Collider other){
//		//If Player enters hitbox trigger -- transition to attack
//		if(other.tag == "Player" &&  behaviour == hostileType.Block && blockState != blockingState.Dead) {
//			Debug.Log ("beginning my attack");
//			blockState = blockingState.Attacking;
//		}
//	}
//
//	/*-----------------------*/
//	// Detect Collision exit //
//	void OnTriggerExit(Collider other){
//		if(other.tag == "Player" &&  behaviour == hostileType.Block && blockState != blockingState.Dead) {
//			Debug.Log ("don't run from me");
//			_animator.ResetTrigger ("proximityAttack");
//			acquirePosition ();
//			blockState = blockingState.Moving;
//		}
//	}

	/*--------------------------------------------------------*/
	//Activates final attack hitbox that reduces player score //
	public void startAttack (){
		attackHitBox.gameObject.SetActive (true);
	}

	/*------------------------------------------------------*/
	//Deactivates attack hitbox after attack animation ends //
	public void endAttack (){
		attackHitBox.gameObject.SetActive (false);
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
