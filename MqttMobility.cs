using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MqttMobility : MonoBehaviour
{
    public MqttManager _mqttManager;

    [Header("Wheel")]
    public float mobi_Pub_Vx = 0;
    public float mobi_Pub_Vy = 0;
    public float mobi_Pub_W = 0;
    public float mobi_Feedback_Vx = 0;
    public float mobi_Feedback_Vy = 0;
    public float mobi_Feedback_W = 0;

    private MobilityData mobi_Pub_json;
    private MobilityData mobi_Feedback_json;

    private string mobi_preSubMessage;

    private bool _zeroHandle = false;

    public class MobilityData
    {
        public float mobi_vx;
        public float mobi_vy;
        public float mobi_w;
    }

    private void Start()
    {
        _mqttManager = this.gameObject.GetComponent<MqttManager>();

        mobi_preSubMessage = _mqttManager._mobilitySubMessage;

        mobi_Pub_json = new MobilityData();
        mobi_Pub_json.mobi_vx = mobi_Pub_Vx;
        mobi_Pub_json.mobi_vy = mobi_Pub_Vy;
        mobi_Pub_json.mobi_w = mobi_Pub_W;
        _mqttManager._mobilityPubMessage = JsonUtility.ToJson(mobi_Pub_json);

        mobi_Feedback_json = new MobilityData();
        mobi_Feedback_json.mobi_vx = mobi_Feedback_Vx;
        mobi_Feedback_json.mobi_vy = mobi_Feedback_Vy;
        mobi_Feedback_json.mobi_w = mobi_Feedback_W;
        _mqttManager._mobilitySubMessage = JsonUtility.ToJson(mobi_Feedback_json);
    }

    private void Update()
    {
        MobilitySplitMessage();
        MobilityCobineMessage();

    }

    // Split Messages
    void MobilitySplitMessage()
    {
        if (_mqttManager._mobilitySubMessage != mobi_preSubMessage)
        {
            // String Read Method
            /*
            string[] mobilityList = _mqttManager._mobilitySubMessage.Split();
            if (mobilityList.Length == 3)
            {
                mob_Feedback_Vx = float.Parse(mobilityList[0]);
                mob_Feedback_Vy = float.Parse(mobilityList[1]);
                mob_Feedback_W = float.Parse(mobilityList[2]);
            }
            */

            // JSON Read Method
            mobi_Feedback_json = JsonUtility.FromJson<MobilityData>(_mqttManager._mobilitySubMessage);
            mobi_Feedback_Vx = mobi_Feedback_json.mobi_vx;
            mobi_Feedback_Vy = mobi_Feedback_json.mobi_vy;
            mobi_Feedback_W = mobi_Feedback_json.mobi_w;

            mobi_preSubMessage = _mqttManager._mobilitySubMessage;

            //Debug.Log("Vx: " + mob_Feedback_Vx + " | Vy: " + mob_Feedback_Vy + " | W: " + mob_Feedback_W);
        }

    }

    // Combine Messages
    void MobilityCobineMessage()
    {
        // String Combine
        /*
        if (mob_Pub_Vx != _mqttManager._mobilityPubMessage[0] || mob_Pub_Vy != _mqttManager._mobilityPubMessage[1] || mob_Pub_W != _mqttManager._mobilityPubMessage[2])
        {
            _mqttManager._mobilityPubMessage = mob_Pub_Vx + " " + mob_Pub_Vy + " " + mob_Pub_W;
            MobilityPublishMessage();
        }
        */

        // JSON Combine
        if (mobi_Pub_Vx != mobi_Pub_json.mobi_vx || mobi_Pub_Vy != mobi_Pub_json.mobi_vy || mobi_Pub_W != mobi_Pub_json.mobi_w)
        {
            mobi_Pub_json.mobi_vx = mobi_Pub_Vx;
            mobi_Pub_json.mobi_vy = mobi_Pub_Vy;
            mobi_Pub_json.mobi_w = mobi_Pub_W;
            _mqttManager._mobilityPubMessage = JsonUtility.ToJson(mobi_Pub_json);

            MobilityPublishMessage();
        }

    }

    void MobilityPublishMessage()
    {
        // Set Pub Topic
        _mqttManager._pubTopic = _mqttManager._mobilityPubTopic;

        // Check Pub Topic
        if (_mqttManager._pubTopic == _mqttManager._mobilityPubTopic)
        {
            // Control same message
            if (_mqttManager._pubMessage != _mqttManager._mobilityPubMessage)
            {
                _mqttManager._pubMessage = _mqttManager._mobilityPubMessage;
                _mqttManager.client_MqttMsgPublishSent();

                _zeroHandle = true;
            }

            // no Pub Zero handling
            if (_mqttManager._mobilityPubMessage == "0 0 0" && _zeroHandle == true)
            {
                // String Pub Method
                /*
                mob_Pub_Vx = 0;
                mob_Pub_Vy = 0;
                mob_Pub_W = 0;
                _mqttManager.client_MqttMsgPublishSent();
                */

                // JSON Pub Method
                mobi_Pub_json.mobi_vx = 0;
                mobi_Pub_json.mobi_vy = 0;
                mobi_Pub_json.mobi_w = 0;
                _mqttManager._mobilityPubMessage = JsonUtility.ToJson(mobi_Pub_json);
                _mqttManager.client_MqttMsgPublishSent();

                _zeroHandle = false;
            }
            
            
        }
        
    }

    // Player Controller
    public void MoveMode1(InputAction.CallbackContext context)
    {
        Vector2 movement = context.ReadValue<Vector2>();
        mobi_Pub_Vx = movement.y;
        mobi_Pub_Vy = movement.x;
    }

    public void RotateMode1(InputAction.CallbackContext context)
    {
        float angularVelocity = context.ReadValue<float>();
        mobi_Pub_W = angularVelocity;
    }

    public void StopMove(InputAction.CallbackContext context)
    {
        mobi_Pub_Vx = 0;
        mobi_Pub_Vy = 0;
        mobi_Pub_W = 0;
        _mqttManager.client_MqttMsgPublishSent();

        Debug.Log("Stop");
    }

}
