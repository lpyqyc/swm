using Swm.Device.SRgv;
using System;


namespace Swm.SRgv.GS
{
    [Obsolete("仅供测试使用")]
    public class FakeDeviceTaskNoGenerator : IDeviceTaskNoGenerator
    {
        int i = 0;
        public int GetNextTaskNo()
        {
            return ++i;
        }
    }
}

