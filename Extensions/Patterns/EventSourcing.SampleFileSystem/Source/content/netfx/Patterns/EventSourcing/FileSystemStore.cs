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
using System.IO;

/// <summary>
/// Implements a very simple unoptimized store for the file system.
/// </summary>
///	<nuget id="netfx-Patterns.EventSourcing.SampleFileSystem" />
partial class FileSystemStore<TObjectId, TBaseEvent> : IQueryableEventStore<TObjectId, TBaseEvent, FileStoredEvent<TObjectId, TBaseEvent>>
	where TBaseEvent : ITimestamped
{
	private string directory;
	private ISerializer serializer;

	public FileSystemStore(string directory, ISerializer serializer)
	{
		if (!Directory.Exists(directory))
			Directory.CreateDirectory(directory);

		this.directory = directory;
		this.serializer = serializer;
		this.Events = Directory.EnumerateFiles(this.directory)
			.OrderBy(file => file)
			.Select(file => this.serializer.Deserialize<FileStoredEvent<TObjectId, TBaseEvent>>(File.ReadAllBytes(file)))
			.AsQueryable();
	}

	public void Commit()
	{
	}

	public void Persist(DomainObject<TObjectId, TBaseEvent> entity)
	{
		var commitId = DateTimeOffset.Now.Ticks;
		var events = entity.GetEvents().ToList();

		for (var eventId = 0; eventId < events.Count; eventId++)
		{
			var @event = events[eventId];

			// Ticks used as a timestamp for ordering
			var fileName = string.Format("{0}-{1}-{2:00}.json", entity.Id, commitId, eventId);
			var filePath = Path.Combine(this.directory, fileName);

			File.WriteAllBytes(filePath, this.serializer.Serialize(new FileStoredEvent<TObjectId, TBaseEvent>(entity, @event)));
		}

		entity.AcceptEvents();
	}

	public IEnumerable<TBaseEvent> Query(EventQueryCriteria<TObjectId> criteria)
	{
		var expr = criteria.ToExpression(this, type => type.FullName);
		var events = this.Events;
		if (expr != null)
			events = events.Where(expr);

		return events.Select(x => x.Event);
	}

	public IQueryable<FileStoredEvent<TObjectId, TBaseEvent>> Events { get; private set; }
}