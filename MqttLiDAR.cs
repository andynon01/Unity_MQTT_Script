using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MqttLiDAR : MonoBehaviour
{
    public MqttManager _mqttManager;

    [Header("LiDAR")]
    public string lidar_N;
    public string lidar_NE;
    public string lidar_E;
    public string lidar_SE;
    public string lidar_S;
    public string lidar_SW;
    public string lidar_W;
    public string lidar_NW;

    private LiDARData _lidarData;

    private string lidar_preSubMessage;

    public class LiDARData
    {
        public string n;
        public string ne;
        public string e;
        public string se;
        public string s;
        public string sw;
        public string w;
        public string nw;

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
        _lidarData.n = "ok";
        _lidarData.ne = "ok";
        _lidarData.e = "ok";
        _lidarData.se = "ok";
        _lidarData.s = "ok";
        _lidarData.sw = "ok";
        _lidarData.w = "ok";
        _lidarData.nw = "ok";

        // Test
        _mqttManager._cameraPubMessage = JsonUtility.ToJson(_lidarData);
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
            lidar_N = _lidarData.n;
            lidar_NE = _lidarData.ne;
            lidar_E = _lidarData.e;
            lidar_SE = _lidarData.se;
            lidar_S = _lidarData.s;
            lidar_SW = _lidarData.sw;
            lidar_W = _lidarData.w;
            lidar_NW = _lidarData.nw;

            lidar_preSubMessage = _mqttManager._lidarSubMessage;
        }
    }
    
}
