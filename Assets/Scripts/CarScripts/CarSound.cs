using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSound : Photon.PunBehaviour
{
    [SerializeField]
    private CarScript carScript;
    [SerializeField]
    private AudioSource SkidSound;
    [SerializeField]
    private AudioSource hitSound;
    private AudioSource engineSound;
    [SerializeField]
    private AudioSource nitroSound;
    [SerializeField]
    private AudioSource carBreak;

    private float startPitch;
    private bool lockHit = false;

    private void Start()
    {
        engineSound = GetComponent<AudioSource>();
        startPitch = GetComponent<AudioSource>().pitch;
    }

    void FixedUpdate()
    {
        AudioChange();
    }

    public void DeleteSounds()
    {
        DistanceEqualizer[] equalizers = GetComponentsInChildren<DistanceEqualizer>();
        for (int i = 0; i < equalizers.Length; i++)
        {
            Destroy(equalizers[i]);
        }

        AudioSource[] sources = GetComponentsInChildren<AudioSource>();
        for (int i = 0; i < sources.Length; i++)
        {
            Destroy(sources[i]);
        }
    }

    public void DestroyAudioFilter()
    {
        if (photonView.isMine)
        {
            AudioSource[] g = GetComponentsInChildren<AudioSource>();
            for (int i = 0; i < g.Length; i++)
            {
                Destroy(g[i].gameObject.GetComponent<DistanceEqualizer>());
                Destroy(g[i].gameObject.GetComponent<AudioLowPassFilter>());
            }
        }
    }

    void AudioChange()
    {
        if (engineSound != null)
            engineSound.pitch = carScript.CurrentRpm / carScript.MaxSpeed * 2.5f + startPitch;
    }

    public void DisableSound()
    {
        if (engineSound != null)
        {
            engineSound.Stop();
            SkidSound.Stop();
        }
    }

    public void EnableSound()
    {
        if (engineSound != null)
            if (engineSound.isPlaying == false)
                engineSound.Play();
    }

    public void EnableNitro()
    {
        if (nitroSound != null)
            if (nitroSound.isPlaying == false)
                nitroSound.Play();
    }

    public void DisableNitro()
    {
        if (nitroSound != null)
            nitroSound.Stop();
    }

    public void DisableSkidSound()
    {
        if (SkidSound != null)
            SkidSound.Stop();
    }

    public void EnableSkidSound()
    {
        if (SkidSound != null)
            if (SkidSound.isPlaying == false)
                SkidSound.Play();
    }

    public void ResetPitch()
    {
        if (engineSound != null)
            if (engineSound.pitch != startPitch)
                engineSound.pitch = startPitch;
    }

    public void CarBreak()
    {
        if (carBreak != null)
            if (!carBreak.isPlaying)
                carBreak.Play();
    }


    public void InitHitSound(float currSpeed)
    {
        if (lockHit == false)
        {
            lockHit = true;

            if (hitSound != null)
            {
                hitSound.volume = 0.1275f;
                hitSound.volume = hitSound.volume * currSpeed / carScript.MaxSpeed;
                hitSound.Play();
            }

            StartCoroutine(AddTimer());
        }
    }

    IEnumerator AddTimer()
    {
        yield return new WaitForSeconds(0.6f);
        lockHit = false;
        yield return null;
    }
}
