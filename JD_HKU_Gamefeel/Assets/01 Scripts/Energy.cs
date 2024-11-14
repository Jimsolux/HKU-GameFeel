using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Energy : MonoBehaviour
{
    [SerializeField] public float energy;
    [SerializeField] private float maxEnergy;
    [SerializeField] private float restoreTime;
    [SerializeField] private float restoreAmount;
    PlayerMovement pm;
    private void Awake()
    {
        pm = GetComponent<PlayerMovement>();
        ResetEnergy();
        //InvokeRepeating("RestoreEnergy", 2, restoreTime);
    }
    private void ResetEnergy()
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
            else if(energy >= maxEnergy)
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
            yield return new WaitForSeconds(3);
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
            Color color = energyImage.color;
            color.a = visibilityFloat;
            energyImage.color = color;
        }
    }

    #endregion
}


