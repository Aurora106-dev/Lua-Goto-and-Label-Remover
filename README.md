## Luau Goto Stripper

**Target Framework:** NET 9.0

This tool removes `goto` statements and label blocks from **luadec-generated Luau/Lua files**, producing cleaner and more readable output.

### Running the program

```bash
dotnet run --project .\LuauGotoStripper
```

### Command

```
/stripgoto
```

### Before:

```lua
local a = 1

if a == 1 then goto label_53 end -- goto label_53
print("this stays")

::label_53:: -- ::label_53::
goto label_12 -- goto label_12
```

### After:

```lua
local a = 1

if a == 1 then end
print("this stays")


print("done")
```
