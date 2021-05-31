using UnityEngine;

public class SensoBodyController : SensoBaseController {

    // Variables for hands objects
    [Header("Body tracking")]
    public Senso.Body Avatar;

    // Initialization
    new void Start ()
    {
        if (Avatar == null)
        {
            Debug.LogError("Avatar is not set! What am I supposed to control?");
            return;
        }
        Avatar.SetController(this);

        base.Start();
    }

    // Every frame
    new void Update ()
    {
        base.Update();
        if (sensoThread != null)
        {
            int cnt = sensoThread.UpdateData(ref receivedData);
            if (cnt > 0)
            {
                bool positionSet = false;
                bool camposSet = false;
                for (int i = cnt - 1; i >= 0; --i)
                {
                    var parsedData = receivedData[i];
                    if (parsedData.type.Equals("avatar"))
                    {
                        if (!positionSet)
                        {
                            var bodyData = JsonUtility.FromJson<Senso.BodyDataFull>(parsedData.packet);
                            if (bodyData != null)
                            {
                                Avatar.SetSensoPose(bodyData.data);
                                positionSet = true;
                                camposSet = true;
                            }
                        }
                        else break;
                    }
                    else if (parsedData.type.Equals("campos"))
                    {
                        if (!camposSet)
                        {
                            var cameraData = JsonUtility.FromJson<Senso.CamPosDataFull>(parsedData.packet);
                            if (cameraData != null)
                            {
                                //Avatar.SetCamPos(cameraData.data);
                                //camposSet = true;
                            }
                        }
                    }
                }
            }
        }
	}

}
