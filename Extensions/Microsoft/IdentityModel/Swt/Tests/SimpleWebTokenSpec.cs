using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Collections.Specialized;
using Xunit;
using Microsoft.IdentityModel.Swt;
using Microsoft.IdentityModel.Tokens;

namespace Clarius.DevStore.Tests
{
	public class SimpleWebTokenSpec
	{
		[Fact]
		public void WhenParsing_ThenExposesClaimsButNotIntrinsicProperties()
		{
			var expiration = DateTime.UtcNow.ToEpochTime();
			var token = new SimpleWebToken(new TokenData
			{
				IdClaim = "23", 
				NameClaim = "kzu", 
				Audience = "http://netfx.codeplex.com",
				Issuer = "clarius", 
				ExpiresOn = expiration,
			}.ToString());
			
			Assert.Equal(2, token.Claims.Count);
			Assert.Equal("23", token.Claims["IdClaim"]);
			Assert.Equal("kzu", token.Claims["NameClaim"]);
			Assert.Equal("http://netfx.codeplex.com", token.Audience);
			Assert.Equal("clarius", token.Issuer);
			Assert.Equal(expiration, token.ExpiresOn.ToEpochTime());
		}

		[Fact]
		public void WhenParsing_ThenCalculatesIsExpired()
		{
			var expiration = DateTime.UtcNow.Subtract(TimeSpan.FromHours(1)).ToEpochTime();
			var token = new SimpleWebToken(new TokenData
			{
				ExpiresOn = expiration,
			}.ToString());

			Assert.True(token.IsExpired);
		}

		[Fact]
		public void WhenKeyDoesNotHaveEquals_ThenThrows()
		{
			Assert.Throws<InvalidSecurityTokenException>(() =>
				new SimpleWebToken("Foo&Bar=23"));
		}

		[Fact]
		public void WhenDuplicateKey_ThenThrows()
		{
			Assert.Throws<InvalidSecurityTokenException>(() =>
				new SimpleWebToken("Id=23&Id=24"));
		}

		[Fact]
		public void WhenMultipleClaimValues_ThenExposesClaims()
		{
			var token = new SimpleWebToken("Role=Admin,User");

			var values = token.Claims.GetValues("Role");

			Assert.Equal(2, values.Length);
		}

		public class TokenData
		{
			private const string TokenFormat = "IdClaim={IdClaim}&NameClaim={NameClaim}&Issuer={Issuer}&Audience={Audience}&ExpiresOn={ExpiresOn}&HMACSHA256={HmacSha256}";
			
			public string IdClaim { get; set; }
			public string NameClaim { get; set; }
			public ulong ExpiresOn { get; set; }
			public string Issuer { get; set; }
			public string Audience { get; set; }
			public string HmacSha256 { get; set; }

			public override string ToString()
			{
				return TokenFormat.FormatWith(this);
			}
		}
	}
}
