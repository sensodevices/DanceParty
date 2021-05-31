using System;
using UnityEngine;

namespace Senso
{
    public enum ESensoFinger
    {
        Thumb, Index, Middle, Third, Little
    }
    
    ///
    /// @brief Arguments for fingers pinch event
    ///
    public class SensoPinchEventArgs : EventArgs
    {
        public bool IsEnd { get; private set; }
        public ESensoFinger[] Fingers { get; private set; }

        public SensoPinchEventArgs(ESensoFinger finger1, ESensoFinger finger2, bool end)
        {
            IsEnd = end;
            Fingers = new ESensoFinger[2] { finger1, finger2 };
        }
    }

    ///
    /// @brief Arguments for fingers pinch event
    ///
    public class GrabEventArgs : EventArgs
    {
        public bool IsEnd { get; private set; }

        public GrabEventArgs(bool end)
        {
            IsEnd = end;
        }
    }

    public abstract class Hand : MonoBehaviour
    {
        public EPositionType HandType;
        protected System.WeakReference m_controller;
        public int BatteryLevel { get; private set; }
        public int Temperature { get; private set; }
        public string MacAddress { get; private set; }
        public HandData Pose { get; private set; }
        public bool Grabbing { get; private set; }

        // Events
        public event EventHandler<SensoPinchEventArgs> OnPinch; // Is fired when two fingers started pinching
        public event EventHandler<GrabEventArgs> OnGrab; // Is fired when hand grab state changed
        public event EventHandler<EventArgs> OnClap;

        public void Start()
        {
            MacAddress = null;
        }

        public void SetBattery(int newLevel)
        {
            BatteryLevel = newLevel;
        }
        public void SetTemperature(int newTemp)
        {
            Temperature = newTemp;
        }

        public void SetMacAddress(string newAddress)
        {
            MacAddress = newAddress;
        }

        public void SetController(SensoHandsController aController)
        {
            m_controller = new System.WeakReference(aController);
        }

        public void VibrateFinger(Senso.EFingerType finger, ushort duration, byte strength)
        {
            if (m_controller.IsAlive)
            {
                SensoHandsController man = m_controller.Target as SensoHandsController;
                if (man != null)
                    man.SendVibro(HandType, finger, duration, strength);
            }
        }

        public virtual void SetSensoPose (HandData newData) 
		{
			Pose = newData;
		}

        public void TriggerPinch(ESensoFinger finger1Type, ESensoFinger finger2Type, bool stop = false)
        {
            var args = new SensoPinchEventArgs(finger1Type < finger2Type ? finger1Type : finger2Type, finger2Type > finger1Type ? finger2Type : finger1Type, stop);
            if (OnPinch != null)
            {
                OnPinch(this, args);
            }
        }

        public void TriggerGrab(bool end)
        {
            var args = new GrabEventArgs(end);
            Grabbing = !end;
            if (OnGrab != null)
            {
                OnGrab(this, args);
            }
        }

        public void TriggerClap()
        {
            if (OnClap != null)
            {
                OnClap(this, EventArgs.Empty);
            }
        }
    }
}
