using System;
using System.Net.Http;

namespace FlickElectricSharp {
	public class FlickElectricException : Exception {
		public HttpResponseMessage WebResponse { get; }

		public FlickElectricException(string message, HttpResponseMessage webResponse) : base(message) {
			WebResponse = webResponse;
		}

		public FlickElectricException(string message, HttpResponseMessage webResponse, Exception exception) : base(message, exception) {
			WebResponse = webResponse;
		}
	}
}
