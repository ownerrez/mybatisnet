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
    - At the end of the config method, we start a background thread which will load each of the Lazy-loadable .xml files in turn.

## Important!

Make sure that if you change the NetFramework behavior, that you also change the NetCore behavior!
- They're basically duplicates of one another, but the shared logic is difficult to move
  into the common project due to dependency on runtime specific Reflect/ILEmit logic.

## Big Quirks!

- Global.xml is always loaded eagerly because we have several queries which depend on it.
  - It would be better if we detected dependencies and lazy loaded them too, but I'm not sure if this is possible.

TODO!
- Check for other lock-required areas. I think I saw some Deserialize methods which might cause race conditions.
- Migrate the Framework code into Core. They've gotten out of sync!
- See if we can reduce the code duplication.
- Double check that netCore serializer hack I made. It's not supported for net8, but should be better tested!