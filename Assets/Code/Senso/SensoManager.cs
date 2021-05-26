using System;
using UnityEngine;
using System.Collections.Generic;

public class SensoManager : MonoBehaviour 
{
    private bool m_rightEnabled;
    private bool m_leftEnabled;
    private bool m_bodyEnabled;

    public Transform[] Hands;
    private LinkedList<SensoHand> sensoHands;

    public Transform avatar;

    public SensoJoint Pelvis;
    public SensoJoint Spine;
    public SensoJoint Neck;
    public SensoJoint[] RightLeg;
    public SensoJoint[] LeftLeg;

    public SensoJoint[] RightArm;
    public SensoJoint[] LeftArm;

    private string GetSensoHost () {
        return searchCLIArgument("sensoHost", SensoHost);
    }
    private Int32 GetSensoPort () {
        var portStr = searchCLIArgument("sensoPort", SensoPort.ToString());
        Int32 port;
        if (!Int32.TryParse(portStr, out port)) {
            port = SensoPort;
        }
        return port;
    }
    public string SensoHost = "127.0.0.1"; //!< IP address of the Senso Server instane
    public Int32 SensoPort = 53450; //!< Port of the Senso Server instance

    private SensoNetworkThread sensoThread;

    public Transform orientationSource;
    private DateTime orientationNextSend;
    private double orientationSendEveryMS = 100.0f;
    public Transform VRCameraHolder;

    static public int DELTA_SAMPLES_CNT = 5;
    private float[] deltaYs;
    private int deltaYInd = 0;

    void Start () {
        if (Hands != null && Hands.Length > 0) {
            sensoHands = new LinkedList<SensoHand>();
            for (int i = 0; i < Hands.Length; ++i) {
                Component[] components = Hands[i].GetComponents(typeof(SensoHand));
                for (int j = 0; j < components.Length; ++j) {
                    var hand = components[j] as SensoHand;
                    sensoHands.AddLast(hand);
                    if (!m_rightEnabled && hand.HandType == ESensoPositionType.RightHand) m_rightEnabled = true;
                    else if (!m_leftEnabled && hand.HandType == ESensoPositionType.LeftHand) m_leftEnabled = true;
                }
            }
        }
        
        // TODO: !!!
        m_bodyEnabled = true;
        if (Pelvis != null) Pelvis.OnStart();
        if (Spine != null) Spine.OnStart();
        if (Neck != null) Neck.OnStart();

        if (RightArm != null)
            for (int i = 0; i < RightArm.Length; ++i)
                RightArm[i].OnStart();
        if (LeftArm != null)
            for (int i = 0; i < LeftArm.Length; ++i)
                LeftArm[i].OnStart();
        if (RightLeg != null)
            for (int i = 0; i < RightLeg.Length; ++i)
                RightLeg[i].OnStart();
        if (LeftLeg != null)
            for (int i = 0; i < LeftLeg.Length; ++i)
                LeftLeg[i].OnStart();

        sensoThread = new SensoNetworkThread(GetSensoHost(), GetSensoPort());
        sensoThread.StartThread();
        //BroadcastMessage("SetSensoManager", this);

        if (orientationSource != null)
        {
            /*var aRig = orientationSource.GetComponent<OVRCameraRig>();
            if (aRig != null)
            {
                aRig.UpdatedAnchors += OrientationUpdated;
            }*/
        }
        deltaYs = new float[DELTA_SAMPLES_CNT];
        for (int i = 0; i < DELTA_SAMPLES_CNT; ++i) deltaYs[i] = 0.0f;
    }

    void OnDisable () {
        sensoThread.StopThread();
    }

    /*void OrientationUpdated(OVRCameraRig rig)
    {
        if (VRCameraHolder != null)
        {
            //VRCameraHolder.localRotation = Quaternion.Inverse(rig.centerEyeAnchor.localRotation);
        }
        if (DateTime.Now >= orientationNextSend)
        {
            sensoThread.SendHMDOrientation(rig.centerEyeAnchor.localEulerAngles);
            orientationNextSend = DateTime.Now.AddMilliseconds(orientationSendEveryMS);
        }
    }*/

