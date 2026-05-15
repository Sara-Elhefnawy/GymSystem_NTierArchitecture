# My Architecture Decision: Why I Chose N-Tier

When starting the GymSystem project, I researched several architecture patterns to find the best fit.

- **Clean Architecture** - Domain-centric with strict dependency rules
- **Vertical Slice Architecture** - Feature-based organization
- **N-Tier Architecture** - Traditional layer-based separation

Each had compelling advantages, but choosing the wrong one could either overcomplicate development or create technical debt. As a first-time MVC developer working on a side project with limited time, I needed to balance learning, productivity, and future scalability.

## Why I Chose N-Tier

### 1. Right Complexity Level

- **Not too simple**: Proper separation of concerns
- **Not too complex**: Avoids Clean's overhead or Vertical Slice's CQRS requirements

### 2. Perfect for GymSystem

My gym app is primarily CRUD:

- Register/update members
- Create/book classes
- Log workouts
- View schedules

N-Tier handles these efficiently without overengineering.

### 3. Future-Proof

- Can refactor to Clean Architecture if domain logic becomes complex
- Can adopt Vertical Slices if features multiply
- Not locked into the architecture permanently
- Provides solid foundation for growth

### 4. Time Constraints

- Side project with limited time
- Need to ship features quickly
- Can't afford to get stuck in architectural complexity

---
# Software Architecture Styles

## Clean Architecture

Clean Architecture organizes code into concentric circles with the domain at the center. Dependencies point inward.

### Structure

```
/src/
  /Domain/
    Entities.cs
    ValueObjects.cs
    Interfaces/
  /Application/
    UseCases.cs
    DTOs.cs
    Interfaces/
  /Infrastructure/
    DataAccess.cs
    FileSystems.cs
    ExternalServices.cs
  /Presentation/
    Controllers.cs
    Views.cs
    ApiEndpoints.cs
```

- **Useful for**: Enterprise apps, long-term projects, domain-driven design
- **Overengineering when**: Simple CRUD app (<10 entities)
- **Pros**: Business logic is central and protected, highly testable, independent of frameworks, easy to swap out infrastructure, clear dependency rules
- **Cons**: Steep learning curve, overkill for simple apps

## Vertical Slice Architecture

Vertical slices organize code by feature rather than by technical concern. Each feature contains everything it needs (controller, service, data access).

### Structure

```
/Features/
  /CreateMember/
    CreateMemberCommand.cs
    CreateMemberHandler.cs
    CreateMemberController.cs
    CreateMemberViewModel.cs
```

- **Useful for**: Medium-sized apps, feature-rich systems, teams wanting isolation
- **Overengineering when**: Trivial app with 2-3 features
- **Pros**: Features are isolated, changes don't cascade, parallel development possible, easy to understand each feature independently
- **Cons**: Code duplication between features, inconsistent patterns across slices, CQRS overhead often required

## N-Tier Architecture

Traditional layer-based architecture with logical separation:

### Structure

```
/src/
  /Presentation/
    Controllers.cs
    Views.cs
  /Business/
    Services.cs
  /DataAccess/
    Repositories.cs
```

- **Useful for**: Simple CRUD apps, internal tools, rapid development
- **Overengineering when**: App has complex business rules that need domain modeling
- **Pros**: Simple to understand, clear separation of concerns, easy to learn, works with most frameworks, fast development
- **Cons**: Business logic can leak into presentation, changes often ripple through all layers, hard to test in isolation, becomes spaghetti at scale

## Differences Between Architectures

| Aspect | N-Tier | Clean Architecture | Vertical Slice |
|--------|--------|-------------------|----------------|
| Organization | By technical layer | By concentric circles | By feature |
| Dependency Direction | Top-down only | Inward (toward domain) | Feature-centric |
| Coupling | Layer coupling | Loose (via interfaces) | Feature-isolated |
| Testing | Layer tests | Domain-first | Feature-first |
| Change Impact | Cross-layer changes | Isolated to circles | Isolated to slices |
| Best For | Simple CRUD apps | Complex business logic | Medium-sized apps with many features |

## Monolithic vs Microservices vs Modular Monolithic

### Monolithic Architecture

```
/MyApp/
  /Controllers/
  /Services/
  /Repositories/
  /Models/
```

- Single deployable unit
- All features in one codebase
- Simple to develop and deploy initially

### Microservices Architecture

```
/OrderService/
  /Controllers/
  /Repositories/
  /Models/
/PaymentService/
  /Controllers/
  /Repositories/
  /Models/
/UserService/
  /Controllers/
  /Repositories/
  /Models/
```

- Many small, independent services
- Each has its own database
- Complex but scalable

- **Useful for**: Large teams (>20 developers), systems requiring independent scaling
- **Overengineering when**: Team <10 people, unsure about service boundaries
- **Pros**: Independent scaling, team autonomy, technology diversity per service, independent deployments
- **Cons**: Distributed system complexity (network, latency, failures), data consistency challenges, expensive operations, debugging nightmare, team requires high DevOps maturity

### Modular Monolithic

```
/MyApp/
  /Modules/
    /Orders/
      OrdersController.cs
      OrdersService.cs
      OrdersRepository.cs
    /Users/
      UsersController.cs
      UsersService.cs
      UsersRepository.cs
  /Shared/
    CommonInterfaces.cs
    Utilities.cs
```

