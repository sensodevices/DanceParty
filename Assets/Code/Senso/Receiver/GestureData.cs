using System;

namespace Senso
{
    [Serializable]
    public class GestureData
    {
        public string type;
        public string name;
        public int[] fingers;
    }

    [Serializable]
    public class GestureDataFull
    {
        public GestureData data;
    }
}

