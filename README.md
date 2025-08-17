# DocumentProcessor

DocumentProcessor is a command line utility built with .NET 8 for document processing.

## Features
- Analyze a pdf wether it is an invoice, correspondence or other document type.


## Usage

Start ollama
```sh
cd ollama
docker-compose up
```

Pull mistral image
```sh
docker exec -it ollama ollama pull mistral
```

Run the CLI

```sh
dotnet run --project DocumentProcessor -- classify-pdf C:\files\correspondence.pdf
```
