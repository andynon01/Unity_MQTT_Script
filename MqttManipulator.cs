using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MqttManipulator : MonoBehaviour
{
    public MqttManager _mqttManager;

    [Header("Manipulator Mode")]
    [Range(1,2)] public int mani_Mode;

    [Header("Arm Position")]
    public float mani_Pub_x;
    public float mani_Pub_y;
    public float mani_Pub_z;
    public float mani_Feedback_x;
    public float mani_Feedback_y;
    public float mani_Feedback_z;

    private ManipulatorData mani_Pub_json;
    private ManipulatorData mani_Feedback_json;

    private string mani_preSubMessage;

    public class ManipulatorData
    {
        public int mani_mode;
        public float mani_x;
        public float mani_y;
        public float mani_z;
    }

    private void Start()
    {
        _mqttManager = this.gameObject.GetComponent<MqttManager>();

        mani_preSubMessage = _mqttManager._manipulatorSubMessage;

        mani_Pub_json = new ManipulatorData();
        mani_Pub_json.mani_mode = 1;
        mani_Pub_json.mani_x = mani_Pub_x;
        mani_Pub_json.mani_y = mani_Pub_y;
        mani_Pub_json.mani_z = mani_Pub_z;

        mani_Feedback_json = new ManipulatorData();
        mani_Feedback_json.mani_mode = 1;
        mani_Feedback_json.mani_x = mani_Feedback_x;
        mani_Feedback_json.mani_y = mani_Feedback_y;
        mani_Feedback_json.mani_z = mani_Feedback_z;

    }
}
