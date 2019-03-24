using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleSound : MonoBehaviour
{
    [SerializeField] Sprite on;
    [SerializeField] Sprite off;
    [SerializeField] AudioSource music;
    [SerializeField] bool findMusic;
    Button button;

    private void Start()
    {
        if(findMusic)
        {
            music = GameObject.Find("Ladybugs").GetComponent<AudioSource>();
        }
        button = GetComponent<Button>();
    }


    public void ToggleMusic()
    {
        if (music.volume > 0.0f)
        {
            music.volume = 0.0f;
            button.image.sprite = off;
        }
        else
        {
            music.volume = 1.0f;
            button.image.sprite = on;
        }
    }
}
