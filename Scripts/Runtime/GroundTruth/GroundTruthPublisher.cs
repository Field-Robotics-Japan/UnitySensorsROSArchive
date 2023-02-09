using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Robotics.ROSTCPConnector;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using RosMessageTypes.Geometry;

[RequireComponent(typeof(FRJ.Sensor.GroundTruth))]
public class GroundTruthPublisher : MonoBehaviour
{
  [SerializeField] private string _topicName = "ground_truth_pose";
  [SerializeField] private string _frameId = "base_link";

  private float _timeElapsed = 0f;
  private float _timeStamp = 0f;

  private ROSConnection _ros;
  public PoseStampedMsg _message;

  private FRJ.Sensor.GroundTruth _ground_truth;

  // Start is called before the first frame update
  void Start()
  {
    this._ground_truth = GetComponent<FRJ.Sensor.GroundTruth>();

    // setup ROS
    this._ros = ROSConnection.instance;
    this._ros.RegisterPublisher<PoseStampedMsg>(this._topicName);

    // setup ROS Message
    this._message = new PoseStampedMsg();
    this._message.header.frame_id = this._frameId;
  }

  // Update is called once per frame
  void Update()
  {
    this._timeElapsed += Time.deltaTime;

    if (this._timeElapsed > (1f / this._ground_truth.updateRate))
      {
        // Update time
        this._timeElapsed = 0;
        this._timeStamp = Time.time;

        // Update Ground Truth data
        this._ground_truth.UpdateGroundTruth();

        // Update ROS Message
        uint sec = (uint)Math.Truncate(this._timeStamp);
        uint nanosec = (uint)((this._timeStamp - sec) * 1e+9);
        this._message.header.stamp.sec = sec;
        this._message.header.stamp.nanosec = nanosec;

        // Position
        Vector3<FLU> position_ros = new Vector3<FLU>(this._ground_truth.GeometryEuclidean.x,
                                                     this._ground_truth.GeometryEuclidean.y,
                                                     this._ground_truth.GeometryEuclidean.z).To<FLU>();

        PointMsg position = new PointMsg(position_ros.x,
                                         position_ros.y,
                                         position_ros.z);
        this._message.pose.position = position;

        // Rotation
        Quaternion<FLU> orientation_ros = new Quaternion<FLU>(this._ground_truth.GeometryQuaternion.x,
                                                              this._ground_truth.GeometryQuaternion.y,
                                                              this._ground_truth.GeometryQuaternion.z,
                                                              this._ground_truth.GeometryQuaternion.w).To<FLU>();
        QuaternionMsg orientation = new QuaternionMsg(orientation_ros.x,
                                                      orientation_ros.y,
                                                      orientation_ros.z,
                                                      orientation_ros.w);
        this._message.pose.orientation = orientation;

        // Publish message w/ topic name
        this._ros.Send(this._topicName, this._message);
      }
  }
}
