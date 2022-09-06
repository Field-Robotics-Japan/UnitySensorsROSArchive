using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;

namespace FRJ.Sensor
{
    [RequireComponent(typeof(FRJ.Sensor.Camera))]
    public class CameraPublisher : MonoBehaviour
    {
        [System.Serializable]
        struct TopicSetting
        {
            public bool publish;
            public string topicName;
            public string frameId;
        }

        [SerializeField] private TopicSetting _image;
        [SerializeField] private TopicSetting _pointcloud;



        private Camera _cam;

        private ROSConnection _ros;
        private PointCloud2Msg _message_pc2;

        private float _timeElapsed = 0f;
        private float _timeStamp = 0f;

        private void Awake()
        {
            _cam = GetComponent<Camera>();
        }

        private void Start()
        {
            _cam.Init();

            // setup ROS
            this._ros = ROSConnection.instance;
            if (_image.publish)
            {

            }
            if (_pointcloud.publish)
            {
                this._ros.RegisterPublisher<PointCloud2Msg>(_pointcloud.topicName);

                this._message_pc2 = new PointCloud2Msg();
                this._message_pc2.header.frame_id = _pointcloud.frameId;
                this._message_pc2.height = 1;
                this._message_pc2.width = (uint)(_cam.resolution.x*_cam.resolution.y);
                this._message_pc2.fields = new PointFieldMsg[4];
                for (int i = 0; i < 4; i++)
                {
                    this._message_pc2.fields[i] = new PointFieldMsg();
                }
                this._message_pc2.fields[0].name = "x";
                this._message_pc2.fields[0].offset = 0;
                this._message_pc2.fields[0].datatype = 7;
                this._message_pc2.fields[0].count = 1;
                this._message_pc2.fields[1].name = "y";
                this._message_pc2.fields[1].offset = 4;
                this._message_pc2.fields[1].datatype = 7;
                this._message_pc2.fields[1].count = 1;
                this._message_pc2.fields[2].name = "z";
                this._message_pc2.fields[2].offset = 8;
                this._message_pc2.fields[2].datatype = 7;
                this._message_pc2.fields[2].count = 1;
                this._message_pc2.fields[3].name = "rgba";
                this._message_pc2.fields[3].offset = 12;
                this._message_pc2.fields[3].datatype = 6;
                this._message_pc2.is_bigendian = false;
                this._message_pc2.point_step = 16;
                this._message_pc2.row_step = (uint)(_cam.resolution.x * _cam.resolution.y * 16);
                this._message_pc2.data = new byte[_cam.resolution.x * _cam.resolution.y * 16];
                this._message_pc2.is_dense = true;
            }
        }

        private void Update()
        {
            this._timeElapsed += Time.deltaTime;
            if (this._timeElapsed > (1f / this._cam.scanRate))
            {
                // Update time
                this._timeElapsed = 0;
                this._timeStamp = Time.time;

                // Update ROS Message
                uint sec = (uint)Math.Truncate(this._timeStamp);
                uint nanosec = (uint)((this._timeStamp - sec) * 1e+9);

                if (this._pointcloud.publish)
                {
                    this._message_pc2.header.stamp.sec = sec;
                    this._message_pc2.header.stamp.nanosec = nanosec;
                    this._message_pc2.data = _cam.data_pc;
                    _ros.Send(this._pointcloud.topicName, this._message_pc2);
                }
            }
        }
    }
}