- Single deployment but with strong module boundaries
- Modules communicate through well-defined interfaces
- Can be split into microservices later

- **Useful for**: Starting new products where future microservices might be needed
- **Overengineering when**: You're absolutely certain you'll never need to split
- **Pros**: Single deployment (simpler ops), strong module boundaries, can split to microservices later, no distributed system complexity
- **Cons**: Can accidentally create tight coupling, deployment still requires whole app, modules can't scale independently, requires team discipline

## Differences: Modular Monolithic vs Microservices

| Aspect | Modular Monolithic | Microservices |
|--------|-------------------|---------------|
| Deployment | Single unit | Multiple independent units |
| Communication | In-method calls | HTTP/gRPC/message queues |
| Data | Single database (logically separated) | Database per service |
| Complexity | Moderate | High |
| Team Structure | Single team | Multiple teams |
| Scalability | Scale everything | Scale individual services |

---

# Repository Pattern & EF Core

## Research Repository Pattern

The Repository Pattern mediates between the domain and data mapping layers, acting like an in-memory collection of domain objects. It centralizes data access logic and provides a clean separation of concerns.

### Classic Repository Pattern Structure

```
/Repositories/
  /Interfaces/
    IMemberRepository.cs
    IClassRepository.cs
  /Implementations/
    MemberRepository.cs
    ClassRepository.cs
```

## Why Some Teams Avoid Repository Pattern with EF Core

Teams avoid the Repository Pattern with EF Core for these reasons:

- **EF Core is already a repository** - DbSet provides the same methods (Find, Add, Remove, Where)
- **Added complexity without benefit** - Writing wrapper code that just calls EF Core is redundant
- **Query limitations** - Generic repositories struggle with efficient filtering and includes (`.Include(x => x.Classes)`)
- **Leaky abstractions** - Can't fully hide EF Core features without crippling functionality
- **Maintenance burden** - Every new entity needs a new repository interface and implementation

## How DbContext Already Behaves Like Repository and Unit of Work

### Repository Pattern Features in DbContext

| Repository Feature | How DbContext Provides It |
|-------------------|---------------------------|
| `GetById()` | `DbContext.Find<T>(id)` |
| `GetAll()` | `DbContext.Set<T>()` |
| `Add()` | `DbContext.Add()` |
| `Update()` | `DbContext.Update()` |
| `Delete()` | `DbContext.Remove()` |
| Query filtering | `Where()`, `FirstOrDefault()`, `Include()` |

### Unit of Work Pattern Features in DbContext

| Unit of Work Feature | How DbContext Provides It |
|----------------------|---------------------------|
| Track multiple changes | ChangeTracker tracks all entities |
| Commit as a transaction | `SaveChangesAsync()` wraps all changes |
| Rollback | Discard changes or do not call SaveChanges |
| Atomic operations | All changes succeed or fail together |

## How to Use an Interface for DbContext Without Creating Repositories

Instead of creating repository wrappers, simply inject `AppDbContext` (or its interface) directly into your services:

### Step 1: Define the DbContext Interface

```csharp
public interface IAppDbContext
{
    DbSet<Member> Members { get; set; }
    DbSet<Class> Classes { get; set; }
    DbSet<Booking> Bookings { get; set; }
    DbSet<WorkoutLog> WorkoutLogs { get; set; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    
    // Optional: For raw SQL queries
    Task<IEnumerable<T>> SqlQueryAsync<T>(string sql, params object[] parameters);
}
```

### Step 2: Implement the Interface on Your DbContext

```csharp
public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    public DbSet<Member> Members { get; set; }
    public DbSet<Class> Classes { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<WorkoutLog> WorkoutLogs { get; set; }
    
    public async Task<IEnumerable<T>> SqlQueryAsync<T>(string sql, params object[] parameters)
    {
        return await Set<T>().FromSqlRaw(sql, parameters).ToListAsync();
    }
}
```

### Step 3: Inject the Interface Directly Into Your Service

```csharp
// N-Tier Business Layer Service
public class MemberService
{
    private readonly IAppDbContext _context;
    
    public MemberService(IAppDbContext context)
    {
        _context = context;
    }
    
    public async Task<Member?> GetMemberAsync(int id)
    {
        // Direct DbContext usage - no repository needed
        return await _context.Members
            .Include(m => m.Bookings)
            .ThenInclude(b => b.Class)
            .FirstOrDefaultAsync(m => m.Id == id);
    }
    
    public async Task CreateMemberAsync(Member member)
    {
        await _context.Members.AddAsync(member);
        await _context.SaveChangesAsync();
    }
    
    public async Task UpdateMemberAsync(Member member)
    {
        _context.Members.Update(member);
        await _context.SaveChangesAsync();
    }
    
    public async Task DeleteMemberAsync(int id)
    {
        var member = await _context.Members.FindAsync(id);
        if (member != null)
        {
            _context.Members.Remove(member);
            await _context.SaveChangesAsync();
        }
    }
}
```

### Step 4: Register in Dependency Injection

```csharp
// Program.cs or Startup.cs
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register the interface
builder.Services.AddScoped<IAppDbContext, AppDbContext>();
```