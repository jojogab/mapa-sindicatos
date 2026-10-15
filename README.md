# Mapa de Sindicatos

Aplicação web para explorar entidades sindicais do Brasil em um mapa interativo. O projeto usa Blazor com renderização interativa no servidor e carrega os registros de uma planilha Excel local.

## Funcionalidades

- Mapa coroplético do Brasil com a quantidade de entidades por estado.
- Seleção de uma UF pelo mapa ou pelo filtro.
- Busca textual nos dados cadastrais, de endereço, contato, filiação e eleição.
- Filtros por UF, grau, área geoeconômica, grupo, classe, categoria, central sindical, cadastro, federação, confederação, funcionamento da direção e forma de eleição.
- Opção para exibir somente entidades com mandato vigente.
- Cartões de resultados e perfil detalhado de cada entidade, com endereço, contato, dados cadastrais e informações eleitorais.
- Alternância entre tema claro e escuro e interface adaptada para telas menores.

## Tecnologias

- **.NET 10 / ASP.NET Core** — aplicação web.
- **Blazor Web App, Interactive Server** — interface interativa e comunicação entre C# e JavaScript.
- **ClosedXML 0.105.1** — leitura da planilha `.xlsx`.
- **Leaflet 1.9.4** — mapa interativo.
- **JavaScript e CSS** — integração do mapa, estilos e interop com Blazor.

Os arquivos de Leaflet são carregados pelo CDN do unpkg. Os limites estaduais são obtidos como GeoJSON do repositório [codeforgermany/click_that_hood](https://github.com/codeforgermany/click_that_hood). Portanto, é necessária uma conexão com a internet para carregar os limites no mapa. Se essa solicitação falhar, a busca e os filtros continuam disponíveis.

## Pré-requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download/dotnet/10.0).
- A planilha de dados descrita abaixo.
- Conexão com a internet para carregar o mapa e os limites estaduais.

## Como executar

Clone o repositório e entre na pasta do projeto:

```bash
git clone <URL-DO-REPOSITORIO>
cd MapaSindicatos
```

Inicie a aplicação:

```bash
dotnet run --project CnesMapaSindicatos/CnesMapaSindicatos.csproj
```

Com o perfil HTTP padrão, acesse [http://localhost:5179](http://localhost:5179). O perfil HTTPS também está configurado em `https://localhost:7040`.

Para compilar ou publicar:

```bash
dotnet build CnesMapaSindicatos/CnesMapaSindicatos.csproj
dotnet publish CnesMapaSindicatos/CnesMapaSindicatos.csproj -c Release
```

## Dados

A aplicação espera encontrar a planilha neste caminho, relativo ao projeto:

```text
CnesMapaSindicatos/DataTeste/SINDICATOSBRASIL14042025.xlsx
```

O arquivo de dados está incluído no repositório e é copiado para as saídas de build e publicação. A aplicação lê a primeira planilha do arquivo, ignora a primeira linha (cabeçalho) e espera os campos nas 31 colunas e na ordem definida no modelo `Sindicato`. Os registros são carregados em memória quando o serviço é iniciado; não há banco de dados nem sincronização automática com uma fonte externa.

Para atualizar a base, substitua o arquivo mantendo o mesmo nome, caminho, aba e estrutura de colunas. Se alterar a estrutura ou o nome do arquivo, atualize também o mapeamento em `Services/SindicatoService.cs`.

> Antes de publicar ou redistribuir o repositório, confirme a origem, a licença e a permissão de compartilhamento da planilha. O fato de um arquivo estar incluído no GitHub não concede direitos de redistribuição sobre ele.

## Estrutura do projeto

```text
MapaSindicatos/
├── CnesMapaSindicatos/
│   ├── Components/         # Páginas e componentes Blazor
│   ├── DataTeste/          # Planilha Excel usada pela aplicação
│   ├── Models/             # Modelo Sindicato e propriedades calculadas
│   ├── Services/           # Importação, busca e filtros dos dados
│   ├── wwwroot/            # CSS, JavaScript e arquivos estáticos
│   ├── Program.cs          # Configuração e inicialização da aplicação
│   └── CnesMapaSindicatos.csproj
└── README.md
```

## Licença

Este repositório não declara uma licença de software. Consulte o responsável pelo projeto antes de reutilizar ou redistribuir o código. A planilha e os dados de terceiros podem ter condições de uso próprias.
