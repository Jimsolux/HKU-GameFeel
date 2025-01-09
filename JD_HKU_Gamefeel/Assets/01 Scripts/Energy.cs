using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Energy : MonoBehaviour
{
    [SerializeField] public float energy;
    [SerializeField] private float maxEnergy;
    [SerializeField] private float restoreTime;
    [SerializeField] private float restoreAmount;
    private bool recentlyUsedEnergy;
    PlayerMovement pm;
    private void Awake()
    {
        pm = GetComponent<PlayerMovement>();
        ResetEnergy();
        //InvokeRepeating("RestoreEnergy", 2, restoreTime);
    }
    public void ResetEnergy()
    {
        energy = maxEnergy;
        UpdateEnergy();
    }
    public void UseEnergy(float amt)
    {
        energy -= amt;
        Mathf.Clamp(energy, 0, maxEnergy);
        UpdateEnergy();
        StartCoroutine(StartRestore());
    }

    public void GainEnergy(float amt)
    {
        energy += amt;
        Mathf.Clamp(energy, 0, maxEnergy);
        UpdateEnergy();
    }

    private void UpdateEnergy()
    {
        //Update the visualisations of the energy.
        UpdateVisualEnergy();
    }

    private void RestoreEnergy()
    {
        if (pm.regensEnergy)
        {
            if (energy < maxEnergy)
            {
                GainEnergy(restoreAmount);
            }
            else if (energy >= maxEnergy)
            {
                CancelInvoke();
            }
        }
    }

    private bool oneCoroutineIsActive;
    private IEnumerator StartRestore()
    {
        if (!oneCoroutineIsActive)
        {
            oneCoroutineIsActive = true;
            yield return new WaitForSeconds(1);
            InvokeRepeating("RestoreEnergy", 0, restoreTime);
            oneCoroutineIsActive = false;
        }
    }

    #region visualisation

    [SerializeField] Image energyImage;
    private void UpdateVisualEnergy()
    {
        if (energyImage != null)
        {
            energyImage.fillAmount = energy / 100;

            float visibilityFloat = energyImage.fillAmount * -1 + 1;
            //if oneCoroutineIsActive (then its busy restoring) Start a coroutine ONCE to start letting stamina bar disappear after 2 seconds.
            StartCoroutine(IHaveRecentlyUsedEnergy());
        }
    }

    private IEnumerator StartChangingStaminaOpacity()
    {
        yield return new WaitForSeconds(0.3f);
        Color color = energyImage.color;
        float visibilityFloat = energyImage.fillAmount * -1 + 1;
        
        color.a = visibilityFloat;
        energyImage.color = color;

        //while (visibilityFloat > 0)
        //{
        //    visibilityFloat = Mathf.Lerp(255, 0, 1);
        //    color.a = visibilityFloat;
        //    energyImage.color = color;
        //}
    }
    #endregion

    private bool rueRunning;
    private IEnumerator IHaveRecentlyUsedEnergy()
    {
        if (rueRunning)//If this coroutine is already running
        {
            StopCoroutine(IHaveRecentlyUsedEnergy());//Stop this routine
            rueRunning = false; //it is no longer running
            StartCoroutine(IHaveRecentlyUsedEnergy());//And start it again.
        }
        //Debug.Log("I am Waiting 1 second and then starting to change opacity.");
        rueRunning = true;
        yield return new WaitForSeconds(1);
        //Start Changing Opacity.
        StartCoroutine(StartChangingStaminaOpacity());
        rueRunning = false;
    }
}


