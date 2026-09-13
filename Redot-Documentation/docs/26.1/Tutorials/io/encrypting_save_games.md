
# Encrypting save games

The class [FileAccess ](class_FileAccess) can open a file at a
location and read/write data (integers, strings and variants).
It also supports encryption.
To create an encrypted file, a passphrase must be provided, like this:

<Tabs>

<TabItem value="gdscript" label="GDScript">

```gdscript
var f = FileAccess.open_encrypted_with_pass("user://savedata.bin", FileAccess.WRITE, "mypass")
f.store_var(game_state)

```

</TabItem>

<TabItem value="csharp" label="Csharp">

```csharp
var f = FileAccess.OpenEncryptedWithPass("user://savedata.bin", (int)FileAccess.ModeFlags.Write, "mypass");
f.StoreVar(gameState);

```

</TabItem>

</Tabs>

This will make the file unreadable to users, but will still not prevent
them from sharing savefiles. To solve this, use the device unique id or
some unique user identifier, for example:

<Tabs>

<TabItem value="gdscript" label="GDScript">

```gdscript
var f = FileAccess.open_encrypted_with_pass("user://savedata.bin", FileAccess.WRITE, OS.get_unique_id())
f.store_var(game_state)

```

</TabItem>

<TabItem value="csharp" label="Csharp">

```csharp
var f = FileAccess.OpenEncryptedWithPass("user://savedata.bin", (int)FileAccess.ModeFlags.Write, OS.GetUniqueId());
f.StoreVar(gameState);

```

</TabItem>

</Tabs>

Note that ``OS.get_unique_id()`` does not work on UWP or HTML5.

:::note
This method cannot really prevent players from editing their savegames
locally because, since the encryption key is stored inside the game, the player
can still decrypt and edit the file themselves. The only way to prevent this
from being possible is to store the save data on a remote server, where players
can only make authorized changes to their save data. If your game deals with
real money, you need to be doing this anyway.

:::
