---
name: new-event
description: RabbitMQ üzerinden yeni bir event yayınlarken (Producer) veya bir event'i dinlerken (Consumer) kullan. "X olunca kuyruğa yaz", "Y event'ini dinle", yeni bir mesaj tipi eklerken bu adımları takip et.
---

# Yeni event ekleme

Bu projede tek exchange var: `rabbitmqdemo` (tipi `direct`). Her event bir routing key'e
sahip ve kendi queue'suna gider. Soyutlama kütüphanesi yok — `RabbitMQ.Client` doğrudan kullanılır.

## Adımlar

1. **Önce `docs/events.md`'yi güncelle.** Event tablosuna satır ekle: event adı, routing key,
   yayınlayan, dinleyen, queue adı. Event DTO'sunun alanlarını da dokümana yaz.
   Kod dokümana uyar, tersi değil.

2. **Contract'ı ekle.** `src/Shared/RabbitMqDemo.Contracts` altına `record` olarak event DTO'sunu
   ekle. Kurallar:
   - Sadece veri, davranış/metod yok.
   - Her event `Guid EventId` alanı içerir (idempotency için).
   - İsimlendirme: `<Varlık><GeçmişZamanFiil>Event`, ör. `TaskCreatedEvent`.

3. **Publisher tarafı (Producer.Api).** İlgili controller action'ında, DB işlemi başarıyla
   bittikten sonra event'i publish et. Yeni bir queue/routing key ise:
   - Exchange'i declare et (idempotent, `ExchangeDeclareAsync`, zaten varsa hata vermez).
   - Queue'yu declare et ve exchange'e routing key ile bind et.
   - Mesajı `BasicPublishAsync` ile gönder, `DeliveryMode = Persistent`, body JSON serialize.

4. **Consumer tarafı (Consumer.Worker).** `BackgroundService` içinde:
   - Aynı exchange/queue/binding'i declare et (consumer da idempotent declare yapar, producer'dan
     bağımsız hangisi önce ayağa kalkarsa kalksın çalışsın diye).
   - `autoAck: false` ile consume et.
   - Mesajı işlemeden önce `EventId`'nin `ProcessedEvents` tablosunda olup olmadığını kontrol et
     (bkz. `.claude/skills/repository-pattern/SKILL.md` ve docs/events.md → İdempotency).
   - İşlem başarılıysa `BasicAckAsync`; hata varsa `BasicNackAsync(requeue: true)`.

5. **Test et.** RabbitMQ'yu ayağa kaldır (`docker compose up -d`), Producer'a istek at, Consumer
   loglarında mesajın işlendiğini doğrula. Aynı mesajı elle iki kez gönderip (RabbitMQ panelinden
   "Publish message") idempotency'nin çalıştığını, yani ikinci seferde tekrar Task oluşmadığını gör.

## Yapma

- Yeni bir mesajlaşma kütüphanesi (MassTransit vb.) ekleme.
- Event DTO'suna iş mantığı/metod ekleme — sadece veri taşısın.
- `autoAck: true` kullanma — mesaj kaybı riski oluşur.
