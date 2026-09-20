# DocumentProcessor

DocumentProcessor is a command line utility built with .NET for document processing.

## Table of Contents
- [System Context](#system-context)
- [Features](#features)
- [Prerequisites](#prerequisites)
  - [Ollama Setup](#ollama-setup)
  - [Alternative: Ollama Setup via Docker](#alternative-ollama-setup-via-docker)
- [Usage](#usage)
  - [Ollama Test](#ollama-test)
  - [PDF Classification](#pdf-classification)
  - [PDF Data Extraction](#pdf-data-extraction)
  - [PDF Image Data Extraction](#pdf-image-data-extraction)
- [Text vs Image Extraction](#text-vs-image-extraction)
- [Extraction Flow](#extraction-flow)
- [Image Extraction Flow](#image-extraction-flow)

## Features
- Analyze a pdf wether it is an invoice, correspondence or other document type.
- Extract document data as a JSON array of `FieldName`/`Text` pairs, matching the selected extraction type.
- Extract document data from rendered PDF images with a vision model.

## System Context

```mermaid
flowchart LR
    user["User"]
    app["DocumentProcessor CLI"]
    files["Local files\nPDFs, prompts, examples, results"]
    ollama["Local Ollama\nLLM"]

    user -->|"runs commands"| app
    app -->|"reads / writes"| files
    app -->|"asks for classification or extraction"| ollama
    ollama -->|"returns JSON"| app
```

## Prerequisites
- [Docker Desktop](https://www.docker.com/products/docker-desktop) up and running
- **Ollama** running locally with a compatible model (e.g., `qwen2.5:3b`, `qwen3:4b`, or `gemma4:31b`)
- **Image extraction only:** a vision-capable Ollama model, e.g. `llava:7b-v1.6`

### Ollama Setup
```sh
# 1. Install Ollama via Homebrew or download from https://ollama.com/download
brew install ollama

# 2. Start Ollama service
ollama serve

# 3. Pull a model (in another terminal)
ollama pull gemma4:31b

# 4. Optional: pull a vision model for image extraction
ollama pull llava:7b-v1.6
```

### Alternative: Ollama Setup via Docker
```sh
cd ollama
docker-compose up -d
docker exec -it ollama ollama pull gemma4:31b
```

## Usage

From the repository root, with Ollama running and the selected model pulled.

### Ollama Test

Send a simple prompt to confirm the configured Ollama endpoint and model are reachable:

```sh
dotnet run --project src/DocumentProcessor -- test-ollama
```

Optionally override the prompt:

```sh
dotnet run --project src/DocumentProcessor -- test-ollama --prompt "Reply with READY."
```

### PDF Classification

Classify a PDF:

```sh
dotnet run --project src/DocumentProcessor -- classify-pdf files/classification/tests/invoice_3.pdf
```

By default, classification results are written to:

```text
files/classification/results/<document-type>/<pdf-filename>_<timestamp>.json
```

Classification results are written as a JSON object containing:

```json
{
  "Date": "2026-09-20T16:37:48.724+02:00",
  "Model": "qwen:4b",
  "ProcessingTimeMs": 1234,
  "Data": {
    "DocumentType": "INVOICE",
    "Confidence": 0.95,
    "Reasoning": "The document contains invoice number, line items and a total amount."
  },
  "RawOcr": "Raw text extracted from the PDF"
}
```

### PDF Data Extraction

Extract data from a PDF:

```sh
dotnet run --project src/DocumentProcessor -- extract-pdf-data \
	files/extraction/tests/invoice_example/invoice_3.pdf \
	--type invoice_example
```

By default, extraction results are written to:

```text
files/extraction/results/<type>/<pdf-filename>_<timestamp>.json
```

### PDF Image Data Extraction

Extract data from a PDF image:

```sh
dotnet run --project src/DocumentProcessor -- extract-pdf-image-data \
	files/image-extraction/tests/invoice_example/invoice_3.pdf \
	--type invoice_example
```

By default, image extraction results are written to:

```text
files/image-extraction/results/<type>/<pdf-filename>_<timestamp>.json
```

## Text vs Image Extraction

| Command | Input sent to model | Use when                                                   |
|---|---|------------------------------------------------------------|
| `extract-pdf-data` | PDF text extracted with PdfPig | The PDF has selectable text                                |
| `extract-pdf-image-data` | Rendered PDF image only | You want a vision model (e.g. `llava:7b-v1.6`) to read the document image |

## Extraction Flow

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
    CLI->>CLI: Build field schema and prompt context
    CLI->>Ollama: Send system prompt, examples, PDF text, schema
    Ollama-->>CLI: Structured JSON response
    CLI->>Output: Write timestamped JSON result
    Output-->>User: Log output file path
```

The current pipeline extracts text directly with PdfPig. OCR is not part of the extraction flow.

Extraction results are written as a JSON object containing:

```json
{
  "Date": "2026-09-20T16:36:46.546+02:00",
  "Model": "qwen:4b",
  "ProcessingTimeMs": 2345,
  "Data": [
    {
      "FieldName": "invoiceNumber",
      "Text": "2025-002"
    }
  ],
  "RawOcr": "Raw text extracted from the PDF"
}
```

## Image Extraction Flow

```mermaid
sequenceDiagram
    actor User
    participant CLI as extract-pdf-image-data
    participant Renderer as PDF renderer
    participant Prompt as Image prompt
    participant Ollama as Vision model
    participant Output as Results folder

    User->>CLI: Run command with PDF path and --type invoice_example
    CLI->>Renderer: Render PDF to PNG image
    Renderer-->>CLI: Base64 image
    CLI->>Prompt: Load image extraction prompt
    CLI->>Ollama: Send prompt, schema and image
    Ollama-->>CLI: Structured JSON response
    CLI->>Output: Write timestamped JSON result
```
