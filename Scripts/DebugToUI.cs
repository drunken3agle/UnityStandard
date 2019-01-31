using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
	Displays Unity's Debug.Log() output in a UI text field
*/
[RequireComponent(typeof(Text))]
public class DebugToUI : MonoBehaviour {

	[Tooltip("Maximum number of messages displayed"), Range(1, 30), SerializeField] 
	private int MaxMessages = 15;

	[Tooltip("Active duration after a message is reveiced, Set to 0 to disable fading"), Range(0.0f, 15.0f), SerializeField] 
	private float FadeTime = 5.0f;
	private float TimeSinceLastMessage = 0.0f;

	private Queue<string> LogMessages;
	private Text DebugOutput;

	void Start() {
		DebugOutput = GetComponent<Text>();
		LogMessages = new Queue<string>(MaxMessages);
		
		Application.logMessageReceived += ReceiveMessage;
	}

	void OnDisable() {
		Application.logMessageReceived -= ReceiveMessage;
	}

	void Update() {
		TimeSinceLastMessage += Time.deltaTime;
		if(TimeSinceLastMessage > FadeTime && FadeTime > 0.0000001f) { // Don't fadeout if timer is zero (floating point error)
			DebugOutput.enabled = false;
		}
	}

	private void UpdateUI() {
		string logString = "";
		foreach (string message in LogMessages) {
			logString += message;
		}

		DebugOutput.text = logString;
	}

	private void ReceiveMessage(string logString, string stackTrace, LogType type) {
		DebugOutput.enabled = true; // Active if fadeout has happened
		TimeSinceLastMessage = 0.0f;

		// Delete old messages before memory needs to be reallocated
		if (LogMessages.Count == MaxMessages) { 
			LogMessages.Dequeue();
		}

		string outputLine = "\n[" + type + "]: " + logString;

		switch (type) {
			case LogType.Exception: // Add stacktrace for exceptions
				outputLine += "\n" + stackTrace;
				break;
			default:
				break;
		}

		LogMessages.Enqueue(outputLine);

		UpdateUI();
	}
}
