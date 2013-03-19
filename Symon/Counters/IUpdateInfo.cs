using System.Diagnostics;
using System;

namespace Symon
{
	public interface IUpdateInfo
	{
        ValueType UpdateInfoInStruct();

        void PrepareCounters(string resourceName);

        void Dispose();
	}

    

}
