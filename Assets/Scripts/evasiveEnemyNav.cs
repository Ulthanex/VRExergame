using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class evasiveEnemyNav : MonoBehaviour {

	/*Game Objects*/
	public Transform player; //The players transform from which to flee from
	private NavMeshAgent agent; //NavMesh AI for the character

	/*Active Map Width*/
	private float mapWidth = 2.5f; //The width of the pathway that the gameobject can walk into +- the value

	/*Current Walking Direction*/
	private float currentAngle; //The Angle direction to head in to avoid the player
	public float boundaryClamp; //The angle of direction the character is clamped to, adjusting as one gets closer to boundary
	public float evasionRange; //The random range that is applied to the current walking direction for that interval
	public int pathDuration = 5;//The length of time one will maintain the path
	private bool meshWalk = false;

	/*----------------*/
	// Initialization //
	void Start () {
		StartCoroutine(evadePlayer());
	//	agent = GetComponent<NavMeshAgent> (); 
	}

	/*----------*/
	// On Awake //
	void Awake () 
	{
		agent = GetComponent<NavMeshAgent> ();    
	}

	/*-----------*/
	// On Update //
//	void Update(){
//
//		//Check that we have nav agent setup first
//		if (agent) {
//		
//			/* Traversing off of Mesh Link onto next prefab Tile */
//			if (agent.isOnOffMeshLink) {
//				OffMeshLinkData data = agent.currentOffMeshLinkData;
//			
//				//Calculate end point of link
//				Vector3 endPos = data.endPos + Vector3.up * agent.baseOffset;
//
//				//Move the agent to the end point
//				agent.transform.position = Vector3.MoveTowards(agent.transform.position, endPos, agent.speed * Time.deltaTime);
//
//				//when the agent reach the end point you should tell it, and the agent will "exit" the link and work normally after that
//				if(agent.transform.position == endPos)
//				{
//					agent.CompleteOffMeshLink();
//				}
//			}else{
//
//				//If Infront of the player -- try to evade based on angle from player to us
//				if (transform.position.z >= player.position.z) {
//					Vector3 playerDir = transform.position - player.position;
//					currentAngle = Vector3.Angle (playerDir, Vector3.forward);
//				} else { //If behind the player, run in a straightfoward direction
//					currentAngle = 0;
//				}
//
//				//Inverse the angle of direction dependent on relative x-axis position to player 
//				// (or leave un-inversed to have them criss-cross the player)
//
//				/* Normal Evasion */
//				//if (player.position.x > transform.position.x) {
//				//	currentAngle = currentAngle*-1; //evasive
//				//}
//
//				/* Criss-cross evasion */
//				if (player.position.x < transform.position.x) {
//					currentAngle = currentAngle * -1; //criss cross
//				}
//
//				Debug.Log ("current Y Rot: " + currentAngle);
//
//				//Clamps running angle into a forward motion (clamped within -30 +30 degree range)
//				float clampedAngle = Mathf.Clamp (currentAngle, -30, 30); //get clamped foward angle from player
//				//Debug.Log ("clamped Y Rot: " + clampedAngle);
//
//				//Calculate character position on map as a percentage to identify the clamped rotation of the character
//				//to prevent them over steering into a wall / also calculate a random offset to potential correct guttering
//				float mapPositionPerc = Mathf.InverseLerp (-mapWidth, mapWidth, this.transform.position.x);
//				//	Debug.Log ("Map Position = " + mapPositionPerc);
//				float negativeAngleBoundary = (boundaryClamp * mapPositionPerc) * -1;
//				float positiveAngleBoundary = boundaryClamp - Mathf.Abs (negativeAngleBoundary);
//				//	Debug.Log ("Negative angle Clamp = " + negativeAngleBoundary);
//				//	Debug.Log ("Positive Angle Clamp = " + positiveAngleBoundary);
//				float lowerBoundEvasion = evasionRange * mapPositionPerc;
//				float upperBoundEvasion = evasionRange - lowerBoundEvasion;
//				//Debug.Log ("Map Position = " + mapPositionPerc);
//				//Debug.Log("Lower Evasion Bound = " + (-lowerBoundEvasion));
//				//Debug.Log("Upper Evasion Bound = " + upperBoundEvasion);
//
//				//Begin moving to point
//				//float rOffset = 0;
//				float rOffset = Random.Range (-lowerBoundEvasion, upperBoundEvasion);
//				Debug.Log ("Offset = " + rOffset);
//				//Vector3 rotation = new Vector3 (0,  currentAngle + Random.Range(-lowerBoundEvasion, upperBoundEvasion) , 0);
//				this.transform.eulerAngles = new Vector3 (0, Mathf.Clamp (clampedAngle + rOffset, negativeAngleBoundary, positiveAngleBoundary), 0);
//				//Vector3 moveposition = transform.position + (rotation) * 2;
//				Vector3 moveposition = transform.position + transform.forward * 2;
//				Vector3 finalPosition = new Vector3 (Mathf.Clamp (moveposition.x, -mapWidth, mapWidth), moveposition.y, moveposition.z);
//				//Debug.Log ("Clamped final x = " + finalPosition.x);
//				agent.destination = finalPosition;
//
//			}
//		}
//	}


	IEnumerator moveAcrossMeshLink(){
		OffMeshLinkData data = agent.currentOffMeshLinkData;
		agent.updateRotation = false;
		//Calculate end point of link
		Vector3 endPos = data.endPos + Vector3.up * agent.baseOffset;

		do {
			//Move the agent to the end point
			agent.transform.position = Vector3.MoveTowards (agent.transform.position, endPos, agent.speed * Time.deltaTime);

			//when the agent reach the end point you should tell it, and the agent will "exit" the link and work normally after that

			if (agent.transform.position == endPos) {
				Debug.Log ("At End Point");
				transform.position = endPos;
				agent.updateRotation = true;
				agent.CompleteOffMeshLink ();
				agent.ResetPath ();
				meshWalk = true;
			}
			yield return null;
		} while (!meshWalk);
	}

	/*-----------------------------------------------------------------------------*/
	// Takes position of player and position on map to derive direction to flee in //
	IEnumerator evadePlayer (){
		while (true) {
		

			/* Traversing off of Mesh Link onto next prefab Tile */
			if (agent.isOnOffMeshLink && !agent.pathPending) {
				meshWalk = false;
				yield return StartCoroutine (moveAcrossMeshLink ());

				/* No Pending path, not on an off Mesh Link */
			} else if (!agent.isOnOffMeshLink && !agent.pathPending && !agent.hasPath) {

				//If Infront of the player -- try to evade based on angle from player to us
				if (transform.position.z >= player.position.z) {
					Vector3 playerDir = transform.position - player.position;
					currentAngle = Vector3.Angle (playerDir, Vector3.forward);
				} else { //If behind the player, run in a straightfoward direction
					currentAngle = 0;
				}
				
				//Inverse the angle of direction dependent on relative x-axis position to player 
				// (or leave un-inversed to have them criss-cross the player)

				/* Normal Evasion */
				//if (player.position.x > transform.position.x) {
				//	currentAngle = currentAngle*-1; //evasive
				//}

				/* Criss-cross evasion */
				if (player.position.x < transform.position.x) {
					currentAngle = currentAngle * -1; //criss cross
				}

				//Debug.Log ("current Y Rot: " + currentAngle);

				//Clamps running angle into a forward motion (clamped within -30 +30 degree range)
				float clampedAngle = Mathf.Clamp (currentAngle, -30, 30); //get clamped foward angle from player
				//Debug.Log ("clamped Y Rot: " + clampedAngle);

				//Calculate character position on map as a percentage to identify the clamped rotation of the character
				//to prevent them over steering into a wall / also calculate a random offset to potential correct guttering
				float mapPositionPerc = Mathf.InverseLerp (-mapWidth, mapWidth, this.transform.position.x);
				//	Debug.Log ("Map Position = " + mapPositionPerc);
				float negativeAngleBoundary = (boundaryClamp * mapPositionPerc) * -1;
				float positiveAngleBoundary = boundaryClamp - Mathf.Abs (negativeAngleBoundary);
				//	Debug.Log ("Negative angle Clamp = " + negativeAngleBoundary);
				//	Debug.Log ("Positive Angle Clamp = " + positiveAngleBoundary);
				float lowerBoundEvasion = evasionRange * mapPositionPerc;
				float upperBoundEvasion = evasionRange - lowerBoundEvasion;
				//Debug.Log ("Map Position = " + mapPositionPerc);
				//Debug.Log("Lower Evasion Bound = " + (-lowerBoundEvasion));
				//Debug.Log("Upper Evasion Bound = " + upperBoundEvasion);

				//Begin moving to point
				float rOffset = Random.Range (-lowerBoundEvasion, upperBoundEvasion);
				//Debug.Log ("Offset = " + rOffset);
				Vector3 startRotation = this.transform.eulerAngles;
				this.transform.eulerAngles = new Vector3 (0, Mathf.Clamp (clampedAngle + rOffset, negativeAngleBoundary, positiveAngleBoundary), 0);
				Vector3 moveposition = transform.position + transform.forward * 2;
				this.transform.eulerAngles = startRotation;

				//get closest hit point
				NavMeshHit hit;    // stores the output in a variable called hit
				if (NavMesh.SamplePosition (moveposition, out hit, 0.5f, NavMesh.AllAreas)) {
					//Debug.Log ("Hit position = X: " + hit.position.x + "  Y:" + hit.position.y + "  Z:" + hit.position.z);
					Vector3 finalPosition = new Vector3 (Mathf.Clamp (hit.position.x, -mapWidth, mapWidth), hit.position.y, hit.position.z);
					//Debug.Log ("Clamped final x = " + finalPosition.x);
					agent.destination = finalPosition;
				} else { //couldn't find a target (could mean we are at the end of our path
					Debug.Log("couldnt find target");
					agent.ResetPath();
					this.gameObject.SetActive (false); //deactivates game element for time being  TO DO
				}



				//////////////////////////////////////////////////////////////////////////////////////////////////
				//player.transform.eulerAngles = new Vector3 (0, (clampedAngle*-1), 0);

				//float yRot = this.transform.eulerAngles.y;
				//float yRot = this.transform.eulerAngles.y;
				//if (yRot >= 180) {yRot -= 360;}
				//Debug.Log ("current Y Rot: " + yRot);

				//if (yRot <= -90 || yRot >= 90) {
				//	Debug.Log ("backwards Y Rot: " + (yRot - 180) );
				//} else {
				//	Debug.Log ("forward Y Rot: " + (-yRot));
				//}


				/*if (yRot >= 90 && yRot <= 270) {
				Debug.Log ("backwards Y Rot: " + (yRot - 180) );
				testSkelly.transform.eulerAngles = new Vector3 (0, (yRot - 180), 0);
			} else {
				Debug.Log ("forward Y Rot: " + (-yRot));
				testSkelly.transform.eulerAngles = new Vector3 (0, (360 + -yRot), 0);
			}
			//Debug.Log ("Forward Facing Opposite Y Rot: " + -this.transform.eulerAngles.y);*/
//			}
				yield return new WaitForSeconds (0.2f);
				//yield return null;
			} else {
				Debug.Log ("Got a path");
				yield return new WaitForSeconds (0.2f);
			}
		}
	}




}
