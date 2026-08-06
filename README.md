# Auto Repair Management

Sistema de gestão para oficina mecânica: cadastro de clientes, veículos e ordens de serviço, com API em .NET e front-end em Blazor.

## Sobre o projeto

Nesse projeto eu quis praticar mais o ecossistema .NET, e aplicar mais o pragmatismo na hora de decidir como seria a arquitetura do software.

## Stack técnica

**Back-end** (`AutoRepairManagement.API`)
- .NET 10 / ASP.NET Core Web API (Minimal APIs)
- Entity Framework Core 10 + SQLite
- FluentValidation
- xUnit (testes)
- OpenAPI, exposto apenas em Development
- Docker / Docker Compose

**Front-end** (`AutoRepairManagement.FrontEnd`)
- Blazor Web App (.NET 10), render mode InteractiveServer
- Bootstrap 5
- Docker / Docker Compose

## Arquitetura

Os dois projetos são organizados por **feature** (vertical slice) em vez de camadas horizontais tradicionais: cada funcionalidade tem sua própria pasta com tudo que ela precisa, e só o que é realmente compartilhado fica em `Core/`.

No back-end, cada feature (`Client`, `Vehicle`, `ServiceOrder`) tem:
- `Entities/` — o modelo de dados (EF Core)
- `DTOs/` — os objetos usados na entrada/saída da API
- `Services/` — a regra de negócio, atrás de uma interface (`IClientService`, `IVehicleService`, etc.)
- `EndPoints/` — os endpoints Minimal API, registrados via `MapGroup`
- `Mappers/` — conversão entre entidade e DTO/response

As respostas HTTP passam por um tipo `Result` próprio (`Core/Helpers/Result.cs`), que padroniza sucesso, erro de validação, conflito, não encontrado e erro interno, mapeados para o `IResult` do ASP.NET Core via `ToHttpResult()`. Exclusão é soft delete (`DeletedAt`), e há índices únicos filtrados por `DeletedAt IS NULL` (ex: placa do veículo, e-mail do cliente).

O front-end segue a mesma lógica de feature-folder, com **code-behind separado**: cada `.razor` tem seu `.razor.cs` (lógica) e `.razor.css` (estilo isolado do componente).

## Estrutura do repositório

```
auto_repair_management/
├── back-end/
│   ├── AutoRepairManagement.API/
│   │   ├── Src/
│   │   │   ├── Core/              # DbContext, Result, helpers compartilhados
│   │   │   └── Features/          # Client, Vehicle, ServiceOrder
│   │   ├── Migrations/
│   │   ├── Dockerfile / compose.yaml
│   │   └── AutoRepairManagement.API.http
│   ├── AutoRepairManagement.Test/ # testes xUnit (services e validators)
│   └── postman/                   # collection para testar a API manualmente
└── front-end/
    └── AutoRepairManagement.FrontEnd/
        ├── Src/
        │   ├── Core/               # layout e componentes compartilhados
        │   └── Features/           # Dashboard
        ├── wwwroot/
        └── Dockerfile / compose.yaml
```

## Como rodar

### Back-end

```bash
cd back-end/AutoRepairManagement.API
dotnet restore
dotnet run
```

A API sobe em `http://localhost:5044` (ou `https://localhost:7130`). O banco SQLite (`autorepair.db`) e as migrations são aplicados automaticamente na inicialização.

Ou via Docker:

```bash
cd back-end/AutoRepairManagement.API
docker compose up --build
```

### Front-end

```bash
cd front-end/AutoRepairManagement.FrontEnd
dotnet restore
dotnet run
```

Ou via Docker:

```bash
cd front-end/AutoRepairManagement.FrontEnd
docker compose up --build
```

## Testes

```bash
cd back-end/AutoRepairManagement.Test
dotnet test
```

Cobre os services e validators de Client, Vehicle e ServiceOrder, além de `ResultExtension`.

## Endpoints da API

Todos seguem o mesmo padrão REST (listagem paginada, busca por id, criação, atualização e remoção):

| Recurso | Rota base |
|---|---|
| Clientes | `/api/v1/clients` |
| Veículos | `/api/v1/vehicles` |
| Ordens de serviço | `/api/v1/serviceOrders` |

Uma collection pronta do Postman está em `back-end/postman/AutoRepairManagement.postman_collection.json`. Em ambiente de Development, a documentação OpenAPI também fica disponível.

## Aprendizados / decisões técnicas

- Minimal API deixou o sistema mais enxuto: menos arquivos e abstrações para o mesmo resultado.
- SQLite resolveu bem para um projeto de demonstração, sem custo de setup.
- Separar por feature em vez de por camada evitou ter que abrir 5 arquivos de camadas diferentes só para entender uma funcionalidade inteira.
- Abordagens como Repository e Controller podem dar errado em contextos muito simples, dificultando desnecessariamente a implementação. No Minimal API, muitas abordagens clássicas têm que ser adaptadas.

## Roadmap

- Pretendo fazer o front-end em Blazor utilizando abordagens semelhantes ao MVVM modular.