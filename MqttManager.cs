using UnityEngine;
using UnityEngine.Serialization;
using System.Collections;
using System.Net;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using uPLibrary.Networking.M2Mqtt.Utility;
using uPLibrary.Networking.M2Mqtt.Exceptions;
using System;
using System.Reflection;

public class MqttManager : MonoBehaviour
{
	private MqttClient client;

	enum AddressFormat
	{
		_ipAddress,
		_domainAddress,
	};

	[Header("MQTT broker configuration")]
	[Tooltip("Address Format")]
	[SerializeField] private AddressFormat _addressFormat;
	[Tooltip("IP address or URL of the host running the broker")]
	[SerializeField] private string _brokerAddress = "127.0.0.1";
	[Tooltip("Port where the broker accepts connections")]
	[SerializeField] private int _brokerPort = 1883;

	[Header("[On Start only] Subscribe configuration")]
	[Tooltip("Subscribe to the topic")]
	[SerializeField] public string[] _subTopicList;
	[Tooltip("Lenght on this List must equal to SubTopicList")]
	[SerializeField][Range(0, 2)] public int[] _subQosList;
	private byte _subQoSLevel;
	private string _unsubTopic;

	[Header("Publish configuration")]
	public bool _allowPublish = false;
	[Tooltip("Publish to the topic")]
	[SerializeField] public string _pubTopic;
	[Tooltip("Publish Quality of Service Level")]
	[SerializeField][Range(0, 2)] private int _pubQos;
	private byte _pubQoSLevel;
	private int _pubQos_Previous;
	[Tooltip("Retained Message")]
	[SerializeField] private bool _isRetain = false;
	[Tooltip("Publish Messages")]
	[SerializeField] public string _pubMessage = "Test Messages";

	[Header("Data Tranfer Frequency")]
	[Tooltip("Send/Recieve Frequency [Hz]")]
	public int _frequency = 100;
	[HideInInspector] public bool _isStamp;
	private float _timeStamp;
	
	[Header("Mobility")]
	public string _mobilitySubTopic = "mobot/unity/mobility";
	public string _mobilitySubMessage;
	public string _mobilityPubTopic;
	public string _mobilityPubMessage;
	
	[Header("LiDAR")]
	public string _lidarSubTopic;
	public string _lidarSubMessage;

	[Header("Manipulator")]
	public string _manipulatorSubTopic;
	public string _manipulatorSubMessage;
	public string _manipulatorPubTopic;
	public string _manipulatorPubMessage;

	[Header("Gripper")]
	public string _gripperSubTopic;
	public string _gripperSubMessage;
	public string _gripperPubTopic;
	public string _gripperPubMessage;

	[Header("Camera")]
	public string _cameraSubTopic;
	public string _cameraSubMessage;
	public string _cameraPubTopic;
	public string _cameraPubMessage;

	private void Start()
	{
        // Set Subscribe Topic
        _mobilitySubTopic = "mobot/unity/mobility";
        _mobilityPubTopic = "unity/mobot/mobility";

        _lidarSubTopic = "mobot/unity/lidar";

		_manipulatorSubTopic = "mobot/unity/manipulator";
		_manipulatorPubTopic = "unity/mobot/manipulator";

		_gripperSubTopic = "mobot/unity/gripper";
		_gripperPubTopic = "unity/mobot/gripper";

		_cameraSubTopic = "mobot/unity/camera";
		_cameraPubTopic = "unity/mobot/camera";

		_subTopicList[0] = _mobilitySubTopic;
		_subTopicList[1] = _lidarSubTopic;
		_subTopicList[2] = _manipulatorSubTopic;
		_subTopicList[3] = _gripperSubTopic;
		_subTopicList[4] = _cameraSubTopic;

		// Time Stamp
		_timeStamp = 0;

		// Add Check Variable
		_pubQos_Previous = _pubQos;

		// Tranform QoS in Inspector to QoS Level in MQTT Library
		TranformQoSLevel();
		CreateMQTTClient();
		SubscribeToTopic();

	}

    private void FixedUpdate()
    {
		// Time Counter & Tranfer Data Frequency
		if (Time.fixedTime - _timeStamp >= 1 / (float)_frequency)
		{
			_isStamp = true;
			_timeStamp = Time.fixedTime;

		}
		else
		{
			_isStamp = false;
		}

	}

    private void Update()
	{
		UpdateFromInspector();

    }

	void CreateMQTTClient()
    {
		// Create client instance
		// IP Address
		if (_addressFormat == AddressFormat._ipAddress)
        {
			client = new MqttClient(IPAddress.Parse(_brokerAddress), _brokerPort, false, null);
		}
		// Domain name
		if (_addressFormat == AddressFormat._domainAddress)
        {
			client = new MqttClient( _brokerAddress , _brokerPort, false, null);
        }

		// Register to message received 
		client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;

		string clientId = Guid.NewGuid().ToString();
		client.Connect(clientId);
	}

	public void SubscribeToTopic()
    {
		if (_subTopicList.Length > 0)
        {
			for (int i = 0; i < _subTopicList.Length; i++)
            {
				if (_subQosList[i] == 0) { _subQoSLevel = MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE; }
				if (_subQosList[i] == 1) { _subQoSLevel = MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE; }
				if (_subQosList[i] == 2) { _subQoSLevel = MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE; }

				client.Subscribe(new string[] { _subTopicList[i] }, new byte[] { _subQoSLevel });
				Debug.Log("Subscribe Topic : " + _subTopicList[i] + " QoS : " + _subQosList[i]);
			}
        }
	}

	public void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
	{
		Debug.Log("Received : " + System.Text.Encoding.UTF8.GetString(e.Message) + " | Topic : " + e.Topic);

		if (e.Topic == _mobilitySubTopic)
		{
			_mobilitySubMessage = System.Text.Encoding.UTF8.GetString(e.Message);
		}

		if (e.Topic == _lidarSubTopic)
        {
			_lidarSubMessage = System.Text.Encoding.UTF8.GetString(e.Message);
		}
		
	}

	public void client_MqttMsgPublishSent()
    {
		if (_isStamp && _allowPublish)
		{
			client.Publish(_pubTopic, System.Text.Encoding.UTF8.GetBytes(_pubMessage), _pubQoSLevel, _isRetain);

			Debug.Log("published");
		}

	}

	void UpdateFromInspector()
    {
		if (_pubQos_Previous != _pubQos)
		{
			TranformQoSLevel();
			_pubQos_Previous = _pubQos;
		}

        UnsubscribeTopic();
    }

	// Tranform QoS in Inspector to QoS Level in MQTT Library
	void TranformQoSLevel()
	{
		if (_pubQos == 0) { _pubQoSLevel = MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE; }
		if (_pubQos == 1) { _pubQoSLevel = MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE; }
		if (_pubQos == 2) { _pubQoSLevel = MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE; }
	}

    void UnsubscribeTopic()
    {
        if (_unsubTopic != null)
        {
            client.Unsubscribe(new string[] { _unsubTopic });
        }

    }

    public void ClearLog()
	{
		var assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
		var type = assembly.GetType("UnityEditor.LogEntries");
		var method = type.GetMethod("Clear");
		method.Invoke(new object(), null);
	}
}
