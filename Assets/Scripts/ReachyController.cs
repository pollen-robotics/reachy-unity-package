using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System;

namespace Reachy
{
    [System.Serializable]
    public class Motor
    {
        public string name;
        public GameObject gameObject;
        public float targetPosition;
        public float presentPosition;
    }

    [System.Serializable]
    public struct SerializableMotor
    {
        public string name;
        public float present_position;
    }

    [System.Serializable]
    public struct SerializableState
    {
        public List<SerializableMotor> motors;
        public string left_eye;
        public string right_eye;
    }

    [System.Serializable]
    public struct MotorCommand
    {
        public string name;
        public float goal_position;
    }

    [System.Serializable]
    public struct SerializableCommands
    {
        public List<MotorCommand> motors;
    }

    public class ReachyController : MonoBehaviour
    {
        public Motor[] motors;
        public Camera leftEye, rightEye;
        private Dictionary<string, Motor> name2motor;
        private string leftEyeFrame, rightEyeFrame;

        const int resWidth = 320;
        const int resHeight = 240;

        void Start()
        {
            name2motor = new Dictionary<string, Motor>();

            for (int i = 0; i < motors.Length; i++)
            {
                Motor m = motors[i];
                name2motor[m.name] = m;
            }

            leftEye.targetTexture = new RenderTexture(resWidth, resHeight, 0);
            rightEye.targetTexture = new RenderTexture(resWidth, resHeight, 0);
            StartCoroutine("UpdateCameraData");
        }

        void Update()
        {
            for (int i = 0; i < motors.Length; i++)
            {
                Motor m = motors[i];

                JointController joint = m.gameObject.GetComponent<JointController>();
                joint.RotateTo(m.targetPosition);

                m.presentPosition = joint.GetPresentPosition();
            }
        }

        IEnumerator UpdateCameraData()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();
                leftEyeFrame = GetEyeRawTextureData(leftEye);
                rightEyeFrame = GetEyeRawTextureData(rightEye);
            }
        }

        string GetEyeRawTextureData(Camera camera)
        {
            Texture2D texture = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
            RenderTexture.active = camera.targetTexture;
            texture.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
            texture.Apply();

            return Convert.ToBase64String(texture.EncodeToJPG());
        }

        void SetMotorTargetPosition(string motorName, float targetPosition)
        {
            name2motor[motorName].targetPosition = targetPosition;
        }

        public void HandleCommand(SerializableCommands commands)
        {
            for (int i = 0; i < commands.motors.Capacity; i++) 
            {
                MotorCommand cmd = commands.motors[i];
                SetMotorTargetPosition(cmd.name, cmd.goal_position);
            }
        }

        public SerializableState GetCurrentState()
        {
            
            SerializableState currentState = new SerializableState() { 
                motors = new List<SerializableMotor>(), 
                left_eye=leftEyeFrame,
                right_eye=rightEyeFrame,
            };

            for (int i = 0; i < motors.Length; i++)
            {
                Motor m = motors[i];
                currentState.motors.Add(new SerializableMotor() { name=m.name,  present_position=m.presentPosition});
            }

            return currentState;
        }
    }
}