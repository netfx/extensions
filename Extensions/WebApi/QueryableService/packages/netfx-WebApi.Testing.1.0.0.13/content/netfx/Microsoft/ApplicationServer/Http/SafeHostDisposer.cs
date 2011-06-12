using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace Microsoft.ApplicationServer.Http
{
	/// <summary>
	/// Ensures a host is opened on construction, and 
	/// disposes it if the host is not faulted on dispose.
	/// </summary>
	internal class SafeHostDisposer : IDisposable
	{
		private ServiceHost host;

		/// <summary>
		/// Opens the host if it's not in the <see cref="CommunicationState.Opened"/> state 
		/// already.
		/// </summary>
		public SafeHostDisposer(ServiceHost host)
		{
			this.host = host;
			if (host.State != CommunicationState.Opened)
				host.Open();
		}

		/// <summary>
		/// Closes the host if it's still in the <see cref="CommunicationState.Opened"/> state.
		/// </summary>
		public void Dispose()
		{
			if (this.host.State == CommunicationState.Opened)
				this.host.Close();
		}
	}
}
