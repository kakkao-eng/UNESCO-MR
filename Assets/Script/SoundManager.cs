    using UnityEngine;
    using System.Collections;
    using System.Collections.Generic;
    using System;

    //Enum สำหรับระบุประเภทของเสียงที่มีในเกม
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
        // รายการเก็บเสียงทั้งหมด (ตามชนิดของ SoundType)
        [SerializeField] private Soundlist[] soundlist;

        //instance แบบ static สำหรับเรียกใช้จากสคริปต์อื่นได้ง่าย
        private static SoundManager instance;

        //ตัว AudioSource สำหรับเล่นเสียง
        private AudioSource audioSource;

        private void Awake()
        {
            // กำหนด instance ให้ตัวนี้เอง (singleton)
            instance = this;
        }

        private void Start()
        {
            // ดึง AudioSource ที่ติดอยู่กับ GameObject นี้
            audioSource = GetComponent<AudioSource>();
        }

        //ฟังก์ชันสำหรับเล่นเสียงสั้น (PlayOneShot)
        public static void PlaySound(SoundType sound, float volume = 1)
        {
            // ดึงชุดเสียงของประเภทที่เลือกจาก soundlist
            AudioClip[] clips = instance.soundlist[(int)sound].Sounds;

            // ถ้าไม่มีเสียงในลิสต์ ให้หยุดทำงานเลย
            if (clips == null || clips.Length == 0) return;

            // สุ่มเสียง 1 ตัวจากชุดเสียง (ถ้ามีหลายคลิป)
            AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];

            // เล่นเสียงแบบ OneShot (เล่นทับกันได้)
            instance.audioSource.PlayOneShot(randomClip, volume);
        }

        //ฟังก์ชันเล่นเสียงแบบวน (loop)
        public static void PlayLoop(SoundType sound, float volume = 1)
        {
            // ดึงชุดเสียงของประเภทนั้น
            AudioClip[] clips = instance.soundlist[(int)sound].Sounds;

            // ถ้าไม่มีเสียงให้หยุด
            if (clips == null || clips.Length == 0) return;

            // ใช้เสียงแรกของลิสต์ (ไม่สุ่ม)
            AudioClip clip = clips[(int)sound];

            // ตั้งค่า AudioSource ให้เล่นเสียงนี้แบบวนลูป
            instance.audioSource.clip = clip;
            instance.audioSource.volume = volume;
            instance.audioSource.loop = true;
            instance.audioSource.Play();
        }

        //หยุดเสียงที่กำลังเล่นอยู่
        public static void StopSound()
        {
            if (instance.audioSource.isPlaying)
            {
                instance.audioSource.Stop();
            }
        }

    #if UNITY_EDITOR
        //ฟังก์ชันนี้จะทำงานเฉพาะใน Unity Editor เท่านั้น
        //ใช้เพื่ออัปเดตชื่อเสียงใน Inspector ให้ตรงกับ Enum อัตโนมัติ
        void OnEnable()
        {
            // ดึงชื่อทั้งหมดของ Enum SoundType
            string[] names = Enum.GetNames(typeof(SoundType));

            // ปรับขนาดอาเรย์ soundlist ให้มีเท่ากับจำนวน Enum
            Array.Resize(ref soundlist, names.Length);

            // วนใส่ชื่อของ Enum ลงใน soundlist เพื่อให้ง่ายต่อการดูใน Inspector
            for (int i = 0; i < soundlist.Length; i++)
            {
                soundlist[i].name = names[i];
            }
        }
    #endif
    }

    //โครงสร้างข้อมูลสำหรับเก็บเสียงของแต่ละประเภท
    [Serializable]
    public struct Soundlist
    {
        // Getter สำหรับดึงชุดเสียง (AudioClip[])
        public AudioClip[] Sounds { get => sounds; }

        [HideInInspector] public string name; // ใช้แสดงชื่อใน Inspector
        [SerializeField] private AudioClip[] sounds; // เก็บเสียงที่เกี่ยวข้องกับประเภทนั้น
    }
