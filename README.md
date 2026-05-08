# 🕸 OSP Vivo Web Scraper

O **OSP Vivo Web Scraper** é uma aplicação desktop desenvolvida em C# (Windows Forms) projetada para automatizar a extração de dados e processos no portal OSP da Vivo. Utilizando o motor do **Playwright**, a ferramenta oferece uma interface robusta para processamento em lote via arquivos CSV.

## 🚀 Funcionalidades

*   **Automação com Playwright:** Motor de navegação moderno para interagir com páginas web de forma rápida e segura.
*   **Gestão de Credenciais:** Área para configurar usuário e senha com opção de salvar os dados localmente para uso futuro.
*   **Modo Oculto (Headless):** Opção para executar o navegador em segundo plano, economizando recursos do sistema.
*   **Processamento de CSV:**
    *   Seleção de arquivo local.
    *   Validação automática de colunas (exige coluna `IDs`).
    *   Limpeza de seleção facilitada.
*   **Múltiplos Modos de Extração:**
    *   Draft
    *   Medição
    *   ID Cancelados
    *   Memória de Cálculo
*   **Monitoramento em Tempo Real:**
    *   Barra de progresso visual.
    *   Terminal de logs detalhado com data e hora.
    *   Opção para exportar logs para arquivo `.txt`.

## 🛠 Tecnologias Utilizadas

*   **Linguagem:** C#
*   **Framework:** .NET 8.0 (Windows Forms)
*   **Automação:** Playwright for .NET
*   **Arquitetura:** Injeção de dependência simples para serviços de log e configuração.

## 📋 Pré-requisitos

Antes de começar, você precisará ter instalado em sua máquina:
*   .NET 8.0 SDK
*   Playwright Browsers (instalados via linha de comando após o build)

## ⚙️ Instalação e Execução

1.  **Clone o repositório:**
    ```bash
    git clone https://github.com/seu-usuario/OSPWebScraper.git
    ```
2.  **Restaure as dependências:**
    ```bash
    dotnet restore
    ```
3.  **Instale os navegadores do Playwright:**
    ```bash
    # Navegue até a pasta do binário ou use o comando dotnet
    playwright install
    ```
4.  **Execute a aplicação:**
    ```bash
    dotnet run
    ```

## 📖 Como Usar

1.  **Configuração:** Insira suas credenciais do portal OSP Vivo. Clique em "Salvar Credenciais" se desejar que o app lembre delas.
2.  **Arquivo de Entrada:** Clique em "Selecionar CSV" e escolha um arquivo que contenha uma coluna chamada `IDs`.
3.  **Extração:** Escolha o modo desejado (ex: Draft ou Medição).
4.  **Iniciar:** Clique no botão "🚀 Iniciar Scraping".
5.  **Acompanhamento:** Acompanhe o progresso pela barra e verifique o terminal de logs para mensagens de sucesso ou erro.
6.  **Logs:** Ao final, se necessário, utilize o botão "Baixar Logs" para salvar o histórico da operação.

## 🗂 Estrutura do Projeto

*   `Form1.cs`: Interface principal e controle de eventos.
*   `Services/`: Contém a lógica de negócio (`PlaywrightService`, `LogService`, `ConfigService`).
*   `Models/`: Definições de dados como `AppSettings`.

---
**Versão:** 1.0.0  
**Desenvolvido por:** Geovane Carvalho

## ⚖️ Licença

Este projeto é para uso interno. Verifique os termos de uso do portal antes de realizar automações.