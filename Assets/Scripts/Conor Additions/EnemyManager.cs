using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Singleton Enemy Manager//
public class EnemyManager : MonoBehaviour {
	private static EnemyManager _instance;
	private static List<Vector3> blockingPos = new List<Vector3> ();
	public static EnemyManager Instance { get { return _instance; } }


	/*----------------------------*/
	// Assigns Singleton instance //
	private void Awake()
	{
		if (_instance != null && _instance != this) {
			Destroy (this.gameObject);
		}else{
			_instance = this;
		}
	}


	/*------------------------------------------------------------------*/
	// Attempts to assign a new random position for the enemy to defend //
	public Vector3 getBlockingPosition(Vector3 oldPos, Vector2 minBoundary, Vector2 maxBoundary, float uniqueRadius){

		//Frees up positions for other enemies to move towards 
		if (blockingPos.Contains (oldPos)) {
			blockingPos.Remove (oldPos);
		}

		//creates a random point to block within the boundary vectors
		bool isUnique; //flag that identifies if point does not come into conflict with other enemies
		int attempts = 0; //count of how many attempts we've tried. accept Position if exceeds 5 tries
		Vector3 newPos; //new position for enemy to block
		do {
			isUnique = true;
			attempts++;
			newPos = new Vector3(Random.Range(minBoundary.x,maxBoundary.x),0,Random.Range(minBoundary.y,maxBoundary.y));
			foreach (var pos in blockingPos){
				if(Vector3.Distance(pos,newPos) < uniqueRadius){
					isUnique = false;
					break;
				}
			}
		} while(!isUnique && attempts < 10);

		//Returns position
		blockingPos.Add(newPos);
		return newPos;
	}

}
