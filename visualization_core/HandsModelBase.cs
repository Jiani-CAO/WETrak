using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Script.TrackHandsPosture.Unity.HandModule
{

    /// <summary>
    /// Supported chiralities
    /// </summary>
    public enum Chirality { Left, Right };
    /// <summary>
    /// Supported hand model types
    /// </summary>
    public enum ModelType { Graphics, Physics };

    /// <summary>
    /// HandModelBase defines abstract methods as a template for building hand models.
    /// It allows you to receive tracking data for a left or right hand and it can be
    /// extended to drive hand visuals or physics representations of hands.
    /// 
    /// A HandModelBase will automatically subscribe and receive tracking data 
    /// if there is a provider in the scene. The provider can be overridden (leapProvider).
    /// </summary>
    [ExecuteInEditMode]
    public abstract class HandsModelBase : MonoBehaviour
    {

        private bool _init = false;
        
        public bool Init
        {
            get { return _init; }
            set { _init = value; }
        }
        
        private bool _isTracked = false;
        /// <summary>
        /// Reports whether the hand is detected and tracked in the current frame.
        /// It is set to true after the event OnBegin and set to false after the event OnFinish
        /// </summary>
        public bool IsTracked
        {
            get { return _isTracked; }
        }

        /// <summary>
        /// The chirality or handedness of this hand (left or right).
        /// </summary>
        public abstract Chirality Handedness { get; set; }
        /// <summary>
        /// The type of the Hand model (graphics or physics).
        /// </summary>
        public abstract ModelType HandModelType { get; }
        /// <summary>
        /// Implement this function to initialise this hand after it is created.
        /// This function is called when a new hand is detected by the Tracking Service.
        /// </summary>
        public virtual void InitHand() { }
        /// <summary>
        /// Called after hand is initialised. 
        /// Calls the event OnBegin and sets isTracked to true.
        /// </summary>
        public virtual void BeginHand()
        {
            _isTracked = true;
        }
        /// <summary>
        /// Called once per frame when the LeapProvider calls the event 
        /// OnUpdateFrame (graphics hand) or OnFixedFrame (physics hand)
        /// </summary>
        public abstract void UpdateHand();
        
        /// <summary>
        /// Returns the Leap Hand object represented by this HandModelBase. 
        /// Note that any physical quantities and directions obtained from the Leap Hand object are 
        /// relative to the Leap coordinate system, which uses a right-handed axes and units 
        /// of millimeters.
        /// </summary>
        /// <returns></returns>
        public abstract DetectedHand GetDetectedHand();
        /// <summary>
        /// Assigns a Leap Hand object to this HandModelBase.
        /// </summary>
        /// <param name="hand"></param>
        public abstract void SetDetectedHand(DetectedHand hand);
        
        public HandDataManager handDataManager;
        
        
        private void Start()
        {
            Debug.Log("Start!");
            handDataManager = GetComponentInParent<HandDataManager>();
        }
        
        public void PrintDetectedHandTransform()
        {
            //Store all children transforms so the user has the ability to reset back to a default pose
            var allChildren = new List<Transform>();
            allChildren.Add(transform);
            allChildren.AddRange(HandBinderAutoBinder.GetAllChildren(transform));
            
            foreach (var child in allChildren)
            {
                Debug.Log(child.transform.position.ToString("f4"));
            }
        }

        void UpdateBase(DetectedHand hand)
        {
            /*
            Debug.Log("UpdateBase");
            */
            if(!_isTracked)
            {
                if (!_init)
                {
                    Debug.Log("Init Hand!");
                    InitHand();
                    _init = true;
                    
                }
                BeginHand();
            }
            else
            {
                SetDetectedHand(hand);

                UpdateHand();
            }
        }
        
        private int _frameIndex = 0;

        private void Update()
        {
            if (Application.isPlaying)
            {
                float targetFrameRate = handDataManager.frameRate;
                float timePerFrame = 1f / targetFrameRate;
                bool leftHandActivate = handDataManager.leftHandActivate;
                bool rightHandActivate = handDataManager.rightHandActivate;

                if (Time.deltaTime < timePerFrame)
                {
                    // 如果当前帧的时间差小于每帧的时间，等待剩余时间
                    float delayTime = timePerFrame - Time.deltaTime;
                    System.Threading.Thread.Sleep((int)(delayTime * 1000));
                }

                DetectedHand hand = null;

                // Get the hand from frame
                if (JsonFrame.HaveHandsList)
                {
                    _isTracked = true;
                    if ((this.Handedness == Chirality.Left && leftHandActivate) ||
                        (this.Handedness == Chirality.Right && rightHandActivate))
                    {
                        // Call an external auxiliary function to convert the JSON raw data into the DetectedHand data structure used internally
                        hand = MakeHandsFunction.MakeCapsuleDetectedHand(_frameIndex, this.Handedness);
                        if (hand != null)
                        {
                            _frameIndex++;
                            UpdateBase(hand);
                        }
                    }
                }
            }
            else
            {
                if (!_init)
                {
                    InitHand();
                    _init = true;
                }
                DetectedHand hand = null;
                hand = MakeHandsFunction.MakeTestHand(isLeft:this.Handedness == Chirality.Left);
                if (hand != null)
                {
                    _frameIndex++;
                    UpdateBase(hand);
                }
            }
        }
    }
}