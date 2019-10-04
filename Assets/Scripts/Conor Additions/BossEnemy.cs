using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossEnemy : MonoBehaviour {

	private enum enemyState{
		Moving,
		Casting,
		Recoiling,
		Teleporting
	}

	[Header("Player Target")]
	public GameObject player; //The players transform from which to flee from
	public motionEnhancer motionEnhancer;
	private SkinnedMeshRenderer playerRenderer = null;

	[Header("Movement Parameters")]
	private float speed; //The speed at which he moves
	public float recoilMultiplier;
	public float maxRange; //The furthest position moved to
	public ParticleSystem teleportEffect;


	[Header("Attack Parameters")]
	public Transform projectileSpawnPoint; //The position at which the magic projectile is created from
	public float attackTimer; //The length of time between attacks
	private float attackProgress; //Incrementor for attack timing


	[Header("Animation Fields")]
	public SkinnedMeshRenderer self;
	public GameObject leftFireParticle;
	public GameObject rightFireParticle;
	public GameObject prefabProjectile;
	private GameObject projectile = null;
	private Vector3 projectilePoint;

	private Animator _animator; //Animator controller
	private enemyState state = enemyState.Moving;



	/*-----------------------------*/
	// Use this for initialization //
	void Awake () {
		_animator = gameObject.GetComponent<Animator> (); //Gets animator for the character
		playerRenderer = player.GetComponentInChildren<SkinnedMeshRenderer> (); //Attempts to grab skinned mesh renderer of player
	}

	/*----------------------------------------------------------------------------------------------------*/
	// On Enable -- System will enable on the start of a new High-itensity interval, resetting parameters //
	void OnEnable(){
		//Resets teleport effect, triggering it on appearance
		teleportEffect.Simulate (0.0f, true, true);
		teleportEffect.Play ();
		//resets other parameters
		state = enemyState.Moving;
		attackProgress = 0;
		self.enabled = true;
		//Disable casting particle effects
		leftFireParticle.SetActive (false);
		rightFireParticle.SetActive (false);
		//Start default coroutine
		StartCoroutine(Movement()); //starts movement coroutine
	}


	/*------------------------------------------------------------------------*/
	// Movement State -- Evades player whilst matching horizontal positioning //
	IEnumerator Movement( ){
		//Whilst we are still moving
		while (state == enemyState.Moving){
			//Get vector position of player + maxRange down the path
			Vector3 movementPos = new Vector3 (Mathf.Clamp (player.transform.position.x, -2.5f, 2.5f), 0, Mathf.Clamp (player.transform.position.z + maxRange, transform.position.z, transform.position.z + maxRange));
			if (movementPos.z < transform.position.z) {
				movementPos.z = transform.position.z;
			}

			//Get direction of movement for blend tree --only if moving
			if (movementPos != transform.position) {
				Vector3 movementDir = movementPos - transform.position;
				float currentAngle = Vector3.Angle (movementDir, Vector3.forward);
				if (movementPos.x <= transform.position.x) {
					currentAngle *= -1;
				}
				_animator.SetFloat ("Direction", currentAngle);
			}

			//obtain player base running speed and set Move character
			speed = motionEnhancer.calculateBaseRunningSpeed(0.25f);	
			transform.position = Vector3.MoveTowards (transform.position, movementPos, speed * Time.deltaTime);

			//If we are at end position --Swap to Idle
			if (transform.position == movementPos) {
				_animator.SetBool ("isMoving", false);
			} else {
				_animator.SetBool ("isMoving", true);
			}

			//Check for transition between movement and attacking
			attackProgress += Time.deltaTime;
			if (attackProgress >= attackTimer && state == enemyState.Moving) {
				state = enemyState.Casting;
			} 
			yield return null;
		} 
		attackProgress = 0; //Reset attack timer

		//Begin casting
		if (state == enemyState.Casting) {
			beginCasting ();
		}
	}

	/*-----------------------------------------------------*/
	// Casting state - turns on animations for the casting // Animation Event
	private void beginCasting(){
		
		//Trigger animator changes
		_animator.SetBool ("isMoving", false);
		_animator.SetTrigger ("startCasting");

		//Enable casting particle effects
		leftFireParticle.SetActive (true);
		rightFireParticle.SetActive (true);
	}

	/*--------------------------------------------------------------------------*/
	// Casting state - turns off animations for the casting, creates projectile // Animation Event
	private void finishCasting(){

		//Create skull projectile
		projectile = (GameObject)Instantiate(prefabProjectile,projectileSpawnPoint.position,Quaternion.identity);
		if (playerRenderer != null) {
			projectilePoint = playerRenderer.bounds.center;
		} else {
			projectilePoint = player.transform.position;
		}
		projectile.GetComponent<SkullProjectile> ().launchProjectile(projectilePoint);

		//Trigger state change back to default moving state
		state = enemyState.Moving;

		//Disable casting particle effects
		leftFireParticle.SetActive (false);
		rightFireParticle.SetActive (false);

		StartCoroutine (Movement());
	}


	/*----------------------------------------------------------*/
	// Recoils backwards at a multiplied speed away from player //
	IEnumerator RecoilMovement(){

		//Disables particle effects -- if caught mid cast
		leftFireParticle.SetActive (false);
		rightFireParticle.SetActive (false);

		while (state == enemyState.Recoiling) {
			speed = motionEnhancer.calculateBaseRunningSpeed(0.35f);	
			transform.position += Vector3.forward * (speed * recoilMultiplier) * Time.deltaTime;
			yield return null;
		}
	}

	/*------------------------------------------------*/
	// Triggers at the end of our recoiling animation // Animation Event
	public void endRecoiling(){
		state = enemyState.Moving;
		StartCoroutine(Movement());
	}

	/*-----------------------------------------------*/
	// Triggers at the end of our teleport animation // Animation Event
	public void endTeleporting(){ StartCoroutine (DisableBoss()); }

	/*----------------------------------------------*/
	//Disables boss until next High-Intensity Phase //
	IEnumerator DisableBoss(){
		self.enabled = false; //Hide skinned renderer
		//Restarts our teleport effect
		teleportEffect.Simulate (0.0f, true, true);
		teleportEffect.Play ();
		yield return new WaitForSeconds(2f);
		this.gameObject.SetActive (false);
	}

	/*------------------------------------------------*/
	// Triggers at the end of a high-intensity period //
	public void HighIntensityEnd(){ 
		state = enemyState.Teleporting;
		_animator.SetTrigger ("isTeleporting"); //starts teleport animation
	}  


	/*---------------------------------------------------------------------------------*/
	// Detect Collisions with our personal collider -- for player stampede recognition //
	void OnTriggerEnter(Collider col){

		//If Player enters hitbox trigger -- transition to recoil
		if(col.tag == "Player" && (state != enemyState.Teleporting || state != enemyState.Recoiling  )) {

			//set state to recoiling and start animation transition
			state = enemyState.Recoiling;
			_animator.SetTrigger ("isDamaged");

			//Start recoil coroutine that ends with character teleporting away
			StartCoroutine(RecoilMovement());
		}
	}




}
