# Renderer consumers in scene 1

This search index lists direct renderer includes and selected renderer symbol
references. It includes conditional and test code. It is not a complete transitive
dependency graph. The manually assessed [dependency map](doc_rendering_dependencies)
explains the contracts. Follow each link to inspect the first matching source line.

| Source file | Matching lines |
|---|---|
| [scene/2d/back_buffer_copy.cpp](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/scene/2d/back_buffer_copy.cpp#L44) | 44, 47, 50 |
| [scene/2d/canvas_group.cpp](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/scene/2d/canvas_group.cpp#L45) | 45, 58, 69, 127 |
| [scene/2d/canvas_modulate.cpp](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/scene/2d/canvas_modulate.cpp#L52) | 52, 61, 63, 116 |
| [scene/2d/cpu_particles_2d.cpp](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/scene/2d/cpu_particles_2d.cpp#L88) | 88, 230, 231, 232, 233, 234, 236, 237, 1222, 1223, 1225, 1227, 1228, 1230, 1232, 1242, 1279, 1661, 1662, 1663, 1714, 1715, 1716 |
| [scene/2d/gpu_particles_2d.cpp](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/scene/2d/gpu_particles_2d.cpp#L76) | 76, 82, 88, 93, 98, 109, 114, 119, 130, 137, 151, 181, 188, 192, 198, 220, 248, 262, 318, 327, 336, 345, 370, 378, 411, 454, 462, 475, 492, 503, 504, 621, 674, 675, 676, 677, 678, 679, 681, 692, 695, 723, 724, 725, 726, 728, 729, 731, 746, 748, 756, 765, 767, 804, 989, 990, 992, 993, 994, 1019, 1020, 1021 |
| [scene/2d/gpu_particles_2d.h](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/scene/2d/gpu_particles_2d.h#L196) | 196, 197, 198, 199, 200 |
| [scene/2d/light_2d.cpp](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/scene/2d/light_2d.cpp#L66) | 66, 89, 98, 107, 116, 125, 134, 143, 152, 161, 170, 180, 190, 199, 207, 213, 218, 231, 232, 237, 245, 316, 317, 318, 319, 339, 344, 345, 416, 418, 430, 454, 491, 499, 515 |
| [scene/2d/light_occluder_2d.cpp](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/scene/2d/light_occluder_2d.cpp#L99) | 99, 114, 125, 156, 160, 161, 171, 177, 178, 179, 183, 187, 213, 222, 223, 248, 250, 267, 290, 312, 319, 321 |
| [scene/2d/line_2d.cpp](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/scene/2d/line_2d.cpp#L314) | 314 |
| [scene/2d/mode7_sprite_2d.cpp](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/scene/2d/mode7_sprite_2d.cpp#L458) | 458, 460, 461, 724, 725, 727, 728, 735 |
| [scene/2d/mode7_sprite_2d.h](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/scene/2d/mode7_sprite_2d.h#L155) | 155 |
| [scene/2d/navigation/navigation_agent_2d.cpp](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/scene/2d/navigation/navigation_agent_2d.cpp#L277) | 277, 366, 368, 1094, 1097, 1107, 1108, 1109, 1126, 1143 |
| [scene/2d/navigation/navigation_obstacle_2d.cpp](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/scene/2d/navigation/navigation_obstacle_2d.cpp#L107) | 107, 115, 151, 181, 182, 201, 202, 214, 218, 456, 471, 516 |
| [scene/2d/navigation/navigation_region_2d.cpp](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/scene/2d/navigation/navigation_region_2d.cpp#L410) | 410, 413, 463, 475, 593, 605, 654, 657 |
| [scene/2d/node_2d.cpp](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/scene/2d/node_2d.cpp#L147) | 147, 386 |
| [scene/2d/parallax_2d.cpp](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/scene/2d/parallax_2d.cpp#L152) | 152, 153 |
| [scene/2d/parallax_layer.cpp](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/scene/2d/parallax_layer.cpp#L83) | 83, 84 |
| [scene/2d/path_2d.cpp](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/scene/2d/path_2d.cpp#L120) | 120, 123, 127, 130, 131, 135, 138, 142, 148, 150, 153, 218, 253 |
| [scene/2d/polygon_2d.cpp](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/scene/2d/polygon_2d.cpp#L136) | 136, 139, 365, 369, 370, 372, 375, 379, 380, 383, 385, 403, 408, 409, 734, 739, 740, 741 |
| [scene/2d/skeleton_2d.cpp](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/scene/2d/skeleton_2d.cpp#L206) | 206, 207, 208, 209, 211, 220, 272, 273, 312, 313, 514, 515, 562, 616, 690, 703, 842, 848, 849 |
| [scene/2d/tile_map_layer.cpp](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/scene/2d/tile_map_layer.cpp#L71) | 71, 171, 213, 354, 355, 418, 432, 433, 467, 584, 596, 689, 1078, 1246, 1253, 1455, 1631, 2048, 2254, 2262, 2661, 2668 |
| [scene/2d/tile_map_layer.h](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/scene/2d/tile_map_layer.h#L524) | 524, 525 |
| [scene/2d/visible_on_screen_notifier_2d.cpp](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/scene/2d/visible_on_screen_notifier_2d.cpp#L91) | 91, 116, 127 |
| [scene/3d/camera_3d.cpp](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/scene/3d/camera_3d.cpp#L83) | 83, 88, 93, 105, 160, 310, 327, 344, 529, 531, 557, 559, 583, 585, 596, 761, 867, 869, 877, 878 |
| [scene/3d/cpu_particles_3d.cpp](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/scene/3d/cpu_particles_3d.cpp#L92) | 92, 93, 120, 193, 195, 569, 570, 1349, 1350, 1351, 1353, 1354, 1356, 1357, 1366, 1791, 1792, 1844, 1845 |
| [scene/3d/decal.cpp](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/scene/3d/decal.cpp#L43) | 43, 69, 80, 89, 98, 107, 116, 125, 134, 144, 153, 162, 303, 304, 308, 309 |
| [scene/3d/fog_volume.cpp](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/scene/3d/fog_volume.cpp#L60) | 60, 87, 95, 97, 98, 103, 113, 122, 146, 147, 152, 153 |
| [scene/3d/fog_volume.h](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/scene/3d/fog_volume.h#L50) | 50, 67, 68 |
| [scene/3d/gpu_particles_3d.cpp](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/scene/3d/gpu_particles_3d.cpp#L79) | 79, 85, 91, 96, 101, 105, 124, 133, 138, 143, 148, 154, 176, 183, 188, 245, 250, 257, 278, 304, 318, 327, 336, 448, 449, 461, 482, 486, 494, 501, 524, 555, 568, 570, 578, 587, 589, 596, 597, 623, 639, 739, 886, 887, 914, 915 |
| [scene/3d/gpu_particles_3d.h](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/scene/3d/gpu_particles_3d.h#L202) | 202, 203, 204, 205, 206 |
| [scene/3d/gpu_particles_collision_3d.cpp](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/scene/3d/gpu_particles_collision_3d.cpp#L48) | 48, 62, 63, 64, 69, 70, 84, 97, 132, 145, 605, 650, 666, 679, 710, 717, 779, 781, 790, 792, 810, 849, 860, 869, 878, 887, 914, 915, 916, 920, 921, 935, 948, 983, 996, 1035, 1046, 1058 |
| [scene/3d/gpu_particles_collision_3d.h](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/scene/3d/gpu_particles_collision_3d.h#L54) | 54, 291 |
| [scene/3d/label_3d.cpp](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/scene/3d/label_3d.cpp#L153) | 153, 154, 378, 380, 381, 382, 383, 384, 385, 386, 387, 388, 389, 390, 392, 393, 408, 409, 410, 412, 476, 481, 654, 655, 656, 657, 658, 659, 660, 662, 663, 667, 775, 787, 1094, 1113, 1114, 1116 |
| [scene/3d/light_3d.cpp](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/scene/3d/light_3d.cpp#L47) | 47, 65, 76, 85, 95, 104, 113, 122, 135, 137, 149, 158, 166, 169, 172, 200, 224, 274, 308, 333, 336, 458, 461, 462, 464, 465, 467, 468, 474, 513, 514, 517, 525, 535, 544, 608, 622, 657, 688 |
| [scene/3d/light_3d.h](https://github.com/Redot-Engine/redot-engine/blob/82c6c949656f8af31a37e0381048beb381fcbb27/scene/3d/light_3d.h#L48) | 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63, 64, 65, 66, 67, 68, 69, 90, 109, 112 |
