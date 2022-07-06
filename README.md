# Gateway
Test payment gateway

## Декомпозиция задачи:
- Реализовать возможность подключения различных мерчантов;
- Добавить авторизацию и аутентификацию с ролями;
- Сделать дополнительный функционал регистрации пользователей - инвайты;
- Написать административную часть с возможностью управления мерчантами и параметрами сервиса;
- Сделать фоновый сервис для запроса статуса платежей;

> Проект написан на ASP.NET Core 6.0 с использованием: Serilog, RestSharp, Newtonsoft.Json, ncrontab, EFC.\
> Аутентификация использует куки.\
> Фронт написан на JS.