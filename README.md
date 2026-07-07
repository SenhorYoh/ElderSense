# ElderSense 🛡️

> Plataforma web de monitorização remota de idosos — conecta cuidadores e idosos, recebe leituras de sensores em tempo real e gera alertas automáticos quando algo sai do normal.

**Projeto de Desenvolvimento Web** · Licenciatura em Engenharia Informática · Instituto Politécnico de Tomar · 2.º ano, 2.º semestre · Ano letivo 2025/2026

---

## 📋 Índice

- [Sobre o projeto](#-sobre-o-projeto)
- [Funcionalidades](#-funcionalidades)
- [Tecnologias](#-tecnologias)
- [Arquitetura](#-arquitetura)
- [Modelo de dados](#-modelo-de-dados)
- [API REST](#-api-rest)
- [Como executar localmente](#-como-executar-localmente)
- [Publicação (Azure)](#-publicação-azure)
- [Credenciais de demonstração](#-credenciais-de-demonstração)
- [Autores](#-autores)
- [Bibliotecas de terceiros](#-bibliotecas-de-terceiros)

---

## 🎯 Sobre o projeto

O **ElderSense** resolve um problema concreto: permitir que um **cuidador** (familiar ou profissional) acompanhe à distância um ou mais **idosos**, através de sensores instalados em casa deles.

Os sensores enviam leituras — batimentos cardíacos, temperatura, deteção de passagem — e, quando um valor sai do intervalo saudável, o sistema cria automaticamente um **alerta que aparece no ecrã do cuidador em tempo real, sem necessidade de atualizar a página** (via SignalR).

Como o projeto não dispõe de hardware físico, inclui um **simulador** que gera leituras automaticamente a cada minuto, permitindo demonstrar todo o fluxo de ponta a ponta.

Existem dois perfis de utilizador com acessos diferenciados:

- **Cuidador** — regista idosos e sensores, e consulta alertas e dados de monitorização.
- **Idoso** — pessoa monitorizada, com acesso limitado à informação que lhe diz respeito.

---

## 🧩 O que foi desenvolvido

Componentes construídos no âmbito do projeto:

- Interface web completa em Razor Pages, com CRUD e validação em servidor
- Modelo de dados em Entity Framework Core, com resolução dos caminhos de *cascade delete*
- Simulador de leituras de sensores como serviço em segundo plano
- Alertas em tempo real via SignalR
- API REST com autenticação JWT
- Autenticação por ASP.NET Identity, login com Google (OAuth) e confirmação por email
- Publicação em Azure App Service + Azure SQL

---

## ✨ Funcionalidades

- 🔐 **Autenticação completa** — registo, login local, login com Google (OAuth) e confirmação de conta por email
- 👥 **Dois perfis com autorização por *roles*** (Cuidador / Idoso)
- 📟 **CRUD de Sensores e Utilizadores** com validação em servidor
- 🔔 **Alertas em tempo real** via SignalR (WebSockets)
- 🤖 **Simulador automático** de leituras de sensores (serviço em segundo plano)
- 🌐 **API REST** protegida com autenticação JWT
- 📧 **Envio de emails** de confirmação via SendGrid
- ⚠️ **Páginas de erro personalizadas** para 401, 403, 404 e 500
- ☁️ **Aplicação publicada** em Azure App Service + Azure SQL

---

## 🛠️ Tecnologias

| Categoria | Tecnologia |
|---|---|
| Framework | ASP.NET Core (.NET) |
| Interface web | Razor Pages |
| API | ASP.NET Core MVC (Controllers) |
| Acesso a dados | Entity Framework Core + LINQ |
| Base de dados | SQL Server / Azure SQL |
| Autenticação | ASP.NET Identity · JWT · Google OAuth |
| Tempo real | SignalR |
| Email | SendGrid |
| Frontend | Bootstrap · Tabler Icons · jQuery |
| Publicação | Azure App Service · Azure SQL Database |

---

## 🏗️ Arquitetura

O projeto separa claramente a **interface humana** (Razor Pages) da **comunicação máquina-a-máquina** (API REST), partilhando o mesmo modelo de dados e a mesma camada de autenticação.

```
ElderSense/
├── Pages/            → Razor Pages (interface do utilizador)
│   ├── Sensores/     → CRUD de sensores
│   ├── Utilizadores/ → CRUD de utilizadores + associação de idosos
│   ├── Alertas/      → listagem + cliente SignalR
│   ├── DadosMonitorizacao/ → histórico de leituras
│   └── Erro/         → páginas de erro personalizadas
├── API/              → Controllers REST + simulador
│   ├── AuthController.cs       → login e emissão de JWT
│   ├── SensoresAPIController.cs → endpoints de leituras
│   ├── SimuController.cs        → lógica de simulação
│   └── SimuWorker.cs            → serviço em segundo plano (BackgroundService)
├── Data/
│   ├── ApplicationDbContext.cs  → DbContext + configuração das relações
│   └── Model/        → entidades (Utilizador, Sensor, DadosMonitorizacao, Alerta)
├── Services/         → TokenService (JWT) e EmailSender (SendGrid)
├── Hubs/             → AlertaHub (SignalR)
├── Migrations/       → migrations do EF Core
└── Program.cs        → configuração e arranque da aplicação
```

---

## 🗄️ Modelo de dados

Quatro entidades principais, para além das tabelas do ASP.NET Identity:

- **Utilizador** — herda de `IdentityUser`; distingue Cuidador de Idoso; relação **muitos-para-muitos auto-referencial** entre cuidadores e idosos.
- **Sensor** — tipo Beacon (presença) ou Pulseira (sinais vitais); pertence a um cuidador e, opcionalmente, a um idoso.
- **DadosMonitorizacao** — cada leitura registada por um sensor.
- **Alerta** — aviso gerado quando uma leitura sai do intervalo saudável; ligado às leituras de origem por relação **muitos-para-muitos**.

As relações seguem as regras de derivação Entidade-Relação, incluindo a resolução de **múltiplos caminhos de *cascade delete*** através da configuração de `DeleteBehavior` no `OnModelCreating` (para evitar o erro 1785 do SQL Server).

Relacionamentos obrigatórios pela avaliação: um **muitos-para-um** (cuidador → sensores/dados/alertas) e um **muitos-para-muitos** (cuidadores ↔ idosos, e alertas ↔ dados).

---

## 🔌 API REST

A API é protegida por **JWT** (HMAC-SHA256, validade de 2 horas). Fluxo: autenticar no endpoint de login para obter o token, e enviá-lo no cabeçalho `Authorization: Bearer <token>` nos restantes pedidos.

| Método | Endpoint | Descrição | Autenticação |
|---|---|---|---|
| `POST` | `/api/Auth/login` | Autentica e devolve o token JWT | — |
| `POST` | `/api/Sensores/leitura` | Regista uma leitura de sensor | 🔒 Bearer |
| `GET`  | `/api/Sensores/historico/{idosoId}` | Histórico de leituras de um idoso | 🔒 Bearer |
| `GET`  | `/api/Sensores/estado/{sensorId}` | Estado (Online/Offline) de um sensor | 🔒 Bearer |
| `GET`  | `/api/Sensores/ping` | Teste de disponibilidade | 🔒 Bearer |

> Testável com **Postman** ou, no Windows, com `Invoke-RestMethod` do PowerShell.

---

## 💻 Como executar localmente

**Pré-requisitos:** .NET SDK, SQL Server (ou acesso a uma Azure SQL), Visual Studio 2022+ / VS Code / Rider.

```bash
# 1. Clonar o repositório
git clone <url-do-repositorio>
cd ElderSense

# 2. Configurar os segredos (connection string, chave JWT, SendGrid, Google OAuth)
#    via User Secrets ou appsettings.Development.json

# 3. Aplicar as migrations à base de dados
dotnet ef database update

# 4. Executar
dotnet run
```

> A configuração de acesso à base de dados e a serviços externos é feita por ficheiros de configuração (`appsettings.json`) e **User Secrets** — nenhum segredo é versionado no repositório.

---

## ☁️ Publicação (Azure)

A aplicação está publicada em **Azure App Service** (plano Basic B1, região France Central) com **Azure SQL Database**. Os segredos de produção são geridos através das *App Settings* do Azure (fora do código).

> **Nota:** o *tier* gratuito do Azure SQL pode pausar a base de dados por inatividade ou por limite de quota; o primeiro acesso após uma pausa pode demorar alguns segundos.

---

## 🔑 Credenciais de demonstração

| Perfil | Email | Password |
|---|---|---|
| Cuidador | `testeprofCuidador@gmail.com` | `qwe123QWE123#` |
| Idoso | `testeprofIdoso@gmail.com` | `qwe123QWE123#` |

> As passwords são guardadas com *hash* PBKDF2 + *salt* pelo ASP.NET Identity — nunca em texto simples.

---

## 👨‍💻 Autores

| Nome | Nº de aluno |
|---|---|
| **Tomás** | 27460 |
| **Yohan** | 27213 |

---

## 📦 Bibliotecas de terceiros

Todas as bibliotecas de terceiros utilizadas são de disponibilização pública (via NuGet ou CDN):

- **Bootstrap** — layout e componentes de interface
- **Tabler Icons** — conjunto de ícones
- **jQuery** — validação do lado do cliente (ASP.NET)
- **Microsoft SignalR (cliente JS)** — comunicação em tempo real
- **SendGrid** — envio de emails
- **Swashbuckle** *(não utilizado nesta versão)*

Pacotes NuGet principais: `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.AspNetCore.Identity.EntityFrameworkCore`, `Microsoft.AspNetCore.Authentication.JwtBearer`, `Microsoft.AspNetCore.Authentication.Google`, `Microsoft.AspNetCore.SignalR`, `SendGrid`.

---

<div align="center">

*Projeto académico desenvolvido no âmbito da unidade curricular de Desenvolvimento Web — IPT, 2025/2026*

</div>
