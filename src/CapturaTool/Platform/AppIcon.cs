namespace CapturaTool.Platform;

/// <summary>
/// El logo del programa (R33), guardado dentro del .exe.
/// Quien llama a Load es dueño del Icon que recibe y debe liberarlo.
/// </summary>
public static class AppIcon
{
    /// <summary>Nombre del logo dentro del programa (ver EmbeddedResource en el .csproj).</summary>
    public const string ResourceName = "CapturaTool.ico";

    /// <summary>El logo con todos sus tamaños: las ventanas eligen el que necesitan.</summary>
    public static Icon Load()
    {
        using Stream stream = OpenResource();
        return new Icon(stream);
    }

    /// <summary>El logo a un tamaño exacto (por ejemplo, el de la bandeja).</summary>
    public static Icon Load(Size size)
    {
        using Stream stream = OpenResource();
        return new Icon(stream, size);
    }

    /// <summary>Abre el archivo .ico guardado dentro del programa.</summary>
    public static Stream OpenResource()
    {
        return typeof(AppIcon).Assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException($"No se encontró el logo {ResourceName} dentro del programa.");
    }

    /// <summary>
    /// Tamaños que trae un archivo .ico, leyendo su índice: 6 bytes de encabezado
    /// y 16 bytes por imagen, donde el primero es el ancho (0 significa 256).
    /// </summary>
    public static IReadOnlyList<int> ReadSizes(Stream stream)
    {
        using var reader = new BinaryReader(stream, System.Text.Encoding.UTF8, leaveOpen: true);
        reader.ReadInt16();                 // reservado
        short type = reader.ReadInt16();    // 1 = ícono
        short count = reader.ReadInt16();
        if (type != 1)
        {
            throw new InvalidDataException("El archivo no es un ícono.");
        }

        var sizes = new List<int>();
        for (int i = 0; i < count; i++)
        {
            byte width = reader.ReadByte();
            sizes.Add(width == 0 ? 256 : width);
            reader.ReadBytes(15); // resto de la entrada
        }
        return sizes;
    }
}
