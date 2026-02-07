Required

NET 9.0

to run the program use

dotnet run --project .\LuauGotoStripper 

simple

Before:

local a = 1

if a == 1 then goto label_53 end -- goto label_53
print("this stays")

::label_53:: -- ::label_53::
goto label_12 -- goto label_12

After

local a = 1

if a == 1 then end
print("this stays")


print("done")

this removes goto and label from luadec generated files command is /stripgoto alr thats all thanks
