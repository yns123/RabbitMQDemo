# Roadmap

Fazlar sırayla yapılır. Bir faz bitince burada işaretlenir ve durulur; bir sonraki faza
kendiliğinden geçilmez (bkz. CLAUDE.md → "Bitti" tanımı).

- [x] Faz 0: CLAUDE.md + skills + docs iskeleti (bu faz)
- [ ] Faz 1: Solution iskeleti — Producer.Api, Consumer.Worker, RabbitMqDemo.Contracts projeleri;
      docker-compose.yml (RabbitMQ); boş `/health` endpoint'leri
- [ ] Faz 2: Producer — `POST /api/tasks` endpoint'i, `TaskCreatedEvent`'i RabbitMQ'ya yayınlar
- [ ] Faz 3: Consumer — kuyruktan okur, `ITaskRepository` + EF Core (SQLite) ile Task'ı kaydeder,
      idempotency (`ProcessedEvents`) uygulanır
- [ ] Faz 4: Testler — repository ve event handling için xUnit testleri
- [ ] Faz 5 (opsiyonel, öğrenme ilerledikçe): dead-letter queue, retry policy, ikinci bir event
      (ör. `TaskCompletedEvent`) ile publish/consume tekrarı
