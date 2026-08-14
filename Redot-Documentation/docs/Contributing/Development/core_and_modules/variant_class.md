
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
| [bool](class_bool) |  |
| [int](class_int) |  |
| [float](class_float) |  |
| [string](class_string) |  |
| [Vector2](class_Vector2) |  |
| [Vector2i](class_Vector2i) |  |
| [Rect2](class_Rect2) | 2D counterpart of AABB |
| [Rect2i](class_Rect2i) |  |
| [Vector3](class_Vector3) |  |
| [Vector3i](class_Vector3i) |  |
| [Transform2d](class_Transform2d) |  |
| [Vector4](class_Vector4) |  |
| [Vector4i](class_Vector4i) |  |
| [Plane](class_Plane) |  |
| [Quaternion](class_Quaternion) |  |
| [AABB](class_AABB) | 3D counterpart of Rect2 |
| [Basis](class_Basis) |  |
| [Transform3d](class_Transform3d) |  |
| [Projection](class_Projection) |  |
| [Color](class_Color) |  |
| [StringName](class_StringName) |  |
| [NodePath](class_NodePath) |  |
| [RID](class_RID) |  |
| [Object](class_Object) | Nullable type |
| [Callable](class_Callable) |  |
| [Signal](class_Signal) |  |
| [Dictionary](class_Dictionary) |  |
| [Array](class_Array) |  |
| [PackedByteArray](class_PackedByteArray) |  |
| [PackedInt32Array](class_PackedInt32Array) |  |
| [PackedInt64Array](class_PackedInt64Array) |  |
| [PackedFloat32Array](class_PackedFloat32Array) |  |
| [PackedFloat64Array](class_PackedFloat64Array) |  |
| [PackedStringArray](class_PackedStringArray) |  |
| [PackedVector2Array](class_PackedVector2Array) |  |
| [PackedVector3Array](class_PackedVector3Array) |  |
| [PackedColorArray](class_PackedColorArray) |  |
| [PackedVector4Array](class_PackedVector4Array) |  |

## Containers: Array and Dictionary

Both [class_Array](class_Array) and [class_Dictionary](class_Dictionary) are implemented using
variants. A Dictionary can match any datatype used as key to any other datatype.
An Array just holds an array of Variants. Of course, a Variant can also hold a
Dictionary or an Array inside, making it even more flexible.

<!-- TODO(Tekk): doc_using_multiple_threads doesnt exist -->
Modifications to a container will modify all references to
it. A Mutex should be created to lock it if
[multi-threaded access](doc_using_multiple_threads) is desired.

### References

-  [core/variant/dictionary.h](https://github.com/redot-engine/redot-engine/blob/master/core/variant/dictionary.h)
-  [core/variant/array.h](https://github.com/redot-engine/redot-engine/blob/master/core/variant/array.h)