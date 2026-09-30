# RabbitMQ Öğrenme Projesi

Basit bir "kuyruğa yaz / kuyruktan oku" (producer/consumer) projesi.
Amaç: RabbitMQ'yu ve event-driven mimariyi doğrudan, sade bir örnekle öğrenmek.
Bu, ../E-CommerceApp/ecommerce projesinden **tamamen bağımsız**, ayrı bir öğrenme projesidir.
"Enterprise" karmaşıklığı DEĞİL — az katman, az soyutlama, okunabilir kod önce gelir.
Emin olmadığın bir konuda yeni teknoloji/katman ekleme, sor.

## Senaryo
Producer API'ye "görev" (Task) oluşturma isteği gelir → `TaskCreatedEvent` RabbitMQ kuyruğuna yayınlanır
→ Consumer Worker kuyruktan okur → Task'ı repository üzerinden veritabanına yazar → loglar.

## Teknoloji kararları (değiştirme)
- Backend: .NET 10
- Producer: ASP.NET Core Web API, **Controller tabanlı** (Minimal API kullanma)
- Consumer: .NET Worker Service (`BackgroundService`)
- Mesajlaşma: RabbitMQ, resmi `RabbitMQ.Client` (7.x, async API) **doğrudan** kullanılır.
  MassTransit / NServiceBus / kendi EventBus soyutlamamız YOK — amaç RabbitMQ'yu bizzat görmek.
- Veritabanı: SQLite + EF Core (`Microsoft.EntityFrameworkCore.Sqlite`). Docker'da SQL Server YOK,
  sadece RabbitMQ container'ı var. Consumer kendi `tasks.db` dosyasına yazar.
- Repository pattern: generic olmayan, domain'e özel `ITaskRepository` + EF Core implementasyonu.
  Genel amaçlı `IRepository<T>` gibi soyutlamalara gitme — tek entity (Task) var, buna gerek yok.
- Test: xUnit
- Çalıştırma: docker compose (sadece RabbitMQ), Producer ve Consumer `dotnet run` ile local çalışır.

## Projeler
| Proje | Klasör | Sorumluluk |
|---|---|---|
| Producer.Api | src/Producer/Producer.Api | Task oluşturma endpoint'i, event'i kuyruğa yayınlar |
| Consumer.Worker | src/Consumer/Consumer.Worker | Kuyruktan okur, repository ile SQLite'a yazar |
| Contracts | src/Shared/RabbitMqDemo.Contracts | Producer ve Consumer'ın ortak paylaştığı event sözleşmeleri (DTO) |

Detaylar: `docs/architecture.md`

## Altın kurallar
1. Producer ve Consumer birbirini **HTTP ile çağırmaz**. Tek iletişim yolu RabbitMQ.
2. Yeni event eklemeden önce `docs/events.md` güncellenir; kod dokümana uyar, tersi değil.
3. Consumer **idempotent** olmalı (aynı mesaj iki kez işlenebilir). Bkz. docs/events.md → İdempotency.
4. Event contract'ları (`RabbitMqDemo.Contracts`) sadece basit DTO içerir, davranış/iş kuralı içermez.
5. Hata durumunda mesaj kuyrukta kaybolmamalı (manuel ack, hata olursa nack/requeue). Bkz. docs/architecture.md.
6. Gizli bilgiler (RabbitMQ şifresi vb.) koda yazılmaz; environment variable / `.env` ile gelir.

## Ne zaman hangi dosyayı oku
- Yeni event yayınlıyor/dinliyorsan → `.claude/skills/new-event/SKILL.md`
- Repository ekliyor/değiştiriyorsan → `.claude/skills/repository-pattern/SKILL.md`
- Producer.Api'ye yeni endpoint ekliyorsan → `.claude/skills/new-endpoint/SKILL.md`
- Sıradaki iş ne → `docs/roadmap.md`

## Komutlar
- RabbitMQ'yu ayağa kaldır: `docker compose up -d`
- RabbitMQ paneli: http://localhost:15672 (guest/guest)
- Build: `dotnet build RabbitMqDemo.slnx`
- Testler: `dotnet test RabbitMqDemo.slnx`
- Producer'ı çalıştır: `dotnet run --project src/Producer/Producer.Api`
- Consumer'ı çalıştır: `dotnet run --project src/Consumer/Consumer.Worker`

## "Bitti" tanımı
Bir iş ancak şunların hepsi sağlanınca bitmiştir:
1. `dotnet build` uyarısız/hatasız
2. `dotnet test` geçiyor
3. `docker compose up -d` sonrası RabbitMQ paneli açılıyor
4. Değişiklik akışı etkiliyorsa, Producer'a istek atılıp Consumer'ın işlediği uçtan uca (curl + loglar) denendi
5. İlgili doküman (events.md, architecture.md, roadmap.md) güncellendi

Bir fazı bitirince docs/roadmap.md'de işaretle ve dur; bir sonraki faza kendiliğinden geçme.
