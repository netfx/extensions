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