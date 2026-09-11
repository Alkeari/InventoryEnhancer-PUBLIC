using System.Xml;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs2;

namespace InventoryEnhancer.UI.Extensions;

/// <summary>
///     Phase 2 of the migration: a status line injected into each side of the vanilla inventory
///     screen, bound to the mixin's state.
///     Both anchors were validated mechanically against the shipped prefab: each selects exactly one
///     node, and each keys on an Id, a structural attribute, rather than on a binding string. Neither
///     is touched by any other installed module. The two panels are vertical stacks whose only
///     siblings are the item list and the search box, so a line drops in with no coordinate math.
///     Per side rather than one center bar, because this mod's filter and sort state is already per
///     side: the left and right panels genuinely have different answers.
///     The brush is one the vanilla inventory prefab itself uses. Gauntlet resolves
///     brushes per module, so a brush borrowed from another screen's markup is not evidence that it
///     resolves in this one.
///     The content is a compile-time literal on purpose. That is what makes a half-applied patch set
///     impossible to reach here, since the only way this content can fail is a malformed literal,
///     which fails identically on every machine the first time the screen opens.
/// </summary>
public static class InventoryPrefabExtensions
{
    public const string Movie = "Inventory";

    private const string RightPanelAnchor =
        "descendant::ListPanel[@Id='PlayerInventoryListWidgetParent']/Children";

    private const string LeftPanelAnchor =
        "descendant::ListPanel[@Id='OtherInventoryListWidgetParent']/Children";

    private static XmlDocument Bar(string binding)
    {
        var document = new XmlDocument();
        document.LoadXml($@"
<TextWidget WidthSizePolicy=""StretchToParent"" HeightSizePolicy=""Fixed"" SuggestedHeight=""28""
            MarginTop=""4"" MarginLeft=""8"" Brush=""Inventory.Experience.Label""
            Text=""@{binding}"" IsVisible=""@InvEnhHasBar"" />");
        return document;
    }

    [PrefabExtension(Movie, RightPanelAnchor)]
    public sealed class RightStatusPatch : PrefabExtensionInsertPatch
    {
        public override InsertType Type => InsertType.Append;

        [PrefabExtensionXmlDocument]
        public XmlDocument GetPrefabExtension()
        {
            return Bar("InvEnhRightStatus");
        }
    }

    [PrefabExtension(Movie, LeftPanelAnchor)]
    public sealed class LeftStatusPatch : PrefabExtensionInsertPatch
    {
        public override InsertType Type => InsertType.Append;

        [PrefabExtensionXmlDocument]
        public XmlDocument GetPrefabExtension()
        {
            return Bar("InvEnhLeftStatus");
        }
    }
}
