# ONE-SHOT CODEX PROMPT — GestionLivraisons Microservices (.NET 8)
# SOLID Principles + Design Patterns: IDisposable, Factory, Repository, Unit of Work
# Architecture: YARP Gateway + 4 Domain APIs + Identity API + MVC Razor SSR Frontend

---

## SYSTEM CONTEXT FOR CODEX

You are a senior .NET 8 software engineer and architecture instructor. Generate a **complete, compilable Visual Studio solution** demonstrating real microservices architecture AND explicitly applying every SOLID principle and every listed design pattern in their correct, canonical form. Every pattern and principle must be visible in the code structure — not implied. All files must be complete, no stubs, no TODOs.

---

## MANDATORY: SOLID PRINCIPLES — WHERE EACH ONE LIVES

Before writing any code, understand exactly how each principle is enforced:

### S — Single Responsibility Principle (SRP)
> *A class should have only one reason to change.*

**Enforcement rules — every class does exactly ONE thing:**
- `Repository<T>` → only data access (no business logic, no validation)
- `ColisService` → only business rules for Colis (no HTTP, no EF queries directly)
- `TokenService` → only JWT generation (not authentication, not user management)
- `VehiculeFactory` → only object construction (not persistence, not validation)
- `AuthTokenHandler` → only attaches Bearer token to requests (nothing else)
- `DashboardApiClient` → only HTTP calls to Dashboard.API (no data transformation)
- `ColisController` → only HTTP request/response orchestration (delegates all logic to service/client)
- `DbInitializer` → only seeding (not migrations, not context configuration)
- **NEVER** put EF queries inside a Controller. **NEVER** put business logic inside a Repository.

### O — Open/Closed Principle (OCP)
> *A class should be open for extension, closed for modification.*

**Enforcement rules:**
- `IVehiculeFactory` interface + `VehiculeFactory` implementation using a `switch expression` → adding `Moto` vehicle type requires zero modification to existing code, only a new `Moto : Vehicule` class and a new case
- `IRepository<T>` generic interface → adding a new entity (`Entrepot`) requires zero changes to existing repositories
- `IColisService`, `ILivreurService`, etc. → new business rules implemented as new service decorators, not modifications
- `ISearchStrategy<T>` (see below) → search filters extended by adding new strategy classes

Implement this search strategy pattern in `Colis.API`:
```csharp
// Open for extension: add new filters without modifying ColisSearchEngine
public interface ISearchStrategy<T>
{
    IQueryable<T> Apply(IQueryable<T> query, ColisSearchParams p);
}

public class LibelleSearchStrategy : ISearchStrategy<Colis>
{
    public IQueryable<Colis> Apply(IQueryable<Colis> query, ColisSearchParams p) =>
        string.IsNullOrWhiteSpace(p.Libelle) ? query : query.Where(c => c.Libelle.Contains(p.Libelle));
}

public class MontantRangeStrategy : ISearchStrategy<Colis>
{
    public IQueryable<Colis> Apply(IQueryable<Colis> query, ColisSearchParams p)
    {
        if (p.MinMontant.HasValue) query = query.Where(c => c.Montant >= p.MinMontant);
        if (p.MaxMontant.HasValue) query = query.Where(c => c.Montant <= p.MaxMontant);
        return query;
    }
}

public class PoidsMaxStrategy : ISearchStrategy<Colis>
{
    public IQueryable<Colis> Apply(IQueryable<Colis> query, ColisSearchParams p) =>
        p.MaxPoids.HasValue ? query.Where(c => c.Poids <= p.MaxPoids) : query;
}

public class DateLivraisonStrategy : ISearchStrategy<Colis>
{
    public IQueryable<Colis> Apply(IQueryable<Colis> query, ColisSearchParams p) =>
        p.DateLivraison.HasValue ? query.Where(c => c.DateLivraison.Date == p.DateLivraison.Value.Date) : query;
}

// Engine composes all strategies — closed for modification
public class ColisSearchEngine(IEnumerable<ISearchStrategy<Colis>> strategies)
{
    public IQueryable<Colis> Apply(IQueryable<Colis> query, ColisSearchParams p)
    {
        foreach (var strategy in strategies) query = strategy.Apply(query, p);
        return query;
    }
}
```
Register all strategies in DI:
```csharp
builder.Services.AddScoped<ISearchStrategy<Colis>, LibelleSearchStrategy>();
builder.Services.AddScoped<ISearchStrategy<Colis>, MontantRangeStrategy>();
builder.Services.AddScoped<ISearchStrategy<Colis>, PoidsMaxStrategy>();
builder.Services.AddScoped<ISearchStrategy<Colis>, DateLivraisonStrategy>();
builder.Services.AddScoped<ColisSearchEngine>();
```

### L — Liskov Substitution Principle (LSP)
> *Subtypes must be substitutable for their base types without altering correctness.*

**Enforcement rules:**
- `Camion : Vehicule` and `Voiture : Vehicule` — any code accepting `Vehicule` works with both without type-checking
- All repositories implementing `IRepository<T>` are fully substitutable — the service layer never checks the concrete type
- `ApplicationUser : IdentityUser` — any method accepting `IdentityUser` works with `ApplicationUser`
- **VIOLATION to avoid**: never write `if (vehicule is Camion c)` in service/business logic. Use polymorphism instead.

Enforce LSP in `Vehicule` with a virtual method:
```csharp
public abstract class Vehicule
{
    // ... properties ...

    // LSP: every subtype provides its own correct implementation
    // Callers use Vehicule, never need to cast
    public abstract string GetDescription();
    public abstract int GetCapacitePassagers(); // returns 0 for Camion (no passengers)
    public abstract bool PeutTransporter(double poidsKg);
}

public class Camion : Vehicule
{
    public double Capacite { get; set; }
    public int NbrEssieux { get; set; }

    public override string GetDescription() =>
        $"Camion {Marque} — {NbrEssieux} essieux — capacité {Capacite} kg";

    public override int GetCapacitePassagers() => 0; // Camion carries no passengers — valid substitution

    public override bool PeutTransporter(double poidsKg) => poidsKg <= Capacite;
}

public class Voiture : Vehicule
{
    public int NbrPlaces { get; set; }

    public override string GetDescription() =>
        $"Voiture {Marque} — {NbrPlaces} places";

    public override int GetCapacitePassagers() => NbrPlaces;

    public override bool PeutTransporter(double poidsKg) => poidsKg <= 500; // Voiture limit
}
```

### I — Interface Segregation Principle (ISP)
> *Clients should not be forced to depend on interfaces they do not use.*

**Enforcement rules — split fat interfaces into focused ones:**

In `Shared.Contracts` and each API's `Repositories/Interfaces/`, define:

```csharp
// ISP: Split repository capabilities into focused interfaces
// Read-only clients only depend on IReadRepository<T>
public interface IReadRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IReadOnlyList<T>> GetAllAsync();
}

// Write clients depend on IWriteRepository<T>
public interface IWriteRepository<T> where T : class
{
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
}

// Full repo composes both — only used where CRUD is needed
public interface IRepository<T> : IReadRepository<T>, IWriteRepository<T> where T : class { }

// Domain-specific read interface — user role only needs this
public interface IColisReadRepository : IReadRepository<Colis>
{
    Task<IReadOnlyList<Colis>> GetByLivreurAsync(int livreurId);
    Task<IReadOnlyList<Colis>> GetRecentAsync(int count);
}

// Domain-specific full interface — admin needs this
public interface IColisRepository : IRepository<Colis>, IColisReadRepository
{
    Task<(IReadOnlyList<Colis> Items, int Total)> SearchPagedAsync(ColisSearchParams p, int page, int pageSize);
}
```

ISP for services:
```csharp
// User role only needs read service
public interface IColisReadService
{
    Task<PagedResult<ColisDto>> SearchAsync(ColisSearchParams p);
    Task<ColisDto?> GetByIdAsync(int id);
}

// Admin needs full service
public interface IColisAdminService : IColisReadService
{
    Task<ColisDto> CreateAsync(CreateColisDto dto);
    Task UpdateAsync(int id, CreateColisDto dto);
    Task DeleteAsync(int id);
}

// ColisService implements the full interface
public class ColisService : IColisAdminService { /* ... */ }
```

