# PiKPO_Labs

## Сравнение производительности REST API и GraphQL

Оба сервиса публикуют метрики OpenTelemetry на `/metrics`, которые собирает Prometheus. В Grafana на `http://localhost:3001` доступен дашборд **REST API vs GraphQL** со сравнением:

- количества запросов в секунду;
- задержки запросов p50 и p95;
- доли HTTP-ответов с кодом 5xx.

Временные ряды строятся по статусу ответа; запросы к `/metrics` исключены из сравнения. Для сбора данных запустите сервисы, Prometheus и Grafana через `docker compose up`.
