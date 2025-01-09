using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GoStartLevel : MonoBehaviour
{
    public void OnFadeComplete()
    {
        SceneManager.LoadScene(0);
    }
}
