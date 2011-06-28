using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Makes all classes public for easier testing. This is NOT shipped with the nuget.

public partial class DomainContext<TContextInterface, TId> { }
public partial interface IAggregateRoot<TId> { }
public partial interface IDomainContext<TId> { }
public partial interface IIdentifiable<TId> { }