using System;
using UnityEngine;

namespace RosSharp.RosBridgeClient
{
    public class ParticleCloudMaker : MonoBehaviour
    {
        public ImageReceiver colorimage;
        public DepthImageReceiver depthimage;

        private Texture2D colorTex;
        private Texture2D depthTex;

    }
}

