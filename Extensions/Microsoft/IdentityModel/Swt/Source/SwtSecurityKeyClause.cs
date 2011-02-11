using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IdentityModel.Tokens;

namespace Microsoft.IdentityModel.Swt
{
	/// <summary>
	/// The <see cref="SwtSecurityTokenHandler"/> passes an instance of this clause to 
	/// the <see cref="SwtIssuerTokenResolver"/> so that it knows it's an SWT that has 
	/// already been verified against the <see cref="SwtIssuerNameRegistry"/> trusted 
	/// issuers list. 
	/// </summary>
	/// <remarks>
	/// Because we only support one symmetric <see cref="SecurityKey"/> 
	/// for SWT, we don't need to differentiate between issuers.
	/// </remarks>
	internal class SwtSecurityKeyClause : SecurityKeyIdentifierClause
	{
		public SwtSecurityKeyClause()
			: base("SWT")
		{
		}
	}
}
