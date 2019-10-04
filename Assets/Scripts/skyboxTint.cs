using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class skyboxTint : MonoBehaviour {

    public Transform player;
    public Transform astronaut;
    public Light light;

    public Color goodLight;
    public Color badLight;
    

	void Update () {
        //boundary tints for the skybox
        Color goodSkybox = Color.HSVToRGB(245 / 359.0f, 116 / 255.0f, 219 / 255.0f);
        Color badSkybox = Color.HSVToRGB(245 / 359.0f, 156 / 255.0f, 73 / 255.0f);

        //linearly interpolating between good and bad skybox based on distance between chaser and avatar in the range of 0-50 game units
        RenderSettings.skybox.SetColor("_Tint", Color.Lerp(goodSkybox, badSkybox, Mathf.InverseLerp(50,0,player.position.z-astronaut.position.z)));
        light.color = Color.Lerp(goodLight, badLight, Mathf.InverseLerp(50, 0, player.position.z - astronaut.position.z));
    }
	
}
