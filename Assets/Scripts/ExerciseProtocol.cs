//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;

//public class ExerciseProtocol : MonoBehaviour {

//    [SerializeField]
//    private List<float> timeList = new List<float>();

//    [SerializeField]
//    Text timer, prompt;

//    [SerializeField]
//    GameObject player, ghost, police, vehicles, ghostModel;

//    [SerializeField]
//    Image blackScreen;

//    [SerializeField]
//    IntensityController IC;

//    [SerializeField]
//    GameManager GM;

//    [SerializeField]
//    SerialPortCommunicator SPC;

//    [SerializeField]
//    GhostController GC;

//    [SerializeField]
//    PlayerController PC;

//    [SerializeField]
//    GameScreenManager GSM;

//    [SerializeField]
//    private float maxDistancePolice = 15.0f, minDistancePolice = 5.0f;

//    private const int lowResistance = 5;
//    private int highResistance;

//    private float policeDiff;
//    private bool intense = false;
//    private bool teleportReady = true;

//    private float currentTime;
//    private bool preSizeUp = true;

//    private List<float> distances = new List<float>();
//    private List<float> ghostDistances = new List<float>();
//    private List<string> timeStamps = new List<string>();

//    // Use this for initialization
//    void Start () {
//        policeDiff = maxDistancePolice - minDistancePolice;
//        IC.police.transform.position -= IC.police.transform.forward * maxDistancePolice; //Moving the police back to the max distance upon starting the script.
//        currentTime = timeList[0];
//        timeList.RemoveAt(0);
//        highResistance = DataSaver.saver.sprintResistance;
//        SPC.resistance = lowResistance;
//    }

//    // Update is called once per frame
//    void Update() {
//        currentTime -= Time.deltaTime;
//        if (currentTime < 5.0f && teleportReady && !intense && timeList.Count != 0)
//        {
//            teleportReady = false;
//            StartCoroutine(FadeToBlack());
//        }

//        if (preSizeUp && (currentTime < 3.1f))
//        {
//            timer.fontSize = 100;
//            if (!intense)
//            {
//                if(timeList.Count != 0)
//                {
//                    prompt.text = "Ready To Sprint?";
//                }
//                else
//                {
//                    prompt.text = "Almost Done";
//                }
//            }
//            else
//            {
//                prompt.text = "Ready to Slow?";
//            }
//            preSizeUp = false;
//        }
//        timer.text = (currentTime).ToString("F1");

//        if (currentTime <= 0)
//        {
//            nextTime();
//        }
//        if (Input.GetKeyDown("up"))
//        {
//            SPC.resistance += 50;
//        }
//        else if (Input.GetKeyDown("down"))
//        {
//            SPC.resistance -= 50;
//        }
//    }

//    void teleport()
//    {
//        //Teleport ghost if there is one
//        Vector3 policeSeperator = new Vector3(0.0f, 0.0f, (player.transform.position - police.transform.position).z);
//        GM.restartSprint();
//        police.transform.position -= policeSeperator;
//        if (GM.ghostExists)
//        {
//            ghostModel.SetActive(false);
//        }
//        PC.freezeMovement = true;
//    }

//    void nextTime()
//    {
//        if(timeList.Count != 0)
//        {
//            intense = !intense; //Change intensity state
//            currentTime = timeList[0];
//            timeList.RemoveAt(0);
//            //When high intensity the difference to get closer is positive (increases speed).
//            if (intense)
//            {
//                IC.changeIntensity(policeDiff/currentTime); // Pass in the listed intesity later
//                prompt.text = "Sprint!";

//                SPC.resistance = highResistance;

//                if (GM.ghostExists)
//                {
//                    ghostModel.SetActive(true);
//                }
//                PC.freezeMovement = false;
//                vehicles.SetActive(true);
//            }
//            else
//            {
//                distances.Add(GM.calculateDistance());
//                timeStamps.Add(System.DateTime.Now.ToLongTimeString());
//                if (GM.ghostExists)
//                {
//                    ghostDistances.Add(GM.calculateGhostDistance());
//                }

//                IC.changeIntensity(-policeDiff/currentTime); // Pass in the listed intesity later
//                prompt.text = "Slow!";
//                SPC.resistance = lowResistance;
//            }
//            preSizeUp = true;
//            teleportReady = true;
//            timer.fontSize = 60;
//            StartCoroutine(textDelay());
//        }
//        else
//        {
//            //End Session
//            prompt.text = "Well Done You've Finished!";
//            GSM.distances = distances;
//            GSM.timeStamps = timeStamps;
//            bool beatGhost = true; //True when there is no ghost set false if there is a ghost and the user didn't beat it overall.

//            if (GM.ghostExists)
//            {
//                //Set screen maanger lists
//                GSM.ghostDistances = ghostDistances;
//                //Calculate total distances covered.
//                float playerTotal = 0.0f, ghostTotal = 0.0f;
//                for(int i = 0; i < distances.Count; i++)
//                {
//                    playerTotal += distances[i];
//                    ghostTotal += ghostDistances[i];
//                }
//                //If user didnt beat ghost set beatGhost to false
//                if(playerTotal < ghostTotal)
//                {
//                    beatGhost = false;
//                }
//            }
//            else
//            {
//                GSM.ghostExists = false;
//            }
//            GM.endSession(beatGhost);
//        }
//    }

//    IEnumerator textDelay()
//    {
//        yield return new WaitForSeconds(1.5f);
//        prompt.text = "";
//    }

//    IEnumerator FadeToBlack()
//    {
//        float alpha = blackScreen.color.a;
//        for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / 1.0f)
//        {
//            Color newColor = new Color(0, 0, 0, Mathf.Lerp(alpha, 1.0f , t));
//            blackScreen.color = newColor;
//            yield return null;
//        }
//        teleport();
//        yield return new WaitForSeconds(0.5f);
//        yield return StartCoroutine(FadeFromBlack());
//    }

//    IEnumerator FadeFromBlack()
//    {
//        float alpha = blackScreen.color.a;
//        for (float t = 0.0f; t < 1.0f; t += Time.deltaTime / 1.0f)
//        {
//            Color newColor = new Color(0, 0, 0, Mathf.Lerp(alpha, 0.0f, t));
//            blackScreen.color = newColor;
//            yield return null;
//        }
//    }
//}
