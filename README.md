
# Projeto de Reuniões

API REST desenvolvida em C# com ASP.NET Core para gerenciamento de salas de reuniões e reservas.

## Tecnologias utilizadas

- C#
- .NET 11
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL 18

## Funcionalidades

### Gerenciamento de salas
- Cadastrar salas com nome, andar e quantidade de assentos.
- Consultar salas por ID.
- Listar salas ordenadas por andar.
- Pesquisar salas pelo nome.
- Paginação de 10 salas por página.
- Atualizar e excluir salas.

### Gerenciamento de reservas
- Cadastrar reservas informando sala, início e fim.
- Consultar e listar reservas.
- Impedir reservas com horários conflitantes na mesma sala.
- Permitir reservas somente entre 08h e 19h, no horário de Brasília.
- Remarcar reservas futuras.
- Cancelar reservas.

### Relatórios
- Consultar a quantidade de horas livres por sala em um período.
- Consultar o total de reuniões com início nos últimos 7 dias.

Observação: o relatório dos últimos 7 dias considera a data de início da reunião, e não a data em que a reserva foi cadastrada.

## Requisitos para executar

- SDK do .NET 11
- PostgreSQL
- Ferramenta dotnet-ef
- Git (para clonar o repositório)

## Configuração do banco de dados

1. Crie um banco de dados PostgreSQL chamado `reunioes_db`.
2. Configure a string de conexão no projeto, utilizando o .NET User Secrets ou outra configuração local segura.
3. Execute as migrations para criar as tabelas.

Comandos:

```powershell
dotnet restore
dotnet ef database update
```

O arquivo `banco.sql` também contém o script SQL de criação das tabelas.

Não execute o script SQL e as migrations sobre o mesmo banco sem verificar quais estruturas já foram criadas.

## Executando a API

Na pasta do projeto, execute:

```powershell
dotnet run
```

A API poderá ser acessada pelo endereço exibido no terminal.

Durante o desenvolvimento, foi utilizado:

`http://localhost:5125`

## Endpoints

### Salas

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| POST | /api/salas | Cadastrar sala |
| GET | /api/salas | Listar salas |
| GET | /api/salas/{id} | Consultar sala por ID |
| PUT | /api/salas/{id} | Atualizar sala |
| DELETE | /api/salas/{id} | Excluir sala |
| GET | /api/salas?nome=Diretoria | Pesquisar por nome |
| GET | /api/salas?pagina=2 | Paginar resultados |
| GET | /api/salas/horas-livres?inicio=2026-11-16&fim=2026-11-16 | Consultar horas livres |

### Reservas

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| POST | /api/reservas | Cadastrar reserva |
| GET | /api/reservas | Listar reservas |
| GET | /api/reservas/{id} | Consultar reserva |
| PUT | /api/reservas/{id} | Remarcar reserva |
| DELETE | /api/reservas/{id} | Cancelar reserva |
| GET | /api/reservas/total-ultimos-7-dias | Total de reuniões nos últimos 7 dias |

## Exemplo de cadastro de sala

Requisição:

POST /api/salas

Corpo JSON:

```json
{
  "nome": "Sala de Diretoria",
  "andar": 2,
  "quantidadeAssentos": 10
}
```

## Exemplo de cadastro de reserva

Requisição:

POST /api/reservas

Corpo JSON:

```json
{
  "salaId": 1,
  "inicio": "2026-11-16T13:00:00Z",
  "fim": "2026-11-16T14:00:00Z"
}
```

As datas das reservas devem ser informadas em UTC, com `Z` no final.

O exemplo acima corresponde ao horário de 10h às 11h em Brasília.

## Estrutura do projeto

- `Controllers/`: endpoints da API.
- `Models/`: modelos de salas e reservas.
- `Data/`: contexto do Entity Framework Core.
- `Migrations/`: migrations do banco de dados.
- `banco.sql`: script SQL de criação do banco.

## Repositório

https://github.com/VictorAffonso792/Projeto_Reunioes_Rodrigo
