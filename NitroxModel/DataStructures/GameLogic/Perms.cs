namespace NitroxModel.DataStructures.GameLogic;

public enum Perms : byte
{
    NONE,
    PLAYER,
    MODERATOR,
    ADMIN,
    CONSOLE,
    DEFAULT = PLAYER
}
