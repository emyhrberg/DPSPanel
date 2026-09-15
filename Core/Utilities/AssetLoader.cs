using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria.ModLoader;

namespace DPSPanel.Core.Utilities;

/// <summary>
/// To add a new asset, simply add a new field like:
/// public static Asset<Texture2D> MyAsset;
/// </summary>
public static class Ass
{
    // My textures
    public static Asset<Texture2D> Default;
    public static Asset<Texture2D> DefaultLarge;
    public static Asset<Texture2D> BarFill;
    public static Asset<Texture2D> BarFillLarge;
    public static Asset<Texture2D> ToggleButton;
    public static Asset<Texture2D> ToggleButtonHighlighted;
    public static Asset<Texture2D> IconLock;
    public static Asset<Texture2D> ConfigUnknownItem;


    // Block's Combo Textures
    public static Asset<Texture2D> Fancy;
    public static Asset<Texture2D> FancyLarge;
    public static Asset<Texture2D> Golden;
    public static Asset<Texture2D> GoldenLarge;
    public static Asset<Texture2D> Leaf;
    public static Asset<Texture2D> LeafLarge;
    public static Asset<Texture2D> Retro;
    public static Asset<Texture2D> RetroLarge;
    public static Asset<Texture2D> Sticks;
    public static Asset<Texture2D> SticksLarge;
    public static Asset<Texture2D> StoneGold;
    public static Asset<Texture2D> StoneGoldLarge;
    public static Asset<Texture2D> Tribute;
    public static Asset<Texture2D> TributeLarge;
    public static Asset<Texture2D> TwigLeaf;
    public static Asset<Texture2D> TwigLeafLarge;
    public static Asset<Texture2D> Valkyrie;
    public static Asset<Texture2D> ValkyrieLarge;

    // Bool for checking if assets are loaded
    public static bool Initialized { get; set; }

    // Constructor
    static Ass()
    {
        foreach (FieldInfo field in typeof(Ass).GetFields())
        {
            if (field.FieldType == typeof(Asset<Texture2D>))
            {
                field.SetValue(null, RequestAsset(field.Name));
            }
        }
    }

    private static Asset<Texture2D> RequestAsset(string path)
    {
        return ModContent.Request<Texture2D>($"DPSPanel/Assets/" + path, AssetRequestMode.AsyncLoad);
    }
}

/// <summary>
/// Initializes asset loading for the mod when the system is loaded with all assets in <see cref="Ass"/>
/// </summary>
[Autoload(Side = ModSide.Client)]
public class AssetLoader : ModSystem
{
    public override void Load() => _ = Ass.Initialized;
}

internal sealed class MissingAssetException : Exception
{
    public override string HelpLink => "https://github.com/tModLoader/tModLoader/wiki/Basic-tModLoader-Modding-FAQ#terrariamodloadermodgettexturestring-name-error";

    public MissingAssetException(List<(string AssetName, string Path)> missingAssets, ICollection<string> validKeys)
        : base(BuildErrorMessage(missingAssets, validKeys))
    {
    }

    private static string BuildErrorMessage(List<(string AssetName, string Path)> missingAssets, ICollection<string> validKeys)
    {
        string message = $"--------------\nMOD CRASH! Missing {missingAssets.Count} texture asset(s):\n\n";

        foreach (var missing in missingAssets)
        {
            message += $"Failed to load Ass.{missing.AssetName}: \"{missing.Path}\"\n";

            if (validKeys != null && validKeys.Count > 0)
            {
                string closestMatch = LevenshteinDistance.FolderAwareEditDistance(missing.Path, validKeys.ToArray());
                if (!string.IsNullOrEmpty(closestMatch))
                {
                    (string a, string b) = LevenshteinDistance.ComputeColorTaggedString(missing.Path, closestMatch);
                    message += $"Did you mean \"{closestMatch}\"?\n";
                    message += $"{a}\n{b}\n";
                }
            }
            message += "\n"; // Space between missing items
        }

        message += "--------------\n";
        message += "Tip: The most common reason for this error is a malformed .png file or a typo in the path. Make sure you are saving textures in the .png format and are not just renaming the file extension of your texture files to .png, that does not work.";

        return message;
    }
}


/// <summary>
/// Short summary:
/// Computes the Levenshtein distance between two strings, which is a measure of how many single-character edits (insertions, deletions, or substitutions) are required to change one string into the other. This implementation also includes a method to compute a folder-aware edit distance for file paths, taking into account the structure of directories and files. Additionally, it provides a method to generate color-tagged strings that visually represent the differences between two strings.
/// </summary>
static class LevenshteinDistance
{
    enum Edits
    {
        Keep, Delete, Insert, Substitute, Blank
    }

