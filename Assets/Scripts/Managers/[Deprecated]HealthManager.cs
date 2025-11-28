
// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.Runtime.InteropServices.WindowsRuntime;
// using UnityEngine;


// [System.Serializable]
// public class StringFloatPair
// {
//     public string key;
//     public float value;
// }

// [System.Serializable]
// public class SerializedDictionary
// {
//     public StringFloatPair[] pairs = new StringFloatPair[0];

//     public Dictionary<string, float> ToDictionary()
//     {
//         Dictionary<string, float> dict = new Dictionary<string, float>();
//         foreach (var pair in pairs)
//         {
//             dict.Add(pair.key, pair.value);
//         }
//         return dict;
//     }
// }
// /// <summary>
// /// class that handles taking and dealing damage
// /// </summary>
// public class HealthManager : MonoBehaviour
// {

//     [SerializeField]
//     private float playerHealth = 50f;
//     [SerializeField]
//     private static SerializedDictionary damageSerialDict;

//     [SerializeField]
//     private static SerializedDictionary healthSerialDict;


//     private static Dictionary<string, float> damageDict = new Dictionary<string, float>();
//     private static Dictionary<string, float> healthDict = new Dictionary<string, float>();

//     public void Start()
//     {
//         damageDict = damageSerialDict.ToDictionary();
//         healthDict = healthSerialDict.ToDictionary();
        
//     }

//     public static float DealDamage(string attackTag, string receiver)
//     {
//         float damageRecieved = 0f;

//         return damageRecieved;        
//     }

// }
