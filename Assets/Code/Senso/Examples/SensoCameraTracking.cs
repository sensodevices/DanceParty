using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SensoCameraTracking : Senso.Body
{
    public Transform HeadPosition;
    public Transform HeadRotation;
    private bool setAzimuth = true;

    // Use this for initialization
    new void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public override void SetSensoPose(Senso.BodyData bodySample)
    {
        Quaternion curCameraRotation = Quaternion.identity;
        if (m_controller.IsAlive)
        {
            var controller = m_controller.Target as SensoBaseController;
            if (controller.HeadPositionSource != null)
            {
                curCameraRotation = controller.HeadPositionSource.rotation;
            }
        }
        SetCameraTransform(bodySample.CameraPosition, bodySample.CameraAzimuth, curCameraRotation);
    }

    private void SetCameraTransform(Vector3 position, float azimuth, Quaternion currentHeadRotation)
    {
        HeadPosition.localPosition = position;
        if (azimuth >= 0.0f && Mathf.Abs(currentHeadRotation.eulerAngles.x) < 60)
        {
            azimuth *= Mathf.Rad2Deg;
            var camAz = (currentHeadRotation * Quaternion.Inverse(HeadPosition.parent.rotation)).eulerAngles.y;
            while (camAz >= 180.0f) camAz -= 360.0f;
            var diff = azimuth - camAz;
            if (diff > 180.0f) diff -= 360.0f;
            if (diff < -180.0f) diff += 360.0f;

            if (setAzimuth && azimuth >= 0)
            {
                HeadRotation.transform.Rotate(Vector3.up, diff, Space.Self);
                setAzimuth = false;
            }
            else
                HeadRotation.transform.Rotate(Vector3.up, diff * 0.0001f, Space.Self);
        }
    }
}
