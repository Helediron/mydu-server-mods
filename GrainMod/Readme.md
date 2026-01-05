# Grain mods

Starting with server 1.5.10 a new kind of mod is supported: Grain mods.

Those are able to define new Grain implementations, or override existing ones.

## Specs

Your grain dll must be placed in the "Mods" folder, and have a name prefixed with "GrainMod" and ending in ".dll".

Your dll may implement IGrainMod to be notified when it is loaded.

## Replacing an existing implementation.

To replace an existing Grain interface implementation with your own, you need to disable the default implementation first.

This is achieved in "dual.yaml" in setting "orleans.exclude_interfaces". This is a list of interfaces (full namespaced name or raw interface name) whose implementations will not be loaded from Grains.dll.

You must then provide a class implementing said interface in a grain dll mod.

### Inheriting from and expanding existing implementations

Grain classes provided by NQ implement grain interfaces methods that are not marked "virtual". But there is a way to still reuse them.

Create your class (say MyTalentGrain), and mark it as inheriting from both NQ class (TalentGrain) and the interface (ITalentGrain).

Then use explicit interface implementation syntax (Task ITalentGrain.Purchase(...)).