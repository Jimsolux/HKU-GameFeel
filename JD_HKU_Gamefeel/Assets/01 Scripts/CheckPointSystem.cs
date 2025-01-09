using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPointSystem : MonoBehaviour
{
    [SerializeField] private List<Vector3> checkPoints;
    private Vector3 myCheckPoint;
    Rigidbody2D rb;
    PlayerMovement pm;
    [SerializeField] float myLastHighestCheckPointValue = -1;
    Energy energy;

    private void Awake()
    {
        EmptyCheckPoints();
        rb = GetComponent<Rigidbody2D>();
        pm = GetComponent<PlayerMovement>();
        energy = GetComponent<Energy>();
    }

    public void ReceiveCheckPoint(Vector2 pos, float value)
    {
        if (!checkPoints.Contains(pos) && value > myLastHighestCheckPointValue)
        {
            checkPoints.Add(pos);
            SetCheckPoint();
        }
        
    }


    private void SetCheckPoint()
    {
        Debug.Log(checkPoints[checkPoints.Count -1]);
        
        myCheckPoint = checkPoints[checkPoints.Count -1];
        AudioManager.instance.PlaySFX("Checkpoint");

    }

    public void Respawn()
    {
        //reset momemtum
        rb.velocity = Vector3.zero;
        //spawn player at location
        this.transform.position = myCheckPoint;
        pm.currentState = PlayerMovement.State.Idle;
        energy.ResetEnergy();
    }

    public void EmptyCheckPoints()
    {
        checkPoints = new List<Vector3>();
    }


}
