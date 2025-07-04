﻿using UnityEngine;

[CreateAssetMenu(fileName = "FossilData", menuName = "Fossil Game/Fossil Data")]
public class FossilData : ScriptableObject
{
    [Header("Fossil Collection")]
    public GameObject[] fossilPrefabs; // Array to hold 11 fossil prefabs
    public string[] fossilNames;       // Array to hold fossil names

}