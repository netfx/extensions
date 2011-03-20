#region BSD License
/* 
Copyright (c) 2010, NETFx
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

* Neither the name of Clarius Consulting nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion
/* 
 * Credits: http://zamd.net/2011/02/08/using-simple-web-token-swt-with-wif/ 
 * kzu: 
 *	- added support for configured trusted issuers.
 *	- made sure to allow default base implementation do its work.
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IdentityModel.Tokens;
using Microsoft.IdentityModel.Tokens;
using System.Xml;

namespace Microsoft.IdentityModel.Swt
{
	/// <summary>
	/// Custom registry that resolves the issuer name for a <see cref="SimpleWebToken"/> 
	/// to one of the configured trusted issuers. If no trusted issuer is specified, 
	/// the token will not be accepted.
	/// </summary>
	internal class SwtIssuerNameRegistry : ConfigurationBasedIssuerNameRegistry
	{
		public SwtIssuerNameRegistry()
		{
		}

		public SwtIssuerNameRegistry(XmlNodeList configuration)
			: base(configuration)
		{
		}

		public override string GetIssuerName(SecurityToken securityToken)
		{
			var swt = securityToken as SimpleWebToken;
			if (swt == null)
				return base.GetIssuerName(securityToken);

			// Find a trusted issuer with the same "name" attribute 
			// as the token, if any.
			return this.ConfiguredTrustedIssuers
				.Where(pair => pair.Value == swt.Issuer)
				.Select(pair => pair.Value)
				.FirstOrDefault();
		}
	}
}