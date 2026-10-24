using FSO.Client;
using FSO.Common;
using FSO.SimAntics;
using FSO.SimAntics.NetPlay;
using FSO.SimAntics.NetPlay.Model;
using FSO.SimAntics.NetPlay.Model.Commands;
using FSO.Client.UI.Framework;
using FSO.Content.TS1;
using FSO.Files.Formats.IFF;
using FSO.Files.Formats.IFF.Chunks;
using System.Reflection;
using Simitone.Client.UI.Screens;
using Simitone.Client.UI.Panels;
using System.Runtime.CompilerServices;

var root = Path.Combine(Path.GetTempPath(), "simitone-checks-" + Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(root);
try
{
    var settings = new GlobalSettings(Path.Combine(root, "config.ini"))
    {
        FXVolume = 3, MusicVolume = 4, VoxVolume = 5, AmbienceVolume = 6,
        GraphicsWidth = 1024, GraphicsHeight = 768, Windowed = true,
        TS1HybridPath = "preserve-this-path", TS1FreeWill = false
    };
    Require(settings.TrySave(out var error), error ?? "Config save failed.");
    var loaded = new GlobalSettings(settings.ActivePath);
    Require(loaded.FXVolume == 3 && loaded.MusicVolume == 4 && loaded.VoxVolume == 5 && loaded.AmbienceVolume == 6,
        "Audio settings did not survive reopening the real config.");
    Require(loaded.GraphicsWidth == 1024 && loaded.GraphicsHeight == 768 && loaded.Windowed &&
        loaded.TS1HybridPath == "preserve-this-path" && !loaded.TS1FreeWill,
        "Display or unrelated settings changed during save.");
    Console.WriteLine("PASS: config round trip preserves exact values");

    var fragile = new FragileConfig(Path.Combine(root, "fragile.ini"));
    var original = File.ReadAllBytes(fragile.ActivePath);
    fragile.FailSerialization = true;
    Require(!fragile.TrySave(out error) && !string.IsNullOrWhiteSpace(error),
        "Serialization failure was reported as success.");
    fragile.Save(); // Existing callers still must not throw.
    Require(File.ReadAllBytes(fragile.ActivePath).SequenceEqual(original),
        "A failed serialization truncated the previous config.");
    Require(Directory.GetFiles(root).Length == 2, "A failed save left temporary files behind.");
    Console.WriteLine("PASS: failed serialization preserves the previous config");
    var invalid = new GlobalSettings(Path.Combine(root, "missing", "config.ini"));
    Require(!invalid.TrySave(out error) && !string.IsNullOrWhiteSpace(error),
        "An invalid destination was reported as saved.");
    invalid.Save();
    Console.WriteLine("PASS: invalid destinations report failure without breaking legacy callers");
    if (OperatingSystem.IsWindows())
    {
        var originalSettings = File.ReadAllBytes(settings.ActivePath);
        using (File.Open(settings.ActivePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
        {
            settings.FXVolume = 9;
            Require(!settings.TrySave(out error) && !string.IsNullOrWhiteSpace(error), "A locked config was reported as saved.");
        }
        Require(File.ReadAllBytes(settings.ActivePath).SequenceEqual(originalSettings), "A failed locked-file save changed the original.");
        Console.WriteLine("PASS: locked-file save preserves the original");
    }
    else Console.WriteLine("PENDING: Windows locked-file check requires Windows");
    var screen = (TS1GameScreen)RuntimeHelpers.GetUninitializedObject(typeof(TS1GameScreen));
    Require(!screen.Save(), "Saving without a lot must return false before accessing game state.");
    screen.vm = (VM)RuntimeHelpers.GetUninitializedObject(typeof(VM));
    screen.Downtown = true;
    Require(!screen.Save(), "Saving while away must stop before accessing or writing game state.");
    Console.WriteLine("PASS: saves without a lot or while away never touch game state");
    var singleton = typeof(FSO.Content.Content).GetField("INSTANCE", BindingFlags.NonPublic | BindingFlags.Static)!;
    var previousContent = singleton.GetValue(null);
    var content = (FSO.Content.Content)RuntimeHelpers.GetUninitializedObject(typeof(FSO.Content.Content));
    var neighbors = (TS1NeighborhoodProvider)RuntimeHelpers.GetUninitializedObject(typeof(TS1NeighborhoodProvider));
    neighbors.FamilyForHouse = new();
    neighbors.MainResource = new IffFile();
    neighbors.UserPath = root;
    content.Neighborhood = neighbors;
    singleton.SetValue(null, content);
    try
    {
        var family = new FAMI { ChunkID = 8, ChunkProcessed = true, ChunkType = "FAMI", HouseNumber = 0, Budget = 100, ValueInArch = 50, FamilyGUIDs = new uint[] { 7, 9 } };
        screen.EvictLot(family, 1); // Already evicted / stale confirmation.
        Require(family.Budget == 100 && family.ValueInArch == 50 && family.HouseNumber == 0,
            "A stale eviction request credited the family or changed its home.");
        neighbors.MainResource.AddChunk(family);
        var occupant = new FAMI { ChunkID = 9, HouseNumber = 2 };
        neighbors.FamilyForHouse[2] = occupant;
        screen.MoveInAndPlay(2, 8, null);
        Require(family.Budget == 100 && family.HouseNumber == 0 && ReferenceEquals(neighbors.FamilyForHouse[2], occupant),
            "Moving into an occupied lot changed the buyer or current occupants.");
        screen.MoveInAndPlay(1, 999, null);
        Require(family.FamilyGUIDs.SequenceEqual(new uint[] { 7, 9 }), "A rejected household operation changed membership.");
        var selector = (UINeighborhoodSelectionPanel)RuntimeHelpers.GetUninitializedObject(typeof(UINeighborhoodSelectionPanel));
        var selectorFlags = BindingFlags.NonPublic | BindingFlags.Instance;
        typeof(UINeighborhoodSelectionPanel).GetField("VisitOnly", selectorFlags)?.SetValue(selector, true);
        typeof(UINeighborhoodSelectionPanel).GetField("Mode", selectorFlags)!.SetValue(selector, 4);
        selector.HousePositions = new() { [1] = new(), [2] = new() };
        neighbors.ZoningDictionary = new() { [1] = 0, [2] = 1 };
        var destinations = new List<int>();
        selector.OnHouseSelect += destinations.Add;
        selector.SelectHouse(1);
        selector.SelectHouse(99);
        Require(destinations.Count == 0, "Residential or unmapped destinations were offered for travel.");
        selector.SelectHouse(2);
        selector.SelectHouse(2);
        selector.SelectHouse(0);
        Require(destinations.SequenceEqual(new[] { 2 }), "A visit must submit one permitted destination exactly once.");
        var control = (UILotControl)RuntimeHelpers.GetUninitializedObject(typeof(UILotControl));
        var privateFields = BindingFlags.Instance | BindingFlags.NonPublic;
        var children = typeof(UIContainer).GetProperty("Children", privateFields)!;
        children.SetValue(selector, new List<UIElement>());
        children.SetValue(screen, new List<UIElement>());
        screen.Bg = (Simitone.Client.UI.Screens.UISimitoneBg)RuntimeHelpers.GetUninitializedObject(typeof(Simitone.Client.UI.Screens.UISimitoneBg));
        screen.Frontend = (Simitone.Client.UI.Panels.UISimitoneFrontend)RuntimeHelpers.GetUninitializedObject(typeof(Simitone.Client.UI.Panels.UISimitoneFrontend));
        typeof(UIElement).GetField("_Parent", privateFields)!.SetValue(control, screen);
        typeof(UILotControl).GetField("TS1NeighSelector", privateFields)!.SetValue(control, selector);
        typeof(UILotControl).GetField("TravelDialogActor", privateFields)!.SetValue(control, (uint)77);
        typeof(UILotControl).GetField("TravelLotVisible", privateFields)!.SetValue(control, true);
        control.vm = screen.vm;
        var driver = new RecordingDriver();
        typeof(VM).GetField("Driver", privateFields)!.SetValue(screen.vm, driver);
        screen.vm.MyUID = 88; // Active Sim changed while the original caller is waiting.
        var complete = typeof(UILotControl).GetMethod("HouseSelected", privateFields)!;
        complete.Invoke(control, new object[] { 0 });
        complete.Invoke(control, new object[] { 2 });
        Require(driver.Commands.Count == 1 && driver.Commands[0] is VMNetDialogResponseCmd response &&
            response.ActorUID == 77 && response.ResponseCode == 0 && response.ResponseText == "0" && control.Visible && !screen.Bg.Visible,
            "Travel cancellation changed actor, submitted twice, or failed to restore controls.");
        Console.WriteLine("PASS: travel cancel restores controls and sends one response for the original caller");
        Console.WriteLine("PASS: visit selector rejects residential/unmapped lots and completes once");
        Console.WriteLine("PASS: stale eviction and invalid move-in preserve money, membership and occupancy");
    }
    finally { singleton.SetValue(null, previousContent); }
    var optionsType = typeof(TS1GameScreen).Assembly.GetType("Simitone.Client.UI.Panels.Options.UIOptionsAlert");
    Require(optionsType != null, "The options dialog and Apply/Cancel behavior are missing.");
    var options = RuntimeHelpers.GetUninitializedObject(optionsType!);
    var fields = BindingFlags.Instance | BindingFlags.NonPublic;
    void SetOption(string name, object value) => optionsType!.GetField(name, fields)!.SetValue(options, value);
    var draftType = optionsType!.GetNestedType("OptionsDraft", BindingFlags.NonPublic)!;
    void Draft(byte fx, int width = 1024, int height = 768, bool windowed = true) =>
        SetOption("Draft", Activator.CreateInstance(draftType, new object[] { fx, (byte)4, (byte)5, (byte)6, width, height, windowed })!);
    bool Apply()
    {
        object?[] args = { null };
        return (bool)optionsType.GetMethod("TryApply", fields)!.Invoke(options, args)!;
    }
    settings = new GlobalSettings(settings.ActivePath);
    SetOption("Settings", settings);
    SetOption("WindowedSizes", new HashSet<Microsoft.Xna.Framework.Point> { new(1024, 768), new(1920, 1080) });
    SetOption("FullscreenSizes", new HashSet<Microsoft.Xna.Framework.Point> { new(1024, 768) });
    var beforeOptions = File.ReadAllBytes(settings.ActivePath);
    Draft(9);
    Require(settings.FXVolume == 3 && File.ReadAllBytes(settings.ActivePath).SequenceEqual(beforeOptions),
        "Editing a detached draft changed live or persisted settings before Apply.");
    Draft(11);
    Require(!Apply() && settings.FXVolume == 3 && File.ReadAllBytes(settings.ActivePath).SequenceEqual(beforeOptions),
        "An invalid volume partially applied.");
    Draft(3, 999, 999, false);
    Require(!Apply() && File.ReadAllBytes(settings.ActivePath).SequenceEqual(beforeOptions), "An unsupported fullscreen mode was accepted.");
    Draft(7);
    Require(Apply() && new GlobalSettings(settings.ActivePath).FXVolume == 7 && settings.TS1HybridPath == "preserve-this-path",
        "Valid options did not persist or changed unrelated settings.");
    settings.GraphicsWidth = 1280;
    settings.GraphicsHeight = 720;
    settings.Save();
    Require(new GlobalSettings(settings.ActivePath).GraphicsWidth == 1280,
        "Audio-only Apply froze later window resize persistence.");
    settings.GraphicsWidth = 1024;
    settings.GraphicsHeight = 768;
    Draft(7, 1920, 1080);
    Require(Apply() && settings.GraphicsWidth == 1024 && settings.GraphicsHeight == 768,
        "A restart-only display choice changed the live UI dimensions.");
    settings.GraphicsWidth = 1280;
    settings.GraphicsHeight = 720;
    settings.Save(); // Existing resize/other settings callers must not erase the pending choice.
    var afterResize = new GlobalSettings(settings.ActivePath);
    Require(afterResize.GraphicsWidth == 1920 && afterResize.GraphicsHeight == 1080,
        "A later save overwrote the accepted restart-only display choice.");
    SetOption("Settings", invalid);
    Draft(3);
    Require(!Apply() && invalid.FXVolume == 10 && invalid.MusicVolume == 10, "Failed Apply did not restore prior live settings.");
    typeof(TS1GameScreen).GetField("OptionsDialog", fields)!.SetValue(screen, options);
    screen.vm.SpeedMultiplier = 0;
    screen.ChangeSpeedTo(1);
    Require(screen.vm.SpeedMultiplier == 0, "Typing an options number resumed the simulation.");
    Console.WriteLine("PASS: options drafts validate, persist on Apply, and roll back live values on write failure");
    return 0;
}
finally
{
    Directory.Delete(root, true);
}

static void Require(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}

sealed class FragileConfig(string path) : IniConfig(path)
{
    public override string HeadingComment => "Regression check";
    public override Dictionary<string, string> DefaultValues { get; set; } = new();
    public bool FailSerialization { get; set; }
    public string Value => FailSerialization ? throw new IOException("Simulated serialization failure") : "previous value";
}

sealed class RecordingDriver : VMNetDriver
{
    public List<VMNetCommandBodyAbstract> Commands = new();
    public override void SendCommand(VMNetCommandBodyAbstract cmd) => Commands.Add(cmd);
    public override void SendDirectCommand(uint pid, VMNetCommandBodyAbstract cmd) => throw new NotSupportedException();
    public override bool Tick(VM vm) => true;
    public override string GetUserIP(uint uid) => "local-check";
}
