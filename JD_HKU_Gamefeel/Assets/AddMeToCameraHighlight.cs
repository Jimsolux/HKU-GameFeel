using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddMeToCameraHighlight : MonoBehaviour
{
    [SerializeField] CameraFollow cameraFollow;
    [SerializeField] GameObject player;
    [SerializeField] bool hasShown = false;

    private void Awake()
    {
        cameraFollow = Camera.main.GetComponent<CameraFollow>();
        player = GameObject.Find("PlayerObject");
    }
    private void FixedUpdate()
    {
        AddMeToHighlights();
    }

    private void AddMeToHighlights()
    {
        //Debug.Log(Vector2.Distance(player.transform.position, this.transform.position));
        if(Vector2.Distance(player.transform.position, this.transform.position) < 25 && !hasShown)
        {
            Debug.Log("Close enough!");
            if (!cameraFollow.objectsToFollow.Contains(this.gameObject))
            {
                cameraFollow.objectsToFollow.Add(this.gameObject);
                hasShown = true;
                Debug.Log("Added me to list!!");

            }
            StartCoroutine(RemoveMeFromHighLights());
        }

    }

    private IEnumerator RemoveMeFromHighLights()
    {
        yield return new WaitForSeconds(5);
        if (cameraFollow.objectsToFollow.Contains(this.gameObject))
        {
            cameraFollow.objectsToFollow.Remove(this.gameObject);
            Debug.Log("Removed.");
        }
    }
}
