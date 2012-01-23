#region BSD License
/* 
Copyright (c) 2011, NETFx
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, 
are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list 
  of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice, this 
  list of conditions and the following disclaimer in the documentation and/or other 
  materials provided with the distribution.

* Neither the name of Clarius Consulting nor the names of its contributors may be 
  used to endorse or promote products derived from this software without specific 
  prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY 
EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES 
OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT 
SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, 
INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED 
TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR 
BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN 
ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH 
DAMAGE.
*/
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace NetFx.Patterns.EventSourcing.Tests
{
	public class MemoryStoreSpec : EventStoreBaseSpec
	{
		protected internal override object CreateStore()
		{
			return new MemoryStore<Guid, DomainEvent>();
		}

		[Fact]
		public void WhenSavingChanges_ThenCanReloadEntity()
		{
			var store = new MemoryStore<Guid, Event>();
			var user = new User("Daniel Cazzulino", new DateTime(1974, 4, 9), "Foo 999");

			store.Persist(user);
			store.Commit();

			var saved = new User(store.Query().For<User>(user.Id).Execute());

			Assert.Equal(user.Id, saved.Id);
			Assert.Equal(user.FullName, saved.FullName);
			Assert.Equal(user.Address, saved.Address);

			user.Move("Bar 888");

			store.Persist(user);
			store.Commit();

			saved = new User(store.Query().For<User>(user.Id).Execute());

			Assert.Equal(user.Address, saved.Address);
		}

		public class UserCreated : Event
		{
			public Guid UserId { get; set; }
			public string FullName { get; set; }
			public DateTime BirthDate { get; set; }
			public string Address { get; set; }
		}

		public class UserMoved : Event
		{
			public string OldAddress { get; set; }
			public string NewAddress { get; set; }
		}

		internal class User : DomainObject<Guid, Event>
		{
			private User()
			{
				Handles<UserCreated>(OnCreated);
				Handles<UserMoved>(OnMoved);
			}

			public User(IEnumerable<Event> events)
				: this()
			{
				this.Load(events);
			}

			public User(string fullName, DateTime birthDate, string address)
				: this()
			{
				this.Apply(new UserCreated
				{
					UserId = Guid.NewGuid(),
					FullName = fullName,
					BirthDate = birthDate,
					Address = address,
				});
			}

			public string FullName { get; private set; }
			public DateTime BirthDate { get; private set; }
			public string Address { get; private set; }

			public void Move(string newAddress)
			{
				if (string.IsNullOrEmpty(newAddress))
					throw new ArgumentException();

				Apply(new UserMoved { OldAddress = this.Address, NewAddress = newAddress });
			}

			private void OnCreated(UserCreated args)
			{
				this.Id = args.UserId;
				this.FullName = args.FullName;
				this.BirthDate = args.BirthDate;
				this.Address = args.Address;
			}

			private void OnMoved(UserMoved args)
			{
				this.Address = args.NewAddress;
			}
		}

		public abstract class Event : ITimestamped
		{
			protected Event()
			{
				this.Id = Guid.NewGuid();
				this.Timestamp = DateTimeOffset.UtcNow;
			}

			public Guid Id { get; set; }
			public DateTimeOffset Timestamp { get; set; }
		}
	}
}