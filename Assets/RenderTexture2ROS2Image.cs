// Copyright 2019-2021 Robotec.ai.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

// RenderTexture2ROS2Image.cs
// Author: Ar-Ray (2022)
// https://github.com/Ar-Ray-code

using System.Collections;
using UnityEngine;
using System.IO;
using System;

namespace ROS2
{
    [RequireComponent(typeof(ROS2UnityComponent))]
    public class RenderTexture2ROS2Image : MonoBehaviour
    {
        // Start is called before the first frame update
        private ROS2UnityComponent ros2Unity;
        private ROS2Node ros2Node;
        private IPublisher<sensor_msgs.msg.Image> image_pub;

        public RenderTexture renderTexture;

        // width and height fit automatically to the renderTexture
        private Int32 width;
        private Int32 height;

        // Initialize ROS2
        void Start()
        {
            ros2Unity = GetComponent<ROS2UnityComponent>();
        }

        // Main loop
        void Update()
        {   
            if (ros2Unity.Ok())
            {
                if (ros2Node == null)
                {
                    ros2Node = ros2Unity.CreateNode("RenderTexture2ROS2Image");
                    image_pub = ros2Node.CreatePublisher<sensor_msgs.msg.Image>("image_raw");
                }

                sensor_msgs.msg.Image image_msg = new sensor_msgs.msg.Image();

                Color[] pixels;
                pixels = CreateTexture2D(renderTexture);

                image_msg.Height = (UInt32)(height);
                image_msg.Width = (UInt32)(width);
                image_msg.Encoding = "rgb8";
                image_msg.Is_bigendian = (byte)(0);
                image_msg.Step = (UInt32)(width * 3); // 3byte width is cols

                var data_array_byte = new byte[width * height * 3];
                var height_1 = 0;
                for (int i = 0; i < height; i++)
                {
                    for (int j = 0; j < width; j++)
                    {
                        height_1 = height - 1 - i;
                        data_array_byte[i * width * 3 + j * 3 + 0] = (byte)(pixels[height_1 * width + j].r * 255);
                        data_array_byte[i * width * 3 + j * 3 + 1] = (byte)(pixels[height_1 * width + j].g * 255);
                        data_array_byte[i * width * 3 + j * 3 + 2] = (byte)(pixels[height_1 * width + j].b * 255);
                    }
                }
                image_msg.Data = data_array_byte;
                image_pub.Publish(image_msg);
            }
        }

        // Input: RenderTexture
        // Output: Color[]
        Color[] CreateTexture2D(RenderTexture rt)
        {
            width = rt.width;
            height = rt.height;

            var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            var oldActive = RenderTexture.active;
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();

            var colors = tex.GetPixels();
            RenderTexture.active = oldActive;

            Destroy(tex);
            return colors;
        }
    }
}