using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MqttGripper : MonoBehaviour
{
    public MqttManager _mqttManager;

    [Header("Gripper Status")]
    public string gripper_Pub_Status;

    public GripperData gripper_Pub_json;

    public class GripperData
    {
        public string gripper_status;
    }

    private void Start()
    {
        _mqttManager = this.gameObject.GetComponent<MqttManager>();

        gripper_Pub_json = new GripperData();
    }
}
