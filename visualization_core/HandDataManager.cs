using System;
using UnityEngine;

namespace Script.TrackHandsPosture
{
    [System.Serializable]
    public class HandDataManager : MonoBehaviour
    {
        public bool leftHandActivate = false;
        public bool rightHandActivate = false;

        public string handsFileName;
        public string handsFilePath;
        public float frameRate;
        private string _fileUrl;
        private static bool _haveRead = false;

        private void Start()
        {
            if (_haveRead == false)
            {
                _fileUrl = handsFilePath + "\\" + handsFileName;
                _haveRead = JsonFrame.ParseJsonFile(_fileUrl);
            }
        }

        private void Update()
        {
            if (_haveRead == false)
            {
                _fileUrl = handsFilePath + "\\" + handsFileName;
                _haveRead = JsonFrame.ParseJsonFile(_fileUrl);
            }
        }
    }
}