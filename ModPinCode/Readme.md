# Changing the gameplaybank: sample


Starting with server 1.5.8 the cshrap code can load DLL overrides that can add entries to the gameplaybank static structure, defining new static and dynamic properties.

*LIMITATION*: the client will not know of and thus ignore those new static and dynamic properties since it uses codegen that cannot be changed.


## Principle

User writes a csharp dll defining new classes inheriting from GameplayObject or one of it's existing children.

That dll is put in a "Bank" subfolder of both backoffice and orleans programs.

Once done those new classes will be fully integrated in the GameplayBank hierarchy and usable, like for instance:

    var pcDef = bank.GetBaseObject<NQutils.Def.PinCodeElement>(element.elementType);
    int mfa = pcDef.maxFailAttempts;

## Docker server changes

For simplicity edit docker-compose.yml and edit to add the required volumes:

```yaml
services:
    orleans:
        volumes:
            - ./Bank:/OrleansGrains/Bank
    backoffice:
        volumes:
            - ./Bank:/Backoffice/Bank
```

you can then place your bank mods in the Bank subfolder of your install directory.


## Sample: PIN code for doors

The sample provided, made of a Dll usual mod and a new dll bank mod, adds a new type "PinCodeELement" that defines static and dynamic properties to store a PIN code and its parameters.

The accomopanying mod provides actions to set and enter the PIN code that will open the door.

To use, add in the BO item hierarchy a child of PinCodeElement like this:

```yaml
PinHatchSmall:
  parent: PinCodeElement
  assetAlias: HatchSmall
  displayName: pin code hatch
  pinCodeMinLength: 2
```

Add some to your inventory, place on a construct, and use the two right-click mod actions "set PIN" and "enter PIN".
