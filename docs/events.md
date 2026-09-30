# Event'ler

Kod bu dokümana uyar, tersi değil. Yeni event eklemeden önce burayı güncelle.

## Konvansiyon

- Exchange: `rabbitmqdemo` (tipi: `direct`)
- Her event bir routing key'e sahiptir: `<varlık>.<geçmiş-zaman-fiil>` (ör. `task.created`)
- Event DTO'ları `src/Shared/RabbitMqDemo.Contracts` altında, sadece veri içerir (davranış yok)
- Her event'in bir `EventId` (Guid) alanı olur → idempotency için kullanılır

## Event listesi

| Event | Routing key | Yayınlayan | Dinleyen | Queue |
|---|---|---|---|---|
| `TaskCreatedEvent` | `task.created` | Producer.Api | Consumer.Worker | `task-created-queue` |

### TaskCreatedEvent

```csharp
public record TaskCreatedEvent(
    Guid EventId,
    Guid TaskId,
    string Title,
    string? Description,
    DateTime CreatedAtUtc);
```

Yayınlandığı an: Producer.Api'de `POST /api/tasks` başarıyla tamamlandığında.

## Idempotency

Aynı mesaj RabbitMQ tarafından (network kopması, consumer'ın ack göndermeden çökmesi vb. nedenlerle)
**iki kez** teslim edilebilir. Bu yüzden consumer, mesajı işlemeden önce daha önce işlenip
işlenmediğini kontrol etmelidir.

Bu projede yöntem: `TaskCreatedEvent.EventId` işlenmiş event'lerin tutulduğu bir tabloda
(`ProcessedEvents`) aranır; zaten varsa mesaj sessizce ack'lenir ve tekrar işlenmez. Yoksa
işlenir ve `EventId` bu tabloya eklenir (aynı DB transaction'ı içinde, Task kaydıyla birlikte).
