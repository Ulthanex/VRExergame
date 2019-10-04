using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class headRotationAdjuster : MonoBehaviour {
    public float minOffset;
    public float maxOffset;

    public float getOffset()
    {
        float angles = transform.eulerAngles.x;
        if (angles <= 90)
        {
            return Mathf.Lerp(0, maxOffset, Mathf.InverseLerp(0, 90, angles));
        }
        return Mathf.Lerp(minOffset, 0, Mathf.InverseLerp(270, 360, angles));
        //return Mathf.Lerp(minOffset, maxOffset, Mathf.InverseLerp(0,180, (transform.eulerAngles.x+90)%180));//offset -90 to 90 range by 90 and mod by 180 because angles loop back to 360 at 0
    }
}
