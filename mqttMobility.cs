using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class mqttMobility : MonoBehaviour
{
    public mqttManager _mqttManager;

    public float mob_Pub_Vx = 0;
    public float mob_Pub_Vy = 0;
    public float mob_Pub_W = 0;
    public float mob_Feedback_Vx = 0;
    public float mob_Feedback_Vy = 0;
    public float mob_Feedback_W = 0;

    private string mob_preSubMessage;
    private string mob_prePubMessage;

    private bool _zeroHandle = false;

    private void Start()
    {
        _mqttManager = this.gameObject.GetComponent<mqttManager>();

        _mqttManager._subTopic = _mqttManager._mobilitySubTopic;
        _mqttManager._pubTopic = _mqttManager._mobilityPubTopic;

        mob_preSubMessage = _mqttManager._mobilitySubMessage;
        mob_prePubMessage = _mqttManager._mobilityPubMessage;
    }

    private void Update()
    {
        MobilitySplitMessage();
        MobilityCobineMessage();

        //Test Publish
        //if (_mqttManager._pubTopic == _mqttManager._mobilityPubTopic)
        //{
        //    if (Input.GetKeyDown("w"))
        //    {
        //        mob_Pub_Vy = 1;
        //    }
        //    if (Input.GetKeyDown("x"))
        //    {
        //        mob_Pub_Vy = -1;
        //    }
        //    if (Input.GetKeyDown("s"))
        //    {
        //        mob_Pub_Vx = 0;
        //        mob_Pub_Vy = 0;
        //    }
        //    if (Input.GetKeyDown("a"))
        //    {
        //        mob_Pub_Vx = -1;
        //    }
        //    if (Input.GetKeyDown("d"))
        //    {
        //        mob_Pub_Vx = 1;
        //    }
        //}

    }

    // Split Massages
    void MobilitySplitMessage()
    {
        if (_mqttManager._mobilitySubMessage != mob_preSubMessage)
        {
            string[] mobilityList = _mqttManager._mobilitySubMessage.Split();
            if (mobilityList.Length == 3)
            {
                mob_Feedback_Vx = float.Parse(mobilityList[0]);
                mob_Feedback_Vy = float.Parse(mobilityList[1]);
                mob_Feedback_W = float.Parse(mobilityList[2]);
            }

            mob_preSubMessage = _mqttManager._mobilitySubMessage;

            //Debug.Log("Vx: " + mob_Feedback_Vx + " | Vy: " + mob_Feedback_Vy + " | W: " + mob_Feedback_W);
        }

    }

    void MobilityCobineMessage()
    {
        if (mob_Pub_Vx != _mqttManager._mobilityPubMessage[0] || mob_Pub_Vy != _mqttManager._mobilityPubMessage[1] || mob_Pub_W != _mqttManager._mobilityPubMessage[2])
        {
            _mqttManager._mobilityPubMessage = mob_Pub_Vx + " " + mob_Pub_Vy + " " + mob_Pub_W;
            MobilityPublishMessage();
        }

    }

    void MobilityPublishMessage()
    {
        if (_mqttManager._pubTopic == _mqttManager._mobilityPubTopic)
        {
            // Control same message
            if (_mqttManager._pubMessage != _mqttManager._mobilityPubMessage)
            {
                _mqttManager._pubMessage = _mqttManager._mobilityPubMessage;
                _mqttManager.client_MqttMsgPublishSent();

                _zeroHandle = true;
            }

            if (_mqttManager._mobilityPubMessage == "0 0 0" && _zeroHandle == true)
            {
                _mqttManager._pubMessage = "0 0 0";
                _mqttManager.client_MqttMsgPublishSent();

                _zeroHandle = false;
            }
            
            
        }
        
    }

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
