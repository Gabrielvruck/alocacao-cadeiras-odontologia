# Desenvolvimento — Aplicar Migrations localmente

Este documento descreve duas opções para que um desenvolvedor aplique as migrations já existentes e crie o banco MySQL para desenvolvimento: (A) MySQL instalado localmente e (B) MySQL via Docker Compose.

Pré-requisitos
- .NET 8 SDK instalado
- dotnet-ef (CLI) instalado globalmente: `dotnet tool install --global dotnet-ef` (ou `dotnet tool update --global dotnet-ef`)

Onde estão as migrations
- As migrations estão em: `src/Infra/Migrations`
- O `DbContext` e a configuração do provedor MySQL estão no projeto `src/Api` (veja `Program.cs`).

Opção A — MySQL instalado localmente

1. Garanta que o MySQL está rodando e que o usuário tem permissão para criar banco/tabelas.

2. (Opcional) Defina a connection string temporariamente via variável de ambiente.

PowerShell (Windows):
```powershell
$env:ConnectionStrings__BancoDados = "server=localhost;port=3306;database=alocacao_cadeiras;user=root;password=senha"
```

Bash (macOS/Linux):
```bash
export ConnectionStrings__BancoDados="server=localhost;port=3306;database=alocacao_cadeiras;user=root;password=senha"
```

Alternativa: adicione a connection string em `src/Api/appsettings.Development.json` sob `ConnectionStrings:BancoDados`.

3. Restaurar e buildar a solução:

```bash
dotnet restore
dotnet build
```

4. Aplicar as migrations (migrations em `src/Infra`, startup em `src/Api`):

```bash
dotnet ef database update --project src/Infra --startup-project src/Api
```

5. Verifique no MySQL:

```sql
SHOW DATABASES;
USE alocacao_cadeiras;
SHOW TABLES; -- deve listar `cadeiras` e `alocacoes`
```

Opção B — MySQL via Docker Desktop (container)

Se você usa Docker Desktop, pode criar um container MySQL rapidamente sem usar Docker Compose. Abaixo há instruções para executar via linha de comando (`docker run`) e notas sobre o uso via GUI do Docker Desktop.

1. Criar e executar um container MySQL usando `docker run`:

```bash
docker run -d \
  --name alocacao-mysql \
  -e MYSQL_ROOT_PASSWORD=senha \
  -e MYSQL_DATABASE=alocacao_cadeiras \
  -p 3306:3306 \
  mysql:8.0
```

2. Verifique se o container subiu corretamente:

```bash
docker ps --filter "name=alocacao-mysql"
docker logs alocacao-mysql --tail 50
```

No Docker Desktop (GUI):
- Abra o Docker Desktop e espere o serviço iniciar.
- Na aba "Containers / Apps" verifique se o container `alocacao-mysql` está em execução (running).
- Você pode inspecionar logs, abrir um terminal (Exec) e ver as portas mapeadas pela interface.

3. Aguarde o MySQL ficar pronto. Para checar disponibilidade execute:

```bash
docker exec -it alocacao-mysql mysql -uroot -psenha -e "SHOW DATABASES;"
```

4. Configure a connection string apontando para `localhost:3306` e aplique as migrations:

PowerShell (Windows):
```powershell
$env:ConnectionStrings__BancoDados = "server=localhost;port=3306;database=alocacao_cadeiras;user=root;password=senha"
dotnet ef database update --project src/Infra --startup-project src/Api
```

Bash (macOS/Linux):
```bash
export ConnectionStrings__BancoDados="server=localhost;port=3306;database=alocacao_cadeiras;user=root;password=senha"
dotnet ef database update --project src/Infra --startup-project src/Api
```

Dicas e resolução de problemas
- Se `dotnet ef` reclamar de versão incompatível, atualize a ferramenta para a versão compatível com EF Core 8.
- Se `ServerVersion.AutoDetect` falhar, especifique explicitamente `ServerVersion.Parse("8.0.33-mysql")` ou similar no `Program.cs` (apenas se necessário).
- Erros de permissão no MySQL: verifique usuário e privilégios (CREATE, ALTER, INDEX).
- Conexão recusada: cheque host/porta, firewall e se o MySQL aceita conexões remotas.

Cuidados
- Não aplicar migrations automaticamente em produção sem revisão — mudanças de schema podem causar downtime.
- Certifique-se de que as migrations no repositório representam o estado desejado do schema antes de aplicá-las.

Resumo
Seguindo uma das opções acima, um desenvolvedor com a configuração adequada e permissões no MySQL conseguirá criar o banco local e aplicar as migrations já comitadas no projeto. Para instruções detalhadas e troubleshooting, veja `docs/DEVELOPMENT.md`.
