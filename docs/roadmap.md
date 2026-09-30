# Roadmap

Fazlar sırayla yapılır. Bir faz bitince burada işaretlenir ve durulur; bir sonraki faza
kendiliğinden geçilmez (bkz. CLAUDE.md → "Bitti" tanımı).

- [x] Faz 0: CLAUDE.md + skills + docs iskeleti
- [x] Faz 1: Solution iskeleti — Producer.Api, Consumer.Worker, RabbitMqDemo.Contracts projeleri;
      docker-compose.yml (RabbitMQ); Producer.Api `/health` endpoint'i (200 doğrulandı, `dotnet build`
      uyarısız). Not: .NET 10 ile proje `.slnx` formatında (`RabbitMqDemo.slnx`).
      Not: bu ortamda Docker Desktop çalışmıyordu, `docker compose up -d` denenmedi geçmedi —
      sıradaki oturumda önce bu doğrulanmalı.
- [x] Faz 2: Producer — `POST /api/tasks` endpoint'i, `TaskCreatedEvent`'i RabbitMQ'ya yayınlar.
      Uçtan uca doğrulandı: `docker compose up -d` → RabbitMQ healthy → `POST /api/tasks` → 202
      Accepted → `rabbitmqdemo` exchange'inde publish_in=1, `task-created-queue`'da 1 mesaj
      (management API ile kontrol edildi). Validasyon hatası (title eksik) ProblemDetails
      formatında 400 döndü.
- [x] Faz 3: Consumer — kuyruktan okur, `ITaskRepository` + EF Core (SQLite) ile Task'ı kaydeder,
      idempotency (`ProcessedEvents`) uygulanır. Uçtan uca doğrulandı: Producer'dan `POST /api/tasks`
      → Consumer Task'ı `tasks.db`'ye yazdı (log + SQL doğrulandı). Aynı `EventId` ile mesaj
      RabbitMQ management API'den manuel tekrar publish edildi (`publish:2, deliver:2, ack:2`,
      `messages:0`); Consumer ikinci seferde "zaten işlenmiş" deyip DB'ye tekrar yazmadı —
      idempotency çalışıyor. `dotnet build` uyarısız/hatasız. İlk migration
      (`InitialCreate`) eklendi, `Database.MigrateAsync()` ile Consumer açılışında otomatik uygulanıyor.
- [x] Faz 4: Testler — repository ve event handling için xUnit testleri. Gerçek Sqlite provider'ına
      karşı (":memory:" bağlantısı) yazıldı, EF InMemory provider kullanılmadı — production'daki
      davranışa daha yakın olsun diye. `TaskRepositoryTests`, `ProcessedEventRepositoryTests`,
      `TaskCreatedEventHandlerTests` (yeni event kaydeder + aynı EventId ikinci kez gelirse
      Task'ı tekrar eklemez). `dotnet test` → 5/5 geçti.
- [ ] Faz 5 (opsiyonel, öğrenme ilerledikçe): dead-letter queue, retry policy, ikinci bir event
      (ör. `TaskCompletedEvent`) ile publish/consume tekrarı
