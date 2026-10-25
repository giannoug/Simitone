using FSO.Content.TS1;
using FSO.Files.Formats.IFF.Chunks;
using FSO.SimAntics;
using FSO.SimAntics.Marshals;
using Simitone.Client.UI.Screens;
using System.Reflection;
using System.Runtime.CompilerServices;

static class TravelChecks
{
    public static void Run(string root)
    {
        const BindingFlags fields = BindingFlags.Instance | BindingFlags.NonPublic;
        var type = typeof(TS1GameScreen);
        var switchLot = type.GetMethod("TrySwitchLot", fields);
        if (switchLot == null) throw new InvalidOperationException("Queued travel has no ownership validation before replacing the world.");
        var singleton = typeof(FSO.Content.Content).GetField("INSTANCE", BindingFlags.NonPublic | BindingFlags.Static)!;
        var previous = singleton.GetValue(null);
        var content = (FSO.Content.Content)RuntimeHelpers.GetUninitializedObject(typeof(FSO.Content.Content));
        var provider = (TS1NeighborhoodProvider)RuntimeHelpers.GetUninitializedObject(typeof(TS1NeighborhoodProvider));
        provider.UserPath = root;
        content.Neighborhood = provider;
        singleton.SetValue(null, content);
        try
        {
            var screen = (TS1GameScreen)RuntimeHelpers.GetUninitializedObject(type);
            screen.vm = (VM)RuntimeHelpers.GetUninitializedObject(typeof(VM));
            screen.ActiveFamily = new FAMI { HouseNumber = 1 };
            screen.Downtown = true;
            var snapshot = new VMMarshal();
            void Set(string name, object? value) => type.GetField(name, fields)!.SetValue(screen, value);
            object? Get(string name) => type.GetField(name, fields)!.GetValue(screen);
            bool Process() { object?[] args = { null }; return (bool)switchLot.Invoke(screen, args)!; }
            Set("SavedLot", snapshot);
            Set("SavedHomeLot", (short)1);
            Set("SavedHomeUserPath", Path.Combine(root, "other-neighborhood"));
            Set("SwitchLot", 1);
            if (Process() || !ReferenceEquals(Get("SavedLot"), snapshot) || (int)Get("SwitchLot")! != -1 || !screen.Downtown)
                throw new InvalidOperationException("Wrong neighborhood consumed snapshot, changed world mode, or kept retrying.");
            if (!Process()) throw new InvalidOperationException("Consumed request was processed twice.");
            Set("SavedHomeUserPath", Path.GetFullPath(root));
            Set("SwitchLot", 99);
            if (Process() || !ReferenceEquals(Get("SavedLot"), snapshot)) throw new InvalidOperationException("Missing destination replaced the home snapshot.");
            screen.ActiveFamily.HouseNumber = 2;
            Set("SwitchLot", 1);
            if (Process() || !ReferenceEquals(Get("SavedLot"), snapshot)) throw new InvalidOperationException("Mismatched family home was accepted.");
            Set("SavedLot", null);
            Set("SwitchLot", 2);
            if (Process()) throw new InvalidOperationException("Returning without a live home snapshot was accepted.");
            Console.WriteLine("PASS: queued travel rejects wrong ownership/missing destinations without consuming home state or retrying");
        }
        finally { singleton.SetValue(null, previous); }
    }
}
