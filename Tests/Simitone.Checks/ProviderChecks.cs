using FSO.Content.TS1;
using FSO.Files.Formats.IFF;
using FSO.Files.Formats.IFF.Chunks;
using System.Runtime.CompilerServices;

static class ProviderChecks
{
    public static void Run(string root)
    {
        var folder = Path.Combine(root, "provider");
        Directory.CreateDirectory(Path.Combine(folder, "Houses"));
        var provider = (TS1NeighborhoodProvider)RuntimeHelpers.GetUninitializedObject(typeof(TS1NeighborhoodProvider));
        provider.UserPath = folder;
        provider.DirtyAvatars = new();
        var path = provider.GetHousePath(1);
        var original = new byte[] { 1, 2, 3, 4 };
        File.WriteAllBytes(path, original);
        var broken = new IffFile();
        broken.AddChunk(new FailingChunk { ChunkID = 1, ChunkType = "FAIL", ChunkProcessed = true, ChunkLabel = "Failure" });
        Fails(() => provider.SaveHouse(1, broken));
        Require(File.ReadAllBytes(path).SequenceEqual(original), "Failed lot serialization truncated the existing house.");
        var neighborhood = Path.Combine(folder, "Neighborhood.iff");
        File.WriteAllBytes(neighborhood, original);
        var content = (FSO.Content.Content)RuntimeHelpers.GetUninitializedObject(typeof(FSO.Content.Content));
        content.WorldObjects = new TS1ObjectProvider(content, null) { Entries = new() };
        provider.ContentManager = content;
        provider.DirtyAvatars.Add(7);
        provider.MainResource = broken;
        Fails(() => provider.SaveNeighbourhood(true));
        Require(provider.DirtyAvatars.Contains(7), "A failed neighborhood write forgot pending avatar edits.");
        Require(File.ReadAllBytes(neighborhood).SequenceEqual(original), "Failed neighborhood serialization truncated the existing file.");
        Require(!Directory.GetFiles(folder, "*.tmp", SearchOption.AllDirectories).Any(), "Provider left temporary files behind.");
        var valid = new IffFile();
        var text = new STR { ChunkID = 1, ChunkType = "STR#", ChunkProcessed = true, ChunkLabel = "Check" };
        text.LanguageSets[0].Strings = new[] { new STRItem { Value = "saved value", Comment = "" } };
        valid.AddChunk(text);
        Require(provider.SaveHouse(1, valid) && new IffFile(path).Get<STR>(1).GetString(0) == "saved value", "Real IFF roundtrip failed.");
        provider.MainResource = valid;
        provider.SaveNeighbourhood(true);
        Require(new IffFile(neighborhood).Get<STR>(1).GetString(0) == "saved value", "Neighborhood roundtrip failed.");
        if (OperatingSystem.IsWindows())
        {
            var before = File.ReadAllBytes(path);
            using (File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None)) Fails(() => provider.SaveHouse(1, valid));
            Require(File.ReadAllBytes(path).SequenceEqual(before), "Locked house file changed.");
        }
        Console.WriteLine("PASS: provider IFF writes preserve previous files/pending edits on failure and roundtrip on success");
    }
    static void Fails(Action action)
    {
        try { action(); } catch (IOException) { return; } catch (UnauthorizedAccessException) { return; }
        throw new InvalidOperationException("An invalid provider save succeeded.");
    }
    static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
    sealed class FailingChunk : IffChunk
    {
        public override void Read(IffFile iff, Stream stream) => throw new NotSupportedException();
        public override bool Write(IffFile iff, Stream stream) { stream.WriteByte(7); throw new IOException("Serialization failed after beginning a chunk."); }
    }
}
