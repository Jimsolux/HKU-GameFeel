using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class BodyStretcher : MonoBehaviour
{

    [SerializeField] GameObject objectToScale;
    [SerializeField] float idleStretchValue;
    [SerializeField] float jumpStretchValue;
    [SerializeField] float idleStretchTime;
    //Original
    [SerializeField] float originalScale;


    private void Awake()
    {
        SaveStartScale();
    }

    private void IdleStretching(float currentScale)
    {
        float stretchedScale = currentScale * idleStretchValue;
        float activeYScale = Mathf.Lerp(currentScale, stretchedScale, idleStretchTime);
        if(activeYScale <= idleStretchValue)
        {

        }

        objectToScale.transform.localScale = new Vector3(objectToScale.transform.localScale.x, activeYScale, objectToScale.transform.localScale.z);
    }

    private void ReturnToOriginalScale()
    {
        if (objectToScale != null)
        {
            objectToScale.transform.localScale = new Vector3(objectToScale.transform.localScale.x, originalScale, objectToScale.transform.localScale.z);
        }
    }

    private void SaveStartScale()
    {
        if (objectToScale != null)
        {
            originalScale = objectToScale.transform.localScale.y;
        }
    }
}
