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
 *	- simplified impl. for netfx, added configuration-driven key and key name.
 *	- made sure to allow default base implementation do its work.
 * 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens;
using System.ServiceModel.Security;
using System.Configuration;

namespace Microsoft.IdentityModel.Swt
{
	/// <summary>
	/// Resolves a <see cref="SwtSecurityKeyClause"/> to the SWT signing key 
	/// specified in appSettings with the <see cref="SigningKeyAppSetting"/> key.
	/// </summary>
	internal class SwtIssuerTokenResolver : IssuerTokenResolver
	{
		/// <summary>
		/// Required key <c>SwtSigningKey</c> in the appSettings configuration section 
		/// containing the 256-bit symmetric key for token signing.
		/// </summary>
		/// <example>
		/// <code>
		/// &lt;appSettings&gt;
		/// 	&lt;add key="SwtSigningKey" value="[your 256-bit symmetric key configured in the STS/ACS]" /&gt;
		/// &lt;/appSettings&gt;
		/// </code>
		/// </example>
		public const string SigningKeyAppSetting = "SwtSigningKey";

		private SecurityKey key;

		public SwtIssuerTokenResolver()
		{
			var signValue = ConfigurationManager.AppSettings[SigningKeyAppSetting];
			if (string.IsNullOrEmpty(signValue))
				throw new InvalidSecurityException(string.Format(
					"Required appSettings key '{0}' containing the 256-bit symmetric key for token signing was not found.",
					SigningKeyAppSetting));

			this.key = new InMemorySymmetricSecurityKey(Convert.FromBase64String(signValue));
		}

		protected override bool TryResolveSecurityKeyCore(SecurityKeyIdentifierClause keyIdentifierClause, out SecurityKey key)
		{
			// We pass ourselves a SwtSecurityKeyClause from the SwtSecurityTokenHandler on ValidateToken.
			var nameClause = keyIdentifierClause as SwtSecurityKeyClause;

			// If it wasn't us passing the clause, let the base class handle other built-in scenarios (i.e. X509)
			if (nameClause == null)
				return base.TryResolveSecurityKeyCore(keyIdentifierClause, out key);

			key = this.key;
			return true;
		}
	}
}