Register:
```csharp
builder.Services.AddScoped<IColisAdminService, ColisService>();
builder.Services.AddScoped<IColisReadService>(sp => sp.GetRequiredService<IColisAdminService>());
```

ISP for `IVehiculeFactory` — split creation from validation:
```csharp
public interface IVehiculeCreator { Vehicule Create(CreateVehiculeDto dto); }
public interface IVehiculeValidator { ValidationResult Validate(CreateVehiculeDto dto); }
public interface IVehiculeFactory : IVehiculeCreator, IVehiculeValidator { }
```

### D — Dependency Inversion Principle (DIP)
> *High-level modules must not depend on low-level modules. Both depend on abstractions.*

**Enforcement rules:**
- `ColisService` depends on `IColisRepository` (abstraction), NOT on `ColisRepository` (concrete)
- `ColisRepository` depends on `ColisDbContext` — registered via DI, never `new ColisDbContext()`
- `UnitOfWork` depends on `IColisRepository`, `ILivreurRepository` — never on concrete classes
- MVC Controllers depend on `IColisAdminService` / `IColisReadService` — never on `ColisService` directly
- Dashboard.API depends on `IColisApiClient`, `ILivreurApiClient` — abstractions over HttpClient wrappers
- **NEVER** use `new` for anything that has side effects or dependencies — always constructor inject

DIP for typed API clients in Dashboard.API:
```csharp
// Abstraction — Dashboard.API's high-level logic depends on this
public interface IColisApiClient
{
    Task<ColisStatsDto?> GetStatsAsync();
    Task<PagedResult<ColisDto>?> GetRecentAsync(int count);
}

// Low-level implementation — depends on HttpClient (also an abstraction)
public class ColisApiClient(HttpClient http) : IColisApiClient { /* ... */ }
```

---

## DESIGN PATTERNS — CANONICAL IMPLEMENTATIONS

### PATTERN 1 — IDisposable (Resource Management)

Implement `IDisposable` correctly on `UnitOfWork` in each API that has a DbContext. Follow the full dispose pattern with `GC.SuppressFinalize`:

```csharp
public class UnitOfWork : IUnitOfWork
{
    private readonly ColisDbContext _context;
    private IColisRepository? _colisRepo;
    private bool _disposed = false;

    public UnitOfWork(ColisDbContext context) => _context = context;

    // Lazy initialization — repositories created only when first accessed
    public IColisRepository Colis =>
        _colisRepo ??= new ColisRepository(_context);

    public async Task<int> SaveAsync() => await _context.SaveChangesAsync();

    // IDisposable — full canonical pattern
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this); // Prevent finalizer from running — we already cleaned up
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _context.Dispose(); // Managed resource — dispose only when called from Dispose()
            }
            // If we had unmanaged resources (native handles, etc.) we'd free them here
            _disposed = true;
        }
    }

    // Finalizer — safety net if consumer forgets to call Dispose()
    ~UnitOfWork()
    {
        Dispose(disposing: false); // Only free unmanaged resources from finalizer
    }
}
```

Also implement `IAsyncDisposable` for async cleanup:
```csharp
public class UnitOfWork : IUnitOfWork, IAsyncDisposable
{
    // ... same as above, plus:

    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}
```

The `IUnitOfWork` interface must extend `IDisposable`:
```csharp
public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    IColisRepository Colis { get; }
    Task<int> SaveAsync();
}
```

Use `IUnitOfWork` with `await using` in services to ensure disposal:
```csharp
// In ColisService — proper async disposal
public async Task<ColisDto> CreateAsync(CreateColisDto dto)
{
    await using var uow = _unitOfWorkFactory.Create(); // factory-created scoped UoW
    var colis = new Colis { /* map from dto */ };
    await uow.Colis.AddAsync(colis);
    await uow.SaveAsync();
    return MapToDto(colis);
}
```

### PATTERN 2 — Factory Pattern (VehiculeFactory in Livreur.API)

Full canonical Factory with interface segregation and validation:

```csharp
// --- Interfaces (ISP applied) ---

public interface IVehiculeCreator
{
    Vehicule Create(CreateVehiculeDto dto);
}

public interface IVehiculeValidator
{
    ValidationResult Validate(CreateVehiculeDto dto);
}

public interface IVehiculeFactory : IVehiculeCreator, IVehiculeValidator { }

// --- Validation Result (SRP: separate concern) ---

public class ValidationResult
{
    public bool IsValid { get; private init; }
    public IReadOnlyList<string> Errors { get; private init; } = [];

    public static ValidationResult Success() => new() { IsValid = true, Errors = [] };
    public static ValidationResult Failure(params string[] errors) => new() { IsValid = false, Errors = errors };
}

// --- Concrete Factory (OCP: extend by adding new vehicle types) ---

public class VehiculeFactory : IVehiculeFactory
{
    // IVehiculeValidator
    public ValidationResult Validate(CreateVehiculeDto dto)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(dto.Matricule)) errors.Add("Matricule est requis.");
        if (string.IsNullOrWhiteSpace(dto.Marque))    errors.Add("Marque est requise.");
        if (dto.VitesseLimite <= 0)                   errors.Add("Vitesse limite doit être positive.");

        errors.AddRange(dto.Type switch
        {
            "Camion"  => ValidateCamion(dto),
            "Voiture" => ValidateVoiture(dto),
            _         => [$"Type de véhicule inconnu: '{dto.Type}'."]
        });

        return errors.Count == 0 ? ValidationResult.Success() : ValidationResult.Failure([.. errors]);
    }

    private static IEnumerable<string> ValidateCamion(CreateVehiculeDto dto)
    {
        if (!dto.Capacite.HasValue || dto.Capacite <= 0) yield return "Capacité requise pour un Camion.";
        if (!dto.NbrEssieux.HasValue || dto.NbrEssieux < 2) yield return "NbrEssieux minimum 2 pour un Camion.";
    }

    private static IEnumerable<string> ValidateVoiture(CreateVehiculeDto dto)
    {
        if (!dto.NbrPlaces.HasValue || dto.NbrPlaces < 1) yield return "NbrPlaces requis pour une Voiture.";
    }

    // IVehiculeCreator — OCP: add new type here without modifying existing branches
    public Vehicule Create(CreateVehiculeDto dto) => dto.Type switch
    {
        "Camion"  => new Camion
        {
            Couleur      = dto.Couleur,
            Marque       = dto.Marque,
            Matricule    = dto.Matricule,
            VitesseLimite= dto.VitesseLimite,
            LivreurId    = dto.LivreurId ?? 0,
            Capacite     = dto.Capacite!.Value,
            NbrEssieux   = dto.NbrEssieux!.Value,
        },
        "Voiture" => new Voiture
        {
            Couleur      = dto.Couleur,
            Marque       = dto.Marque,
            Matricule    = dto.Matricule,
            VitesseLimite= dto.VitesseLimite,
            LivreurId    = dto.LivreurId ?? 0,
            NbrPlaces    = dto.NbrPlaces!.Value,
        },
        _ => throw new InvalidOperationException($"Type inconnu: {dto.Type}. Valider avant de créer.")
    };
}
```

Register as Singleton (stateless, thread-safe):
```csharp
builder.Services.AddSingleton<IVehiculeFactory, VehiculeFactory>();
```

Usage in endpoint — always validate before create:
```csharp
app.MapPost("/api/vehicules", async (CreateVehiculeDto dto, IVehiculeFactory factory, IUnitOfWork uow) =>
{
    var validation = factory.Validate(dto); // IVehiculeValidator
    if (!validation.IsValid)
        return Results.BadRequest(new { Errors = validation.Errors });

    var vehicule = factory.Create(dto); // IVehiculeCreator — only called after validation
    await uow.Vehicules.AddAsync(vehicule);
    await uow.SaveAsync();
    return Results.Created($"/api/vehicules/{vehicule.Id}", vehicule.ToDto());
});
```

### PATTERN 3 — Repository Pattern (Generic + Specific)