    internal static string FolderAwareEditDistance(string source, string[] targets)
    {
        if (targets.Length == 0) return null;
        var separator = '/';
        var sourceParts = source.Split(separator);

        var sourceFolders = Enumerable.Reverse(sourceParts).Skip(1).ToList();
        var sourceFile = sourceParts.Last();

        int missingFolderPenalty = 4;
        int extraFolderPenalty = 3;

        var scores = targets.Select(target => {
            var targetParts = target.Split(separator);

            var targetFolders = Enumerable.Reverse(targetParts).Skip(1).ToList();
            var targetFile = targetParts.Last();

            var commonFolders = sourceFolders.Where(x => targetFolders.Contains(x));
            var reducedSourceFolders = sourceFolders.Except(commonFolders).ToList();
            var reducedTargetFolders = targetFolders.Except(commonFolders).ToList();

            int score = 0;
            int folderDiff = reducedSourceFolders.Count - reducedTargetFolders.Count;
            if (folderDiff > 0)
                score += folderDiff * missingFolderPenalty;
            else if (folderDiff < 0)
                score += -folderDiff * extraFolderPenalty;

            if (reducedSourceFolders.Count > 0 && reducedSourceFolders.Count >= reducedTargetFolders.Count)
            {
                foreach (var item in reducedTargetFolders)
                {
                    int min = Int32.MaxValue;
                    foreach (var item2 in reducedSourceFolders)
                    {
                        min = Math.Min(min, LevenshteinDistance.Compute(item, item2));
                    }
                    score += min;
                }
            }
            else if (reducedSourceFolders.Count > 0)
            {
                foreach (var item in reducedSourceFolders)
                {
                    int min = Int32.MaxValue;
                    foreach (var item2 in reducedTargetFolders)
                    {
                        min = Math.Min(min, LevenshteinDistance.Compute(item, item2));
                    }
                    score += min;
                }
            }
            score += LevenshteinDistance.Compute(targetFile, sourceFile);

            return new
            {
                Target = target,
                Score = score
            };
        });
        return scores.OrderBy(x => x.Score).First().Target;
    }

    public static int Compute(string s, string t)
    {
        int n = s.Length;
        int m = t.Length;
        int[,] d = new int[n + 1, m + 1];

        if (n == 0) return m;
        if (m == 0) return n;

        for (int i = 0; i <= n; d[i, 0] = i++) { }
        for (int j = 0; j <= m; d[0, j] = j++) { }

        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= m; j++)
            {
                int cost = (t[j - 1] == s[i - 1]) ? 0 : 2;
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 2, d[i, j - 1] + 2),
                    d[i - 1, j - 1] + cost);
            }
        }
        return d[n, m];
    }

    public static (string, string) ComputeColorTaggedString(string s, string t)
    {
        int n = s.Length;
        int m = t.Length;
        int[,] d = new int[n + 1, m + 1];

        if (n == 0) return ("", "");
        if (m == 0) return ("", "");

        for (int i = 0; i <= n; d[i, 0] = i++) { }
        for (int j = 0; j <= m; d[0, j] = j++) { }

        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= m; j++)
            {
                int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }

        var x = n;
        var y = m;
        var editsFromStoT = new Stack<(Edits, char)>();
        var editsFromTtoS = new Stack<(Edits, char)>();

        while (x != 0 || y != 0)
        {
            var cost = d[x, y];
            if (y - 1 < 0)
            {
                editsFromStoT.Push((Edits.Delete, s[x - 1]));
                editsFromTtoS.Push((Edits.Blank, ' '));
                x--;
                continue;
            }

            if (x - 1 < 0)
            {
                editsFromStoT.Push((Edits.Insert, t[y - 1]));
                editsFromTtoS.Push((Edits.Blank, ' '));
                y--;
                continue;
            }

            var costLeft = d[x, y - 1];
            var costUp = d[x - 1, y];
            var costDiagonal = d[x - 1, y - 1];

            if (costDiagonal <= costLeft && costDiagonal <= costUp && (costDiagonal == cost - 1 || costDiagonal == cost))
            {
                if (costDiagonal == cost - 1)
                {
                    editsFromStoT.Push((Edits.Substitute, s[x - 1]));
                    editsFromTtoS.Push((Edits.Substitute, t[y - 1]));
                    x--; y--;
                }
                else
                {
                    editsFromStoT.Push((Edits.Keep, s[x - 1]));
                    editsFromTtoS.Push((Edits.Keep, t[y - 1]));
                    x--; y--;
                }
            }
            else if (costLeft <= costDiagonal && costLeft == cost - 1)
            {
                editsFromStoT.Push((Edits.Insert, t[y - 1]));
                editsFromTtoS.Push((Edits.Blank, ' '));
                y--;
            }
            else
            {
                editsFromStoT.Push((Edits.Delete, s[x - 1]));
                editsFromTtoS.Push((Edits.Blank, ' '));
                x--;
            }
        }

        string FinalizeText(Stack<(Edits, char)> results)
        {
            string result = "";
            Edits editCurrent = Edits.Keep;
            while (results.Count > 0)
            {
                var entry = results.Pop();
                Edits nextEdit = entry.Item1;
                if (editCurrent != nextEdit)
                {
                    if (editCurrent != Edits.Keep && editCurrent != Edits.Blank) result += "]";
                    if (nextEdit == Edits.Delete) result += "[c/ff0000:";
                    else if (nextEdit == Edits.Insert) result += "[c/00ff00:";
                    else if (nextEdit == Edits.Substitute) result += "[c/ffff00:";
                }
                result += entry.Item2;
                editCurrent = nextEdit;
            }
            if (editCurrent != Edits.Keep && editCurrent != Edits.Blank) result += "]";
            return result;
        }

        return (FinalizeText(editsFromStoT), FinalizeText(editsFromTtoS));
    }
}

