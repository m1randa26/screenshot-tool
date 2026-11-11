using System.Drawing;
using CapturaTool.Capture;

namespace CapturaTool.Tests;

public class SelectionGeometryTests
{
    private static readonly Size Monitor = new(1920, 1080);

    [Fact]
    public void FromPoints_DragDownRight()
    {
        Rectangle r = SelectionGeometry.FromPoints(new Point(100, 50), new Point(300, 250));

        Assert.Equal(new Rectangle(100, 50, 200, 200), r);
    }

    [Fact]
    public void FromPoints_DragUpLeft_GivesSameRectangle()
    {
        Rectangle r = SelectionGeometry.FromPoints(new Point(300, 250), new Point(100, 50));

        Assert.Equal(new Rectangle(100, 50, 200, 200), r);
    }

    [Fact]
    public void FromPoints_DragUpRight()
    {
        Rectangle r = SelectionGeometry.FromPoints(new Point(100, 250), new Point(300, 50));

        Assert.Equal(new Rectangle(100, 50, 200, 200), r);
    }

    [Fact]
    public void Resolve_NormalSelection_StaysTheSame()
    {
        var selection = new Rectangle(100, 50, 200, 200);

        Assert.Equal(selection, SelectionGeometry.Resolve(selection, Monitor));
    }

    [Fact]
    public void Resolve_Click_GivesFullMonitor()
    {
        var click = new Rectangle(500, 500, 0, 0);

        Assert.Equal(new Rectangle(0, 0, 1920, 1080), SelectionGeometry.Resolve(click, Monitor));
    }

    [Fact]
    public void Resolve_TinySelection_GivesFullMonitor()
    {
        var tiny = new Rectangle(500, 500, 2, 2);

        Assert.Equal(new Rectangle(0, 0, 1920, 1080), SelectionGeometry.Resolve(tiny, Monitor));
    }

    [Fact]
    public void Resolve_ThinStrip_GivesFullMonitor()
    {
        var strip = new Rectangle(100, 500, 800, 3);

        Assert.Equal(new Rectangle(0, 0, 1920, 1080), SelectionGeometry.Resolve(strip, Monitor));
    }

    [Fact]
    public void Resolve_SmallestValidSelection_IsKept()
    {
        var small = new Rectangle(500, 500, SelectionGeometry.MinSize, SelectionGeometry.MinSize);

        Assert.Equal(small, SelectionGeometry.Resolve(small, Monitor));
    }

    [Fact]
    public void Resolve_SelectionOutsideEdges_IsClippedToMonitor()
    {
        // El mouse salió del monitor por arriba a la izquierda y por abajo a la derecha
        var selection = Rectangle.FromLTRB(-50, -20, 2000, 1100);

        Assert.Equal(new Rectangle(0, 0, 1920, 1080), SelectionGeometry.Resolve(selection, Monitor));
    }

    [Fact]
    public void Resolve_SelectionPartlyOutside_KeepsInsidePart()
    {
        var selection = Rectangle.FromLTRB(1800, 1000, 2100, 1200);

        Assert.Equal(Rectangle.FromLTRB(1800, 1000, 1920, 1080), SelectionGeometry.Resolve(selection, Monitor));
    }
}
