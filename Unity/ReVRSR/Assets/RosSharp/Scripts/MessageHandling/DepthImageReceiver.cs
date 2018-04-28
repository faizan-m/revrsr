using System;
using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    [RequireComponent(typeof(MeshRenderer))]
    public class DepthImageReceiver : MessageReceiver
    {
        public override Type MessageType { get { return (typeof(SensorImage)); } }

        private SensorImage imageData;
        private bool isMessageReceived;

        private MeshRenderer meshRenderer;
        private Texture2D texture2D;

        private void Awake()
        {
            MessageReception += ReceiveMessage;
        }
        private void Start()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            meshRenderer.material = new Material(Shader.Find("Standard"));
        }
        private void Update()
        {
            if (isMessageReceived)
                ProcessMessage();
        }
        private void ReceiveMessage(object sender, MessageEventArgs e)
        {
            imageData = ((SensorImage)e.Message);
            // Debug.Log(imageData.encoding);
            isMessageReceived = true;
        }

        private void ProcessMessage()
        {
            texture2D = new Texture2D(imageData.width, imageData.height, TextureFormat.RFloat, false);
            texture2D.LoadRawTextureData(imageData.data);
            texture2D.Apply();
            meshRenderer.material.SetTexture("_MainTex", texture2D);
            meshRenderer.material.SetTextureScale("_MainTex", new Vector2(1,-1));
            isMessageReceived = false;
        }
    }
}

