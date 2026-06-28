
# Variant class

## About

Variant is the most important datatype in Redot. A Variant takes up only 24
bytes on 64-bit platforms (20 bytes on 32-bit platforms) and can store almost
any engine datatype inside of it. Variants are rarely used to hold information
for long periods of time, instead they are used mainly for communication,
editing, serialization and generally moving data around.

A Variant can:

-  Store almost any datatype.
-  Perform operations between many variants (GDScript uses Variant as
   its atomic/native datatype).
-  Be hashed, so it can be compared quickly to other variants.
-  Be used to convert safely between datatypes.
-  Be used to abstract calling methods and their arguments (Redot
   exports all its functions through variants).
-  Be used to defer calls or move data between threads.
-  Be serialized as binary and stored to disk, or transferred via
   network.
-  Be serialized to text and use it for printing values and editable
   settings.
-  Work as an exported property, so the editor can edit it universally.
-  Be used for dictionaries, arrays, parsers, etc.

Basically, thanks to the Variant class, writing Redot itself was a much,
much easier task, as it allows for highly dynamic constructs not common
of C++ with little effort. Become a friend of Variant today.

:::note

All types within Variant except Nil and Object **cannot** be ``null`` and
must always store a valid value. These types within Variant are therefore
called *non-nullable* types.

One of the Variant types is *Nil* which can only store the value ``null``.
Therefore, it is possible for a Variant to contain the value ``null``, even
though all Variant types excluding Nil and Object are non-nullable.

:::

### References

-  [core/variant/variant.h](https://github.com/redot-engine/redot-engine/blob/master/core/variant/variant.h)

## List of variant types

These types are available in Variant:

| Type | Notes |
| --- | --- |
| Nil (can only store ``null``) | Nullable type |
| [class_bool](/docs/Classes/bool) |  |
| [class_int](/docs/Classes/int) |  |
| [class_float](/docs/Classes/float) |  |
| [class_string](/docs/Classes/string) |  |
| [class_vector2](/docs/Classes/vector2) |  |
| [class_vector2i](/docs/Classes/vector2i) |  |
| [class_rect2](/docs/Classes/rect2) | 2D counterpart of AABB |
| [class_rect2i](/docs/Classes/rect2i) |  |
| [class_vector3](/docs/Classes/vector3) |  |
| [class_vector3i](/docs/Classes/vector3i) |  |
| [class_transform2d](/docs/Classes/transform2d) |  |
| [class_vector4](/docs/Classes/vector4) |  |
| [class_vector4i](/docs/Classes/vector4i) |  |
| [class_plane](/docs/Classes/plane) |  |
| [class_quaternion](/docs/Classes/quaternion) |  |
| [class_aabb](/docs/Classes/aabb) | 3D counterpart of Rect2 |
| [class_basis](/docs/Classes/basis) |  |
| [class_transform3d](/docs/Classes/transform3d) |  |
| [class_projection](/docs/Classes/projection) |  |
| [class_color](/docs/Classes/color) |  |
| [class_stringname](/docs/Classes/stringname) |  |
| [class_nodepath](/docs/Classes/nodepath) |  |
| [class_rid](/docs/Classes/rid) |  |
| [class_object](/docs/Classes/object) | Nullable type |
| [class_callable](/docs/Classes/callable) |  |
| [class_signal](/docs/Classes/signal) |  |
| [class_dictionary](/docs/Classes/dictionary) |  |
| [class_array](/docs/Classes/array) |  |
| [class_packedbytearray](/docs/Classes/packedbytearray) |  |
| [class_packedint32array](/docs/Classes/packedint32array) |  |
| [class_packedint64array](/docs/Classes/packedint64array) |  |
| [class_packedfloat32array](/docs/Classes/packedfloat32array) |  |
| [class_packedfloat64array](/docs/Classes/packedfloat64array) |  |
| [class_packedstringarray](/docs/Classes/packedstringarray) |  |
| [class_packedvector2array](/docs/Classes/packedvector2array) |  |
| [class_packedvector3array](/docs/Classes/packedvector3array) |  |
| [class_packedcolorarray](/docs/Classes/packedcolorarray) |  |
| [class_packedvector4array](/docs/Classes/packedvector4array) |  |

## Containers: Array and Dictionary

Both [class_array](/docs/Classes/array) and [class_dictionary](/docs/Classes/dictionary) are implemented using
variants. A Dictionary can match any datatype used as key to any other datatype.
An Array just holds an array of Variants. Of course, a Variant can also hold a
Dictionary or an Array inside, making it even more flexible.

Modifications to a container will modify all references to
it. A Mutex should be created to lock it if
[multi-threaded access](../../../tutorials/performance/using_multiple_threads.md) is desired.

### References

-  [core/variant/dictionary.h](https://github.com/redot-engine/redot-engine/blob/master/core/variant/dictionary.h)
-  [core/variant/array.h](https://github.com/redot-engine/redot-engine/blob/master/core/variant/array.h)