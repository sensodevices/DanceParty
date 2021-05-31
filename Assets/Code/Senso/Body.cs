using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Senso
{
    public abstract class Body : MonoBehaviour
    {
        protected System.WeakReference m_controller;
        public BodyData Pose { get; private set; }

        // Use this for initialization
        public void Start()
        {

        }
        
        public void SetController(SensoBaseController aController)
        {
            m_controller = new System.WeakReference(aController);
        }

        public virtual void SetSensoPose(BodyData newData)
        {
            Pose = newData;
        }
    }
}

