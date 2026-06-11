using System;

namespace Mudit.Core.Services.SaveService
{
	[Serializable]
	internal class SaveEnvelope
	{
		public int version;
		public string typeId;
		public string payload;
	}
}
