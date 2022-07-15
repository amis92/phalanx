using WarHub.ArmouryModel.Source;

namespace WarHub.ArmouryModel.Concrete;

internal static class ErrorSymbols
{
    public static TErrorSymbol WithErrorDetailsFrom<TErrorSymbol>(this TErrorSymbol @this, ErrorSymbolBase other)
        where TErrorSymbol : ErrorSymbolBase
    {
        return @this with
        {
            CandidateSymbols = other.CandidateSymbols,
            ResultKind = other.ResultKind,
            ErrorInfo = other.ErrorInfo,
            ErrorUnreported = other.ErrorUnreported,
        };
    }

    public static ErrorSymbolBase CreateResourceDefinition(ResourceKind kind) => kind switch

    {
        ResourceKind.Characteristic => new ErrorResourceDefinitionSymbol(),
        ResourceKind.Cost => new ErrorResourceDefinitionSymbol(),
        ResourceKind.Profile => new ErrorResourceDefinitionSymbol(),
        ResourceKind.Error => new ErrorResourceDefinitionSymbol(),
        _ => throw new NotSupportedException($"Can't instantiate error symbol for '{kind}' resource definition."),
    };

    public static ErrorSymbolBase CreateResourceEntry(ResourceKind kind) => kind switch

    {
        ResourceKind.Characteristic => new ErrorCharacteristicSymbol(),
        ResourceKind.Cost => new ErrorCostSymbol(),
        ResourceKind.Profile => new ErrorProfileSymbol(),
        ResourceKind.Publication => new ErrorPublicationSymbol(),
        ResourceKind.Rule => new ErrorRuleSymbol(),
        ResourceKind.Group => new ErrorResourceEntrySymbol(),
        ResourceKind.Error => new ErrorResourceEntrySymbol(),
        _ => throw new NotSupportedException($"Can't instantiate error symbol for '{kind}' resource."),
    };

    public static ErrorSymbolBase CreateContainerEntry(ContainerEntryKind kind) => kind switch
    {
        ContainerEntryKind.Selection => new ErrorSelectionEntrySymbol(),
        ContainerEntryKind.SelectionGroup => new ErrorSelectionEntryGroupSymbol(),
        ContainerEntryKind.Category => new ErrorCategoryEntrySymbol(),
        ContainerEntryKind.Force => new ErrorForceEntrySymbol(),
        ContainerEntryKind.Error => new ErrorContainerEntrySymbol(),
        _ => throw new NotSupportedException($"Can't instantiate error symbol for '{kind}' resource."),
    };

    internal record ErrorSymbolBase : ISymbol, IErrorSymbol
    {
        public virtual SymbolKind Kind => SymbolKind.Error;

        public string? Id { get; init; }

        public string Name { get; init; } = "Error";

        public string? Comment => null;

        public ISymbol? ContainingSymbol => null;

        public IModuleSymbol? ContainingModule => null;

        public IGamesystemNamespaceSymbol? ContainingNamespace => null;

        public ImmutableArray<ISymbol> CandidateSymbols { get; init; } = ImmutableArray<ISymbol>.Empty;

        public CandidateReason CandidateReason => CandidateSymbols.IsEmpty
            ? CandidateReason.None
            : ResultKind.ToCandidateReason();

        public LookupResultKind ResultKind { get; init; }

        public DiagnosticInfo? ErrorInfo { get; init; }

        public bool ErrorUnreported { get; init; }

        public void Accept(SymbolVisitor visitor)
        {
            visitor.VisitError(this);
        }

        public TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
        {
            return visitor.VisitError(this);
        }

