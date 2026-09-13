# Your first 3D game

In this step-by-step tutorial series, you will create your first complete 3D
game with Redot. By the end of the series, you will have a simple yet finished
project of your own like the animated gif below.

![Image](/img/GettingStarted/first_3d_game/img/squash-the-creeps-final.gif)

The game we'll code here is similar to [doc_your_first_2d_game](/en/26.1/Getting%20Started/first_2d_game/index), with a twist:
you can now jump and your goal is to squash the creeps. This way, you will both
**recognize patterns** you learned in the previous tutorial and **build upon
them** with new code and features.

You will learn to:

- Work with 3D coordinates with a jumping mechanic.
- Use kinematic bodies to move 3D characters and detect when and how they
  collide.
- Use physics layers and a group to detect interactions with specific entities.
- Code basic procedural gameplay by instancing monsters at regular time
  intervals.
- Design a movement animation and change its speed at runtime.
- Draw a user interface on a 3D game.

And more.

This tutorial is for beginners who followed the complete getting started series.
We'll start slow with detailed instructions and shorten them as we do similar
steps. If you're an experienced programmer, you can browse the complete demo's
source code here: `Squash the Creep source code
&lt;https://github.com/redot-engine/redot-3d-dodge-the-creeps&gt;`__.

:::note

You can follow this series without having done the 2D one. However, if
you're new to game development, we recommend you to start with 2D. 3D game
code is always more complex and the 2D series will give you foundations to
follow along more comfortably.

:::

We prepared some game assets so we can jump straight to the code. You can
download them here: `Squash the Creeps assets
&lt;https://github.com/redot-engine/redot-3d-dodge-the-creeps/releases/tag/1.1.0&gt;`__.

We will first work on a basic prototype for the player's movement. We will then
add the monsters that we'll spawn randomly around the screen. After that, we'll
implement the jump and squashing mechanic before refining the game with some
nice animation. We'll wrap up with the score and the retry screen.

## In this section

- [Setting up the game area](/en/26.1/Getting%20Started/first_3d_game/01.game_setup)
- [Player scene and input actions](/en/26.1/Getting%20Started/first_3d_game/02.player_input)
- [Moving the player with code](/en/26.1/Getting%20Started/first_3d_game/03.player_movement_code)
- [Designing the mob scene](/en/26.1/Getting%20Started/first_3d_game/04.mob_scene)
- [Spawning monsters](/en/26.1/Getting%20Started/first_3d_game/05.spawning_mobs)
- [Jumping and squashing monsters](/en/26.1/Getting%20Started/first_3d_game/06.jump_and_squash)
- [Killing the player](/en/26.1/Getting%20Started/first_3d_game/07.killing_player)
- [Score and replay](/en/26.1/Getting%20Started/first_3d_game/08.score_and_replay)
- [Character animation](/en/26.1/Getting%20Started/first_3d_game/09.adding_animations)
- [Going further](/en/26.1/Getting%20Started/first_3d_game/going_further)
