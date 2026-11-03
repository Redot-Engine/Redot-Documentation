# Mermaid diagrams

Mermaid turns text into diagrams. Use it to show a series of steps, messages
between parts of a game, or changes in a character's state.

## Add your first diagram

Create a `diagrams` folder next to your page's `.md` file. Inside it, create a
text file named `scene-lifecycle.mmd`:

```text
example.md
diagrams/
    scene-lifecycle.mmd
```

Copy this text into the new file:

```text
flowchart LR
    accTitle: Scene lifecycle
    accDescr: A scene starts, runs, and then exits.
    Start --> Running --> Exit
```

The first line chooses a flowchart. `LR` means left to right, and `-->` joins
the boxes. `accTitle` and `accDescr` add a title and description.

Add this line to your `.md` page, outside a code block. Leave a blank line
before and after it:

```html
<mermaid ref="./diagrams/scene-lifecycle.mmd" />
```

`ref` names the file to display. `./` starts from the page's folder. Here is the
result:

<mermaid ref="./diagrams/scene-lifecycle.mmd" />

## Preview your page

Open a terminal in the repository's main folder and run:

```bash
dotnet run --project Redot-Documentation
```

Open the address shown in the terminal in your browser, then open your page.
After an `.mmd` edit, press `Ctrl+C` in the terminal and run the command again
to update the diagram. Check the result there even if it works in the Mermaid
Live Editor.

## Describe the diagram

`caption` adds text below a diagram. `alt` describes it for people who use
screen readers:

```html
<mermaid ref="./diagrams/scene-lifecycle.mmd"
         alt="A scene starts, runs, and then exits."
         caption="The three stages of the scene lifecycle." />
```

If you leave out `alt`, the site uses `accTitle`. If there is no title, it uses
the filename. Describe complex diagrams in the article too. Captions use plain
text.

Keep values in quotes. Use `&quot;` for a double quote inside a quoted value,
and `&amp;` for `&`.

## Use a shorter name

A **slug** is a name based on the filename: `Scene_Lifecycle.mmd` becomes
`scene-lifecycle`. Remove `.mmd`, use lowercase, and replace spaces or
punctuation with hyphens.

You can use the slug in place of `ref`:

```html
<mermaid slug="scene-lifecycle" />
```

Use `ref` or `slug`, never both. If a slug matches two files, use `ref`.

A separate closing tag works too. Leave it empty:

```html
<mermaid slug="scene-lifecycle"></mermaid>
```

These tags also work in tabs and callouts.

## Try other diagrams

### Sequence diagrams

Read sequence diagrams from top to bottom. This one shows a game loading a
level. Save it as `diagrams/scene-loading-sequence.mmd`:

```text
sequenceDiagram
    accTitle: Scene loading sequence
    accDescr: The game asks a loader to read assets, then receives a scene-ready reply.
    participant Game
    participant Loader
    participant Assets
    Game->>Loader: Load level
    Loader->>Assets: Read resources
    Assets-->>Loader: Resources ready
    Loader-->>Game: Scene ready
```

Each `participant` names a column. `->>` draws a solid arrow; `-->>` draws a
dashed arrow.

<mermaid ref="./diagrams/scene-loading-sequence.mmd"
         alt="The game requests a scene, the loader reads assets, then sends a ready reply."
         caption="A request and its replies between three participants."></mermaid>

### State diagrams

A state diagram shows how a character switches between states, such as idle
and moving. Save this example as `diagrams/character-movement-states.mmd`:

```text
stateDiagram-v2
    accTitle: Character movement states
    accDescr: A character starts idle, moves while input is active, and returns to idle before removal.
    [*] --> Idle
    Idle --> Moving: Input received
    Moving --> Idle: Input released
    Idle --> [*]: Removed
```

Read `Idle --> Moving: Input received` as "input changes Idle to Moving." Text
after `:` labels the arrow; `[*]` marks the start or end.

<mermaid slug="character-movement-states"
         alt="A character moves when input arrives, stops when it ends, then can be removed."
         caption="Named states and labeled transitions." />

## Keep versions together

Keep a diagram with its page. A page in `docs/26.3/` looks for slugs in `26.3`,
not `latest`. Copy both pages and diagrams when you create a new version.

In versioned pages, `/` starts at that version's folder. `../` goes up one
folder; keep the path within the same version. Use `.mmd` files from this
repository, not web addresses.

The `About`, `Community`, and `Contributing` folders share diagrams across
versions. Pages in these folders look for slugs among the shared diagrams.
To use a shared diagram in a versioned page, start its path at `docs/`:

```html
<mermaid ref="/Contributing/Documentation/diagrams/scene-lifecycle.mmd" />
```

Update the affected `ref` or `slug` when you move or rename a diagram.

## Before you submit

If you see **Diagram unavailable**, check the file path or slug and the diagram
text. Split diagrams that are hard to follow into smaller ones.

Run `dotnet test` from the repository's main folder to check diagrams and their
references across all configured versions.
