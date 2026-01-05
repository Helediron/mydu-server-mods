# Interaction validation through mod

Starting with client 1.4.12 and server 1.5.10, mods can hook player interaction of kinds "simple" (basic activation) and switch (toggle on or off like doors).

When enabled, the client will emit a javascript event with parameters describing the action and a token, and expect a call back to a javascript endpoint to accept or deny the action.


## Enabling

On target element or element kind, set a non-empty value to static or dynamic property "interactionAuthorize" with the name of the javascript event that will be triggered.

Optionally set "interactionParameters" (static or dynamic) to store extra data passed to the event.

Aditionnaly, "activateLabel" and "deactivateLabel", dynamic or static properties, can be set to override the default text action hints ("open" and "close" for a door for instance).


## Catching and replying

Use 'engine.on(eventName, callback)' to catch the event.

The callback signature is:

    void myCallback(token, isEnable, parameters, elementTypeId, constructId, elementId)

With parameters:

- token: a string token to identify the request
- isEnable: true for simple or switch enable, false for switch disable
- parameters: string, value of "interactionParameters" dynamic or static property
- elemntTypeId: long, typeid of activated element
- constructId: construct id containing the element
- elementId: target element global id


Your code *must* accept or deny the action by calling "CPPMod.authorizeInteraction(token, acceptOrDeny, '')", passing the token, a boolean, true to accept, false to deny the operation, and a final string parameter unused for now.


## Example

The sample code in this directory allows anyone to set a fee to open a door.

To use it one must first trigger the "toll/opt in" right-click menu action, then by right clicking on a door element a toll can be set or unset.

The quantas will be dedectud from the account of the player taking action at each door opening.