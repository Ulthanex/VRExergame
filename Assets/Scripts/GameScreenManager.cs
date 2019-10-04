using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameScreenManager : MonoBehaviour
{
    [SerializeField]
    private GameObject resultsScreenButton;

	//Enumeration of possible screens
    public enum Screens { GameScreen, ResultsScreen, NumScreens }

	[Header("GUI Screens")]
	//List of all screens & current selected screen  
	public Canvas[] mScreens;
	public Screens mCurrentScreen;

	[Header("GUI Elements")]
	//Hit panel animator for displaying collisions
	public Animator hitPanelAnimator;
	public Text scoreText;
	public Text multiplierText;
	public Text multiplierTimerText;

	[Header("Result Elements")]
	public Text scoreResText;
	public Text multiplierResText;
	public Text ghostKilledResText;
	public Text skeletonKilledResText;
	public Text BossHitResText;
	public Text ghostCollResText;
	public Text skeletonCollResText;
	public Text ChaserHitResText;

	[Header("Others")]
    public List<float> distances;
    public List<float> ghostDistances;
    public List<string> timeStamps;

    public bool ghostExists = true;

    [SerializeField]
    Text results;


	/*----------*/
	// On Awake //
    void Awake()
    {
        mScreens = new Canvas[(int)Screens.NumScreens];
       
		//Identifying screens in the scene, grouping them all within Screen.
        Canvas[] screens = GetComponentsInChildren<Canvas>();
        for (int count = 0; count < screens.Length; ++count){
            for (int slot = 0; slot < mScreens.Length; ++slot){
                if (mScreens[slot] == null && ((Screens)slot).ToString() == screens[count].name){
                    mScreens[slot] = screens[count];
                    break;
                }
            }
        }

		//Disable all the screens except the initial screen (Game Screen).
        for (int screen = 1; screen < mScreens.Length; ++screen)
        {
            mScreens[screen].enabled = false;
        }

        //Set the curent screen to the GameScreen.
        mCurrentScreen = Screens.GameScreen;
    }


	/*--------------------------------------------------*/
    //Method used when pressing play from the main menu.//
	public void GoToResults(int score,float multiplier, int gKills, int sKills, int bHits, int gColls, int sColls, int cHits)
	{
		resultsScreenButton.SetActive(false);
		TransitionTo(Screens.ResultsScreen);

//		public Text scoreResText;
//		public Text multiplierResText;
//		public Text ghostKilledResText;
//		public Text skeletonKilledResText;
//		public Text BossHitResText;
//		public Text ghostCollResText;
//		public Text skeletonCollResText;
//		public Text ChaserHitResText;

		scoreResText.text = "K " + score.ToString();
		multiplierResText.text = "S " + multiplier.ToString("0.0");
		ghostKilledResText.text = gKills.ToString();
		skeletonKilledResText.text = sKills.ToString();
		BossHitResText.text = bHits.ToString();
		ghostCollResText.text = gColls.ToString();
		skeletonCollResText.text = sColls.ToString();
		ChaserHitResText.text = cHits.ToString();

	}




//    public void GoToResults()
//    {
//        resultsScreenButton.SetActive(false);
//        TransitionTo(Screens.ResultsScreen);
//
//        string resultsText = "";
//        float totalDistance = 0.0f;
//        //float ghostTotalDistance = 0.0f;
//
//        resultsText += "ID: " + DataSaver.saver.userID + " SR: " + DataSaver.saver.sprintResistance.ToString() + "\n\n";
//
//        for (int i = 0; i < distances.Count; i++)
//        {
//            resultsText += "ET" +i.ToString() + ": " + timeStamps[i] + "\n";
//            resultsText += "PD" + (i + 1).ToString() + ": " + distances[i].ToString("F2") + ",  PR" + (i + 1).ToString() + ": " + (distances[i] / 4.8f).ToString("F2") + "\n";
//            totalDistance += distances[i];
//            //if (ghostExists)
//            //{
//            //    resultsText += "GD" + (i + 1).ToString() + ": " + ghostDistances[i].ToString("F2") + ",  GR" + (i + 1).ToString() + ": " + (ghostDistances[i] / 4.8f).ToString("F2") + "\n";
//            //    ghostTotalDistance += ghostDistances[i];
//            //}
//        }
//        resultsText += "PTD: " + totalDistance.ToString("F2") + ", PTR: " + (totalDistance / 4.8).ToString("F2");
//        //if (ghostExists)
//        //{
//        //    resultsText += "\nGTD: " + ghostTotalDistance.ToString("F2") + ", GTR: " + (ghostTotalDistance / 4.8).ToString("F2");
//        //}
//        results.text = resultsText;
//    }


	/*---------------------------------------------------------------------*/
	//Transitions back to Main Menu Scene from results screen button press //
    public void BackToMenu()
    {
        Destroy(GameObject.FindGameObjectWithTag("GameSave"));
        SceneManager.LoadScene("Menu");
    }

	/*--------------------------------------------------------------------------*/
	//Update GUI visual of score & multiplier if specified & play hit animation //
	public void updateScore(int value, bool hit = false){
        //Update Scoring label
        scoreText.text = " K " + value.ToString();
	
		//Play hit animation if player collision
		if (hit) {
			hitPanelAnimator.SetTrigger ("hit");
		}
	}

	/*-----------------------------------------------------*/
	//Updates GUI visual for multiplier and time remaining //
	public void updateMultiplier(float value, float timer, bool active){
		//Update multiplier (disabling if = -1)
		if (!active) {
			multiplierText.enabled = false;
			multiplierTimerText.enabled = false;
		} else {
            multiplierText.enabled = true;
			multiplierTimerText.enabled = true;
			multiplierTimerText.text = "G " + timer.ToString ("0.0");
			multiplierText.text = " S " + value.ToString ("0.0");
		}

	}


	/*------------------------------------------*/
    //Method for transitioning between screens. //
    private void TransitionTo(Screens screen)
    {
        mScreens[(int)mCurrentScreen].enabled = false;
        mScreens[(int)screen].enabled = true;
        mCurrentScreen = screen;
    }
}