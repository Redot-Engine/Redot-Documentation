
# C++ usage guidelines

As of Redot 26.3, we are moving to C++20. Below are some guidelines for C++ usage in Redot.

:::info

See [doc_code_style_guidelines](doc_code_style_guidelines) for formatting guidelines.

:::

## Disallowed features

**Any feature not listed below is allowed.** Using features like ``constexpr``
variables and ``nullptr`` is encouraged when possible. Still, try to keep your
use of modern C++ features conservative. Their use needs to serve a real
purpose, such as improving code readability or performance.

### Standard Template Library

We don't allow using the [STL](https://en.wikipedia.org/wiki/Standard_Template_Library)
as Redot provides its own data types (among other things).
See [doc_faq_why_not_stl](doc_faq_why_not_stl) for more information.

This means that pull requests should **not** use ``std::string``,
``std::vector`` and the like. Instead, use Redot's datatypes as described below:

- Use ``String`` instead of ``std::string``.
- Use ``Vector`` instead of ``std::vector``. In some cases, ``LocalVector``
  can be used as an alternative (ask core developers first).
- Use ``Array`` instead of ``std::array``.

:::note

Redot also has a List datatype (which is a linked list). While List is already used
in the codebase, it typically performs worse than other datatypes like Vector
and Array. Therefore, List should be avoided in new code unless necessary.

:::

### ``auto`` keyword

Please be conservative with the use of the ``auto`` keyword for type inference. While it can avoid
repetition, it can also lead to confusing code:

```cpp
// Not so confusing...
auto button = memnew(Button);

// ...but what about this?
auto result = EditorNode::get_singleton()->get_complex_result();

```

Keep in mind hover documentation often isn't readily available for pull request
reviewers. Most of the time, reviewers will use GitHub's online viewer to review
pull requests.

While we are not forbidding its usage outright, it should only be used when the type is obvious and 
the code is not too complex.

### Lambdas

Lambdas should be used conservatively when they make code effectively faster or
simpler, and do not impede readability. Please ask before using lambdas in a
pull request.

### ``#pragma once`` directive

Please prefer ``#pragma once`` in new files over ``#ifdef``-based include guards.

:::info

See [doc_code_style_guidelines_header_includes](doc_code_style_guidelines_header_includes) for guidelines on sorting
includes in C++ and Objective-C files.

:::
