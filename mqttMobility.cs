using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mqttMobility : MonoBehaviour
{
    public mqttManager _mqttManager;

    public float mob_Vx = 0;
    public float mob_Vy = 0;
    public float mob_W = 0;
    private string mob_preMessage;

    private void Start()
    {
        _mqttManager = this.gameObject.GetComponent<mqttManager>();

        _mqttManager._subTopic = _mqttManager._mobilityTopic;
        mob_preMessage = _mqttManager._mobilityMessage;
    }

    private void Update()
    {
        MobilitySplitMassage();
    }

    // Split Massages
    void MobilitySplitMassage()
    {
        if (_mqttManager._mobilityMessage != mob_preMessage)
        {
            string[] mobilityList = _mqttManager._mobilityMessage.Split();
            if (mobilityList.Length == 3)
            {
                mob_Vx = float.Parse(mobilityList[0]);
                mob_Vy = float.Parse(mobilityList[1]);
                mob_W = float.Parse(mobilityList[2]);
            }

            mob_preMessage = _mqttManager._mobilityMessage;

            Debug.Log("Vx: " + mob_Vx + " | Vy: " + mob_Vy + " | W: " + mob_W);
        }

    }
}
