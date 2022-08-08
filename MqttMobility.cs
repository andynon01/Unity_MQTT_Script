using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MqttMobility : MonoBehaviour
{
    public MqttManager _mqttManager;

    [Header("Wheel")]
    public float mob_Pub_Vx = 0;
    public float mob_Pub_Vy = 0;
    public float mob_Pub_W = 0;
    public float mob_Feedback_Vx = 0;
    public float mob_Feedback_Vy = 0;
    public float mob_Feedback_W = 0;

    private MobilityData mob_Pub_json;
    private MobilityData mob_Feedback_json;

    private string mob_preSubMessage;

    private bool _zeroHandle = false;

    private void Start()
    {
        _mqttManager = this.gameObject.GetComponent<MqttManager>();

        //_mqttManager._subTopic = _mqttManager._mobilitySubTopic;

        mob_preSubMessage = _mqttManager._mobilitySubMessage;

        mob_Pub_json = new MobilityData();
        mob_Pub_json.vx = mob_Pub_Vx;
        mob_Pub_json.vy = mob_Pub_Vy;
        mob_Pub_json.w = mob_Pub_W;

        mob_Feedback_json = new MobilityData();
        mob_Feedback_json.vx = mob_Feedback_Vx;
        mob_Feedback_json.vy = mob_Feedback_Vy;
        mob_Feedback_json.w = mob_Feedback_W;
    }

    private void Update()
    {
        MobilitySplitMessage();
        MobilityCobineMessage();

    }

    public class MobilityData
    {
        public float vx;
        public float vy;
        public float w;
    }

    // Split Messages
    void MobilitySplitMessage()
    {
        if (_mqttManager._mobilitySubMessage != mob_preSubMessage)
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
            mob_Feedback_json = JsonUtility.FromJson<MobilityData>(_mqttManager._mobilitySubMessage);
            mob_Feedback_Vx = mob_Feedback_json.vx;
            mob_Feedback_Vy = mob_Feedback_json.vy;
            mob_Feedback_W = mob_Feedback_json.w;

            mob_preSubMessage = _mqttManager._mobilitySubMessage;

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
        if (mob_Pub_Vx != mob_Pub_json.vx || mob_Pub_Vy != mob_Pub_json.vy || mob_Pub_W != mob_Pub_json.w)
        {
            mob_Pub_json.vx = mob_Pub_Vx;
            mob_Pub_json.vy = mob_Pub_Vy;
            mob_Pub_json.w = mob_Pub_W;
            _mqttManager._mobilityPubMessage = JsonUtility.ToJson(mob_Pub_json);

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
                mob_Pub_json.vx = 0;
                mob_Pub_json.vy = 0;
                mob_Pub_json.w = 0;
                _mqttManager._mobilityPubMessage = JsonUtility.ToJson(mob_Pub_json);
                _mqttManager.client_MqttMsgPublishSent();

                _zeroHandle = false;
            }
            
            
        }
        
    }

    // Player Controller
    public void MoveMode1(InputAction.CallbackContext context)
    {
        Vector2 movement = context.ReadValue<Vector2>();
        mob_Pub_Vx = movement.y;
        mob_Pub_Vy = movement.x;

        //Debug.Log(movement);
    }

    public void RotateMode1(InputAction.CallbackContext context)
    {
        float angularVelocity = context.ReadValue<float>();
        mob_Pub_W = angularVelocity;

        //Debug.Log(angularVelocity);
    }

    public void StopMove(InputAction.CallbackContext context)
    {
        mob_Pub_Vx = 0;
        mob_Pub_Vy = 0;
        mob_Pub_W = 0;
        _mqttManager.client_MqttMsgPublishSent();

        Debug.Log("Stop");
    }

}
