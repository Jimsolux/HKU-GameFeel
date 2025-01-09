using UnityEngine;

public class EnergyPickup : MonoBehaviour
{
    Energy playerEnergy;
    [SerializeField] float amount;

    private void Awake()
    {
        playerEnergy = GameObject.Find("PlayerObject").GetComponent<Energy>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerEnergy = collision.GetComponent<Energy>();
            playerEnergy.GainEnergy(amount);
            Destroy(this.gameObject);
        }
    }
}
