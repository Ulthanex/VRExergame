using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntensityController : MonoBehaviour {

    [Header("GameObjects:")]
    [SerializeField]
    public GameObject police;

    private PoliceBehaviour pBehaviour;

    [Header("Skyboxes:")]
    [SerializeField]
    private Material daySkybox;
    [SerializeField]
    private Material nightSkybox;

    [Header("Scene Lights:")]
    [SerializeField]
    private Light dayLight;
    [SerializeField]
    private Light nightLight;

    public bool day = true;
    
    void Start()
    {
        pBehaviour = police.GetComponent<PoliceBehaviour>();
    }
	
	// Update is called once per frame
	public void changeIntensity (float policeSpeedDiff) {
        changeLighting();
        pBehaviour.addedIntensitySpeed = policeSpeedDiff;
        policeStateTransition();
    }

    void changeLighting()
    {
        GameObject[] lightArray = GameObject.FindGameObjectsWithTag("Light");

        if (day)
        {
            RenderSettings.skybox = nightSkybox;

            foreach (GameObject light in lightArray)
            {
                light.transform.GetChild(0).gameObject.SetActive(true);
            }
            day = false;
        }
        else
        {
            RenderSettings.skybox = daySkybox;

            foreach (GameObject light in lightArray)
            {
                light.transform.GetChild(0).gameObject.SetActive(false);
            }
            day = true;
        }
        dayLight.enabled = !dayLight.enabled;
        nightLight.enabled = !nightLight.enabled;
    }

    void policeStateTransition()
    {
        for(int i = 0; i < 4; i++)
        {
            police.transform.GetChild(i).gameObject.SetActive(!day);
        }
    }
}