```csharp
// --- Generic Read Interface (ISP) ---
public interface IReadRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IReadOnlyList<T>> GetAllAsync();
    Task<bool> ExistsAsync(int id);
}

// --- Generic Write Interface (ISP) ---
public interface IWriteRepository<T> where T : class
{
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
}

// --- Full Generic Repository (composes Read + Write) ---
public interface IRepository<T> : IReadRepository<T>, IWriteRepository<T> where T : class { }

// --- Generic Implementation (DIP: depends on DbContext abstraction via EF) ---
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly DbContext _context;
    protected readonly DbSet<T> _set;

    public Repository(DbContext context)
    {
        _context = context;
        _set = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id) =>
        await _set.FindAsync(id);

    public async Task<IReadOnlyList<T>> GetAllAsync() =>
        await _set.ToListAsync();

    public async Task<bool> ExistsAsync(int id) =>
        await _set.FindAsync(id) is not null;

    public async Task AddAsync(T entity) =>
        await _set.AddAsync(entity);

    public void Update(T entity) =>
        _context.Entry(entity).State = EntityState.Modified;

    public void Delete(T entity) =>
        _set.Remove(entity);
}

// --- Domain-Specific Interface (ISP + OCP) ---
public interface IColisRepository : IRepository<Colis>
{
    Task<(IReadOnlyList<Colis> Items, int Total)> SearchPagedAsync(
        ColisSearchParams p, int page, int pageSize);
    Task<IReadOnlyList<Colis>> GetByLivreurAsync(int livreurId);
    Task<IReadOnlyList<Colis>> GetRecentAsync(int count);
    Task<ColisStatsProjection> GetStatsAsync();
}

// --- Domain-Specific Implementation (SRP: only Colis data access logic) ---
public class ColisRepository(ColisDbContext context, ColisSearchEngine searchEngine)
    : Repository<Colis>(context), IColisRepository
{
    private readonly ColisDbContext _colisContext = context;

    public async Task<(IReadOnlyList<Colis> Items, int Total)> SearchPagedAsync(
        ColisSearchParams p, int page, int pageSize)
    {
        // OCP: delegates to search engine which composes strategy objects
        var query = searchEngine.Apply(_colisContext.Colis.AsQueryable(), p);
        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(c => c.DateLivraison)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (items, total);
    }

    public async Task<IReadOnlyList<Colis>> GetByLivreurAsync(int livreurId) =>
        await _colisContext.Colis.Where(c => c.LivreurId == livreurId).ToListAsync();

    public async Task<IReadOnlyList<Colis>> GetRecentAsync(int count) =>
        await _colisContext.Colis
            .OrderByDescending(c => c.DateLivraison)
            .Take(count)
            .ToListAsync();

    public async Task<ColisStatsProjection> GetStatsAsync()
    {
        var all = await _colisContext.Colis.ToListAsync();
        return new ColisStatsProjection
        {
            TotalColis    = all.Count,
            TotalMontant  = all.Sum(c => c.Montant),
            ColisParLivreur = all
                .GroupBy(c => c.LivreurId)
                .ToDictionary(g => g.Key.ToString(), g => g.Count()),
            MontantParMois  = all
                .GroupBy(c => c.DateLivraison.ToString("yyyy-MM"))
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key, g => g.Sum(c => c.Montant))
        };
    }
}

// --- Livreur-specific ---
public interface ILivreurRepository : IRepository<Livreur>
{
    Task<bool> CINExistsAsync(string cin, int? excludeId = null);
    Task<IReadOnlyList<Livreur>> GetByVilleAsync(string ville);
}

public class LivreurRepository(LivreurDbContext context) : Repository<Livreur>(context), ILivreurRepository
{
    private readonly LivreurDbContext _livreurContext = context;

    public async Task<bool> CINExistsAsync(string cin, int? excludeId = null) =>
        await _livreurContext.Livreurs
            .AnyAsync(l => l.CIN == cin && (!excludeId.HasValue || l.Id != excludeId));

    public async Task<IReadOnlyList<Livreur>> GetByVilleAsync(string ville) =>
        await _livreurContext.Livreurs.Where(l => l.Ville == ville).ToListAsync();
}

// --- Vehicule-specific ---
public interface IVehiculeRepository : IRepository<Vehicule>
{
    Task<bool> MatriculeExistsAsync(string matricule, int? excludeId = null);
    Task<IReadOnlyList<Vehicule>> GetByLivreurAsync(int livreurId);
}
```

### PATTERN 4 — Unit of Work Pattern

```csharp
// --- Interface (DIP + ISP + IDisposable) ---
public interface IUnitOfWork : IDisposable, IAsyncDisposable
{
    // Each repo exposed as its domain-specific interface (ISP)
    IColisRepository Colis { get; }
    Task<int> SaveAsync();
}

// Livreur.API has its own UoW
public interface ILivreurUnitOfWork : IDisposable, IAsyncDisposable
{
    ILivreurRepository Livreurs { get; }
    IVehiculeRepository Vehicules { get; }
    Task<int> SaveAsync();
}

// --- Concrete UoW with Lazy Initialization + Full IDisposable Pattern ---
public class UnitOfWork : IUnitOfWork
{
    private readonly ColisDbContext _context;

    // Lazy repo initialization — only created when accessed (avoids unnecessary allocations)
    private IColisRepository? _colisRepo;

    private bool _disposed = false;

    public UnitOfWork(ColisDbContext context, ColisSearchEngine searchEngine)
    {
        _context      = context;
        _searchEngine = searchEngine;
    }

    private readonly ColisSearchEngine _searchEngine;

    // Lazy property — SRP: UoW doesn't create repos eagerly; it coordinates them
    public IColisRepository Colis =>
        _colisRepo ??= new ColisRepository(_context, _searchEngine);

    public async Task<int> SaveAsync()
    {
        // Could add: domain event dispatching, audit logging here (single place)
        return await _context.SaveChangesAsync();
    }

    // IDisposable — canonical full pattern
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
                _context.Dispose(); // Dispose managed resources
            // Unmanaged resources would be freed here regardless of 'disposing'
            _disposed = true;
        }
    }

    // Finalizer — safety net for consumers who forget to dispose
    ~UnitOfWork() => Dispose(disposing: false);

    // IAsyncDisposable — prefer this in async contexts
    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }
}

// --- LivreurUnitOfWork ---
public class LivreurUnitOfWork : ILivreurUnitOfWork
{
    private readonly LivreurDbContext _context;
    private ILivreurRepository?  _livreurRepo;
    private IVehiculeRepository? _vehiculeRepo;
    private bool _disposed = false;

    public LivreurUnitOfWork(LivreurDbContext context) => _context = context;

    public ILivreurRepository  Livreurs  => _livreurRepo  ??= new LivreurRepository(_context);
    public IVehiculeRepository Vehicules => _vehiculeRepo ??= new VehiculeRepository(_context);

    public async Task<int> SaveAsync() => await _context.SaveChangesAsync();

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed) { if (disposing) _context.Dispose(); _disposed = true; }
    }

    ~LivreurUnitOfWork() => Dispose(disposing: false);

    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }
}
```

---

## SOLUTION STRUCTURE

