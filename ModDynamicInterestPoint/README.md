# Dynamic points of interest.

Client 1.4.9 adds two flags to VoxelCsgApplied with constructId = 0: 9 (add interest point) and 10 (remove interest point).

Both take in hashContext a JSON serialized NQ::DynamicInterestPoint.

`markType` is the icon type to use. Low values are reserved. There must be a nqdef entry with key (assuming markType=1000) `ui.planetMapCustom.picker1000`.

The value behind this key must be a '.node' file defining a node with the same name (picker1000).

There is a complete working example client setup for ModLoader in the "client/" subdirectory.
