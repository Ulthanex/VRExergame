using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Enum for current exergame personality condition
[System.Serializable]
public enum personalityType{ 
	Conqueror,
	Survivor,
	Mixed
}

//Enum for grouped game components for personality conditions
[System.Serializable]
public struct gameComponent{
	public GameObject gObject;
	public float probability;
}


//Singleton Game Manager//
public class GameManager : MonoBehaviour
{

    [Header("Game Objects:")]
    public GameObject sectionHolder; //Holds all tile sections
	public GameObject chaser;
	public GameScreenManager gui;


    [Header("Ground Params")]
    public int activeRoadTiles = 10;
	public GameObject conquerorPrefab;
	public GameObject survivorPrefab;
	public GameObject mixedPrefab;


    [Header("Game Elements:")]
    //public GameObject truck;
	public List<gameComponent> conquerorGameComponents = new List<gameComponent> (); //list of game components for conqueror & probabilities
	public List<gameComponent> survivorGameComponents = new List<gameComponent> (); //list of game components for survivor & probabilities
	public List<gameComponent> mixedGameComponents = new List<gameComponent>(); //list of game components for mixed & probabilities
	public int conquerorMaxCount = 5; //maximum count of game components per tile
    public int survivorMaxCount = 5; //maximum count of game components per tile
	public int mixedMaxCount = 5; //maximum count of game components per tile
    public List<GameObject> enemyList = new List<GameObject>(); //list of all enemies

	[Header("Personality Type")]
	public personalityType gameCondition;

    //private vars
	private static GameManager _instance;
	private GameObject section = null;
    private bool spawn; //Whether enemies are being created
	private int mixedSegmentCount = 0; //If Mixed personality type condition -- Determines which elements to activate

    private List<GameObject> sections = new List<GameObject>(); //Holds all the currently created tiles
	private List<LevelTile> activeSections = new List<LevelTile>(); //Holds list of all sections active and enabled for spawning

	//Scoring --------------------
	private int playerScore = 0; //Current player score
	private float multiplierScore = 1; //current player multiplier
	private float maxMultiplier = 1; //maximum multiplier reached
	private bool multiplierActive = false; //whether the multiplier is used
	private bool multiplierTimerActive = false; //whether multiplier is currently counting down
	private float multiplierDuration = 4f; //length of multiplier
	private float multiplierTimer; 
	private int skeletonKills = 0; //number of skeletons killed
	private int ghostKills = 0; //number of ghosts killed
	private int bossHits = 0; //number of boss hits
	private int skeletonCollisions = 0; //number of crashes into skeletons
	private int ghostCollisions = 0; // number of crashes into ghosts
	private int chaserCollisions = 0; //number of hits by chaser


    //private float spawnRange = 500f;
    
    

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
	 
	/*------------------------*/
	//Singleton Getter Method //
	public static GameManager Instance { get { return _instance; } }


	/*------------------------*/
	// Initialises Game Build //
    void Start()
    {
		//Checks for personality type assigned within main menu
		if (DataSaver.saver != null) {
			gameCondition = DataSaver.saver.hexType;
		}

		//sets initial position by taking  (no. tiles/2 e.g. 12/2 = 6) and multiplying players forward position by 16f * tile count
		PlayerController.Instance.transform.position += Vector3.forward * 16.0f * conquerorPrefab.transform.localScale.z* activeRoadTiles / 2.0f;

		//Enables spawning of enemies/objects
        spawn = true;

		//Activates specific extra features dependent on game condition
		switch (gameCondition) {

		//Enables Chaser if in survivor gameplay
		case personalityType.Survivor:
			chaser.SetActive (true);
			break;

		//Or enables score multiplier for conqueror
		case personalityType.Conqueror:
			multiplierActive = true;
			break;

        //Score Multiplier/chaser alternates   -- One condition, both are left on
		case personalityType.Mixed:
			//chaser.SetActive (true);
			//multiplierActive = true;
			incrementMixedSegment();
			break;

		}
			
		//Sets up our initial build of the map
        initialBuild();
    }
		