```
GestionLivraisons.sln
├── src/
│   ├── Gateway/                        ← YARP Reverse Proxy          (port 5000)
│   ├── Services/
│   │   ├── Identity.API/               ← Auth + JWT                  (port 5001)
│   │   │   ├── Data/
│   │   │   │   ├── AppIdentityDbContext.cs
│   │   │   │   └── IdentitySeeder.cs
│   │   │   ├── Models/
│   │   │   │   └── ApplicationUser.cs
│   │   │   ├── Services/
│   │   │   │   └── TokenService.cs     ← SRP: only JWT generation
│   │   │   └── Program.cs
│   │   │
│   │   ├── Colis.API/                  ← Colis domain               (port 5002)
│   │   │   ├── Data/
│   │   │   │   ├── ColisDbContext.cs
│   │   │   │   └── ColisSeeder.cs
│   │   │   ├── Models/
│   │   │   │   ├── Colis.cs
│   │   │   │   └── ColisStatsProjection.cs
│   │   │   ├── Repositories/
│   │   │   │   ├── Interfaces/
│   │   │   │   │   ├── IReadRepository.cs   ← ISP
│   │   │   │   │   ├── IWriteRepository.cs  ← ISP
│   │   │   │   │   ├── IRepository.cs       ← ISP (composes Read+Write)
│   │   │   │   │   └── IColisRepository.cs  ← domain-specific
│   │   │   │   ├── Repository.cs            ← Generic implementation
│   │   │   │   └── ColisRepository.cs       ← SRP + LSP
│   │   │   ├── Search/
│   │   │   │   ├── ColisSearchParams.cs
│   │   │   │   ├── ISearchStrategy.cs       ← OCP hook point
│   │   │   │   ├── LibelleSearchStrategy.cs
│   │   │   │   ├── MontantRangeStrategy.cs
│   │   │   │   ├── PoidsMaxStrategy.cs
│   │   │   │   ├── DateLivraisonStrategy.cs
│   │   │   │   └── ColisSearchEngine.cs     ← OCP: composes strategies
│   │   │   ├── Services/
│   │   │   │   ├── Interfaces/
│   │   │   │   │   ├── IColisReadService.cs    ← ISP: User role
│   │   │   │   │   └── IColisAdminService.cs   ← ISP: Admin role
│   │   │   │   └── ColisService.cs             ← SRP + DIP
│   │   │   ├── UnitOfWork/
│   │   │   │   ├── IUnitOfWork.cs              ← IDisposable + ISP
│   │   │   │   └── UnitOfWork.cs               ← IDisposable full pattern
│   │   │   └── Program.cs
│   │   │
│   │   ├── Livreur.API/                ← Livreur + Vehicule domain  (port 5003)
│   │   │   ├── Data/
│   │   │   │   ├── LivreurDbContext.cs
│   │   │   │   └── LivreurSeeder.cs
│   │   │   ├── Models/
│   │   │   │   ├── Livreur.cs
│   │   │   │   ├── Vehicule.cs         ← abstract + LSP virtual methods
│   │   │   │   ├── Camion.cs           ← LSP correct subtype
│   │   │   │   └── Voiture.cs          ← LSP correct subtype
│   │   │   ├── Factories/
│   │   │   │   ├── IVehiculeCreator.cs    ← ISP
│   │   │   │   ├── IVehiculeValidator.cs  ← ISP
│   │   │   │   ├── IVehiculeFactory.cs   ← ISP (composes Creator+Validator)
│   │   │   │   ├── ValidationResult.cs
│   │   │   │   └── VehiculeFactory.cs    ← OCP + SRP
│   │   │   ├── Repositories/
│   │   │   │   ├── Interfaces/
│   │   │   │   │   ├── IReadRepository.cs
│   │   │   │   │   ├── IWriteRepository.cs
│   │   │   │   │   ├── IRepository.cs
│   │   │   │   │   ├── ILivreurRepository.cs
│   │   │   │   │   └── IVehiculeRepository.cs
│   │   │   │   ├── Repository.cs
│   │   │   │   ├── LivreurRepository.cs
│   │   │   │   └── VehiculeRepository.cs
│   │   │   ├── UnitOfWork/
│   │   │   │   ├── ILivreurUnitOfWork.cs    ← IDisposable
│   │   │   │   └── LivreurUnitOfWork.cs     ← full IDisposable pattern
│   │   │   ├── Services/
│   │   │   │   ├── Interfaces/
│   │   │   │   │   ├── ILivreurReadService.cs
│   │   │   │   │   └── ILivreurAdminService.cs
│   │   │   │   └── LivreurService.cs
│   │   │   └── Program.cs
│   │   │
│   │   ├── Client.API/                 ← Client domain              (port 5004)
│   │   │   ├── Data/, Models/, Repositories/, UnitOfWork/, Services/
│   │   │   └── Program.cs
│   │   │
│   │   └── Dashboard.API/              ← Aggregator (no DB)         (port 5005)
│   │       ├── Clients/
│   │       │   ├── IColisApiClient.cs      ← DIP abstraction
│   │       │   ├── ColisApiClient.cs       ← DIP implementation
│   │       │   ├── ILivreurApiClient.cs
│   │       │   ├── LivreurApiClient.cs
│   │       │   ├── IClientApiClient.cs
│   │       │   └── ClientApiClient.cs
│   │       └── Program.cs
│   │
│   └── Frontend/
│       └── GestionLivraisons.Web/      ← MVC Razor SSR              (port 5006)
│           ├── Infrastructure/
│           │   └── AuthTokenHandler.cs     ← SRP: only token injection
│           ├── HttpClients/
│           │   ├── Interfaces/             ← DIP: controllers depend on these
│           │   │   ├── IAuthApiClient.cs
│           │   │   ├── IColisApiClient.cs
│           │   │   ├── ILivreurApiClient.cs
│           │   │   ├── IClientApiClient.cs
│           │   │   └── IDashboardApiClient.cs
│           │   ├── AuthApiClient.cs
│           │   ├── ColisApiClient.cs
│           │   ├── LivreurApiClient.cs
│           │   ├── ClientApiClient.cs
│           │   └── DashboardApiClient.cs
│           ├── Models/ViewModels/
│           │   ├── LoginVM.cs
│           │   ├── RegisterVM.cs
│           │   ├── ColisSearchVM.cs
│           │   └── DashboardVM.cs
│           ├── Controllers/
│           │   ├── AccountController.cs    ← depends on IAuthApiClient (DIP)
│           │   ├── AdminController.cs      ← depends on IXxxApiClient (DIP)
│           │   ├── ColisController.cs      ← depends on IColisApiClient (DIP)
│           │   └── DashboardController.cs  ← depends on IDashboardApiClient (DIP)
│           ├── Views/
│           │   ├── Shared/
│           │   │   ├── _Layout.cshtml
│           │   │   ├── _NavBar.cshtml
│           │   │   └── _ValidationScripts.cshtml
│           │   ├── Account/Login.cshtml
│           │   ├── Account/Register.cshtml
│           │   ├── Admin/
│           │   │   ├── Livreurs.cshtml
│           │   │   ├── Clients.cshtml
│           │   │   ├── Vehicules.cshtml    ← Camion/Voiture type dropdown + JS conditional fields
│           │   │   ├── Colis.cshtml
│           │   │   └── Comptes.cshtml
│           │   ├── Colis/
│           │   │   ├── Index.cshtml        ← search form + paginated table
│           │   │   └── Details.cshtml
│           │   └── Dashboard/
│           │       └── Index.cshtml        ← Chart.js bar + line + doughnut
│           ├── Program.cs
│           └── appsettings.json
│
└── shared/
    └── Shared.Contracts/               ← DTOs + abstractions shared across services
        ├── DTOs/
        │   ├── ColisDto.cs
        │   ├── LivreurDto.cs
        │   ├── ClientDto.cs
        │   ├── AuthDto.cs
        │   └── DashboardDto.cs
        └── Shared.Contracts.csproj
```

---

## COMPLETE FILE IMPLEMENTATIONS

### `Shared.Contracts/DTOs/ColisDto.cs`
```csharp
namespace Shared.Contracts.DTOs;

public record ColisDto(
    int Id, string Libelle, DateTime DateLivraison,
    decimal Montant, double Poids, double Volume,
    int LivreurId, string? LivreurNom, int ClientId, string? ClientNom);

public record CreateColisDto(
    string Libelle, DateTime DateLivraison,
    decimal Montant, double Poids, double Volume,
    int LivreurId, int ClientId);

public record ColisSearchParams(
    string? Libelle = null, decimal? MinMontant = null, decimal? MaxMontant = null,
    double? MaxPoids = null, DateTime? DateLivraison = null);

public class PagedResult<T>
{
    public List<T> Items     { get; init; } = [];
    public int     TotalCount { get; init; }
    public int     Page       { get; init; }
    public int     PageSize   { get; init; }
    public int     TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
}
```

### `Shared.Contracts/DTOs/AuthDto.cs`
```csharp
namespace Shared.Contracts.DTOs;

public record LoginRequestDto(string Email, string Password);
public record RegisterRequestDto(string Email, string Password, string FullName);

public class AuthResponseDto
{
    public bool    Success  { get; init; }
    public string? Token    { get; init; }
    public string? Email    { get; init; }
    public string? FullName { get; init; }
    public string? Role     { get; init; }
    public string? Error    { get; init; }
}

public record UserDto(string Id, string Email, string FullName, string Role);
```

