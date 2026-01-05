# mydu-server-mods

This repository has been created to serve as a sharing and learning platform for the myDU community and the development of server modifications. You will find example code for various server mods that can be used to expand on myDU servers.

# Content index

- [API Reference](APIReference/) : reference files, of note:
    - [Gameplay Rules](APIReference/IDLFiles/gameplayRules.yaml) : read-only reference for item hierarchy.
    - [Interfaces](APIReference/OrleansInterfaces/) : all grains interfaces
    - [Services](APIReference/Services/) : all services interfaces accessible through IServiceProvider
- [Guides](Guides/) : A set of guides on specific topics.
    - [Backoffice patch system](Guides/BackofficeItembankPatch.md) : backoffice patching mechanism
    - [Customizing PvP hit or miss formula](Guides/CustomHitMissFormula.md)
    - [Custom plug definition](Guides/CustomPlugDefinition.md) : add custom plug types
    - [Hosting on a dynamic IP](Guides/DynamicIP.md) : instructions for hosting on a dynamic IP address
    - [Fitting capacity on all elements](Guides/FittingCapacity.md)
    - [High resolution voxel textures](Guides/HighResolutionTextures.md)
    - [Legacy Territory Scanner](Guides/LegacyTerritoryScanner.md)
    - [Workaround for hairping routing not working](Guides/RouterWithoutHairpinSupport.md)
    - [Server garbage collection](Guides/ServerGarbageCollection.md): instructions on how to garbage collect ever-growing data
    - [Teleporter config](Guides/TeleporterConfigurationg.md): instructions on how to setup teleportes
    - [User content hierarchical storage mode](Guides/UserContentHierarchicalStorage.md) : how to enable hierarchical user content storage
    - [Warp beacons](Guides/WarpBeaconCustomization.md) : how to tweak warp beacons
- Sample mods
    - [Custom Grain and grain overwrite](GrainMod/) : how to write mods that implement/override grains
    - [Dynamic property hooking for custom interactions](ModButtonInteraction/)
    - [Bypassing FTUE](ModBypassFTUE/)
    - [Standard interaction hooks](ModDoorToll/) : intercept any interaction to provide custom behavior
    - [Server-side interest point injection](ModDynamicInterestPoint) : inject points of interests from the server
    - [Elevators](ModElevators/) : various mechanism to implement server-side elevators
    - [Blueprint import/export](ModInterchange/)
    - [Server-side asset loading](ModLoader/)
    - [Dynamic Item Hieararchy structure modification](ModPinCode/)
    - [Engine rotation (VTOL)](ModRotateEngine/)
    - [Player-controlled teleporters](ModTeleporterConfig/) 
