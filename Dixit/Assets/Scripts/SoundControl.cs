using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundControl : MonoBehaviour
{
    public AudioMixer m_AudioMixer;
    public float m_muted = -80;
    public float m_unmuted = 0;

    private bool musicActive = true;
    private bool sfxActive = true;

    public void ToggleMusic()
    {
        m_AudioMixer.SetFloat("MusicVolume", musicActive ? m_muted : m_unmuted);
        musicActive = !musicActive;
    }

    public void ToggleSfx() {
        m_AudioMixer.SetFloat("SFXVolume", sfxActive ? m_muted : m_unmuted);
        sfxActive = !sfxActive;
    }
}