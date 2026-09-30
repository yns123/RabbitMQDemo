---
name: new-endpoint
description: Producer.Api'ye yeni bir REST endpoint (controller action) eklerken veya mevcut birini değiştirirken kullan. Yeni DTO, doğrulama veya hata yanıtı gerektiren her Producer.Api işinde bu adımları takip et.
---

# Producer.Api'ye yeni endpoint ekleme

Producer.Api'nin tek işi: istek al, doğrula, (varsa) event yayınla, 2xx dön. Veritabanına
**hiç dokunmaz** — kalıcılık Consumer.Worker'ın işi.

## Adımlar

1. **Controller tabanlı yaz.** Minimal API kullanma; `[ApiController]` + `[Route("api/[controller]")]`
   ile controller action'ı ekle.

2. **Request/Response DTO'ları ayrı olsun.** Event contract'ı (`RabbitMqDemo.Contracts`) ile
   HTTP DTO'sunu karıştırma — action, gelen isteği DTO'dan event'e kendi map eder.

3. **Doğrulama.** Basit model validation yeterli (`[Required]`, `ModelState.IsValid` veya
   data annotations). Ayrı bir validation kütüphanesi (FluentValidation vb.) ekleme — öğrenme
   projesi için gereksiz.

4. **Event yayınlanıyorsa** → önce `.claude/skills/new-event/SKILL.md`'yi oku.

5. **Hata yanıtları `ProblemDetails` formatında olsun** (`ASP.NET Core`'un yerleşik
   `ProblemDetails`'i, ekstra paket gerekmez).

6. **Health check.** Yeni servis/bağımlılık eklemiyorsan `/health` endpoint'ine dokunma; RabbitMQ
   bağlantısı health check'e dahil edilecekse önce bunu CLAUDE.md'de not al.

## Yapma

- Producer.Api'den Consumer.Worker'a veya herhangi bir servise HTTP ile çağrı yapma.
- Producer.Api'ye DbContext/repository ekleme.
- Minimal API stiliyle endpoint tanımlama.
