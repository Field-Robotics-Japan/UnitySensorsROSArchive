using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;
using RosMessageTypes.Geometry;

[RequireComponent(typeof(FRJ.Sensor.IMU))]
public class IMUPublisher : MonoBehaviour
{

  [SerializeField] private string _topicName = "imu/raw_data";
  [SerializeField] private string _frameId   = "imu_link";
    
  private float _timeElapsed = 0f;
  private float _timeStamp   = 0f;

  private ROSConnection _ros;
  private ImuMsg _message;    

  private FRJ.Sensor.IMU _imu;
    
  void Start()
  {
    // Get Rotate Lidar
    this._imu = GetComponent<FRJ.Sensor.IMU>();

    // setup ROS
    this._ros = ROSConnection.instance;
    this._ros.RegisterPublisher<ImuMsg>(this._topicName);

    // setup ROS Message
    this._message = new ImuMsg();
    this._message.header.frame_id = this._frameId;
  }

    void Update()
    {
        this._timeElapsed += Time.deltaTime;

        if(this._timeElapsed > (1f/this._imu.scanRate))
        {
            // Update time
            this._timeElapsed = 0;
            this._timeStamp = Time.time;
            // Update ROS Message
            uint sec = (uint)Math.Truncate(this._timeStamp);
            uint nanosec = (uint)( (this._timeStamp - sec)*1e+9 );
            this._message.header.stamp.sec = sec;
            this._message.header.stamp.nanosec = nanosec;
            QuaternionMsg orientation =
                new QuaternionMsg(this._imu.GeometryQuaternion.x,
                                  this._imu.GeometryQuaternion.y,
                                  this._imu.GeometryQuaternion.z,
                                  this._imu.GeometryQuaternion.w);
            this._message.orientation = orientation;
            Vector3Msg angular_velocity =
                new Vector3Msg(this._imu.AngularVelocity.x,
                               this._imu.AngularVelocity.y,
                               this._imu.AngularVelocity.z);
            this._message.angular_velocity = angular_velocity;
            Vector3Msg linear_acceleration =
                new Vector3Msg(this._imu.LinearAcceleration.x,
                               this._imu.LinearAcceleration.y,
                               this._imu.LinearAcceleration.z);             
            this._message.linear_acceleration = linear_acceleration;
            this._ros.Send(this._topicName, this._message);
        }
    }
}
