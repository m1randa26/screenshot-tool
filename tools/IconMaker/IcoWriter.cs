using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace IconMaker;

/// <summary>
/// Escribe un archivo .ico con varias imágenes.
/// Hasta 64 px usa el formato clásico (mapa de bits de 32 bits con transparencia),
/// que lee cualquier parte de Windows; 256 px va comprimido como PNG, como hace Windows.
/// </summary>
public static class IcoWriter
{
    public static void Write(string path, IReadOnlyList<Bitmap> images)
    {
        // Primero se prepara cada imagen; después se escribe el índice y los datos
        var frames = images.Select(image => image.Width >= 256 ? ToPng(image) : ToBmp(image)).ToList();

        using var file = File.Create(path);
        using var writer = new BinaryWriter(file);

        // Encabezado: reservado, tipo (1 = ícono), cantidad de imágenes
        writer.Write((short)0);
        writer.Write((short)1);
        writer.Write((short)images.Count);

        // Índice: una entrada de 16 bytes por imagen
        int offset = 6 + 16 * images.Count;
        for (int i = 0; i < images.Count; i++)
        {
            int size = images[i].Width;
            writer.Write((byte)(size >= 256 ? 0 : size)); // 0 significa 256
            writer.Write((byte)(size >= 256 ? 0 : size));
            writer.Write((byte)0);    // colores de paleta (no usa)
            writer.Write((byte)0);    // reservado
            writer.Write((short)1);   // planos
            writer.Write((short)32);  // bits por píxel
            writer.Write(frames[i].Length);
            writer.Write(offset);
            offset += frames[i].Length;
        }

        foreach (byte[] frame in frames)
        {
            writer.Write(frame);
        }
    }

    private static byte[] ToPng(Bitmap image)
    {
        using var stream = new MemoryStream();
        image.Save(stream, ImageFormat.Png);
        return stream.ToArray();
    }

    /// <summary>
    /// Imagen en formato de ícono clásico: encabezado de 40 bytes (con el alto al doble),
    /// píxeles azul-verde-rojo-alfa de abajo hacia arriba, y una máscara de 1 bit por píxel.
    /// </summary>
    private static byte[] ToBmp(Bitmap image)
    {
        int size = image.Width;
        int maskRowBytes = (size + 31) / 32 * 4; // cada fila de la máscara ocupa múltiplos de 4 bytes

        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        writer.Write(40);                          // tamaño del encabezado
        writer.Write(size);                        // ancho
        writer.Write(size * 2);                    // alto: imagen + máscara
        writer.Write((short)1);                    // planos
        writer.Write((short)32);                   // bits por píxel
        writer.Write(0);                           // sin compresión
        writer.Write(size * size * 4 + maskRowBytes * size);
        writer.Write(0);
        writer.Write(0);
        writer.Write(0);
        writer.Write(0);

        BitmapData data = image.LockBits(
            new Rectangle(0, 0, size, size), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
        var pixels = new byte[size * 4 * size];
        for (int y = 0; y < size; y++)
        {
            Marshal.Copy(data.Scan0 + y * data.Stride, pixels, y * size * 4, size * 4);
        }
        image.UnlockBits(data);

        // Píxeles, de la fila de abajo hacia arriba
        for (int y = size - 1; y >= 0; y--)
        {
            writer.Write(pixels, y * size * 4, size * 4);
        }

        // Máscara: 1 donde el píxel es totalmente transparente (para programas viejos)
        for (int y = size - 1; y >= 0; y--)
        {
            var row = new byte[maskRowBytes];
            for (int x = 0; x < size; x++)
            {
                byte alpha = pixels[(y * size + x) * 4 + 3];
                if (alpha == 0)
                {
                    row[x / 8] |= (byte)(0x80 >> (x % 8));
                }
            }
            writer.Write(row);
        }

        writer.Flush();
        return stream.ToArray();
    }
}
