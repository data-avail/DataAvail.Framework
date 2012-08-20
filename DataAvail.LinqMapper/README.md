Problem
-------

The [Jimmy Bogard’s](http://lostechies.com/jimmybogard/author/jimmybogard/) library [AutoMapper](http://automapper.org/) is an excellent tool but unfortunately it doesn’t support projections via Linq - see description of the problem by Jimmy himself 
[here](http://lostechies.com/jimmybogard/2011/02/09/autoprojecting-linq-queries/). 
This library is attempt to naive solution of the problem defined. The top layer of this library is intentionally made similar to one of AutoMapper, but internals are very simplified and now supports only scare of the whole projections available in LINQ.
Create projections

Similar to AutoMapper library the mapping by projection is created via

<code>LinqMapper.CreateMap&lt;SrcType, DestType&gt;()</code>

The line above create default mapping between type SrcType and DestType. 
Default mapping follow these rules:

1. Properties with the same name and type (simple) are mapped.
2. Properties with the same name and type (complex, if there is defined mapping for these types) are mapped.
3. Properties with the same name and both derived from IEnumerable of complex type (where defined mapping between them) are mapped

Now you can add additional mapping rules to the defaults.

Custom mapping rules
--------------------

###MapFrom 
Map destination property from source’s property defined in option.
<code>
LinqMapper.CreateMap&lt;SrcType, DestType&gt;()
	.MapFrom(p => p.DestName, opt => opt.MapFrom(s => s.SrcName));
</code>

###Ignore 
Ignore mapping for property.
<code>
LinqMapper.CreateMap&lt;SrcType, DestType&gt;()
	.MapFrom(p => p.DestName, opt => opt.Ignore());
</code>

###ResolveUsing 
Custom mapping between source and destination.
<code>
LinqMapper.CreateMap&lt;SrcType, DestType&gt;()
	.MapFrom(p => p.DestName, opt => opt.ResolveUsing( s => s.SrcName + “I see you”));
</code>

Custom mapping should be defined in fashion known by underlying LINQ provider.
Mapping
-------
<code>
IQueryable&lt;TSrc&gt; dest = LinqMapper.Map&lt;TSrc, TDest&gt;(IQueryable&lt;TSrc&gt; Src, params string [] Expands) 
</code>

Map source query to destination by defined rules. 

The second parameter define which complex properties should be projected for query. 
For example if Item source entity has Parent property (also entity) - one should be defined in the list of the Expands in order to be selected from underlying data source (same for the properties with enumerable types).

<code>LinqMapper.Map&lt;TSrc, TDest&gt;(srcQueryable, “Parent”)</code>