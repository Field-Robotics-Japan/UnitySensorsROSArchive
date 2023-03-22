using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;

namespace FRJ.Sensor
{
    [RequireComponent(typeof(DCamera))]
    public class DCameraPublisher : MonoBehaviour
    {
        [SerializeField]
        private string _topic;
        [SerializeField]
        private string _frameId;

        private DCamera _dcam;

        private ROSConnection _ros;
        private PointCloud2Msg _message;

        private float _timeElapsed = 0f;
        private float _timeStamp = 0f;

        private void Awake()
        {
            _dcam = GetComponent<DCamera>();
        }

        private void Start()
        {
            _ros = ROSConnection.GetOrCreateInstance();
            _ros.RegisterPublisher<PointCloud2Msg>(_topic);

            _message = new PointCloud2Msg();
            _message.header.frame_id = _frameId;
            _message.height = 1;
            _message.width = (uint)(_dcam.resolution.x*_dcam.resolution.y);
            _message.fields = new PointFieldMsg[4];
            for(int i = 0; i < 4; i++) _message.fields[i] = new PointFieldMsg();
            _message.fields[0].name = "x";
            _message.fields[0].offset = 0;
            _message.fields[0].datatype = 7;
            _message.fields[0].count = 1;
            _message.fields[1].name = "y";
            _message.fields[1].offset = 4;
            _message.fields[1].datatype = 7;
            _message.fields[1].count = 1;
            _message.fields[2].name = "z";
            _message.fields[2].offset = 8;
            _message.fields[2].datatype = 7;
            _message.fields[2].count = 1;
            _message.fields[3].name = "rgba";
            _message.fields[3].offset = 12;
            _message.fields[3].datatype = 6;
            _message.is_bigendian = false;
            _message.point_step = 16;
            _message.row_step = (uint)(_dcam.resolution.x*_dcam.resolution.y*16);
            _message.data = new byte[(uint)(_dcam.resolution.x*_dcam.resolution.y*16)];
            _message.is_dense = true;
        }

        private void Update()
        {
            _timeElapsed += Time.deltaTime;
            if (_timeElapsed > (1f / _dcam.scanRate))
            {
                _timeElapsed = 0;
                _timeStamp = Time.time;

                uint sec = (uint)Math.Truncate(this._timeStamp);
                uint nanosec = (uint)((this._timeStamp - sec) * 1e+9);
                _message.header.stamp.sec = sec;
                _message.header.stamp.nanosec = nanosec;
                _message.data = _dcam.data;
                _ros.Publish(_topic, _message);
            }
        }
    }
}
