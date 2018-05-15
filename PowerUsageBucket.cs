using System;

namespace FlickElectricSharp {
	public class PowerUsageBucket {
		public PowerUsageBucket(string icpNumber, string meterSerialNumber, string channelNumber, DateTime startedAt, DateTime endedAt, double value, string unitCode, string status) {
			IcpNumber = icpNumber;
			MeterSerialNumber = meterSerialNumber;
			ChannelNumber = channelNumber;
			StartedAt = startedAt;
			EndedAt = endedAt;
			Value = value;
			UnitCode = unitCode;
			Status = status;
		}

		public string IcpNumber { get;  }
		public string MeterSerialNumber { get; }
		public string ChannelNumber { get; }
		public DateTime StartedAt { get; }
		public DateTime EndedAt { get; }
		public double Value { get; }
		public string UnitCode { get; }
		public string Status { get; }
	}
}
