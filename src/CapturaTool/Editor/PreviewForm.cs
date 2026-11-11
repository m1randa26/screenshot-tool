namespace CapturaTool.Editor;

/// <summary>
/// PROVISIONAL (T04): muestra la captura a tamaño real para verificar nitidez y resolución.
/// En T05 se reemplaza por EditorForm.
/// Es dueña de la imagen: la libera al cerrarse.
/// </summary>
public class PreviewForm : Form
{
    private readonly Bitmap _image;

    public PreviewForm(Bitmap image)
    {
        _image = image;

        Text = $"Vista previa (provisional): {image.Width} × {image.Height} px";
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(1000, 700);
        AutoScroll = true;

        var picture = new PictureBox
        {
            Image = image,
            SizeMode = PictureBoxSizeMode.AutoSize, // tamaño real, sin escalar
        };
        Controls.Add(picture);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing); // libera primero el PictureBox que usa la imagen
        if (disposing)
        {
            _image.Dispose();
        }
    }
}
