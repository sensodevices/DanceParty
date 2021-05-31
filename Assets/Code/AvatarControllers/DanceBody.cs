using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DanceBody : Senso.Body
{
    public Senso.BodyJoint Pelvis;
    public Senso.BodyJoint Spine;
    public Senso.BodyJoint Neck;

    public Senso.BodyJoint[] RightLeg;
    public Senso.BodyJoint[] LeftLeg;

    public Senso.BodyJoint[] RightArm;
    public Senso.BodyJoint[] LeftArm;

    // Object used to raycast from foots to floor
    public Transform RBottom;
    public Transform LBottom;

    private Vector3 pelvisPosDiff;

    // Use this for initialization
    new void Start ()
    {
        base.Start();
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

        pelvisPosDiff = this.transform.localPosition - Pelvis.jointGameObject.localPosition;
    }

    // Update is called once per frame
    public override void SetSensoPose (Senso.BodyData bodySample)
    {
        base.SetSensoPose(bodySample);
        if (Pelvis != null)
        {
            Pelvis.ApplyQuaternion(bodySample.PelvisRotation);
        }
        if (Spine != null)
        {
            Spine.ApplyQuaternion(bodySample.SpineRotation);
        }
        if (Neck != null)
        {
            Neck.ApplyQuaternion(bodySample.NeckRotation);
        }

        if (RightLeg != null && RightLeg.Length == 3)
        {
            // Thigh
            RightLeg[0].ApplyQuaternion(bodySample.HipRightRotation);
            // Knee
            RightLeg[1].ApplyQuaternion(bodySample.KneeRightRotation);
            // Foot
            RightLeg[2].ApplyQuaternion(bodySample.FootRightRotation);
        }
        if (LeftLeg != null && LeftLeg.Length == 3)
        {
            // Thigh
            LeftLeg[0].ApplyQuaternion(bodySample.HipLeftRotation);
            // Calf
            LeftLeg[1].ApplyQuaternion(bodySample.KneeLeftRotation);
            // Foot
            LeftLeg[2].ApplyQuaternion(bodySample.FootLeftRotation);
        }

        if (RightArm != null && RightArm.Length == 3)
        {
            // Clavicle
            RightArm[0].ApplyQuaternion(bodySample.ClavicleRightRotation);
            // Shoulder
            RightArm[1].ApplyQuaternion(bodySample.ShoulderRightRotation);
            // Elbow
            RightArm[2].ApplyQuaternion(bodySample.ElbowRightRotation);
        }
        if (LeftArm != null && LeftArm.Length == 3)
        {
            // Clavicle
            LeftArm[0].ApplyQuaternion(bodySample.ClavicleLeftRotation);
            // Shoulder
            LeftArm[1].ApplyQuaternion(bodySample.ShoulderLeftRotation);
            // Elbow
            LeftArm[2].ApplyQuaternion(bodySample.ElbowLeftRotation);
        }
        this.transform.localPosition = bodySample.PelvisPosition + pelvisPosDiff;

        // Ground check
        RaycastHit floorHit;
        float lDistance = 0.0f, rDistance = 0.0f;
        if (Physics.Raycast(LBottom.position, Vector3.down, out floorHit))
        {
            lDistance = floorHit.distance;
        }
        if (Physics.Raycast(RBottom.position, Vector3.down, out floorHit))
        {
            rDistance = floorHit.distance;
        }
        var minDistance = Mathf.Min(rDistance, lDistance);
        this.transform.Translate(0.0f, -minDistance, 0.0f);
    }


}
