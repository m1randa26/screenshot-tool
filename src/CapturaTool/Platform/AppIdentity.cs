namespace CapturaTool.Platform;

/// <summary>
/// Todo lo que distingue a la versión estable de la Insider (R34, ADR 0004).
/// Se elige al compilar: con <c>-p:Channel=Insider</c> se define la constante INSIDER.
/// </summary>
public sealed class AppIdentity
{
    public static readonly AppIdentity Stable = new(
        displayName: "CapturaTool",
        mutexName: "Local\\CapturaTool_SingleInstance",
        hotkeyModifiers: KeyModifiers.Control | KeyModifiers.Alt,
        iconResourceName: "CapturaTool.ico");

    public static readonly AppIdentity Insider = new(
        displayName: "CapturaTool Insider",
        mutexName: "Local\\CapturaTool.Insider_SingleInstance",
        hotkeyModifiers: KeyModifiers.Control | KeyModifiers.Alt | KeyModifiers.Shift,
        iconResourceName: "CapturaTool.Insider.ico");

#if INSIDER
    public static AppIdentity Current => Insider;
#else
    public static AppIdentity Current => Stable;
#endif

    private AppIdentity(string displayName, string mutexName, KeyModifiers hotkeyModifiers, string iconResourceName)
    {
        DisplayName = displayName;
        MutexName = mutexName;
        HotkeyModifiers = hotkeyModifiers;
        IconResourceName = iconResourceName;
    }

    /// <summary>Nombre que ve el usuario: ventanas, bandeja, avisos y mensajes.</summary>
    public string DisplayName { get; }

    /// <summary>Nombre del mutex de instancia única (R3).</summary>
    public string MutexName { get; }

    /// <summary>Teclas que acompañan a la letra de cada atajo (la Insider agrega Shift).</summary>
    public KeyModifiers HotkeyModifiers { get; }

    /// <summary>Nombre del logo dentro del programa (ver EmbeddedResource en el .csproj).</summary>
    public string IconResourceName { get; }

    /// <summary>Nombre de la entrada de arranque con Windows (R1).</summary>
    public string AutoStartValueName => DisplayName;

    /// <summary>Carpeta de configuración y log: %APPDATA%\[nombre].</summary>
    public string DataFolder =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), DisplayName);

    /// <summary>Un atajo escrito para mostrarlo, por ejemplo "Ctrl + Alt + Shift + S".</summary>
    public string DescribeHotkey(Keys key)
    {
        var parts = new List<string>();
        if (HotkeyModifiers.HasFlag(KeyModifiers.Control)) parts.Add("Ctrl");
        if (HotkeyModifiers.HasFlag(KeyModifiers.Alt)) parts.Add("Alt");
        if (HotkeyModifiers.HasFlag(KeyModifiers.Shift)) parts.Add("Shift");
        parts.Add(key.ToString());
        return string.Join(" + ", parts);
    }
}
