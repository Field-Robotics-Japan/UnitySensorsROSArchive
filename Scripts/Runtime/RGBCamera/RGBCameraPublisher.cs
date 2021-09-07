using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using UnityEngine.Jobs;
using Unity.Jobs;
using UnityEngine;

using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;

[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(FRJ.Sensor.RGBCamera))]
public class RGBCameraPublisher : MonoBehaviour
{

  [SerializeField] private string _rawTopicName        = "image/raw";
  [SerializeField] private string _compressedTopicName = "image/compressed";
  [SerializeField] private string _frameId   = "camera";
    
  private float _timeElapsed = 0f;
  private float _timeStamp   = 0f;

  private ROSConnection _ros;
  private ImageMsg _message;    

  private FRJ.Sensor.RGBCamera _camera;
    
  void Start()
  {
    // Get Rotate Lidar
    this._camera = GetComponent<FRJ.Sensor.RGBCamera>();
    this._camera.Init();

    // setup ROS
    this._ros = ROSConnection.instance;
    this._ros.RegisterPublisher<ImageMsg>(this._rawTopicName);

    // setup ROS Message
    this._message = new ImageMsg();
    this._message.header.frame_id = this._frameId;
    this._message.height = this._camera.height;
    this._message.width  = this._camera.width;
    this._message.encoding = "jpeg";
  }

  void Update()
  {
    this._timeElapsed += Time.deltaTime;

    if(this._timeElapsed > (1f/this._camera.scanRate)) {
      // Update ROS Message
      uint sec = (uint)Math.Truncate(this._timeStamp);
      uint nanosec = (uint)( (this._timeStamp - sec)*1e+9 );
      this._message.header.stamp.sec = sec;
      this._message.header.stamp.nanosec = nanosec;
      this._message.data = this._camera.data;

      // Update time
      this._timeElapsed = 0;
      this._timeStamp = Time.time;
    }
  }
}
