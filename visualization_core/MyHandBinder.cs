using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Script.TrackHandsPosture.Unity.HandModule
{
    [DisallowMultipleComponent]
    public class MyHandBinder : HandsModelBase
    {
        #region Inspector

        /// <summary> 
        /// The data structure that contains transforms that get bound to the leap data 
        /// </summary>
        public BoundHand boundHand = new BoundHand();

        /// <summary> 
        /// The length of the elbow to maintain the correct offset from the wrist 
        /// </summary>
        [Tooltip("The length of the elbow to maintain the correct offset from the wrist")]
        public float elbowLength;

        /// <summary> 
        /// The rotation offset that will be assigned to all the fingers 
        /// </summary>
        [Tooltip("The rotation offset that will be assigned to all the fingers")]
        public Vector3 globalFingerRotationOffset;

        /// <summary> 
        /// The rotation offset that will be assigned to the wrist 
        /// </summary>
        [Tooltip("The rotation offset that will be assigned to the wrist")]
        public Vector3 wristRotationOffset;


        /// <summary> 
        /// Stores all the children's default pose 
        /// </summary>
        public SerializedTransform[] defaultHandPose;


        #endregion
        #region Hand Model Base
        
        /// <summary> 
        /// The chirality or handedness of the hand.
        /// Custom editor requires Chirality in a non overridden property, Public Chirality exists for the editor.
        /// </summary>
        public Chirality chirality;

        /// <summary>
        /// The chirality or handedness of this hand (left or right).
        /// To set, change the public Chirality.
        /// </summary>
        public override Chirality Handedness { get { return chirality; } set { } }
        /// <summary>
        /// The type of the Hand model (set to Graphics).
        /// </summary>
        public override ModelType HandModelType { get { return ModelType.Graphics; } }

        /// <summary>
        /// The Leap Hand object this hand model represents.
        /// </summary>
        public DetectedHand detectedHand;

        /// <summary>
        /// Returns the Leap Hand object represented by this HandModelBase. 
        /// Note that any physical quantities and directions obtained from the Leap Hand object are 
        /// relative to the Leap Motion coordinate system, which uses a right-handed axes and units 
        /// of millimeters.
        /// </summary>
        /// <returns></returns>
        public override DetectedHand GetDetectedHand() { return detectedHand; }

        /// <summary>
        /// Assigns a Leap Hand object to this HandModelBase.
        /// </summary>
        /// <param name="hand"></param>
        public override void SetDetectedHand(DetectedHand hand) { detectedHand = hand; }
    
        #endregion
        
        #region Hand Binder Logic

        /// <summary>
        /// Called once per frame when the LeapProvider calls the event OnUpdateFrame.
        /// Update the BoundGameobjects so that the positions and rotations match that of the leap hand
        /// </summary>
        public override void UpdateHand()
        {
            if (IsTracked && boundHand != null)
            {
                TransformWrist();
                TransformFingerBones();
            }
        }

        /// <summary>
        /// Set the wrist into the correct position and rotation
        /// </summary>
        void TransformWrist()
        {
            
        }

        public bool useMetaBone = false;
        /// <summary>
        /// Set a bone of the hand model into the correct position and rotation
        /// </summary>
        void TransformFingerBones()
        {
            
            for (int fingerIndex = 0; fingerIndex < boundHand.fingers.Length; fingerIndex++)
            {
                if (boundHand.fingers[fingerIndex] == null)
                {
                    continue;
                }

                var finger = boundHand.fingers[fingerIndex];
    
                for (int boneIndex = 0; boneIndex < finger.boundBones.Length; boneIndex++)
                {
                    if (!useMetaBone)
                    {
                        if ((FingerType)fingerIndex != FingerType.TYPE_THUMB &&
                            (BoneType)boneIndex == BoneType.TYPE_METACARPAL)
                        {
                            continue;
                        }
                    }
                    
                    if (detectedHand.fingers[fingerIndex].bones[boneIndex] == null)
                    {
                        continue;
                    }
                    var boundBone = finger.boundBones[boneIndex];
                    var detectedBone = detectedHand.fingers[fingerIndex].bones[boneIndex];
                    var boundTransform = boundBone.boundTransform;

                    //Continue if the user has not defined a transform for this finger
                    if (boundBone.boundTransform == null)
                    {
                        continue;
                    }
                    /*Debug.Log("Transform bones:" + detectedBone.rotation.ToString());*/
                    boundTransform.transform.localRotation = Quaternion.Euler(boundTransform.transform.localRotation.eulerAngles.x, detectedBone.rotation.eulerAngles.y, detectedBone.rotation.eulerAngles.z);
                }
            }
        }

        #endregion
        
        #region Cleanup

        /// <summary>
        /// When this component is removed from an object, ensure the hand is reset back to how it started
        /// </summary>
        private void OnDestroy()
        {
            ResetHand();
        }

        /// <summary>
        /// Reset is called when the user hits the Reset button in the Inspector's context menu or when adding the component the first time.
        /// </summary>
        private void Reset()
        {
            //Return if we already have assigned base transforms
            if (defaultHandPose == null || defaultHandPose.Length == 0)
            {
                Debug.Log("Hand binder reset!");
                SetDefaultHandPose();
            }
            else
            {
                ResetHand();
            }
        }

        void SetDefaultHandPose()
        {
            //Store all children transforms so the user has the ability to reset back to a default pose
            var allChildren = new List<Transform>();
            allChildren.Add(transform);
            allChildren.AddRange(HandBinderAutoBinder.GetAllChildren(transform));

            var baseTransforms = new List<SerializedTransform>();
            foreach (var child in allChildren)
            {
                var serializedTransform = new SerializedTransform();
                serializedTransform.reference = child.gameObject;
                serializedTransform.transform = new TransformStore();
                serializedTransform.transform.position = child.localPosition;
                serializedTransform.transform.rotation = child.localRotation.eulerAngles;
                serializedTransform.transform.scale = child.localScale;
                baseTransforms.Add(serializedTransform);
            }
            defaultHandPose = baseTransforms.ToArray();
        }

        public void PrintTransform()
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
        
        /// <summary>
        /// Reset the boundGameobjects back to the default pose given by DefaultHandPose.
        /// </summary>
        public void ResetHand(bool forceReset = false)
        {
            if (this == null) return;

            if (defaultHandPose == null)
            {
                Debug.Log("Default hand pose is missing, please reset the 3D model and rebind the hand");
                return;
            };

            if (boundHand.startScale != Vector3.zero)
            {
                transform.localScale = boundHand.startScale;
            }

            for (int i = 0; i < defaultHandPose.Length; i++)
            {

                var baseTransform = defaultHandPose[i];
                if (baseTransform != null && baseTransform.reference != null)
                {
                    baseTransform.reference.transform.localPosition = baseTransform.transform.position;
                    baseTransform.reference.transform.localRotation = Quaternion.Euler(baseTransform.transform.rotation);

                    if (baseTransform.transform.scale == Vector3.zero)
                    {
                        baseTransform.transform.scale = baseTransform.reference.transform.localScale;
                    }

                    baseTransform.reference.transform.localScale = baseTransform.transform.scale;
                }
            }
        }

        #endregion
    }
}