using System.Drawing;
using CapturaTool.Editor;
using CapturaTool.Editor.Annotations;

namespace CapturaTool.Tests;

public class AnnotationDocumentTests : IDisposable
{
    // Anotación de prueba: solo tiene un nombre para reconocerla, no dibuja nada
    private class TestAnnotation : Annotation
    {
        public TestAnnotation(string name) : base(Color.Red)
        {
            Name = name;
        }

        public string Name { get; }

        public override void Draw(Graphics g)
        {
        }
    }

    private readonly AnnotationDocument _document = new(new Bitmap(100, 50));

    public void Dispose() => _document.Dispose();

    private string[] Names() =>
        _document.Annotations.Cast<TestAnnotation>().Select(a => a.Name).ToArray();

    [Fact]
    public void NewDocument_IsEmpty_AndCannotUndoOrRedo()
    {
        Assert.Empty(_document.Annotations);
        Assert.False(_document.CanUndo);
        Assert.False(_document.CanRedo);
    }

    [Fact]
    public void Add_KeepsCreationOrder()
    {
        _document.Add(new TestAnnotation("a"));
        _document.Add(new TestAnnotation("b"));
        _document.Add(new TestAnnotation("c"));

        Assert.Equal(new[] { "a", "b", "c" }, Names());
        Assert.True(_document.CanUndo);
    }

    [Fact]
    public void Undo_RemovesLastAnnotation()
    {
        _document.Add(new TestAnnotation("a"));
        _document.Add(new TestAnnotation("b"));

        bool changed = _document.Undo();

        Assert.True(changed);
        Assert.Equal(new[] { "a" }, Names());
        Assert.True(_document.CanRedo);
    }

    [Fact]
    public void Undo_WithNothingToUndo_ReturnsFalse()
    {
        Assert.False(_document.Undo());
        Assert.Empty(_document.Annotations);
    }

    [Fact]
    public void Undo_Repeatedly_ReturnsToOriginalCapture()
    {
        _document.Add(new TestAnnotation("a"));
        _document.Add(new TestAnnotation("b"));
        _document.Add(new TestAnnotation("c"));

        _document.Undo();
        _document.Undo();
        _document.Undo();

        Assert.Empty(_document.Annotations);
        Assert.False(_document.CanUndo);
        Assert.False(_document.Undo());
    }

    [Fact]
    public void Redo_RestoresInReverseOrderOfUndo()
    {
        _document.Add(new TestAnnotation("a"));
        _document.Add(new TestAnnotation("b"));
        _document.Add(new TestAnnotation("c"));
        _document.Undo();
        _document.Undo();

        Assert.True(_document.Redo());
        Assert.Equal(new[] { "a", "b" }, Names());

        Assert.True(_document.Redo());
        Assert.Equal(new[] { "a", "b", "c" }, Names());
        Assert.False(_document.CanRedo);
    }

    [Fact]
    public void Redo_WithNothingUndone_ReturnsFalse()
    {
        _document.Add(new TestAnnotation("a"));

        Assert.False(_document.Redo());
        Assert.Equal(new[] { "a" }, Names());
    }

    [Fact]
    public void AddAfterUndo_ClearsRedo()
    {
        _document.Add(new TestAnnotation("a"));
        _document.Add(new TestAnnotation("b"));
        _document.Undo();

        _document.Add(new TestAnnotation("c"));

        Assert.False(_document.CanRedo);
        Assert.False(_document.Redo());
        Assert.Equal(new[] { "a", "c" }, Names());
    }

    [Fact]
    public void MixedUndoRedo_KeepsOrder()
    {
        _document.Add(new TestAnnotation("a"));
        _document.Add(new TestAnnotation("b"));
        _document.Undo();
        _document.Redo();
        _document.Add(new TestAnnotation("c"));
        _document.Undo();
        _document.Undo();
        _document.Redo();

        Assert.Equal(new[] { "a", "b" }, Names());
        Assert.True(_document.CanRedo);
    }

    [Fact]
    public void BaseImage_IsTheSameImage_AndIsNotTouchedByAnnotations()
    {
        var image = new Bitmap(10, 10);
        using var document = new AnnotationDocument(image);

        document.Add(new TestAnnotation("a"));
        document.Undo();

        Assert.Same(image, document.BaseImage);
        Assert.Equal(new Size(10, 10), document.BaseImage.Size);
    }

    [Fact]
    public void Dispose_ReleasesBaseImage()
    {
        var image = new Bitmap(10, 10);
        var document = new AnnotationDocument(image);

        document.Dispose();

        // Un Bitmap liberado lanza ArgumentException al leer sus medidas
        Assert.Throws<ArgumentException>(() => image.Width);
    }
}
