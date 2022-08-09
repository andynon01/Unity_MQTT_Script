using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MqttCamera : MonoBehaviour
{
    public MqttManager _mqttManager;

    [Header("Camera Rotation")]
    public float cam_Pub_yaw;
    public float cam_Pub_pit;

    [Header("Camera Depth")]
    public float cam_Feedback_Depth;

    public CameraData cam_Pub_json;

    public class CameraData
    {
        public float cam_yaw;
        public float cam_pit;
    }

    private void Start()
    {
        _mqttManager = this.gameObject.GetComponent<MqttManager>();

        cam_Pub_json = new CameraData();
    }
}
