// using System;
// using Microsoft.Xna.Framework;
// using MonoMod.Cil;
// using Terraria.Graphics.Renderers;
// using Terraria.ModLoader;

// namespace UI
// {
//     public class PlayerHeadFlipSystem : ModSystem
//     {
//         public static bool shouldFlipHeadDraw = true;

//         public override void Load()
//         {
//             MonoModHooks.Modify(typeof(MapHeadRenderer).GetMethod("DrawPlayerHead"), IL_MapHeadRenderer_DrawPlayerHead);
//         }

//         public static void IL_MapHeadRenderer_DrawPlayerHead(ILContext il)
//         {
//             try
//             {
//                 ILCursor c = new ILCursor(il);

//                 // extra code
//                 c.GotoNext(MoveType.Before, i => i.MatchLdcR4(2));
//                 c.Index += 2;

//                 ILLabel skipCentering = il.DefineLabel();

//                 c.EmitLdsfld(typeof(PlayerHeadFlipSystem).GetField(nameof(shouldFlipHeadDraw)));
//                 c.EmitBrfalse(skipCentering);
//                 c.EmitDelegate<Func<Vector2, Vector2>>((Vector2 inCenter) =>
//                 {
//                     return new Vector2(inCenter.X * 0.8f, inCenter.Y);
//                 });
//                 c.MarkLabel(skipCentering);

//                 // find where the draw data loads the 
//                 c.GotoNext(MoveType.Before, i => i.MatchLdcI4(0));

//                 // define the labels for the if, else statement
//                 ILLabel normRet = il.DefineLabel();
//                 ILLabel altLabel = il.DefineLabel();

//                 c.EmitLdsfld(typeof(PlayerHeadFlipSystem).GetField(nameof(shouldFlipHeadDraw)));
//                 c.EmitBrtrue(altLabel); // if(!shouldFlipHeadDraw)
//                                         // {
//                 c.Index++;                // push 0
//                 c.EmitBr(normRet);        // }
//                                           // else
//                 c.MarkLabel(altLabel);    // {
//                 c.EmitLdcI4(1);            // push 1
//                 c.MarkLabel(normRet);    // }
//             }
//             catch (Exception e)
//             {
//                 // oop!
//                 // MonoModHooks.DumpIL(ModContent.GetInstance<XGWorld>(), il);
//             }
//         }
//     }
// }
