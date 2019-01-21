using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {

    public enum SOUNDS
    {
        JUMP = 0,
        VICTORY,
        RESTART,
        NUM_SOUNDS
    }

    [SerializeField] AudioClip[] sounds;

    public void PlaySound(SOUNDS _sound)
    {
        GetComponent<AudioSource>().clip = sounds[(int)_sound];
        GetComponent<AudioSource>().pitch = 1.0f;
        GetComponent<AudioSource>().Play();
    }

    public void PlaySoundRandomPitch(SOUNDS _sound)
    {
        GetComponent<AudioSource>().clip = sounds[(int)_sound];
        GetComponent<AudioSource>().pitch = 1.0f + Random.Range(-0.08f, 0.1f);
        GetComponent<AudioSource>().Play();
    }
}
