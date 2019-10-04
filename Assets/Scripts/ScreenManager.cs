using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScreenManager : MonoBehaviour
{
    public enum Screens { MenuScreen, ReadyScreen, NumScreens }

    private Canvas[] mScreens;
    private Screens mCurrentScreen;

    [SerializeField]
    Text ID, sprintRes, straightBool, personalityCondition;

	public Button loadGame;

	/*-------------------*/
	// On Initialisation //
    void Awake()
    {
        mScreens = new Canvas[(int)Screens.NumScreens];
        //Identifying screens in the scene.
        Canvas[] screens = GetComponentsInChildren<Canvas>();
        for (int count = 0; count < screens.Length; ++count)
        {
            for (int slot = 0; slot < mScreens.Length; ++slot)
            {
                if (mScreens[slot] == null && ((Screens)slot).ToString() == screens[count].name)
                {
                    mScreens[slot] = screens[count];
                    break;
                }
            }
        }

        //Disabling all the screens except the title screen.
		if (mScreens.Length > 2) {
			for (int screen = 1; screen < mScreens.Length; ++screen) {
				mScreens [screen].enabled = false;
			}
		}

        //Set the curent screen to the titlescreen.
        mCurrentScreen = Screens.MenuScreen;
    }


	/*-------------------------------------------------------*/
    //Method used when pressing load game from the main menu.//
    public void GoToReady()
    {
		if(ID.text != "" && sprintRes.text != "" && personalityCondition.text != "") //straightBool.text != "")
        {
            //Grabs user participant ID
			DataSaver.saver.userID = ID.text;

			//Grabs participants set sprint Resistance
            DataSaver.saver.sprintResistance = int.Parse(sprintRes.text);

			if (personalityCondition.text == "0") {
				DataSaver.saver.hexType = personalityType.Survivor;
			} else if (personalityCondition.text == "1") {
				DataSaver.saver.hexType = personalityType.Conqueror;
			} else {
				DataSaver.saver.hexType = personalityType.Mixed;
			}

            //if(straightBool.text == "1")
           // {
           //     DataSaver.saver.straight = true;
           // }
           // else
           //{
           //     DataSaver.saver.straight = false;
           // }

            DataSaver.saver.Load();
            //TransitionTo(Screens.ReadyScreen);
			StartGame();
            
        }
    }


	/*-------------------------------------------------------------------*/
	// Enables load game button once suitable input is within each field //
	public void CheckInput(){

		if (ID.text != "" && sprintRes.text != "" && personalityCondition.text != "") {
			loadGame.enabled = true;
			loadGame.gameObject.SetActive (true);    
		} else {
			loadGame.enabled = false;
			loadGame.gameObject.SetActive (false);
		}
	}

	/*------------------------------------*/
	//Transitions back to main input menu //
    public void ReturnToMenu()
    {
        TransitionTo(Screens.MenuScreen);
    }

	/*-------------------------*/
	//Transitions to game Menu //
    public void StartGame()
    {
        SceneManager.LoadScene(3);
    }

	/*------------------------*/
    //Method to quit the game //
    public void ExitGame()
    {
        Application.Quit();
    }

	/*-----------------------------------------*/
    //Method for transitioning between screens.//
    private void TransitionTo(Screens screen)
    {
        mScreens[(int)mCurrentScreen].enabled = false;
        mScreens[(int)screen].enabled = true;
        mCurrentScreen = screen;
    }
}