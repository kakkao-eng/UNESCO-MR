using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

//Enum สำหรับระบุประเภทของเสียงที่มีในเกม (ไม่มีการแก้ไข)
public enum SoundType
{
    Background,
    ClickButton,
    ClickBoxSound,
    ClickOpenBox,
    WinGameSound,
    LoseGameSound,
    WarningTime,
    WarningHitFossil,
    Hammer,
    Drill,
    Brush,
    Glue,
    V1Tutorial,
    V2tutorial,
    V3GameEnd
}

    [RequireComponent(typeof(AudioSource)), ExecuteInEditMode]
    public class SoundManager : MonoBehaviour
    {
        // รายการเก็บเสียงทั้งหมด (ตามชนิดของ SoundType)
        [SerializeField] private Soundlist[] soundlist;

        //instance แบบ static สำหรับเรียกใช้จากสคริปต์อื่นได้ง่าย
        private static SoundManager instance;

        //ตัว AudioSource สำหรับเล่นเสียง FX (PlaySound/PlayOneShot)
        [SerializeField]private AudioSource audioSource;
        // ตัว AudioSource สำหรับเล่นเสียงวนลูป/BGM/Tool Loop (PlayLoop)
        [SerializeField]private AudioSource loopAudioSource; // <--- เพิ่มตัวนี้

        private void Awake()
        {
            // กำหนด instance ให้ตัวนี้เอง (singleton)
            instance = this;
            
            // 1. ตั้งค่า AudioSource ตัวแรก (สำหรับ FX: PlaySound)
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;

            // 2. ตั้งค่า AudioSource ตัวที่สอง (สำหรับ Loop/BGM: PlayLoop)
            loopAudioSource = gameObject.AddComponent<AudioSource>(); // <--- เพิ่มตัวที่สอง
            loopAudioSource.playOnAwake = false;
            loopAudioSource.loop = true; 
        }

        private void Start()
        {
            // ลบโค้ดเดิมออกได้ เพราะ Awake จัดการหมดแล้ว
        }

        //ฟังก์ชันสำหรับเล่นเสียงสั้น (PlayOneShot) - ใช้ audioSource (FX)
        public static void PlaySound(SoundType sound, float volume = 1)
        {
            // ดึงชุดเสียงของประเภทที่เลือกจาก soundlist
            AudioClip[] clips = instance.soundlist[(int)sound].Sounds;

            // ถ้าไม่มีเสียงในลิสต์ ให้หยุดทำงานเลย
            if (clips == null || clips.Length == 0) return;

            // สุ่มเสียง 1 ตัวจากชุดเสียง (ถ้ามีหลายคลิป)
            AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];

            // เล่นเสียงแบบ OneShot (เล่นทับกันได้)
            instance.audioSource.PlayOneShot(randomClip, volume); // <--- ใช้ audioSource เดิม
        }

        //ฟังก์ชันเล่นเสียงแบบวน (loop) - ใช้ loopAudioSource
        public static void PlayLoop(SoundType sound, float volume = 1)
        {
            // ดึงชุดเสียงของประเภทนั้น
            AudioClip[] clips = instance.soundlist[(int)sound].Sounds;

            // ถ้าไม่มีเสียงให้หยุด
            if (clips == null || clips.Length == 0) return;

            // ใช้เสียงแรกของลิสต์ (ไม่สุ่ม) และป้องกัน Index Out of Bounds
            AudioClip clip = clips[0]; // <--- แก้ไขให้ใช้ index 0 เสมอ

            // ตั้งค่า AudioSource ให้เล่นเสียงนี้แบบวนลูป
            instance.loopAudioSource.clip = clip; // <--- ใช้ loopAudioSource ใหม่
            instance.loopAudioSource.volume = volume;
            instance.loopAudioSource.loop = true; // ตั้งค่าใหม่ (เผื่อไว้)
            instance.loopAudioSource.Play();
        }

        //หยุดเสียง FX ที่กำลังเล่นอยู่
        public static void StopSound()
        {
            if (instance.audioSource.isPlaying)
            {
                instance.audioSource.Stop();
            }
        }

    // ฟังก์ชันสำหรับหยุดเสียงวนลูป (สำหรับ BGM และ Tool Loop)
        public static void StopLoopSound() // <--- เพิ่มฟังก์ชันนี้
        {
            if (instance.loopAudioSource.isPlaying)
            {
                instance.loopAudioSource.Stop();
            }
        }
        public AudioClip GetClip(SoundType soundType)
        {
            if (soundlist == null || soundlist.Length == 0) return null;
            var clips = soundlist[(int)soundType].Sounds;
            if (clips == null || clips.Length == 0) return null;
            return clips[0]; // ดึงคลิปแรก
        }

    #if UNITY_EDITOR
        //ฟังก์ชันนี้จะทำงานเฉพาะใน Unity Editor เท่านั้น (ไม่มีการแก้ไข)
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

//โครงสร้างข้อมูลสำหรับเก็บเสียงของแต่ละประเภท (ไม่มีการแก้ไข)
[Serializable]
public struct Soundlist
{
    public AudioClip[] Sounds { get => sounds; }
    [HideInInspector] public string name;
    [SerializeField] private AudioClip[] sounds;
}
