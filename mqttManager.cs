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

public class mqttManager : MonoBehaviour
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
	[SerializeField] public string _subTopic;
	[SerializeField] private string[] _subTopicList;
	[Tooltip("Subscribe Quality of Service Level")]
	[SerializeField][Range(0, 2)] private int _subQos;
	private byte _subQoSLevel;
	private string _unsubTopic;

	[Header("Publish configuration")]
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
	public string _mobilitySubMessage = "0 0 0";
	[HideInInspector] public string _mobilitySubTopic = "RobotToUnity/mobility";
	public string _mobilityPubMessage = "0 0 0";
	[HideInInspector] public string _mobilityPubTopic = "RobotToUnity/mobility";


	// Use this for initialization
	void Start()
	{
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

        //Test Publish
        //if (_pubMessage != null)
        //{
        //	if (Input.GetKeyDown("p"))
        //	{
        //		client_MqttMsgPublishSent();
        //	}
        //	if (Input.GetKeyDown("c"))
        //	{
        //		ClearLog();
        //	}
        //}
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
		if (_subTopic != null)
        {
			// Subscribe to the topic 
			client.Subscribe(new string[] { _subTopic }, new byte[] { _subQoSLevel });
			Debug.Log("Subscribe Topic : " + _subTopic + " QoS : " + _subQos);
		}

		if (_subTopicList.Length > 0)
        {
			for (int i = 0; i < _subTopicList.Length; i++)
            {
				client.Subscribe(new string[] { _subTopicList[i] }, new byte[] { _subQoSLevel });
				Debug.Log("Subscribe Topic : " + _subTopicList[i] + " QoS : " + _subQos);
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
		
	}

	public void client_MqttMsgPublishSent()
    {
		if (_isStamp)
		{
			Debug.Log("publishing...");
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
		if (_subQos == 0) { _subQoSLevel = MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE; }
		if (_subQos == 1) { _subQoSLevel = MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE; }
		if (_subQos == 2) { _subQoSLevel = MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE; }
	}

    void UnsubscribeTopic()
    {
        if (_unsubTopic != null)
        {
            client.Unsubscribe(new string[] { _unsubTopic });
        }

    }

    void ClearLog()
	{
		var assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
		var type = assembly.GetType("UnityEditor.LogEntries");
		var method = type.GetMethod("Clear");
		method.Invoke(new object(), null);
	}
}
