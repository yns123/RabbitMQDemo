---
name: repository-pattern
description: Consumer.Worker'da repository ekliyor veya değiştiriyorsan kullan. Yeni bir entity, yeni bir repository metodu veya EF Core/SQLite ile ilgili her değişiklikte bu kuralları takip et.
---

# Repository pattern

Bu projede repository pattern **öğrenme amaçlı, sade** şekilde uygulanır: generic `IRepository<T>`
YOK, her entity için kendi özel repository interface + implementasyonu var. Şu an tek entity
(`Task`) olduğu için tek repository (`ITaskRepository`) yeterli.

## Katmanlar (Consumer.Worker içinde)

```
Consumer.Worker/
├─ Domain/
│  └─ TaskItem.cs                 # entity (POCO, EF Core attribute'suz, fluent config ile map edilir)
├─ Data/
│  ├─ ConsumerDbContext.cs        # DbSet<TaskItem>, DbSet<ProcessedEvent>
│  └─ Configurations/             # IEntityTypeConfiguration<T> ile fluent mapping
├─ Repositories/
│  ├─ ITaskRepository.cs
│  └─ TaskRepository.cs           # ConsumerDbContext kullanır
└─ Messaging/
   └─ TaskCreatedEventHandler.cs  # ITaskRepository'yi DI ile alır, iş mantığı burada değil repository'de
```

## Kurallar

1. **Interface domain dilinde konuşur, EF Core detayı sızdırmaz.**
   `ITaskRepository.AddAsync(TaskItem task)`, `ITaskRepository.ExistsByEventIdAsync(Guid eventId)`
   gibi — `IQueryable<TaskItem>` veya `DbSet` döndürme.

2. **Repository sadece veri erişimi yapar, iş kuralı içermez.** "Event zaten işlenmiş mi" kontrolü
   repository'nin bir metodu olabilir (`ProcessedEventRepository.ExistsAsync`) ama karar verme
   (ack/nack, log) çağıran tarafta (event handler) kalır.

3. **Task kaydı + ProcessedEvent kaydı aynı transaction'da yazılır.** İkisi de başarılı olmadan
   ack gönderilmez — yarım yazılmış idempotency kaydı olmasın diye. Bunun için ya aynı
   `SaveChangesAsync` çağrısında iki entity de eklenir, ya da `DbContext.Database.BeginTransactionAsync`
   kullanılır.

4. **DbContext lifetime.** Worker'da her mesaj için yeni bir scope aç
   (`IServiceScopeFactory.CreateScope()`), `ConsumerDbContext`'i o scope'tan al. `BackgroundService`
   singleton olduğu için DbContext'i doğrudan constructor'a inject etme.

5. **Migration.** `dotnet ef migrations add <Ad> --project src/Consumer/Consumer.Worker`.
   SQLite dosyası (`tasks.db`) Consumer.Worker'ın çalışma dizininde oluşur, .gitignore'da.

## Yapma

- Genel amaçlı `IRepository<T>` / `IUnitOfWork` ekleme — tek entity için gereksiz soyutlama.
- Repository'den controller/handler'a `IQueryable` sızdırma.
- Producer.Api'ye repository ekleme — Producer DB'ye hiç dokunmuyor, sadece event yayınlıyor.
