using Microsoft.Xna.Framework.Input;
using Terraria;

namespace DPSPanel.Core.Utilities;

public static class KeyboardHelper
{
    public static bool Pressed(Keys key) => Main.keyState.IsKeyDown(key) && Main.oldKeyState.IsKeyUp(key);
}
