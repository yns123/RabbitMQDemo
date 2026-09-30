# Mimari

## Akış

```
[Producer.Api]  --POST /api/tasks-->  (istemci)
      |
      | TaskCreatedEvent yayınla (Exchange: rabbitmqdemo, RoutingKey: task.created)
      v
   [RabbitMQ]
      |
      | Queue: task-created-queue
      v
[Consumer.Worker] --ITaskRepository--> [SQLite: tasks.db]
```

- Exchange tipi: `direct` (basit örnek için yeterli; fanout/topic'e ileride geçilebilir).
- Producer sadece **yayınlar**, kimin dinlediğini bilmez.
- Consumer sadece **dinler**, kimin yayınladığını bilmez.
- Aradaki tek sözleşme: `RabbitMqDemo.Contracts` içindeki event DTO'su + routing key.

## Hata ve yeniden deneme

- Consumer mesajı işlerken hata alırsa: `BasicNackAsync(requeue: true)` ile mesaj kuyruğa geri konur.
- Sürekli hata veren mesajın kuyruğu tıkamaması için ileride bir **dead-letter queue** eklenebilir
  (şimdilik basit tutuluyor, eklenmedi — gerekirse önce burada tartışılıp sonra eklenir).
- Consumer manuel ack kullanır (`autoAck: false`); mesaj DB'ye yazılıp işlem başarılı olduktan
  sonra ack'lenir. Böylece Consumer çökerse mesaj kaybolmaz, tekrar işlenir.

## Klasör yapısı

```
RabbitMqDemo/
├─ src/
│  ├─ Producer/
│  │  └─ Producer.Api/            # Controller'lar, Program.cs, RabbitMQ publisher
│  ├─ Consumer/
│  │  └─ Consumer.Worker/         # BackgroundService, RabbitMQ consumer, DbContext, ITaskRepository
│  └─ Shared/
│     └─ RabbitMqDemo.Contracts/  # Event DTO'ları (ör. TaskCreatedEvent)
├─ tests/
│  └─ Consumer.Worker.Tests/      # Repository ve event handler testleri (xUnit)
├─ docs/
│  ├─ architecture.md
│  ├─ events.md
│  └─ roadmap.md
├─ docker-compose.yml             # sadece RabbitMQ
└─ RabbitMqDemo.sln
```

## Repository pattern

Tek entity (`Task`) olduğu için generic `IRepository<T>` yerine domain'e özel `ITaskRepository`
kullanılır (bkz. `.claude/skills/repository-pattern/SKILL.md`). Amaç RabbitMQ ve repository
pattern'i net görmek; gereksiz soyutlama eklenmez.