	/*------------------------------------*/
	//Creates the initial build of the map//
    public void initialBuild()
    {
		
		//populates a number of tile prefabs equal to our activeRoadTiles value
        for (int x = 0; x < activeRoadTiles ; x++)
        {
			//instantiates a new tile prefab at the position of the game manager
			if (gameCondition == personalityType.Conqueror) {
				section = (GameObject)Instantiate (conquerorPrefab, transform.position, transform.rotation);
			} else if (gameCondition == personalityType.Survivor) {
				section = (GameObject)Instantiate (survivorPrefab, transform.position, transform.rotation);
			} else {
				section = (GameObject)Instantiate (mixedPrefab, transform.position, transform.rotation);
			}
           
			//If initial section, disable collider trigger
			if(x <= activeRoadTiles /2){section.GetComponent<LevelTile>().disableTrigger();}

			//Sets the parent of the section to equal the section Holder
			section.transform.parent = sectionHolder.transform;

			//Adds the section to our internal list of sections
            sections.Add(section);
            
			//Adds the second half of road tiles created to the spawnPositions list, enabling
			//Prefabs to be created on the map
			if (x > activeRoadTiles / 2 )
            {
				activeSections.Add (section.GetComponent<LevelTile> ());
            }

			//Sets the tile position foward by 16f, of which the next tile will be instantiated at
            transform.position +=transform.forward * 16.0f * conquerorPrefab.transform.localScale.z;
        }
    }

	/*------------------------------------------------------------------------------------------------*/
	//Spawns a new tile when the body trigger of the player collides (called by the player controller)//
    public void spawnTile()
    {
        //Removes the first tile in the buffer and destroys the gameobject- FIFO system.
        Destroy(sections[0]);
        sections.RemoveAt(0);

		//Removes first active tile, moving spawn positions further along
		activeSections.RemoveAt (0);

        //Instantiates our new tile prefab
		if (gameCondition == personalityType.Conqueror) {
			section = (GameObject)Instantiate (conquerorPrefab, transform.position, transform.rotation);
		} else if (gameCondition == personalityType.Survivor) {
			section = (GameObject)Instantiate (survivorPrefab, transform.position, transform.rotation);
		} else {
			section = (GameObject)Instantiate (mixedPrefab, transform.position, transform.rotation);
		}

		//sets the tiles parent to that of the section holder
        section.transform.parent = sectionHolder.transform;

        //Adjust tile to current HIIT condition -- Starts off as high so only low transition required
        if(HIITController.Instance.currentState == ExerciseProtocolState.highIntensity)
        {
            section.GetComponent<LevelTile>().highIntensityTransition();
        }
     

		//adds the new tile to the list of sections & generates enemies if spawn is true
        sections.Add(section);
		activeSections.Add (section.GetComponent<LevelTile> ());
		if (spawn) {
			spawnObjectsTile (section.GetComponent<LevelTile> ());
		}

		//Moves game manager position along for next tile
        transform.position = transform.position + (transform.forward * 16.0f * conquerorPrefab.transform.localScale.z);
    }

	/*----------------------------------------------*/
	//Transitions Tiles dependent on exercise state //
	public void transitionTile(){
        ExerciseProtocolState state = HIITController.Instance.currentState;
        for (int i = 0; i < sections.Count; i++) {
			if (state == ExerciseProtocolState.lowIntensity || state == ExerciseProtocolState.trainingLow) {
				sections [i].GetComponent<LevelTile> ().lowIntensityTransition (); //Transition tiles graphically to low
				if (activeSections.Contains (sections [i].GetComponent<LevelTile> ())){
					spawnObjectsTile (sections [i].GetComponent<LevelTile> ());
				}
			} else if (state == ExerciseProtocolState.highIntensity || state == ExerciseProtocolState.trainingHigh) {
				sections [i].GetComponent<LevelTile> ().highIntensityTransition (); //Transition tiles graphically to high
			} else {
				spawn = true;
				if (activeSections.Contains (sections [i].GetComponent<LevelTile> ())){
					spawnObjectsTile (sections [i].GetComponent<LevelTile> ());
				}
			}
		}
	}
		
	/*--------------------------------------------*/
	//Used by the HIITController Exercise protocol// NEW ADAPTION
	public void spawnObjectsTile(LevelTile t){
		
		//Spawn certain amount of game componenets within the tile
		if (gameCondition == personalityType.Conqueror) {
			t.generateEnemies (conquerorGameComponents, conquerorMaxCount, PlayerController.Instance.gameObject);
		} else if (gameCondition == personalityType.Survivor) {
			t.generateEnemies (survivorGameComponents, survivorMaxCount, PlayerController.Instance.gameObject);
		} else {
			t.generateEnemies (mixedGameComponents, mixedMaxCount, PlayerController.Instance.gameObject);
		}
	}


	/*-------------------------------------------------------------------------*/
	//Clears all game components when transitioning from low intensity to high //
	public void clearGameComponents(){
		for(int z = enemyList.Count-1; z >= 0 ; z--){
			Destroy (enemyList [z]);
			enemyList.RemoveAt (z);
		}
	}


	/*------------------------------*/
	//Called by the HIIT Controller //
	public void setSpawning(bool condition)
    {
        spawn = condition;
    }