    private float getHMDYawDelta()
    {
        float sum = 0;
        for (int i = 0; i < DELTA_SAMPLES_CNT; ++i) sum += deltaYs[i];
        //Debug.Log(sum);
        return sum / DELTA_SAMPLES_CNT;
    }

    void FixedUpdate () {
        sensoThread.UpdateData();
        /*SensoHandData leftSample = null, rightSample = null;
        if (m_rightEnabled) {
            rightSample = sensoThread.GetSample(ESensoPositionType.RightHand);
        }
        if (m_leftEnabled) {
            leftSample = sensoThread.GetSample(ESensoPositionType.LeftHand);
        }
        if (sensoHands != null) {
            foreach (var hand in sensoHands) {
                if (hand.HandType == ESensoPositionType.RightHand) {
                    hand.SensoPoseChanged(rightSample);
                } else if (hand.HandType == ESensoPositionType.LeftHand) {
                    hand.SensoPoseChanged(leftSample);
                }
            }
        }
        
        // Gestures
        var gestures = sensoThread.GetGestures();
        if (gestures != null) {
            for (int i = 0; i < gestures.Length; ++i) {
                if (gestures[i].Type == ESensoGestureType.PinchStart || gestures[i].Type == ESensoGestureType.PinchEnd) {
                    fingerPinch(gestures[i].Hand, gestures[i].Fingers[0], gestures[i].Fingers[1], gestures[i].Type == ESensoGestureType.PinchEnd);
                }
            }
            
        }

        if (orientationSource != null && DateTime.Now >= orientationNextSend) {
            sensoThread.SendHMDOrientation(orientationSource.transform.localEulerAngles.y);
            orientationNextSend = DateTime.Now.AddMilliseconds(orientationSendEveryMS);
        }*/

        if (m_bodyEnabled)
        {
            var bodySample = sensoThread.GetBodySample();
            
            if (RightLeg != null && RightLeg.Length == 3)
            {
                //var tq = new Quaternion(-bodySample.pelvisRotation.x, bodySample.pelvisRotation.y, -bodySample.pelvisRotation.z, bodySample.pelvisRotation.w);
                //var quat = bodySample.hipRotation[0] * Quaternion.Inverse(tq);
                // Thigh
                RightLeg[0].ApplyQuaternion(bodySample.hipRotation[0], bodySample.pelvisRotation);
                // Knee
                var quat = bodySample.kneeRotation[0] * Quaternion.Inverse(bodySample.hipRotation[0]);
                RightLeg[1].ApplyQuaternion(quat);
                // Foot
                quat = bodySample.footRotation[0] * Quaternion.Inverse(bodySample.kneeRotation[0]);
                RightLeg[2].ApplyQuaternion(quat);
            }
            if (LeftLeg != null && LeftLeg.Length == 3)
            {
                var tq = new Quaternion(-bodySample.pelvisRotation.x, bodySample.pelvisRotation.y, -bodySample.pelvisRotation.z, bodySample.pelvisRotation.w);
                var  quat = bodySample.hipRotation[1] * Quaternion.Inverse(tq);
                // Thigh
                LeftLeg[0].ApplyQuaternion(quat);
                // Calf
                quat = bodySample.kneeRotation[1] * Quaternion.Inverse(bodySample.hipRotation[1]);
                LeftLeg[1].ApplyQuaternion(quat);
                // Foot
                quat = bodySample.footRotation[1] * Quaternion.Inverse(bodySample.kneeRotation[1]);
                LeftLeg[2].ApplyQuaternion(quat);
            }
            
            if (Pelvis != null)
            {
                Pelvis.ApplyQuaternion(bodySample.pelvisRotation);
            }
            if (Spine != null)
            {
                var quat = bodySample.spineRotation * Quaternion.Inverse(bodySample.pelvisRotation);
                Spine.ApplyQuaternion(quat);
            }
            if (Neck != null)
            {
                //OVRCameraRig aRig = orientationSource.GetComponent<OVRCameraRig>();
                //deltaYs[deltaYInd] = bodySample.neckRotation.eulerAngles.y - aRig.centerEyeAnchor.eulerAngles.y;
                if (deltaYs[deltaYInd] > 180.0f) deltaYs[deltaYInd] -= 360.0f;
                if (deltaYs[deltaYInd] < -180.0f) deltaYs[deltaYInd] += 360.0f;
                if (++deltaYInd == DELTA_SAMPLES_CNT) deltaYInd -= DELTA_SAMPLES_CNT;
                //var quat = bodySample.neckRotation * Quaternion.Inverse(bodySample.spineRotation);
                //Neck.ApplyQuaternion(quat);
            }
            if (RightArm != null && RightArm.Length == 3)
            {
                // Clavicle
                //var tq = new Quaternion(bodySample.spineRotation.x, bodySample.spineRotation.y, bodySample.spineRotation.z, bodySample.spineRotation.w);
                var quat = bodySample.clavicleRotation[0] * Quaternion.Inverse(bodySample.spineRotation);
                RightArm[0].ApplyQuaternion(quat);
                // Shoulder
                quat = bodySample.shoulderRotation[0] * Quaternion.Inverse(bodySample.clavicleRotation[0]);
                RightArm[1].ApplyQuaternion(quat);
                // Elbow
                quat = bodySample.elbowRotation[0] * Quaternion.Inverse(bodySample.shoulderRotation[0]);
                RightArm[2].ApplyQuaternion(quat);
            }
            if (LeftArm != null && LeftArm.Length == 3)
            {
                //var tq = new Quaternion(bodySample.spineRotation.x, bodySample.spineRotation.y, bodySample.spineRotation.z, bodySample.spineRotation.w);
                var quat = bodySample.clavicleRotation[1] * Quaternion.Inverse(bodySample.spineRotation);
                // Clavicle
                LeftArm[0].ApplyQuaternion(quat);
                // Shoulder
                quat = bodySample.shoulderRotation[1] * Quaternion.Inverse(bodySample.clavicleRotation[1]);
                LeftArm[1].ApplyQuaternion(quat);
                // Elbow
                quat = bodySample.elbowRotation[1] * Quaternion.Inverse(bodySample.shoulderRotation[1]);
                LeftArm[2].ApplyQuaternion(quat);
            }
            avatar.localPosition = bodySample.position;
        }
        float dY = getHMDYawDelta() / 20000;
        if (Mathf.Abs(dY) > 0.00005)
        {
            VRCameraHolder.RotateAroundLocal(Vector3.up, dY);
        }
        Debug.Log(dY);
    }

    ///
    /// @brief Send vibration command to the server
    ///
    public void SendVibro(ESensoPositionType hand, ESensoFingerType finger, ushort duration, byte strength)
    {
        sensoThread.VibrateFinger(hand, finger, duration, strength);
    }

    ///
    /// @brief Searches for the parameter in arguments list
    ///
    private static string searchCLIArgument (string param, string def = "") {
        if (Application.platform == RuntimePlatform.Android) {
            return def;
        }
        var args = System.Environment.GetCommandLineArgs();
        int i;
        string[] searchArgs = { param, "-" + param, "--" + param };

        for (i = 0; i < args.Length; ++i) {
            if (Array.Exists(searchArgs, elem => elem.Equals(args[i])) && args.Length > i + 1 ) {
                return args[i + 1];
            }
        }
        return def;
    }

    /// Events
    public void fingerPinch(ESensoPositionType handType, ESensoFingerType finger1Type, ESensoFingerType finger2Type, bool stop = false)
    {
        SensoHand aHand = null;
        foreach (var hand in sensoHands) 
            if (hand.HandType == handType) {
                aHand = hand;
                break;
            }

        if (aHand != null) {
            aHand.TriggerPinch(finger1Type, finger2Type, stop);
        }
    }
}
 