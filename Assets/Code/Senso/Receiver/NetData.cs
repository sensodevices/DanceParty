using System;
using UnityEngine;

namespace Senso
{
    ///
    /// @brief Enumeration for all Senso position types
    public enum EPositionType
    {
        Unknown, RightHand, LeftHand, Body, PositionsCount
    };

    [Serializable]
    public class PosQuat
    {
        public float[] pos;
        public float[] quat;
		public float[] acc;
		public float[] spd;
    }
    [Serializable]
    public class Quat
    {
        public float[] quat;
    }

    [Serializable]
    public class NetData
    {
        public string src;
        public string name;
        public string type;
        [NonSerialized]
        public string packet;
    }
}