	/*-------------------------------------------------------------------*/
	//Changes the current parameters of the mixed condition game elements//
	public void incrementMixedSegment(){
		switch (mixedSegmentCount) { //Determines what changes to make

		case 0:  //First Segment -- multiplier active, no chaser
			multiplierActive = true;
			break;

		case 1: //Second Segment -- no multiplier, chaser active
			multiplierActive = false;
			chaser.transform.position = new Vector3 (0, 0, PlayerController.Instance.transform.position.z - 50f);
			chaser.SetActive (true);
			break;

		case 2: //Final Segment -- multipler and chaser active
			multiplierActive = true;
			break;

		}
		mixedSegmentCount++;
	}


	/*----------------------------------------------------------*/
	// Adjusts player score when colliding with hostile element //
	public void adjustPoints(int value, int condition){

		switch (condition) {
		case 0: //ghost killed
			ghostKills ++;
			break;

		case 1: //skeleton killed
			skeletonKills ++;
			break;

		case 2: //ghost collision
			ghostCollisions ++;
			break;

		case 3: //skeleton collision
			skeletonCollisions ++;
			break;

		case 4: //boss collision
			bossHits ++;
			break;

		case 5: //chaser collision
			chaserCollisions ++;
			break;

		case 6: //other
			break;
		}
			
        //Subtracting points
        if (value < 0) {
			playerScore = playerScore + value > 0 ? playerScore + value : 0; //subtract value to a limit of 0
			multiplierTimer = 0; //Immediately resets multiplier 
			gui.updateScore(playerScore,true); //Tell ui to update score and display hit animation
		//Adding points
		} else if (value > 0) {
            playerScore += Mathf.FloorToInt(value*multiplierScore); //increases score

			if(multiplierActive){ //If game mode enables multiplier
				multiplierScore += 0.1f; //increments multiplier score
				if(maxMultiplier < multiplierScore){ maxMultiplier = multiplierScore;} //Increases maximum multiplier
				multiplierTimer = multiplierDuration; //Resets multiplier duration
				if (!multiplierTimerActive) { //If a timer coroutine isn't already running
					multiplierTimerActive = true; 
					StartCoroutine (scoreMultiplier ());
				}
			}
			gui.updateScore (playerScore); //Tells ui to update score
		}
	}

	/*---------------------------------------*/
	//Keeps track of score multiplier uptime //
	IEnumerator scoreMultiplier(){
        while (multiplierTimer > 0) { //Multiplier timer ticks down until deactivation
			multiplierTimer -= Time.deltaTime;//decrements timer
			gui.updateMultiplier(multiplierScore,multiplierTimer,true);//tells ui to update multiplier
			yield return null;
		}
		multiplierScore = 1;
		multiplierTimerActive = false;
		gui.updateMultiplier(multiplierScore,multiplierTimer,false);//tells ui to update multiplier
	}






	//Restarts The exercise sprint  -- Currently unused
   /* public void restartSprint()
    {
        //Destroy everything then initial build.
        while (vehicles[0] == null)
        {
            vehicles.RemoveAt(0);
        }
        while(vehicles.Count != 0)
        {
            removeVehicle();
        }
        while(sections.Count != 0)
        {
            Destroy(sections[0]);
            sections.RemoveAt(0);
        }

        transform.position = Vector3.zero;
        initialBuild();
    }
    */

	/*--------------------------------------------*/
	//Ends the session one we reach the end state //
	public void endSession(){
		gui.GoToResults(playerScore, maxMultiplier, ghostKills, skeletonKills, bossHits, ghostCollisions, skeletonCollisions, chaserCollisions);
	}


//    public void endSession(bool beatGhost)
//    {
//        float[] playerSpeedRecording;
//        float[] playerLeanRecording;
//
//        //Convert track recording to array and save if it was LONGER.
//        if (sectionRecording.Count > prevSectionRecording.Length)
//        {
//            prevSectionRecording = sectionRecording.ToArray();
//        }
//        if(truckRecording.Count > prevTruckRecording.Length)
//        {
//            prevTruckRecording = truckRecording.ToArray();
//        }
//        Time.timeScale = 0;
//
//        //Get ghost information only if score is higher otherwise its the same
//        if (beatGhost)
//        {
//            playerSpeedRecording = player.GetComponent<PlayerController>().getPlayerSpeedRecording();
//            playerLeanRecording = player.GetComponent<PlayerController>().getPlayerLeanRecording();
//        }
//        else
//        {
//            playerSpeedRecording = DataSaver.saver.ghostSpeedRecording;
//            playerLeanRecording = DataSaver.saver.ghostLeanRecording;
//        }
//        
//        DataSaver.saver.Save(prevSectionRecording, prevTruckRecording, playerSpeedRecording, playerLeanRecording);
//        resultsButton.SetActive(true);
//    }
}