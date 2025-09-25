using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public enum SoundType
{
    Background,
    ClickButton,
    ClickBox,
    WinGameSound,
    LoseGameSound,
    WarningTime,
    WarningHitFossil,
    Hammer,
    Drill,
    Brush,
    Glue
}

[RequireComponent(typeof(AudioSource)), ExecuteInEditMode]
public class SoundManager : MonoBehaviour
{
    [SerializeField] private Soundlist[] soundlist;
    private static SoundManager instance;
    private AudioSource audioSource;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
    public static void PlaySound(SoundType sound, float volume = 1)
    {
        AudioClip[] clips = instance.soundlist[(int)sound].Sounds;
        AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];
        instance.audioSource.PlayOneShot(randomClip, volume);
    }
    public static void PlayLoop(SoundType sound, float volume = 1)
    {
        AudioClip[] clips = instance.soundlist[(int)sound].Sounds;
        if (clips == null || clips.Length == 0) return;

        AudioClip clip = clips[0]; // ใช้อันแรก (Background มักมีอันเดียว)
        instance.audioSource.clip = clip;
        instance.audioSource.volume = volume;
        instance.audioSource.loop = true;
        instance.audioSource.Play();
    }
    public static void StopSound()
    {
        if (instance.audioSource.isPlaying)
        {
            instance.audioSource.Stop();
        }
    }
#if UNITY_EDITOR
    void OnEnable()
    {
        string[] names = Enum.GetNames(typeof(SoundType));
        Array.Resize(ref soundlist, names.Length);
        for (int i = 0; i < soundlist.Length; i++)
        {
            soundlist[i].name = names[i];
        }
    }
#endif
}

[Serializable]
public struct Soundlist
{
    public AudioClip[] Sounds { get => sounds; }
    [HideInInspector] public string name;
    [SerializeField] private AudioClip[] sounds;
}