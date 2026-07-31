Укажите данные для подключения к БД в данной папке апи при помощи секрета
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:Postgres" 'Host=localhost;Port=5432;Database=dumbsalmon;Username=postgres;Password=ваш_пароль'

Или укажите их в appsettings.json

