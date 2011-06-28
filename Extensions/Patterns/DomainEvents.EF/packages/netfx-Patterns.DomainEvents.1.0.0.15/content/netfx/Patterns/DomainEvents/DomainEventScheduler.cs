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
using System;
using System.Threading.Tasks;

/// <summary>
/// Provides access to the <see cref="Default"/> scheduler to run 
/// async background event handlers.
/// </summary>
partial class DomainEventScheduler
{
	/// <summary>
	/// Initializes the <see cref="DomainEventScheduler"/> class with the 
	/// default task-based scheduler.
	/// </summary>
	static DomainEventScheduler()
	{
		Default = new TaskPoolDomainEventScheduler();
	}

	/// <summary>
	/// Gets the default async event scheduler.
	/// </summary>
	public static IDomainEventScheduler Default { get; private set; }

	/// <summary>
	/// An async event scheduler that uses <see cref="TaskFactory.StartNew(Action)"/> 
	/// to run background event handlers.
	/// </summary>
	private partial class TaskPoolDomainEventScheduler : IDomainEventScheduler
	{
		/// <summary>
		/// Schedules the specified action to run async as soon as possible.
		/// </summary>
		public void Schedule(Action action)
		{
			Task.Factory.StartNew(action);
		}
	}
}
