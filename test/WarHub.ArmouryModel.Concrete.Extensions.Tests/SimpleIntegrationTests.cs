using FluentAssertions;
using WarHub.ArmouryModel.Concrete;
using WarHub.ArmouryModel.Source;
using Xunit;

namespace WarHub.ArmouryModel;

public class SimpleIntegrationTests
{
    [Fact]
    public void EmptyCatalogueAndGamesystem_compile()
    {
        // arrange
        var gst = NodeFactory.Gamesystem("foo");
        var cat = NodeFactory.Catalogue(gst, "bar");
        var compilation = WhamCompilation.Create();
        // act
        var result = compilation.AddSourceTrees(SourceTree.CreateForRoot(gst), SourceTree.CreateForRoot(cat));
        // assert
        result.GetDiagnostics().Should().BeEmpty();
    }

    [Fact]
    public void Compilation_connects_root_entry_link_to_shared_entry()
    {
        // arrange
        var gst = NodeFactory.Gamesystem("foo");
        var cat = NodeFactory.Catalogue(gst, "bar")
            .AddSharedSelectionEntries(
                NodeFactory.SelectionEntry("entry").Tee(out var entry))
            .AddEntryLinks(
                NodeFactory.EntryLink(entry));
        var compilation = WhamCompilation.Create();
        // act
        var result = compilation.AddSourceTrees(SourceTree.CreateForRoot(gst), SourceTree.CreateForRoot(cat));
        // assert
        result.GetDiagnostics().Should().BeEmpty();
        var catalogue = result.GlobalNamespace.Catalogues.Single(x => !x.IsGamesystem);
        catalogue.RootContainerEntries.Single().ReferencedEntry
            .Should().Be(catalogue.SharedSelectionEntryContainers.Single());
    }

    [Fact]
    public void Compilation_connects_group_default_entry_id_to_entry_link()
    {
        // arrange
        var gst = NodeFactory.Gamesystem("foo");
        var cat = NodeFactory.Catalogue(gst, "bar")
            .AddSharedSelectionEntries(
                NodeFactory.SelectionEntry("entry").Tee(out var entry))
            .AddSharedSelectionEntryGroups(
                NodeFactory.SelectionEntryGroup("group").Tee(out var group)
                .AddEntryLinks(
                    NodeFactory.EntryLink(entry).Tee(out var entryLink))
                .WithDefaultSelectionEntryId(entryLink.Id));
        var compilation = WhamCompilation.Create();
        // act
        var result = compilation.AddSourceTrees(SourceTree.CreateForRoot(gst), SourceTree.CreateForRoot(cat));
        // assert
        result.GetDiagnostics().Should().BeEmpty();
        var catalogue = result.GlobalNamespace.Catalogues.Single(x => !x.IsGamesystem);
        var groupSymbol = catalogue.SharedSelectionEntryContainers.Single(x => x.ContainerKind is ContainerKind.SelectionGroup);
        var groupChild = groupSymbol.ChildSelectionEntries.Single();
        groupSymbol.Should().BeAssignableTo<ISelectionEntryGroupSymbol>()
            .Which.DefaultSelectionEntry
            .Should().BeSameAs(groupChild);
        groupChild.ReferencedEntry
            .Should().Be(catalogue.SharedSelectionEntryContainers.Single(x => x.ContainerKind is ContainerKind.Selection));
    }

    [Fact]
    public void Compilation_connects_characteristic_to_characteristic_type()
    {
        // arrange
        var gst = NodeFactory.Gamesystem("foo")
            .AddProfileTypes(
                NodeFactory.ProfileType("weapon")
                .AddCharacteristicTypes(NodeFactory.CharacteristicType("strength").Tee(out var charType))
                .Tee(out var weaponType));
        var cat = NodeFactory.Catalogue(gst, "bar")
            .AddSelectionEntries(
                NodeFactory.SelectionEntry("entry")
                .AddProfiles(
                    NodeFactory.Profile(weaponType, "bow")
                    .AddCharacteristics(NodeFactory.Characteristic(charType, "5")))
                .Tee(out var entry));
        var compilation = WhamCompilation.Create();
        // act
        var result = compilation.AddSourceTrees(SourceTree.CreateForRoot(gst), SourceTree.CreateForRoot(cat));
        // assert
        result.GetDiagnostics().Should().BeEmpty();
        var catalogue = result.GlobalNamespace.Catalogues.Single(x => !x.IsGamesystem);
        var profile = catalogue.RootContainerEntries.Single()
            .Resources.OfType<IProfileSymbol>().Single();
        var charTypeSymbol = catalogue.Gamesystem.ResourceDefinitions.Single(x => x.ResourceKind == ResourceKind.Profile).Definitions.Single();
        profile.Characteristics.Single().Type.Should().Be(charTypeSymbol);
    }

    [Fact]
    public void Given_bad_root_entry_link_When_GetDiagnostics_Then_returns_error()
    {
        // arrange
        var invalidEntry = NodeFactory.SelectionEntry("entry");
        var gst = NodeFactory.Gamesystem("foo");
        var cat = NodeFactory.Catalogue(gst, "bar")
            .AddEntryLinks(
                NodeFactory.EntryLink(invalidEntry));
        var compilation = WhamCompilation.Create();
        // act
        var result = compilation.AddSourceTrees(SourceTree.CreateForRoot(gst), SourceTree.CreateForRoot(cat));
        // assert
        result.GetDiagnostics().Should().ContainSingle()
            .Which.Should().Match<Diagnostic>(x =>
                x.Id == "WHAM0006"
                && x.GetMessage(null) == "ERR_NoBindingCandidates"
                && x.Severity == DiagnosticSeverity.Error);
        var catalogue = result.GlobalNamespace.Catalogues.Single(x => !x.IsGamesystem);
        catalogue.RootContainerEntries.Single().ReferencedEntry
            .Should().BeAssignableTo<IErrorSymbol>();
    }
}
