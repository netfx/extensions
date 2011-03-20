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
#pragma warning disable 0436
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IdentityModel.Tokens;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Claims;

namespace Microsoft.IdentityModel.Swt
{
	/// <summary>
	/// Parses an SWT token. See http://groups.google.com/group/oauth-wrap-wg.
	/// </summary>
	internal class SimpleWebToken : SecurityToken
	{
		private DateTime validFrom = DateTime.UtcNow;

		public SimpleWebToken(string rawToken)
		{
			Guard.NotNullOrEmpty(() => rawToken, rawToken);

			this.RawToken = rawToken;
			this.Parse();
		}

		public bool IsExpired
		{
			get
			{
				var expiresOn = this.ExpiresOn.ToEpochTime();
				var currentTime = DateTime.UtcNow.ToEpochTime();

				return currentTime > expiresOn;
			}
		}

		/// <summary>
		/// Converts the SimpleWebToken to a ClaimsIdentity
		/// </summary>
		public ClaimsIdentity ToClaimsIdentity(string nameClaimType = null, string roleClaimType = null)
		{
			var claims = this.Claims
				.AllKeys
				.SelectMany(key => this.Claims.GetValues(key)
					.Select(value => new { Key = key, Value = value }))
				.Select(keyValue => new Claim(keyValue.Key, keyValue.Value));

			return new ClaimsIdentity(claims, "OAUTH-SWT",
				nameClaimType ?? ClaimTypes.Name, roleClaimType ?? ClaimTypes.Role);
		}

		public override string ToString()
		{
			return this.RawToken;
		}

		private void Parse()
		{
			this.Claims = new NameValueCollection();

			foreach (var rawNameValue in this.RawToken.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries))
			{
				if (rawNameValue.StartsWith(SwtConstants.HmacSha256 + "="))
					continue;

				var nameValue = rawNameValue.Split('=');

				if (nameValue.Length != 2)
					throw new InvalidSecurityTokenException(string.Format(
						"Invalid token contains a name/value pair missing an = character: '{0}'", rawNameValue));

				var key = HttpUtility.UrlDecode(nameValue[0]);

				if (this.Claims.AllKeys.Contains(key))
					throw new InvalidSecurityTokenException("Duplicated name token.");

				var values = HttpUtility.UrlDecode(nameValue[1]);

				switch (key)
				{
					case SwtConstants.Audience:
						this.Audience = values;
						break;
					case SwtConstants.ExpiresOn:
						this.ExpiresOn = ulong.Parse(values).ToDateTimeFromEpoch();
						break;
					case SwtConstants.Issuer:
						this.Issuer = values;
						break;
					default:
						// We may have more than one value in SWT.
						foreach (var value in values.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
						{
							this.Claims.Add(key, value);
						}
						break;
				}
			}
		}

		public string Audience { get; private set; }
		public NameValueCollection Claims { get; private set; }
		public DateTime ExpiresOn { get; private set; }
		public string Issuer { get; private set; }
		public string RawToken { get; private set; }

		/* SecurityToken */
		public override DateTime ValidFrom { get { return this.validFrom; } }
		public override DateTime ValidTo { get { return this.ExpiresOn; } }
		public override string Id { get { throw new NotImplementedException(); } }
		public override ReadOnlyCollection<SecurityKey> SecurityKeys { get { return new List<SecurityKey>().AsReadOnly(); } }
	}
}
#pragma warning restore 0436