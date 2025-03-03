# Custom element links

Starting from client 1.4.5 and server 1.5.2, them Item Hierarchy can be
used to add "plugs" to elements, in order to create new link capabilities.

## Plug types

Additional values were added for custom connection types:

```
enum PlugType
    PLUG_INVALID     = 0
    PLUG_ITEM        = 1
    PLUG_ATMOFUEL    = 2
    PLUG_SPACEFUEL   = 3
    PLUG_FLUID       = 4
    PLUG_ELECTRICITY = 5
    PLUG_SIGNAL      = 6
    PLUG_HEAT        = 7
    PLUG_CONTROL     = 8
    PLUG_MARKET      = 9
    PLUG_ROCKETFUEL  = 10
    PLUG_GRAVITON    = 11
    PLUG_USER1       = 12
    PLUG_USER2       = 13
    PLUG_USER3       = 14
    PLUG_USER4       = 15
    PLUG_USER5       = 16
    PLUG_END         = 17
end
```


## Example item hierarchy override

Suppose one wants to implement a cooling system with a generator unit
connected to industry units.

Here is a sample item hierarchy that would achieve that:


```yaml
Cooling:
  parent: ConstructElement
  displayName: cooling system providing coolant fluid to other elements
  extraPlugs:
  - isOut: true   # true for out, false for in
    kind: 12      # plugType kind
    count: 5      # maximum number of connections
    name: "kool"

IndustryUnit:
  parent: IndustryInfrastructure
  displayName: Industry
  <...>
  extraPlugs:
  - isOut: false
    kind: 12
    count: 1
    name: "kool"
```

Once injected, children of 'Cooling' element can be linked to industry units.