        public TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument argument)
        {
            return visitor.VisitError(this, argument);
        }
    }

    internal record ErrorPublicationSymbol : ErrorResourceDefinitionSymbol, IPublicationSymbol
    {
        string? IPublicationSymbol.ShortName => null;

        string? IPublicationSymbol.Publisher => null;

        DateOnly? IPublicationSymbol.PublicationDate => null;

        Uri? IPublicationSymbol.PublicationUrl => null;
    }

    internal record ErrorResourceDefinitionSymbol : ErrorSymbolBase, IResourceDefinitionSymbol
    {
        public virtual ResourceKind ResourceKind { get; init; } = ResourceKind.Error;

        ImmutableArray<IResourceDefinitionSymbol> IResourceDefinitionSymbol.Definitions =>
            ImmutableArray<IResourceDefinitionSymbol>.Empty;
    }

    internal record ErrorCostSymbol : ErrorResourceEntrySymbol, ICostSymbol
    {
        decimal ICostSymbol.Value => default;
    }

    internal record ErrorCharacteristicSymbol : ErrorResourceEntrySymbol, ICharacteristicSymbol
    {
        string ICharacteristicSymbol.Value => string.Empty;
    }

    internal record ErrorProfileSymbol : ErrorResourceEntrySymbol, IProfileSymbol
    {
        public override ResourceKind ResourceKind => ResourceKind.Profile;

        IResourceDefinitionSymbol Type { get; init; } = new ErrorResourceDefinitionSymbol();

        IResourceDefinitionSymbol? IResourceEntrySymbol.Type => Type;

        ImmutableArray<ICharacteristicSymbol> IProfileSymbol.Characteristics => ImmutableArray<ICharacteristicSymbol>.Empty;
    }

    internal record ErrorRuleSymbol : ErrorResourceEntrySymbol, IRuleSymbol
    {
        public override ResourceKind ResourceKind => ResourceKind.Rule;

        string IRuleSymbol.DescriptionText => string.Empty;
    }

    internal record ErrorCategoryEntrySymbol : ErrorContainerEntrySymbol, ICategoryEntrySymbol
    {
        public override ContainerEntryKind ContainerKind => ContainerEntryKind.Category;

        bool ICategoryEntrySymbol.IsPrimaryCategory => false;

        ICategoryEntrySymbol? ICategoryEntrySymbol.ReferencedEntry => null;
    }

    internal record ErrorForceEntrySymbol : ErrorContainerEntrySymbol, IForceEntrySymbol
    {
        public override ContainerEntryKind ContainerKind => ContainerEntryKind.Force;

        ImmutableArray<IForceEntrySymbol> IForceEntrySymbol.ChildForces =>
            ImmutableArray<IForceEntrySymbol>.Empty;

        ImmutableArray<ICategoryEntrySymbol> IForceEntrySymbol.Categories =>
            ImmutableArray<ICategoryEntrySymbol>.Empty;
    }

    internal record ErrorSelectionEntrySymbol : ErrorSelectionEntryContainerSymbol, ISelectionEntrySymbol
    {
        public override ContainerEntryKind ContainerKind => ContainerEntryKind.Selection;

        SelectionEntryKind ISelectionEntrySymbol.EntryKind => SelectionEntryKind.Upgrade;
    }

    internal record ErrorSelectionEntryGroupSymbol : ErrorSelectionEntryContainerSymbol, ISelectionEntryGroupSymbol
    {
        public override ContainerEntryKind ContainerKind => ContainerEntryKind.SelectionGroup;

        ISelectionEntrySymbol? ISelectionEntryGroupSymbol.DefaultSelectionEntry => null;
    }

    internal record ErrorSelectionEntryContainerSymbol : ErrorContainerEntrySymbol, ISelectionEntryContainerSymbol
    {
        ICategoryEntrySymbol? ISelectionEntryContainerSymbol.PrimaryCategory => null;

        ImmutableArray<ICategoryEntrySymbol> ISelectionEntryContainerSymbol.Categories =>
            ImmutableArray<ICategoryEntrySymbol>.Empty;

        ImmutableArray<ISelectionEntryContainerSymbol> ISelectionEntryContainerSymbol.ChildSelectionEntries =>
            ImmutableArray<ISelectionEntryContainerSymbol>.Empty;

        ISelectionEntryContainerSymbol? ISelectionEntryContainerSymbol.ReferencedEntry => null;
    }

    internal abstract record ErrorEntryBaseSymbol : ErrorSymbolBase, IEntrySymbol
    {
        IEntrySymbol? IEntrySymbol.ReferencedEntry => null;

        bool IEntrySymbol.IsHidden => false;

        bool IEntrySymbol.IsReference => false;

        IPublicationReferenceSymbol? IEntrySymbol.PublicationReference => null;

        ImmutableArray<IEffectSymbol> IEntrySymbol.Effects => ImmutableArray<IEffectSymbol>.Empty;

        ImmutableArray<IResourceEntrySymbol> IEntrySymbol.Resources => ImmutableArray<IResourceEntrySymbol>.Empty;
    }

    internal record ErrorResourceEntrySymbol : ErrorEntryBaseSymbol, IResourceEntrySymbol
    {
        public override SymbolKind Kind => SymbolKind.Resource;

        IResourceEntrySymbol? IResourceEntrySymbol.ReferencedEntry => null;

        IResourceDefinitionSymbol? IResourceEntrySymbol.Type => null;

        public virtual ResourceKind ResourceKind { get; init; } = ResourceKind.Error;
    }

    internal record ErrorContainerEntrySymbol : ErrorEntryBaseSymbol, IContainerEntrySymbol
    {
        public override SymbolKind Kind => SymbolKind.ContainerEntry;

        public virtual ContainerEntryKind ContainerKind { get; init; } = ContainerEntryKind.Error;

        public ImmutableArray<ICostSymbol> Costs => ImmutableArray<ICostSymbol>.Empty;

        ImmutableArray<IConstraintSymbol> IContainerEntrySymbol.Constraints =>
            ImmutableArray<IConstraintSymbol>.Empty;
    }

    internal record ErrorGamesystemSymbol : ErrorCatalogueBaseSymbol
    {
        public override bool IsGamesystem => true;

        public override ICatalogueSymbol Gamesystem => this;
    }

    internal record ErrorCatalogueSymbol : ErrorCatalogueBaseSymbol
    {
        public Func<ICatalogueSymbol>? GamesystemProvider { get; init; }

        public override bool IsGamesystem => false;

        public override ICatalogueSymbol Gamesystem =>
            GamesystemProvider?.Invoke() ?? new ErrorGamesystemSymbol();
    }

    internal abstract record ErrorCatalogueBaseSymbol : ErrorSymbolBase, ICatalogueSymbol
    {
        public override SymbolKind Kind => SymbolKind.Catalogue;

        bool ICatalogueSymbol.IsLibrary => false;

        public abstract bool IsGamesystem { get; }

        public abstract ICatalogueSymbol Gamesystem { get; }

        ImmutableArray<ICatalogueReferenceSymbol> ICatalogueSymbol.CatalogueReferences =>
            ImmutableArray<ICatalogueReferenceSymbol>.Empty;

        ImmutableArray<ISymbol> ICatalogueSymbol.AllItems =>
            ImmutableArray<ISymbol>.Empty;

        ImmutableArray<IResourceDefinitionSymbol> ICatalogueSymbol.ResourceDefinitions =>
            ImmutableArray<IResourceDefinitionSymbol>.Empty;

        ImmutableArray<IContainerEntrySymbol> ICatalogueSymbol.RootContainerEntries =>
            ImmutableArray<IContainerEntrySymbol>.Empty;

        ImmutableArray<IResourceEntrySymbol> ICatalogueSymbol.RootResourceEntries =>
            ImmutableArray<IResourceEntrySymbol>.Empty;

        ImmutableArray<ISelectionEntryContainerSymbol> ICatalogueSymbol.SharedSelectionEntryContainers =>
            ImmutableArray<ISelectionEntryContainerSymbol>.Empty;

        ImmutableArray<IResourceEntrySymbol> ICatalogueSymbol.SharedResourceEntries =>
            ImmutableArray<IResourceEntrySymbol>.Empty;
    }
}
