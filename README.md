# DocumentProcessor

DocumentProcessor is a command line utility built with .NET 8 for document processing.

## Features
- Analyze a pdf wether it is an invoice, correspondence or other document type.

## Prerequisites
- [Docker Desktop](https://www.docker.com/products/docker-desktop) up and running
- Running Ollama with mistral model
  ```sh
  cd ollama
  docker-compose up
  docker exec -it ollama ollama pull mistral
  ```

## Usage

Run the CLI

```sh
dotnet run --project DocumentProcessor -- classify-pdf C:\files\correspondence.pdf
```
