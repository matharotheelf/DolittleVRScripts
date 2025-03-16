using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class VolumeDetection : MonoBehaviour
{
    [SerializeField] AudioSource microphoneSource;
    [SerializeField] AudioMixer volumeMixer;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(Microphone.devices);
        microphoneSource.clip = Microphone.Start(Microphone.devices[0], true, 1800, 44100);
        microphoneSource.Play();
    }

        // Update is called once per frame
        void Update()
    {
        float volume = 0;
        volumeMixer.GetFloat("Attenuation", out volume);
    }
}
