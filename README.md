# ORBatis

A stripped down version of iBatis.

It only builds for net48 and net9.0, almost all the rest of the functionality has been deleted, 
and the configuration system has been heavily hacked!

In particular:
- We attempt to load embedded SqlMaps lazily
  - This is accomplished by major hacks to the `SqlMapper.cs` and `DomSqlMapBuilder.cs` classes.
  - `SqlMapper`
    - Now has a `RegisterEntityToMap` method
    - This stores a configuration method in a per-file dictionary lookup
    - The first consumer of this file will implicitly call the configuration method.
    - Duplicate callers will await a shared task.
  - `DomSqlMapBuilder`
    - If we detect that a file can be loaded lazily, we will attempt to do so.
    - Calls the `RegisterEntityToMap` method to configure the lazy-load method
    - Also tweaked the `ConfigureSqlMap` method to be easier to call for lazy callers.
    - ~At the end of the config method, we start a background thread which will load each of the Lazy-loadable .xml files in turn.~
    - Rework design to not depend on configScope so much
    - Delete some of the FileScope/Cache logic
  - `TypeAliasDeSerializer`
    - Add locking to prevent concurrent Alias collision

## Important!

Make sure that if you change the NetFramework behavior, that you also change the NetCore behavior!
- They're basically duplicates of one another, but the shared logic is difficult to move
  into the common project due to dependency on runtime specific Reflect/ILEmit logic.

Many parameters are passed statically on the configScope to child entities.
- This means that we ABSOLUTELY CANNOT ALLOW two entity.xml maps to be built at the same time.
  - If we permit this, then in testing everything will look fine...
  - but when we deploy to production we'll get all sorts of strange, unexpected errors.
  - Maps in Holiday.xml getting assigned to the Booking.xml namespace and similar.
- To avoid this, we would need to totally re-write the configuration process. Not an easy feat.

## Big Quirks!

- Global.xml is always loaded eagerly because we have several queries which depend on it.
  - It would be better if we detected dependencies and lazy loaded them too, but I'm not sure if this is possible.
- Because of the "Namespace" feature, I need to load the entire XML into memory for every SQL map to double check that it doesn't belong to a different .xml namespace.
  - i.e. GridReviewForOverview in GridReview.xml actually belongs to the "Review.xml" namespace.
  - This causes the up-front compilation time to increase 3.5x
