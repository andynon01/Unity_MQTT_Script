using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MqttLiDAR : MonoBehaviour
{
    public MqttManager _mqttManager;

    [Header("LiDAR Feedback")]
    public string lidar_N = "ok";
    public string lidar_NE = "ok";
    public string lidar_E = "ok";
    public string lidar_SE = "ok";
    public string lidar_S = "ok";
    public string lidar_SW = "ok";
    public string lidar_W = "ok";
    public string lidar_NW = "ok";

    private LiDARData _lidarData;

    private string lidar_preSubMessage;

    public class LiDARData
    {
        public string lidar_q1;
        public string lidar_q2;
        public string lidar_q3;
        public string lidar_q4;
        public string lidar_q5;
        public string lidar_q6;
        public string lidar_q7;
        public string lidar_q8;

    }

    enum LiDAR_Status
    {
        ok,
        beware,
        stop,
    }

    private void Start()
    {
        _mqttManager = this.gameObject.GetComponent<MqttManager>();

        lidar_preSubMessage = _mqttManager._lidarSubMessage;
        
        _lidarData = new LiDARData();
        _lidarData.lidar_q1 = "ok";
        _lidarData.lidar_q2 = "ok";
        _lidarData.lidar_q3 = "ok";
        _lidarData.lidar_q4 = "ok";
        _lidarData.lidar_q5 = "ok";
        _lidarData.lidar_q6 = "ok";
        _lidarData.lidar_q7 = "ok";
        _lidarData.lidar_q8 = "ok";
        _mqttManager._lidarSubMessage = JsonUtility.ToJson(_lidarData);

    }

    private void Update()
    {
        LiDARSplitMessages();

    }

    void LiDARSplitMessages()
    {
        // JSON Read Method
        if (lidar_preSubMessage != _mqttManager._lidarSubMessage)
        {
            _lidarData = JsonUtility.FromJson<LiDARData>(_mqttManager._lidarSubMessage);
            lidar_N = _lidarData.lidar_q1;
            lidar_NE = _lidarData.lidar_q2;
            lidar_E = _lidarData.lidar_q3;
            lidar_SE = _lidarData.lidar_q4;
            lidar_S = _lidarData.lidar_q5;
            lidar_SW = _lidarData.lidar_q6;
            lidar_W = _lidarData.lidar_q7;
            lidar_NW = _lidarData.lidar_q8;

            lidar_preSubMessage = _mqttManager._lidarSubMessage;
        }
    }
    
}
