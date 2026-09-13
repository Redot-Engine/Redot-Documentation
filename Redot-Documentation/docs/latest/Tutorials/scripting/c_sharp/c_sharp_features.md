
# C# language features

This page provides an overview of the commonly used features of both C# and Redot
and how they are used together.

## Type conversion and casting

C# is a statically typed language. Therefore, you can't do the following:

```csharp
var mySprite = GetNode("MySprite");
mySprite.SetFrame(0);

```

The method ``GetNode()`` returns a ``Node`` instance.
You must explicitly convert it to the desired derived type, ``Sprite2D`` in this case.

For this, you have various options in C#.

**Casting and Type Checking**

Throws ``InvalidCastException`` if the returned node cannot be cast to Sprite2D.
You would use it instead of the ``as`` operator if you are pretty sure it won't fail.

```csharp
Sprite2D mySprite = (Sprite2D)GetNode("MySprite");
mySprite.SetFrame(0);

```

**Using the AS operator**

The ``as`` operator returns ``null`` if the node cannot be cast to Sprite2D,
and for that reason, it cannot be used with value types.

```csharp
Sprite2D mySprite = GetNode("MySprite") as Sprite2D;
// Only call SetFrame() if mySprite is not null
mySprite?.SetFrame(0);

```

**Using the generic methods**

Generic methods are also provided to make this type conversion transparent.

``GetNode&lt;T&gt;()`` casts the node before returning it. It will throw an ``InvalidCastException`` if the node cannot be cast to the desired type.

```csharp
Sprite2D mySprite = GetNode<Sprite2D>("MySprite");
mySprite.SetFrame(0);

```

``GetNodeOrNull&lt;T&gt;()`` uses the ``as`` operator and will return ``null`` if the node cannot be cast to the desired type.

```csharp
Sprite2D mySprite = GetNodeOrNull<Sprite2D>("MySprite");
// Only call SetFrame() if mySprite is not null
mySprite?.SetFrame(0);

```

**Type checking using the IS operator**

To check if the node can be cast to Sprite2D, you can use the ``is`` operator.
The ``is`` operator returns false if the node cannot be cast to Sprite2D,
otherwise it returns true. Note that when the ``is`` operator is used against ``null``
the result is always going to be ``false``.

```csharp
if (GetNode("MySprite") is Sprite2D)
{
    // Yup, it's a Sprite2D!
}

if (null is Sprite2D)
{
    // This block can never happen.
}

```

You can also declare a new variable to conditionally store the result of the cast
if the ``is`` operator returns ``true``.

```csharp
if (GetNode("MySprite") is Sprite2D mySprite)
{
    // The mySprite variable only exists inside this block, and it's never null.
    mySprite.SetFrame(0);
}

```

For more advanced type checking, you can look into [Pattern Matching ](https://docs.microsoft.com/en-us/dotnet/csharp/pattern-matching).

## Preprocessor defines

Redot has a set of defines that allow you to change your C# code
depending on the environment you are compiling to.

### Examples

For example, you can change code based on the platform:

```csharp
public override void _Ready()
{
```

    #if (Redot_32 || Redot_MOBILE || Redot_WEB)
            // Use simple objects when running on less powerful systems.
            SpawnSimpleObjects();
    #else
            SpawnComplexObjects();
    #endif
        }

Or you can detect which engine your code is in, useful for making cross-engine libraries:

```csharp
public void MyPlatformPrinter()
{
```

    #if Redot
            GD.Print("This is Redot.");
    #elif UNITY_5_3_OR_NEWER
            print("This is Unity.");
    #else
            throw new NotSupportedException("Only Redot and Unity are supported.");
    #endif
        }

Or you can write scripts that target multiple Redot versions and take
advantage of features that are only available on some of those versions:

```csharp
public void UseCoolFeature()
{
```

    #if Redot4_3_OR_GREATER || Redot4_2_2_OR_GREATER
            // Use CoolFeature, that was added to Redot in 4.3 and cherry-picked into 4.2.2, here.
    #else
            // Use a workaround for the absence of CoolFeature here.
    #endif
        }

### Full list of defines

* ``Redot`` is always defined for Redot projects.

* ``TOOLS`` is defined when building with the Debug configuration (editor and editor player).

* ``Redot_REAL_T_IS_DOUBLE`` is defined when the ``RedotFloat64`` property is set to ``true``.

* One of ``Redot_64`` or ``Redot_32`` is defined depending on if the architecture is 64-bit or 32-bit.

* One of ``Redot_LINUXBSD``, ``Redot_WINDOWS``, ``Redot_OSX``,
  ``Redot_ANDROID``, ``Redot_IOS``, ``Redot_WEB``
  depending on the OS. These names may change in the future.
  These are created from the ``get_name()`` method of the
  [OS ](class_OS) singleton, but not every possible OS
  the method returns is an OS that Redot with .NET runs on.

* ``RedotX``, ``RedotX_Y``, ``RedotX_Y_Z``, ``Redotx_OR_GREATER``,
  ``RedotX_y_OR_GREATER``, and ``RedotX_Y_z_OR_GREATER``, where ``X``, ``Y``,
  and ``Z`` are replaced by the current major, minor and patch version of Redot.
  ``x``, ``y``, and ``z`` are replaced by all values from 0 to the current version number for that
  component.

:::note

These defines were first added in Redot 4.0.4 and 4.1. Version defines for
prior versions do not exist, regardless of the current Redot version.

:::

  For example: Redot 4.0.5 defines ``Redot4``, ``Redot4_OR_GREATER``,
  ``Redot4_0``, ``Redot4_0_OR_GREATER``, ``Redot4_0_5``,
  ``Redot4_0_4_OR_GREATER``, and ``Redot4_0_5_OR_GREATER``. Redot 4.3.2 defines
  ``Redot4``, ``Redot4_OR_GREATER``, ``Redot4_3``, ``Redot4_0_OR_GREATER``,
  ``Redot4_1_OR_GREATER``, ``Redot4_2_OR_GREATER``, ``Redot4_3_OR_GREATER``,
  ``Redot4_3_2``, ``Redot4_3_0_OR_GREATER``, ``Redot4_3_1_OR_GREATER``, and
  ``Redot4_3_2_OR_GREATER``.

When **exporting**, the following may also be defined depending on the export features:

* One of ``Redot_PC``, ``Redot_MOBILE``, or ``Redot_WEB`` depending on the platform type.

* One of ``Redot_WINDOWS``, ``Redot_LINUXBSD``, ``Redot_MACOS``, ``Redot_ANDROID``, ``Redot_IOS``, or ``Redot_WEB`` depending on the platform.

To see an example project, see the OS testing demo:
https://github.com/redot-engine/redot-demo-projects/tree/master/misc/os_test