### `Shared.Contracts/DTOs/DashboardDto.cs`
```csharp
namespace Shared.Contracts.DTOs;

public class DashboardDto
{
    public int                       TotalColis      { get; init; }
    public decimal                   TotalMontant    { get; init; }
    public int                       TotalLivreurs   { get; init; }
    public int                       TotalClients    { get; init; }
    public Dictionary<string, int>   ColisParLivreur { get; init; } = [];
    public Dictionary<string, int>   ColisParVille   { get; init; } = [];
    public Dictionary<string, decimal> MontantParMois { get; init; } = [];
    public List<ColisDto>            ColisRecents    { get; init; } = [];
}
```

---

## IDENTITY.API — COMPLETE Program.cs

```csharp
using Identity.API.Data;
using Identity.API.Models;
using Identity.API.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.DTOs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppIdentityDbContext>(o =>
    o.UseSqlite("Data Source=identity.db"));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(o =>
{
    o.Password.RequireDigit = true;
    o.Password.RequiredLength = 6;
    o.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<AppIdentityDbContext>()
.AddDefaultTokenProviders();

// SRP: TokenService only generates tokens
builder.Services.AddScoped<TokenService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppIdentityDbContext>();
    await db.Database.MigrateAsync();
    await IdentitySeeder.SeedAsync(scope.ServiceProvider);
}

app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/api/auth/login", async (
    LoginRequestDto dto,
    UserManager<ApplicationUser> um,
    SignInManager<ApplicationUser> sm,
    TokenService ts) =>
{
    var user = await um.FindByEmailAsync(dto.Email);
    if (user is null)
        return Results.Ok(new AuthResponseDto { Success = false, Error = "Identifiants incorrects." });

    if (!await sm.CheckPasswordSignInAsync(user, dto.Password, false) is { Succeeded: true })
        return Results.Ok(new AuthResponseDto { Success = false, Error = "Identifiants incorrects." });

    var role  = (await um.GetRolesAsync(user)).FirstOrDefault() ?? "User";
    var token = ts.GenerateToken(user, role);

    return Results.Ok(new AuthResponseDto
    {
        Success = true, Token = token,
        Email = user.Email, FullName = user.FullName, Role = role
    });
});

app.MapPost("/api/auth/register", async (
    RegisterRequestDto dto,
    UserManager<ApplicationUser> um) =>
{
    var user = new ApplicationUser
    {
        UserName = dto.Email, Email = dto.Email,
        FullName = dto.FullName, EmailConfirmed = true
    };
    var result = await um.CreateAsync(user, dto.Password);
    if (!result.Succeeded)
        return Results.BadRequest(new AuthResponseDto
        {
            Success = false,
            Error = string.Join(", ", result.Errors.Select(e => e.Description))
        });

    await um.AddToRoleAsync(user, "User");
    return Results.Ok(new AuthResponseDto
        { Success = true, Email = user.Email, FullName = user.FullName, Role = "User" });
});

app.MapGet("/api/auth/users", async (UserManager<ApplicationUser> um) =>
{
    var result = new List<UserDto>();
    foreach (var u in um.Users.ToList())
    {
        var role = (await um.GetRolesAsync(u)).FirstOrDefault() ?? "User";
        result.Add(new UserDto(u.Id, u.Email!, u.FullName, role));
    }
    return Results.Ok(result);
});

app.MapDelete("/api/auth/users/{id}", async (string id, UserManager<ApplicationUser> um) =>
{
    var user = await um.FindByIdAsync(id);
    return user is null ? Results.NotFound() : Results.Ok(await um.DeleteAsync(user));
});

app.Run();
```

---

## COLIS.API — COMPLETE Program.cs

```csharp
using Colis.API.Data;
using Colis.API.Repositories;
using Colis.API.Repositories.Interfaces;
using Colis.API.Search;
using Colis.API.Services;
using Colis.API.Services.Interfaces;
using Colis.API.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.DTOs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ColisDbContext>(o => o.UseSqlite("Data Source=colis.db"));

// OCP: register all search strategies — add new ones without changing engine
builder.Services.AddScoped<ISearchStrategy<Colis.API.Models.Colis>, LibelleSearchStrategy>();
builder.Services.AddScoped<ISearchStrategy<Colis.API.Models.Colis>, MontantRangeStrategy>();
builder.Services.AddScoped<ISearchStrategy<Colis.API.Models.Colis>, PoidsMaxStrategy>();
builder.Services.AddScoped<ISearchStrategy<Colis.API.Models.Colis>, DateLivraisonStrategy>();
builder.Services.AddScoped<ColisSearchEngine>();

// Repository + UoW (DIP: bind abstractions to implementations)
builder.Services.AddScoped<IColisRepository, ColisRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ISP: bind admin and read service to same implementation
builder.Services.AddScoped<IColisAdminService, ColisService>();
builder.Services.AddScoped<IColisReadService>(sp => sp.GetRequiredService<IColisAdminService>());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ColisDbContext>();
    await db.Database.MigrateAsync();
    ColisSeeder.Seed(db);
}

app.UseSwagger();
app.UseSwaggerUI();

// Endpoints inject IColisAdminService (DIP — not ColisService directly)
app.MapGet("/api/colis", async (
    IColisReadService svc,
    string? libelle, decimal? minMontant, decimal? maxMontant,
    double? maxPoids, DateTime? dateLivraison,
    int page = 1, int pageSize = 10) =>
{
    var p = new ColisSearchParams(libelle, minMontant, maxMontant, maxPoids, dateLivraison);
    return Results.Ok(await svc.SearchAsync(p, page, pageSize));
});

app.MapGet("/api/colis/{id:int}", async (int id, IColisReadService svc) =>
    await svc.GetByIdAsync(id) is { } dto ? Results.Ok(dto) : Results.NotFound());

app.MapGet("/api/colis/stats", async (IColisAdminService svc) =>
    Results.Ok(await svc.GetStatsAsync()));

app.MapPost("/api/colis", async (CreateColisDto dto, IColisAdminService svc) =>
{
    var created = await svc.CreateAsync(dto);
    return Results.Created($"/api/colis/{created.Id}", created);
});

app.MapPut("/api/colis/{id:int}", async (int id, CreateColisDto dto, IColisAdminService svc) =>
{
    await svc.UpdateAsync(id, dto);
    return Results.NoContent();
});

app.MapDelete("/api/colis/{id:int}", async (int id, IColisAdminService svc) =>
{
    await svc.DeleteAsync(id);
    return Results.NoContent();
});

app.Run();
```

---

## LIVREUR.API — COMPLETE Program.cs

