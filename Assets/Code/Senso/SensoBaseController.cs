using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SensoBaseController : MonoBehaviour
{
    public Transform HeadPositionSource;
    private System.DateTime orientationNextSend;
    private System.DateTime pingNextSend;
    public double orientationSendEveryMS = 100.0f;
    public double pingSendEveryMS = 100.0f;
    public Vector3 subtractFromOrientation;

    // Where to connect to
    public string SensoHost = "127.0.0.1"; //!< IP address of the Senso Server instane
    public int SensoPort = 53450; //!< Port of the Senso Server instance
    protected Senso.NetworkThread sensoThread;
    protected Senso.NetData[] receivedData;

    public bool StartOnLaunch = true;
    public bool UseUDP = true;

    [Header("Senso Tracking")]
    public bool useIRPattern = false;
    [Tooltip("Head top IR pattern")]
    public int HeadTopPattern = 0;
    [Tooltip("Face IR pattern")]
    public int FacePattern = 0;

    // Use this for initialization
    public void Start () {
        receivedData = new Senso.NetData[Senso.NetworkThread.MAX_PACKET_CNT];
        if (StartOnLaunch) StartTracking();
    }

    // Update is called once per frame
    public void Update()
    {
        if (sensoThread != null)
        {
            var now = System.DateTime.Now;
            if (HeadPositionSource != null && now >= orientationNextSend)
            {
                sensoThread.SetHeadLocationAndRotation(HeadPositionSource.transform.localPosition, HeadPositionSource.transform.rotation * Quaternion.Inverse(Quaternion.Euler(subtractFromOrientation)));
                orientationNextSend = now.AddMilliseconds(orientationSendEveryMS);
            }
            if (now >= pingNextSend)
            {
                sensoThread.SendPing();
                pingNextSend = now.AddMilliseconds(orientationSendEveryMS);
            }
        }
    }

    public void OnDestroy()
    {
        StopTracking();
    }

    public void StartTracking()
    {
        if (sensoThread == null)
        {
            if (UseUDP)
            {
                sensoThread = new Senso.UDPThread(SensoHost, SensoPort);
            }
            else
            {
                sensoThread = new Senso.TCPThread(SensoHost, SensoPort);
            }
            if (useIRPattern)
            {
                sensoThread.SetHeadPattern(HeadTopPattern, FacePattern);
            }
            sensoThread.StartThread();
        }
    }

    public void StopTracking()
    {
        if (sensoThread != null)
        {
            sensoThread.StopThread();
            sensoThread = null;
        }
    }
}
