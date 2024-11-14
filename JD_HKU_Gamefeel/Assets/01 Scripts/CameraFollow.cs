using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private List<GameObject> objectsToFollow = new List<GameObject>();

    private void FixedUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, CalculateAveragePosition(), 1f * Time.deltaTime);
        //Camera.main.transform.position = CalculateAveragePosition();
    }

    private Vector3 CalculateAveragePosition()
    {
        if (objectsToFollow == null || objectsToFollow.Count == 0)
            return Vector3.zero; // Return zero if there are no objects

        Vector3 totalScreenPosition = Vector3.zero;

        foreach (GameObject obj in objectsToFollow)
        {
            if (obj != null)
            {
                Vector3 screenPosition = obj.transform.position;
                totalScreenPosition += screenPosition;


            }
        }
        Vector3 averageScreenPosition = totalScreenPosition / objectsToFollow.Count;
        Vector3 properVector = new Vector3(averageScreenPosition.x, averageScreenPosition.y, -10);
        return properVector;
    }
}
