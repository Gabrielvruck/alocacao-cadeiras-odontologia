# Sistema de Cadastro de Cadeiras de Dentista

## Objetivo
API em .NET 8 para gerenciar cadeiras de dentista e alocações. Permite criar, listar, atualizar e excluir cadeiras, além de alocar automaticamente horários entre cadeiras (rodízio) conforme período informado.

## Visão geral do sistema
- Arquitetura: DDD (camadas `Api`, `Aplicacao`, `Infra`, `Dominio`).
- Banco de dados: MySQL (pomelo provider).
- Validação: FluentValidation.
- Migrations: Entity Framework Core (migrations geradas no projeto `src/Infra`).
- Testes: xUnit + Moq + FluentAssertions (projetos em `tests/Aplicacao.Tests`).

## Requisitos
- .NET 8 SDK instalado
- MySQL (local ou remoto)
- `dotnet-ef` (ferramenta CLI) para gerar/aplicar migrations

## Desenvolvimento (rápido)

Instruções completas para configurar o MySQL (local, container via Docker Desktop) e aplicar as migrations já existentes estão em `docs/DEVELOPMENT.md`.

Resumo rápido:
- Defina a connection string de desenvolvimento (variável de ambiente ou `src/Api/appsettings.Development.json`).
- Execute `dotnet restore` e `dotnet build`.
- Aplique as migrations: `dotnet ef database update --project src/Infra --startup-project src/Api`.

## Como configurar a conexão com o banco (MySQL)
1. Edite a connection string no `appsettings.json` ou `appsettings.Development.json` do projeto `src/Api`.
   Exemplo:

```json
{
  "ConnectionStrings": {
    "BancoDados": "server=localhost;port=3306;database=alocacao_cadeiras;user=root;password=senha"
  }
}
```

2. Alternativamente defina variável de ambiente (Windows PowerShell):

```powershell
$env:ConnectionStrings__BancoDados = "server=localhost;port=3306;database=alocacao_cadeiras;user=root;password=senha"
```
Observação: Não coloque credenciais sensíveis no repositório público.

3. Acesse o Swagger UI em `http://localhost:{porta}/swagger` para testar endpoints CRUD e alocação.

## Endpoints principais (resumo)
- `GET /api/cadeiras` — lista cadeiras
- `GET /api/cadeiras/{id}` — obtém cadeira por id
- `POST /api/cadeiras` — cria cadeira
- `PUT /api/cadeiras/{id}` — atualiza cadeira
- `DELETE /api/cadeiras/{id}` — remove cadeira
- `POST /api/alocacoes/automaticas` — cria alocações por rodízio (verificar rota conforme controller)
