using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using UnityEngine;
namespace Script.TrackHandsPosture
{
    [Serializable]
    public class JsonHandsList
    {
        public List<JsonHands> handsList;
    }
    [Serializable]
    public class JsonHands
    {
        public long timestamp;
        public int fingers;
        public int frame_id;
        public int hands;
        public JsonHand right_hand = new JsonHand();
        public JsonHand left_hand = new JsonHand();

        public JsonHands()
        {
            timestamp = 0;
            fingers = 0;
            frame_id = 0;
            hands = 0;
            right_hand = null;
            left_hand = null;
        }
    }
    [Serializable]
    public class JsonHand
    { 
        public BoneAngles bone_angles;
        public JsonHand()
        {
            bone_angles = null;
        }
    }
    [Serializable]
    public class BoneAngles
    {
        public JsonFinger Pinky = null;
        public JsonFinger Index = null;
        public JsonFinger Ring = null;
        public JsonFinger Thumb = null;
        public JsonFinger Middle = null;
    }
    [Serializable]
    public class JsonFinger
    {
        public JsonBone Distal;
        public JsonBone Intermediate;
        public JsonBone Proximal;
        public JsonBone Metacarpals;
    }
    [Serializable]
    public class JsonBone
    { 
        public double Roll;
        public double Yaw;
        public double Pitch;
    }
    
    [System.Serializable]
    public static class JsonFrame
    {
        public static JsonHandsList JsonHandsList = new JsonHandsList();
        public static StreamReader  JsonStream = null;
        public static bool HaveHandsList = false;
        // Read data from json file
        public static string ReadDataFromJsonFile(string fileUrl)
        {
            string readData;
            Debug.Log(fileUrl);
            JsonStream = File.OpenText(fileUrl);
            readData = JsonStream.ReadToEnd();
            JsonStream.Close();
            return readData;
        }

        public static bool ParseJsonFile(string fileUrl)
        {
            string jsonData = ReadDataFromJsonFile(fileUrl); 
            JsonUtility.FromJsonOverwrite(jsonData, JsonHandsList);
            PrintJsonData();
            HaveHandsList = true;
            return HaveHandsList;
        }
        
        public static JsonHands GetOneFrameHands(int frameIndex)
        {
            if (frameIndex >= JsonHandsList.handsList.Count)
                return null;
            return JsonHandsList.handsList[frameIndex];
        }

        public static void PrintJsonData()
        {
            for (int i = 0; i < 5; i++)
            {
                double x =  (float)JsonHandsList.handsList[i].left_hand.bone_angles.Pinky.Distal.Pitch;
                Debug.Log(JsonHandsList.handsList[i].frame_id);
                // Debug.Log("Pitch is: " + x.ToString());
                double y =  (float)JsonHandsList.handsList[i].left_hand.bone_angles.Pinky.Distal.Yaw;
                // Debug.Log("Yaw is: " + y.ToString());
            }
        }
    }
}