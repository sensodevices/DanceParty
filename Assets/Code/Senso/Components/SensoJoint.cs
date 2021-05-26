using UnityEngine;

[System.Serializable]
public class SensoJoint : System.Object
{
    public enum Axis { Pitch, Roll, Yaw };
    public enum AngleApplyOrder { ZYX }

    public Transform jointGameObject;

    public Axis X = Axis.Pitch;
    public Axis Y = Axis.Yaw;
    public Axis Z = Axis.Roll;

    public bool XInverted = false;
    public bool YInverted = false;
    public bool ZInverted = false;

    private Quaternion startRot;
    private Quaternion globalStartRot;

    public void OnStart()
    {
        if (jointGameObject != null)
        {
            startRot = jointGameObject.localRotation;
            globalStartRot = jointGameObject.rotation;
        }
    }

    public void ApplyQuaternion(Quaternion quat)
    {
        Quaternion newQuat = transformQuat(quat);
        if (jointGameObject != null)
            jointGameObject.localRotation = startRot * newQuat;
    }

    public void ApplyGlobalQuaternion(Quaternion quat)
    {
        Quaternion newQuat = transformQuat(quat);
        if (jointGameObject != null)
            jointGameObject.rotation = globalStartRot * newQuat;
    }

    public void ApplyQuaternion(Quaternion quat, Quaternion substract)
    {
        var newQuat = transformQuat(quat * inverseQuat(substract));
        if (jointGameObject != null)
            jointGameObject.localRotation = startRot * newQuat;
    }

    private Quaternion transformQuat(Quaternion quat)
    {
        Quaternion newQuat = new Quaternion();
        newQuat.w = quat.w;

        newQuat.x = GetAxisVal(quat, X, XInverted);
        newQuat.y = GetAxisVal(quat, Y, YInverted);
        newQuat.z = GetAxisVal(quat, Z, ZInverted);
        return newQuat;
    }

    private Quaternion inverseQuat(Quaternion quat)
    {
        Quaternion ret = Quaternion.identity;
        if ((X == Axis.Pitch && XInverted)
         || (Y == Axis.Pitch && YInverted)
         || (Z == Axis.Pitch && ZInverted))
            ret.x = -quat.x;
        else ret.x = quat.x;

        if ((X == Axis.Roll && XInverted)
         || (Y == Axis.Roll && YInverted)
         || (Z == Axis.Roll && ZInverted))
            ret.z = -quat.z;
        else ret.z = quat.z;

        if ((X == Axis.Yaw && XInverted)
         || (Y == Axis.Yaw && YInverted)
         || (Z == Axis.Yaw && ZInverted))
            ret.y = -quat.y;
        else ret.y = quat.y;

        ret.w = quat.w;
        return Quaternion.Inverse(ret);
    }

    public void ApplyQuaternion(Quaternion quat, AngleApplyOrder ord)
    {
        Quaternion newQuat = transformQuat(quat);
        jointGameObject.localRotation = startRot;
        Vector3 angles = newQuat.eulerAngles;

        if (ord == AngleApplyOrder.ZYX)
        {
            jointGameObject.Rotate(Vector3.forward, angles.y, Space.Self);
            jointGameObject.Rotate(Vector3.up, angles.x, Space.Self);
            jointGameObject.Rotate(Vector3.right, angles.z, Space.Self);
        }
    }

    private float GetAxisVal(Quaternion quat, Axis ax, bool inverted)
    {
        float val = 0.0f;
        if (ax == Axis.Pitch) val = quat.x;
        else if (ax == Axis.Roll) val = quat.z;
        else if (ax == Axis.Yaw) val = quat.y;
        return inverted ? -val : val; 
    }
}
