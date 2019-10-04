using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelTile : MonoBehaviour {

	[Header("Personality Type Condition")]
	public personalityType gameCondition;

	[Header("Light Sources")]
	public List<GameObject> lights = new List<GameObject>();
	public List<ParticleSystem> fireEffects = new List<ParticleSystem>();

	[Header("Hit box")]
	public BoxCollider spawnTrigger;


	/*--------------------------------------*/
	// Transition from High Intensity to low//
	public void lowIntensityTransition(){

		//Disable Secondary lighting
		foreach(GameObject l in lights){
			l.SetActive(true);
		}

		foreach(ParticleSystem ps in fireEffects){
			ParticleSystem.EmissionModule temp = ps.emission;
			temp.enabled = true;
		}
			
	}


	/*--------------------------------------*/
	// Transition from low Intensity to High//
	public void highIntensityTransition(){

		//Disable Secondary lighting
		foreach(GameObject l in lights){
			l.SetActive(false);
		}

		foreach(ParticleSystem ps in fireEffects){
			ParticleSystem.EmissionModule temp = ps.emission;
			temp.enabled = false;
		}
	}

	/*----------------*/
	//Generate enemies//
	public void generateEnemies(List<gameComponent> enemies, int maxCount, GameObject player){
		List<Vector3> enemyPos = new List<Vector3> ();
		Vector2 minBoundary = new Vector2 (-2.25f, this.transform.position.z - 8f);
		Vector2 maxBoundary = new Vector2 (2.25f, this.transform.position.z + 8f);
		float uniqueRadius = 0.5f;
		int currentCount = 0;

		//creates a random point to block within the boundary vectors
		bool isUnique; //flag that identifies if point does not come into conflict with other enemies
		int attempts = 0; //count of how many attempts we've tried. accept Position if exceeds 5 tries
		Vector3 newPos; //new position for enemy to block

		//Repeat until we have hit max count of allocated enemies within the tile -----------------
		do {

			//Repeat until we find a point that isn't allocated by another enemy ==================
			do {
				isUnique = true;
				attempts++;
				newPos = new Vector3(Random.Range(minBoundary.x,maxBoundary.x),0,Random.Range(minBoundary.y,maxBoundary.y));
				foreach (Vector3 pos in enemyPos){
					if(Vector3.Distance(pos,newPos) < uniqueRadius){
						isUnique = false;
						break;
					}
				}
			} while(!isUnique && attempts < 10);
			enemyPos.Add(newPos); //add to list of assigned positions
			attempts = 0; //resets attempts
			//found unique position -- look which game component to assign =======================

			//Cumulative weight allocation -- Randomly pick enemy to spawn based on weighted randomness
			int index = -1; //default value
			float weightedSum = 0f; //cumulative sum of enemy weights
			for(int i = 0; i < enemies.Count ; i++){ //Calculate the cumulative weights of all enemy components
				weightedSum += enemies[i].probability;
			}
			float rand = Random.Range(0, weightedSum); //Generate our random selection value with the weighted sum range
			for(int i = 0; i < enemies.Count ; i++){
				if(rand < enemies[i].probability){//if our random number is within interval
					index = i; //Assign index for enemy to spawn
					break;
				}else{
					rand -= enemies[i].probability; //decrement value until it fits into an enemy weighted interval
				}
			}
			//Instantiate selected enemy at position & increment count -- TO BE DONE, CREATE BETTER WAY TO ROTATE ON INSTANTIATION
			GameObject enemy = Instantiate(enemies[index].gObject, newPos, Quaternion.identity, this.transform);
			if(enemies[index].gObject.name == "SkeletonSoldier" || enemies[index].gObject.name == "SurvivorSkeletonSoldier" ||  enemies[index].gObject.name == "SurvivorGhost"){
				enemy.transform.eulerAngles = new Vector3 (0, 180, 0);
            }
            else
            {
                enemy.transform.eulerAngles = new Vector3(0, 0, 0);
            }
			GameManager.Instance.enemyList.Add(enemy);

			currentCount++;
		} while(currentCount < maxCount);
		//----------------------------------------------------------------------------------------------
	}

	/*-----------------------------------------------------------------*/
	// Disable initial trigger for spawning a new tile on initial build//
	public void disableTrigger(){
		spawnTrigger.enabled = false;
	}


	/*---------------------------------------------------------------------------------*/
	// Detect Collision exits with our personal collider -- disables trigger to prevent multiple tile despawns //
	void OnTriggerExit(Collider col){
		if(col.tag == "Player") {
			spawnTrigger.enabled = false;
		}
	}

}
