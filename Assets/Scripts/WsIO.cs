using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NativeWebSocket;
using Reachy;

public class WsIO : MonoBehaviour
{
    public string url = "ws://127.0.0.1:6171";
    public ReachyController reachy;

    WebSocket websocket;
    SerializableCommands commands;
    bool needUpdate;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine("Connect");
    }

    async void Connect()
    {
        websocket = new WebSocket(url);

        websocket.OnOpen += () =>
        {
            Debug.Log("Connection open!");
        };
        websocket.OnError += (e) =>
        {
            Debug.Log("Error! " + e);
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("Connection closed!");
        };

        websocket.OnMessage += (bytes) =>
        {
            var message = System.Text.Encoding.UTF8.GetString(bytes);

            needUpdate = true;
            commands = JsonUtility.FromJson<SerializableCommands>(message);
            websocket.SendText(JsonUtility.ToJson(reachy.GetCurrentState()));
        };

        await websocket.Connect();
    }

    // Update is called once per frame
    void Update()
    {
        if (needUpdate)
        {
            reachy.HandleCommand(commands);
            needUpdate = false;
        }
    }
}
