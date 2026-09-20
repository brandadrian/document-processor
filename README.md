# DocumentProcessor

DocumentProcessor is a command line utility built with .NET for document processing.

## Features
- Analyze a pdf wether it is an invoice, correspondence or other document type.
- Extract document data as a JSON array of `CategoryName`/`Text` pairs, matching the selected extraction type.

## Prerequisites
- [Docker Desktop](https://www.docker.com/products/docker-desktop) up and running
- **Ollama** running locally with a compatible model (e.g., `qwen2.5:3b`, `qwen3:4b`, or `gemma4:31b`)

### Ollama Setup
```sh
# 1. Install Ollama via Homebrew or download from https://ollama.com/download
brew install ollama

# 2. Start Ollama service
ollama serve

# 3. Pull a model (in another terminal)
ollama pull gemma4:31b
```

### Alternative: Ollama Setup via Docker
```sh
cd ollama
docker-compose up -d
docker exec -it ollama ollama pull gemma4:31b
```

## Usage

From the repository root, with Ollama running and the selected model pulled.

### PDF Classification

Classify a PDF:

```sh
dotnet run --project src/DocumentProcessor -- classify-pdf files/classification/tests/invoice_3.pdf
```

### PDF Data Extraction

Extract data from a PDF:

```sh
dotnet run --project src/DocumentProcessor -- extract-pdf-data \
	files/extraction/training/invoice/invoice_3.pdf \
	--type invoice
```

# Extraction Flow

```mermaid
sequenceDiagram
    actor User
    participant CLI as extract-pdf-data
    participant Text as PdfPig text reader
    participant Training as Training resources
    participant Prompt as Type prompt
    participant Ollama
    participant Output as Results folder

    User->>CLI: Run command with PDF path and --type
    CLI->>Training: Load training PDFs and expected JSON for type
    Training-->>CLI: Example text plus expected extraction
    CLI->>Prompt: Load extraction_prompt.txt for type
    Prompt-->>CLI: System prompt
    CLI->>Text: Extract text from input PDF
    Text-->>CLI: PDF text
    CLI->>CLI: Build category schema and prompt context
    CLI->>Ollama: Send system prompt, examples, PDF text, schema
    Ollama-->>CLI: Structured JSON response
    CLI->>Output: Write timestamped JSON result
    Output-->>User: Log output file path
```

The current pipeline extracts text directly with PdfPig. OCR is not part of the extraction flow.