```csharp
using Livreur.API.Data;
using Livreur.API.Factories;
using Livreur.API.Repositories;
using Livreur.API.Repositories.Interfaces;
using Livreur.API.Services;
using Livreur.API.Services.Interfaces;
using Livreur.API.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Shared.Contracts.DTOs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<LivreurDbContext>(o => o.UseSqlite("Data Source=livreur.db"));

// DIP: bind abstractions
builder.Services.AddScoped<ILivreurRepository, LivreurRepository>();
builder.Services.AddScoped<IVehiculeRepository, VehiculeRepository>();
builder.Services.AddScoped<ILivreurUnitOfWork, LivreurUnitOfWork>();

// ISP: split admin vs read service
builder.Services.AddScoped<ILivreurAdminService, LivreurService>();
builder.Services.AddScoped<ILivreurReadService>(sp => sp.GetRequiredService<ILivreurAdminService>());

// Factory — OCP + ISP + SRP
builder.Services.AddSingleton<IVehiculeFactory, VehiculeFactory>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<LivreurDbContext>();
    await db.Database.MigrateAsync();
    LivreurSeeder.Seed(db);
}

app.UseSwagger();
app.UseSwaggerUI();

// Livreur endpoints
app.MapGet("/api/livreurs", async (ILivreurReadService svc) =>
    Results.Ok(await svc.GetAllAsync()));

app.MapGet("/api/livreurs/{id:int}", async (int id, ILivreurReadService svc) =>
    await svc.GetByIdAsync(id) is { } dto ? Results.Ok(dto) : Results.NotFound());

app.MapGet("/api/livreurs/stats", async (ILivreurAdminService svc) =>
    Results.Ok(await svc.GetStatsAsync()));

app.MapPost("/api/livreurs", async (CreateLivreurDto dto, ILivreurAdminService svc) =>
{
    var created = await svc.CreateAsync(dto);
    return Results.Created($"/api/livreurs/{created.Id}", created);
});

app.MapPut("/api/livreurs/{id:int}", async (int id, CreateLivreurDto dto, ILivreurAdminService svc) =>
{
    await svc.UpdateAsync(id, dto);
    return Results.NoContent();
});

app.MapDelete("/api/livreurs/{id:int}", async (int id, ILivreurAdminService svc) =>
{
    await svc.DeleteAsync(id);
    return Results.NoContent();
});

// Vehicule endpoints — Factory pattern used here
app.MapGet("/api/vehicules", async (ILivreurUnitOfWork uow) =>
    Results.Ok((await uow.Vehicules.GetAllAsync()).Select(v => v.ToDto())));

app.MapPost("/api/vehicules", async (
    CreateVehiculeDto dto,
    IVehiculeFactory factory,      // ISP: IVehiculeCreator + IVehiculeValidator
    ILivreurUnitOfWork uow) =>
{
    // Validate first (IVehiculeValidator)
    var validation = factory.Validate(dto);
    if (!validation.IsValid)
        return Results.BadRequest(new { Errors = validation.Errors });

    // Create via factory (IVehiculeCreator) — LSP: result is always usable as Vehicule
    var vehicule = factory.Create(dto);
    await uow.Vehicules.AddAsync(vehicule);
    await uow.SaveAsync();
    return Results.Created($"/api/vehicules/{vehicule.Id}", vehicule.ToDto());
});

app.MapDelete("/api/vehicules/{id:int}", async (int id, ILivreurUnitOfWork uow) =>
{
    var v = await uow.Vehicules.GetByIdAsync(id);
    if (v is null) return Results.NotFound();
    uow.Vehicules.Delete(v);
    await uow.SaveAsync();
    return Results.NoContent();
});

app.Run();
```

---

## GATEWAY — COMPLETE

### `Gateway/Program.cs`
```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        var jwtConfig = builder.Configuration.GetSection("Jwt");
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = jwtConfig["Issuer"],
            ValidAudience            = jwtConfig["Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(
                                           Encoding.UTF8.GetBytes(jwtConfig["Key"]!))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.MapReverseProxy();
app.Run();
```

### `Gateway/appsettings.json`
```json
{
  "Urls": "https://localhost:5000",
  "Jwt": {
    "Key": "GestionLivraisonsSecretKey2024XYZ!@#",
    "Issuer": "GestionLivraisons",
    "Audience": "GestionLivraisonsClients"
  },
  "ReverseProxy": {
    "Routes": {
      "auth-route":      { "ClusterId": "identity-cluster", "Match": { "Path": "/api/auth/{**rest}" } },
      "colis-route":     { "ClusterId": "colis-cluster",    "Match": { "Path": "/api/colis/{**rest}" },     "AuthorizationPolicy": "default" },
      "livreur-route":   { "ClusterId": "livreur-cluster",  "Match": { "Path": "/api/livreurs/{**rest}" },  "AuthorizationPolicy": "default" },
      "vehicule-route":  { "ClusterId": "livreur-cluster",  "Match": { "Path": "/api/vehicules/{**rest}" }, "AuthorizationPolicy": "default" },
      "client-route":    { "ClusterId": "client-cluster",   "Match": { "Path": "/api/clients/{**rest}" },   "AuthorizationPolicy": "default" },
      "dashboard-route": { "ClusterId": "dashboard-cluster","Match": { "Path": "/api/dashboard/{**rest}" }, "AuthorizationPolicy": "default" }
    },
    "Clusters": {
      "identity-cluster":  { "Destinations": { "d1": { "Address": "https://localhost:5001/" } } },
      "colis-cluster":     { "Destinations": { "d1": { "Address": "https://localhost:5002/" } } },
      "livreur-cluster":   { "Destinations": { "d1": { "Address": "https://localhost:5003/" } } },
      "client-cluster":    { "Destinations": { "d1": { "Address": "https://localhost:5004/" } } },
      "dashboard-cluster": { "Destinations": { "d1": { "Address": "https://localhost:5005/" } } }
    }
  }
}
```

---

## GestionLivraisons.Web — COMPLETE Program.cs

```csharp
using GestionLivraisons.Web.HttpClients;
using GestionLivraisons.Web.HttpClients.Interfaces;
using GestionLivraisons.Web.Infrastructure;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(o =>
{
    o.IdleTimeout    = TimeSpan.FromHours(8);
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();

// Cookie auth for MVC [Authorize] — populated from JWT claims on login
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath        = "/Account/Login";
        o.AccessDeniedPath = "/Account/AccessDenied";
        o.ExpireTimeSpan   = TimeSpan.FromHours(8);
    });

builder.Services.AddAuthorization();

// SRP: AuthTokenHandler only injects Bearer token — nothing else
builder.Services.AddTransient<AuthTokenHandler>();

var gatewayUrl = new Uri(builder.Configuration["Gateway:BaseUrl"]!);

// DIP: Controllers depend on IXxxApiClient interfaces, not concrete classes
// All typed clients route through Gateway + AuthTokenHandler (DIP layering)
builder.Services
    .AddHttpClient<IAuthApiClient, AuthApiClient>(c => c.BaseAddress = gatewayUrl)
    .AddHttpMessageHandler<AuthTokenHandler>();

builder.Services
    .AddHttpClient<IColisApiClient, ColisApiClient>(c => c.BaseAddress = gatewayUrl)
    .AddHttpMessageHandler<AuthTokenHandler>();

builder.Services
    .AddHttpClient<ILivreurApiClient, LivreurApiClient>(c => c.BaseAddress = gatewayUrl)
    .AddHttpMessageHandler<AuthTokenHandler>();

builder.Services
    .AddHttpClient<IClientApiClient, ClientApiClient>(c => c.BaseAddress = gatewayUrl)
    .AddHttpMessageHandler<AuthTokenHandler>();

builder.Services
    .AddHttpClient<IDashboardApiClient, DashboardApiClient>(c => c.BaseAddress = gatewayUrl)
    .AddHttpMessageHandler<AuthTokenHandler>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute("default", "{controller=Account}/{action=Login}/{id?}");
app.Run();
```

---

## MVC CONTROLLERS — IMPLEMENT FULLY WITH DIP

### `Controllers/AccountController.cs`
```csharp
// DIP: depends on IAuthApiClient, not AuthApiClient
public class AccountController(IAuthApiClient authClient) : Controller
{
    [HttpGet] public IActionResult Login() => View(new LoginVM());
    [HttpGet] public IActionResult Register() => View(new RegisterVM());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVM model)
    {
        if (!ModelState.IsValid) return View(model);

        var response = await authClient.LoginAsync(
            new LoginRequestDto(model.Email, model.Password));

        if (response?.Success != true)
        {
            ModelState.AddModelError("", response?.Error ?? "Échec de la connexion.");
            return View(model);
        }

        // Store JWT for outbound API calls (AuthTokenHandler reads this)
        HttpContext.Session.SetString("jwt_token", response.Token!);

        // Create cookie identity for MVC [Authorize]
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name,  response.FullName!),
            new(ClaimTypes.Email, response.Email!),
            new(ClaimTypes.Role,  response.Role!),
        };
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity(claims,
                CookieAuthenticationDefaults.AuthenticationScheme)));

        return response.Role == "Admin"
            ? RedirectToAction("Index", "Dashboard")
            : RedirectToAction("Index", "Colis");
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterVM model)
    {
        if (!ModelState.IsValid) return View(model);
        var response = await authClient.RegisterAsync(
            new RegisterRequestDto(model.Email, model.Password, model.FullName));
        if (response?.Success != true)
        {
            ModelState.AddModelError("", response?.Error ?? "Erreur lors de l'inscription.");
            return View(model);
        }
        TempData["Success"] = "Compte créé. Connectez-vous.";
        return RedirectToAction(nameof(Login));
    }

    public async Task<IActionResult> Logout()
    {
        HttpContext.Session.Remove("jwt_token");
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }
}
```

