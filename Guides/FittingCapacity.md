# Adding fitting capacity to new elements

Starting with server 1.5.4 and client 1.4.7, the "fittingCapacity"
properties (static and dynamic) were moved to "Element".

However the code only takes into account for capacity elements connected
throug a restricted plugs (that is non-empty elementTypes).

That means one needs to add an extraPlugs entry to for instance KinematicsController,
and your new element, both being restricted.

Here is an example:

```yaml
KinematicsController:
  parent: ControlUnit
  displayName: Piloting Control Units
  description: 'A Control Unit is used to pilot your contruct.'
  customProperties:
    fittingCapacityMax:
      label: Max Capacity
      locKey: ib_cp_max_capacity
      unit: ''
      precision: 0
      order: 0
  extraPlugs:  # the new part
  - isOut: true
    kind: 8    # PLUG_CONTROL
    count: 10
    name: "control"
    elementTypes: [3158559890, 539285168] # adjust ids here
CapacityConsumer:
  parent: Bazar
  displayName: "capacity consumer"
  fittingCapacity: 50
  extraPlugs:
  - isOut: false
    kind: 8
    count: 1
    name: "control"
    elementTypes: [1388670697]
CapacityAugmentor:
  parent: ManualButtonUnit
  displayName: "capacity augmentor"
  fittingCapacity: -50   # yes negative values work !!
  extraPlugs:
  - isOut: false
    kind: 8
    count: 1
    name: "control"
    elementTypes: [1388670697]
```