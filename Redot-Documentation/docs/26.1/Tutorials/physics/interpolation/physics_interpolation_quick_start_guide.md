
# Quick start guide

- Turn on physics interpolation: [Project Settings &gt; Physics &gt; Common &gt; Physics Interpolation](class_ProjectSettings_property_physics/common/physics_interpolation)
- Make sure you move objects and run your game logic in ``_physics_process()``
  rather than ``_process()``. This includes moving objects directly *and
  indirectly* (by e.g. moving a parent, or using another mechanism to automatically
  move nodes).
- Be sure to call [Node.reset_physics_interpolation](class_Node_method_reset_physics_interpolation)
  on nodes *after* you first position or teleport them, to prevent "streaking".
- Temporarily try setting [Project Settings &gt; Physics &gt; Common &gt; Physics Tick per Second](class_ProjectSettings_property_physics/common/physics_ticks_per_second)
  to 10 to see the difference with and without interpolation.