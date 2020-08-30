/* created by: SWT-P_SS_20_Dixit */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Sound Control for muting/unmuting Music and SFX audio
/// \author SWT-P_SS_20_Dixit
/// </summary>
public class SoundControl : MonoBehaviour
{
    /// <summary>
    /// Audiomixer that stores Music and SFX AudioMixer group
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public AudioMixer m_AudioMixer;

    /// <summary>
    /// float value for muting sound
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public float m_muted = -80;
    /// <summary>
    /// float value for unmuting sound
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public float m_unmuted = 0;

    private bool musicActive = true;
    private bool sfxActive = true;

    /// <summary>
    /// Switches between unmuted/muted music audio
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public void ToggleMusic()
    {
        m_AudioMixer.SetFloat("MusicVolume", musicActive ? m_muted : m_unmuted);
        musicActive = !musicActive;
    }

    /// <summary>
    /// Switches between unmuted/muted SFX audio
    /// </summary>
    /// \author SWT-P_SS_20_Dixit
    public void ToggleSfx() {
        m_AudioMixer.SetFloat("SFXVolume", sfxActive ? m_muted : m_unmuted);
        sfxActive = !sfxActive;
    }
}