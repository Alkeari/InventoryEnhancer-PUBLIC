using System.Xml;
using Bannerlord.UIExtenderEx.Attributes;
using Bannerlord.UIExtenderEx.Prefabs2;

namespace InventoryEnhancer.UI.Extensions;

/// <summary>
///     Each side's Sort &amp; Filter button and its menu, inserted into the vanilla inventory screen.
///     Both anchors key on an Id, a structural attribute, rather than on a binding string, and each
///     selects exactly one node in the shipped prefab. <c>Append</c> inserts the markup as the anchor's
///     next sibling.
///     The button goes into the character name bar, after its last child. The menu goes after the
///     screen's main layout instead, so it draws over both item panels in the middle of the screen.
///     The content is a compile-time literal on purpose. That is what makes a half-applied patch set
///     impossible to reach here, since the only way this content can fail is a malformed literal,
///     which fails identically on every machine the first time the screen opens.
/// </summary>
public static class InventoryPrefabExtensions
{
    public const string Movie = "Inventory";

    private const string NameBarLastChildAnchor = "descendant::ButtonWidget[@Id='NextCharacterButton']";

    private const string MainLayoutAnchor = "descendant::ListPanel[@Id='MainLayout']";

    // The vanilla arrows sit 187 from the name bar's edges. Measured on a v1.5.2 screenshot, the bar's
    // visible frame ends about 116 in and each arrow's visible tip starts about 194 in, so a 40 square
    // centered in that gap starts 136 in. The same margin on both sides keeps the pair symmetrical.
    private const int SquareMargin = 136;

    private static XmlDocument Document(string xml)
    {
        var document = new XmlDocument();
        document.LoadXml(xml);
        return document;
    }

    private static XmlDocument Button(string panel, bool isRight)
    {
        var alignment = isRight ? "Right" : "Left";
        var marginLeft = isRight ? 0 : SquareMargin;
        var marginRight = isRight ? SquareMargin : 0;

        return Document($@"
<InventoryEnhancerSideButton DataSource=""{{..\{panel}}}""
            Parameter.ButtonAlignment=""{alignment}"" Parameter.ButtonMarginLeft=""{marginLeft}"" Parameter.ButtonMarginRight=""{marginRight}"" />");
    }

    private static XmlDocument Menu(string panel)
    {
        return Document($@"<InventoryEnhancerSideMenu DataSource=""{{{panel}}}"" />");
    }

    [PrefabExtension(Movie, NameBarLastChildAnchor)]
    public sealed class LeftButtonPatch : PrefabExtensionInsertPatch
    {
        public override InsertType Type => InsertType.Append;

        [PrefabExtensionXmlDocument]
        public XmlDocument GetPrefabExtension()
        {
            return Button("InvEnhLeftPanel", isRight: false);
        }
    }

    [PrefabExtension(Movie, NameBarLastChildAnchor)]
    public sealed class RightButtonPatch : PrefabExtensionInsertPatch
    {
        public override InsertType Type => InsertType.Append;

        [PrefabExtensionXmlDocument]
        public XmlDocument GetPrefabExtension()
        {
            return Button("InvEnhRightPanel", isRight: true);
        }
    }

    [PrefabExtension(Movie, MainLayoutAnchor)]
    public sealed class LeftMenuPatch : PrefabExtensionInsertPatch
    {
        public override InsertType Type => InsertType.Append;

        [PrefabExtensionXmlDocument]
        public XmlDocument GetPrefabExtension()
        {
            return Menu("InvEnhLeftPanel");
        }
    }

    [PrefabExtension(Movie, MainLayoutAnchor)]
    public sealed class RightMenuPatch : PrefabExtensionInsertPatch
    {
        public override InsertType Type => InsertType.Append;

        [PrefabExtensionXmlDocument]
        public XmlDocument GetPrefabExtension()
        {
            return Menu("InvEnhRightPanel");
        }
    }
}