### `Controllers/AdminController.cs`
```csharp
// DIP: depends on interfaces, not concrete HttpClient classes
// SRP: only HTTP orchestration — no business logic, no data mapping beyond ViewModels
[Authorize(Roles = "Admin")]
public class AdminController(
    ILivreurApiClient livreurClient,
    IClientApiClient  clientClient,
    IColisApiClient   colisClient,
    IAuthApiClient    authClient) : Controller
{
    // Livreurs
    public async Task<IActionResult> Livreurs()
        => View(await livreurClient.GetAllAsync() ?? []);

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateLivreur(CreateLivreurDto dto)
    {
        if (!ModelState.IsValid) { TempData["Error"] = "Données invalides."; return RedirectToAction(nameof(Livreurs)); }
        var r = await livreurClient.CreateAsync(dto);
        TempData[r.IsSuccessStatusCode ? "Success" : "Error"] =
            r.IsSuccessStatusCode ? "Livreur créé." : "Erreur création livreur.";
        return RedirectToAction(nameof(Livreurs));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditLivreur(int id, CreateLivreurDto dto)
    {
        if (!ModelState.IsValid) { TempData["Error"] = "Données invalides."; return RedirectToAction(nameof(Livreurs)); }
        var r = await livreurClient.UpdateAsync(id, dto);
        TempData[r.IsSuccessStatusCode ? "Success" : "Error"] =
            r.IsSuccessStatusCode ? "Livreur modifié." : "Erreur modification.";
        return RedirectToAction(nameof(Livreurs));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteLivreur(int id)
    {
        await livreurClient.DeleteAsync(id);
        TempData["Success"] = "Livreur supprimé.";
        return RedirectToAction(nameof(Livreurs));
    }

    // Vehicules — uses VehiculeFactory server-side via Livreur.API
    public async Task<IActionResult> Vehicules()
        => View(await livreurClient.GetVehiculesAsync() ?? []);

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateVehicule(CreateVehiculeDto dto)
    {
        // Factory validation happens inside Livreur.API — API returns 400 with errors
        var r = await livreurClient.CreateVehiculeAsync(dto);
        if (!r.IsSuccessStatusCode)
        {
            TempData["Error"] = "Validation échouée: " + await r.Content.ReadAsStringAsync();
            return RedirectToAction(nameof(Vehicules));
        }
        TempData["Success"] = "Véhicule créé.";
        return RedirectToAction(nameof(Vehicules));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteVehicule(int id)
    {
        await livreurClient.DeleteVehiculeAsync(id);
        TempData["Success"] = "Véhicule supprimé.";
        return RedirectToAction(nameof(Vehicules));
    }

    // Clients — same CRUD pattern as Livreurs
    public async Task<IActionResult> Clients()
        => View(await clientClient.GetAllAsync() ?? []);

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateClient(CreateClientDto dto) { /* same pattern */ return RedirectToAction(nameof(Clients)); }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditClient(int id, CreateClientDto dto) { /* same pattern */ return RedirectToAction(nameof(Clients)); }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteClient(int id) { await clientClient.DeleteAsync(id); TempData["Success"] = "Client supprimé."; return RedirectToAction(nameof(Clients)); }

    // Colis admin — same pattern
    public async Task<IActionResult> Colis()
        => View(await colisClient.GetAllAsync() ?? new PagedResult<ColisDto>());

    // Comptes
    public async Task<IActionResult> Comptes()
        => View(await authClient.GetUsersAsync() ?? []);

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCompte(string id)
    {
        await authClient.DeleteUserAsync(id);
        TempData["Success"] = "Compte supprimé.";
        return RedirectToAction(nameof(Comptes));
    }
}
```

### `Controllers/ColisController.cs`
```csharp
[Authorize]
public class ColisController(IColisApiClient colisClient) : Controller
{
    public async Task<IActionResult> Index(
        string? libelle, decimal? minMontant, decimal? maxMontant,
        double? maxPoids, DateTime? dateLivraison, int page = 1)
    {
        var search = new ColisSearchParams(libelle, minMontant, maxMontant, maxPoids, dateLivraison);
        var result = await colisClient.SearchAsync(search, page, 10);
        ViewBag.Search = search;
        return View(result ?? new PagedResult<ColisDto>());
    }

    public async Task<IActionResult> Details(int id)
    {
        var colis = await colisClient.GetByIdAsync(id);
        return colis is null ? NotFound() : View(colis);
    }
}
```

### `Controllers/DashboardController.cs`
```csharp
// SRP: only fetches and passes DashboardDto to View — zero data processing
[Authorize(Roles = "Admin")]
public class DashboardController(IDashboardApiClient dashboardClient) : Controller
{
    public async Task<IActionResult> Index()
        => View(await dashboardClient.GetDashboardAsync() ?? new DashboardDto());
}
```

---

## RAZOR VIEWS — IMPLEMENT FULLY

### `Views/Shared/_Layout.cshtml` — Bootstrap 5.3 + Font Awesome 6.5 + Chart.js 4.4
Include: sidebar (Admin only), navbar with user name + role badge + logout, TempData alert banners (auto-dismiss 4s via JS), `@RenderBody()`, `@await RenderSectionAsync("Scripts", required: false)`.

