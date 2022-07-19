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

	[Header("MQTT broker configuration")]
	[Tooltip("IP address or URL of the host running the broker")]
	[SerializeField] private string _brokerAddress = "127.0.0.1";
	[Tooltip("Port where the broker accepts connections")]
	[SerializeField] private int _brokerPort = 1883;
	
	[Header("[On Start only] Subscribe configuration")]
	[Tooltip("Subscribe to the topic")]
	[SerializeField] private string _subTopic = "test";
	[SerializeField] private string[] _subTopicList;
	[Tooltip("Subscribe Quality of Service Level")]
	[SerializeField][Range(0, 2)] private int _subQos;
	private byte _subQoSLevel;

	[Header("Publish configuration")]
	[Tooltip("Publish to the topic")]
	[SerializeField] private string _publishTopic = "test";
	[Tooltip("Publish Quality of Service Level")]
	[SerializeField][Range(0, 2)] private int _publishQos;
	private byte _publishQoSLevel;
	private int _publishQos_Previous;
	[Tooltip("Retained Message")]
	[SerializeField] private bool _isRetain = false;
	[Tooltip("Publish Messages")]
	[SerializeField] private string _publishMessage = "Test Messages";

	// Use this for initialization
	void Start()
	{
		// Add Check Variable
		_publishQos_Previous = _publishQos;

		// Tranform QoS in Inspector to QoS Level in MQTT Library
		TranformQoSLevel();
		CreateMQTTClient();
		SubscribeToTopic();

	}

	private void Update()
	{
		UpdateFromInspector();

		if (_publishMessage != null)
		{
			if (Input.GetKeyDown("s"))
			{
				client_MqttMsgPublishSent();
			}
			if (Input.GetKeyDown("c"))
			{
				ClearLog();
			}
		}
	}

	void CreateMQTTClient()
    {
		// create client instance 
		client = new MqttClient(IPAddress.Parse(_brokerAddress), _brokerPort, false, null);
		// register to message received 
		client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;

		string clientId = Guid.NewGuid().ToString();
		client.Connect(clientId);
	}

	void SubscribeToTopic()
    {
		if (_subTopic != null)
        {
			// subscribe to the topic 
			client.Subscribe(new string[] { _subTopic }, new byte[] { _subQoSLevel });
			Debug.Log("Subscribe Topic : " + _subTopic + " QoS : " + _subQos);
		}
        else
        {
			client.Subscribe(new string[] { "Null" }, new byte[] { _subQoSLevel });
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

	void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
	{
		Debug.Log("Received: " + System.Text.Encoding.UTF8.GetString(e.Message));
	}

	void client_MqttMsgPublishSent()
    {
		Debug.Log("publishing...");
		client.Publish(_publishTopic, System.Text.Encoding.UTF8.GetBytes( _publishMessage ), _publishQoSLevel, _isRetain);
		Debug.Log("published");
	}

	void UpdateFromInspector()
    {
		if (_publishQos_Previous != _publishQos)
		{
			TranformQoSLevel();
			_publishQos_Previous = _publishQos;
		}
	}

	// Tranform QoS in Inspector to QoS Level in MQTT Library
	void TranformQoSLevel()
	{
		if (_publishQos == 0) { _publishQoSLevel = MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE; }
		if (_publishQos == 1) { _publishQoSLevel = MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE; }
		if (_publishQos == 2) { _publishQoSLevel = MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE; }
		if (_subQos == 0) { _subQoSLevel = MqttMsgBase.QOS_LEVEL_AT_MOST_ONCE; }
		if (_subQos == 1) { _subQoSLevel = MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE; }
		if (_subQos == 2) { _subQoSLevel = MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE; }
	}

	void ClearLog()
	{
		var assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
		var type = assembly.GetType("UnityEditor.LogEntries");
		var method = type.GetMethod("Clear");
		method.Invoke(new object(), null);
	}
}
