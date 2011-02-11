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