### `Views/Dashboard/Index.cshtml`
```razor
@model DashboardDto
@{ ViewData["Title"] = "Tableau de Bord"; }

<div class="d-flex justify-content-between align-items-center mb-4">
    <h4 class="fw-bold mb-0"><i class="fas fa-tachometer-alt me-2 text-primary"></i>Tableau de Bord</h4>
    <small class="text-muted">Données en temps réel via microservices</small>
</div>

<!-- KPI Cards -->
<div class="row g-3 mb-4">
    <div class="col-xl-3 col-md-6">
        <div class="card border-0 shadow-sm h-100">
            <div class="card-body d-flex align-items-center gap-3">
                <div class="bg-primary bg-opacity-10 rounded-3 p-3">
                    <i class="fas fa-box fa-lg text-primary"></i>
                </div>
                <div>
                    <div class="text-muted small">Total Colis</div>
                    <div class="fs-4 fw-bold">@Model.TotalColis</div>
                </div>
            </div>
        </div>
    </div>
    <div class="col-xl-3 col-md-6">
        <div class="card border-0 shadow-sm h-100">
            <div class="card-body d-flex align-items-center gap-3">
                <div class="bg-success bg-opacity-10 rounded-3 p-3">
                    <i class="fas fa-coins fa-lg text-success"></i>
                </div>
                <div>
                    <div class="text-muted small">Chiffre d'Affaires</div>
                    <div class="fs-4 fw-bold">@Model.TotalMontant.ToString("N2") TND</div>
                </div>
            </div>
        </div>
    </div>
    <div class="col-xl-3 col-md-6">
        <div class="card border-0 shadow-sm h-100">
            <div class="card-body d-flex align-items-center gap-3">
                <div class="bg-warning bg-opacity-10 rounded-3 p-3">
                    <i class="fas fa-truck fa-lg text-warning"></i>
                </div>
                <div>
                    <div class="text-muted small">Livreurs</div>
                    <div class="fs-4 fw-bold">@Model.TotalLivreurs</div>
                </div>
            </div>
        </div>
    </div>
    <div class="col-xl-3 col-md-6">
        <div class="card border-0 shadow-sm h-100">
            <div class="card-body d-flex align-items-center gap-3">
                <div class="bg-info bg-opacity-10 rounded-3 p-3">
                    <i class="fas fa-users fa-lg text-info"></i>
                </div>
                <div>
                    <div class="text-muted small">Clients</div>
                    <div class="fs-4 fw-bold">@Model.TotalClients</div>
                </div>
            </div>
        </div>
    </div>
</div>

<!-- Charts Row -->
<div class="row g-3 mb-4">
    <div class="col-lg-6">
        <div class="card border-0 shadow-sm">
            <div class="card-header bg-transparent fw-semibold border-0 pt-3">
                <i class="fas fa-chart-bar me-2 text-primary"></i>Colis par Livreur
            </div>
            <div class="card-body"><canvas id="barChart" height="220"></canvas></div>
        </div>
    </div>
    <div class="col-lg-6">
        <div class="card border-0 shadow-sm">
            <div class="card-header bg-transparent fw-semibold border-0 pt-3">
                <i class="fas fa-chart-line me-2 text-success"></i>Montant par Mois
            </div>
            <div class="card-body"><canvas id="lineChart" height="220"></canvas></div>
        </div>
    </div>
</div>

<div class="row g-3">
    <div class="col-lg-4">
        <div class="card border-0 shadow-sm">
            <div class="card-header bg-transparent fw-semibold border-0 pt-3">
                <i class="fas fa-chart-pie me-2 text-warning"></i>Colis par Ville
            </div>
            <div class="card-body"><canvas id="pieChart" height="220"></canvas></div>
        </div>
    </div>
    <div class="col-lg-8">
        <div class="card border-0 shadow-sm">
            <div class="card-header bg-transparent fw-semibold border-0 pt-3">
                <i class="fas fa-history me-2 text-info"></i>Derniers Colis
            </div>
            <div class="card-body p-0">
                <table class="table table-hover mb-0">
                    <thead class="table-light"><tr><th>Libellé</th><th>Montant</th><th>Date</th><th>Livreur</th></tr></thead>
                    <tbody>
                        @foreach (var c in Model.ColisRecents)
                        {
                            <tr>
                                <td>@c.Libelle</td>
                                <td>@c.Montant.ToString("N2") TND</td>
                                <td>@c.DateLivraison.ToString("dd/MM/yyyy")</td>
                                <td><span class="badge bg-secondary">@(c.LivreurNom ?? $"#{c.LivreurId}")</span></td>
                            </tr>
                        }
                    </tbody>
                </table>
            </div>
        </div>
    </div>
</div>

@section Scripts {
<script>
    const palette = ['#0d6efd','#198754','#ffc107','#dc3545','#6f42c1','#0dcaf0','#fd7e14'];

    new Chart(document.getElementById('barChart'), {
        type: 'bar',
        data: {
            labels: @Html.Raw(Json.Serialize(Model.ColisParLivreur.Keys)),
            datasets: [{ label: 'Colis', data: @Html.Raw(Json.Serialize(Model.ColisParLivreur.Values)),
                backgroundColor: 'rgba(13,110,253,0.75)', borderRadius: 6 }]
        },
        options: { responsive: true, plugins: { legend: { display: false } }, scales: { y: { beginAtZero: true, ticks: { stepSize: 1 } } } }
    });

    new Chart(document.getElementById('lineChart'), {
        type: 'line',
        data: {
            labels: @Html.Raw(Json.Serialize(Model.MontantParMois.Keys)),
            datasets: [{ label: 'Montant (TND)', data: @Html.Raw(Json.Serialize(Model.MontantParMois.Values)),
                borderColor: '#198754', backgroundColor: 'rgba(25,135,84,0.1)', fill: true,
                tension: 0.4, pointBackgroundColor: '#198754' }]
        },
        options: { responsive: true, scales: { y: { beginAtZero: true } } }
    });

    new Chart(document.getElementById('pieChart'), {
        type: 'doughnut',
        data: {
            labels: @Html.Raw(Json.Serialize(Model.ColisParVille.Keys)),
            datasets: [{ data: @Html.Raw(Json.Serialize(Model.ColisParVille.Values)), backgroundColor: palette }]
        },
        options: { responsive: true, plugins: { legend: { position: 'bottom' } } }
    });
</script>
}
```

### `Views/Admin/Vehicules.cshtml`
Include JavaScript that shows/hides Camion-specific fields (`Capacite`, `NbrEssieux`) or Voiture-specific fields (`NbrPlaces`) based on the `Type` dropdown selection. LSP is demonstrated in the table by calling `vehicule.GetDescription()` polymorphically.

### `Views/Colis/Index.cshtml`
Search form (Bootstrap card) with fields: `libelle`, `minMontant`, `maxMontant`, `maxPoids`, `dateLivraison`. Results in Bootstrap table with pagination (`asp-route-page`). Show "Page X de Y — Z résultats".

---

## NUGET PACKAGES PER PROJECT

```
Identity.API:        Identity.EF(8.*), EF.Sqlite(8.*), EF.Design(8.*), JwtBearer(8.*), Swashbuckle(6.*)
Colis.API:           EF.Sqlite(8.*), EF.Design(8.*), Swashbuckle(6.*)
Livreur.API:         EF.Sqlite(8.*), EF.Design(8.*), Swashbuckle(6.*)
Client.API:          EF.Sqlite(8.*), EF.Design(8.*), Swashbuckle(6.*)
Dashboard.API:       Swashbuckle(6.*)
Gateway:             Yarp.ReverseProxy(2.*), JwtBearer(8.*)
GestionLivraisons.Web: Authentication.Cookies(8.*)
Shared.Contracts:    (none — pure class library)
```

---

## PRINCIPLE-TO-FILE TRACEABILITY MAP

This table must be verifiable by reading the code — every principle appears in a named file:

| Principle / Pattern | File(s) where enforced |
|---|---|
| SRP | `TokenService.cs`, `ColisRepository.cs`, `VehiculeFactory.cs`, `AuthTokenHandler.cs`, `DashboardController.cs`, `ColisSeeder.cs` |
| OCP | `ISearchStrategy.cs` + `LibelleSearchStrategy.cs` + `MontantRangeStrategy.cs` + `ColisSearchEngine.cs`, `VehiculeFactory.cs` switch expression |
| LSP | `Vehicule.cs` (abstract), `Camion.cs`, `Voiture.cs` — `GetDescription()`, `PeutTransporter()`, `GetCapacitePassagers()` |
| ISP | `IReadRepository.cs`, `IWriteRepository.cs`, `IRepository.cs`, `IColisReadService.cs`, `IColisAdminService.cs`, `IVehiculeCreator.cs`, `IVehiculeValidator.cs` |
| DIP | All Controllers depend on `IXxxApiClient`, all Services depend on `IXxxRepository`, Dashboard.API depends on `IColisApiClient`/`ILivreurApiClient`/`IClientApiClient` |
| IDisposable | `UnitOfWork.cs` (Dispose + DisposeAsync + finalizer), `LivreurUnitOfWork.cs` |
| Factory | `VehiculeFactory.cs`, `IVehiculeFactory.cs`, `IVehiculeCreator.cs`, `IVehiculeValidator.cs`, `ValidationResult.cs` |
| Repository | `Repository.cs` (generic), `ColisRepository.cs`, `LivreurRepository.cs`, `VehiculeRepository.cs` |
| Unit of Work | `IUnitOfWork.cs`, `UnitOfWork.cs`, `ILivreurUnitOfWork.cs`, `LivreurUnitOfWork.cs` |

---

## FINAL OUTPUT CONTRACT

Generate files in this exact order:
1. `GestionLivraisons.sln`
2. `Shared.Contracts/` — all DTOs
3. `Identity.API/` — Models → Data → Services → Program.cs → appsettings.json
4. `Colis.API/` — Models → Data → Repositories/Interfaces → Repository.cs → ColisRepository.cs → Search/ → Services/Interfaces → ColisService.cs → UnitOfWork/ → Program.cs
5. `Livreur.API/` — Models (Vehicule abstract + Camion + Voiture with LSP methods) → Data → Factories/ → Repositories/ → UnitOfWork/ → Services/ → Program.cs
6. `Client.API/` — same layered structure as Colis.API (simplified)
7. `Dashboard.API/` — Clients/Interfaces → Clients/ → Program.cs
8. `Gateway/` — Program.cs → appsettings.json
9. `GestionLivraisons.Web/` — Infrastructure → HttpClients/Interfaces → HttpClients → Models/ViewModels → Controllers → Views → Program.cs → appsettings.json

The solution must:
- `dotnet build` → **zero errors**
- Every SOLID principle visible in at least 2 files each
- `IDisposable` implemented with the **full canonical 3-part pattern** (Dispose(bool), ~Finalizer, GC.SuppressFinalize) in both UnitOfWork classes
- Factory validates **before** creating — never `Create()` without `Validate()` first
- Repository never contains business logic — service layer never writes raw EF queries
- MVC controllers have **zero** `new`, **zero** `DbContext`, **zero** `HttpClient` instantiation — all injected via interfaces
