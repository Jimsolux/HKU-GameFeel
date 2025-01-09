using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Collectables : MonoBehaviour
{

    public float coins;
    [SerializeField] AudioSource coinGain;
    [SerializeField] TextMeshProUGUI coinText;
    private void Awake()
    {
        coinGain = GetComponent<AudioSource>();
        UpdateVisuals();
    }


    public void GainCoin()
    {
        coins++;
        PlayRandomWholeNote();
        UpdateVisuals();
    }

    public void SpendCoin(float value)
    {
        coins-= value;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        coinText.text = coins.ToString(); ;
    }

    int nextSemitone;
    public void PlayRandomWholeNote()
    {
        // Whole note intervals in semitones
        int[] wholeNoteSemitones = { 0, 2, 4, 6, 8,10, 12};

        // Randomly pick a semitone interval
        //int randomIndex = Random.Range(0, wholeNoteSemitones.Length);
        int semitone = wholeNoteSemitones[nextSemitone];
        nextSemitone++;
        if (nextSemitone >= wholeNoteSemitones.Length)
        {
            nextSemitone = 0;
        }
        // Calculate the pitch multiplier
        float pitch = Mathf.Pow(2f, semitone / 12f);

        // Apply the pitch and play the audio
        coinGain.pitch = pitch;
        coinGain.Play();
    }

}
