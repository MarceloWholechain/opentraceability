# Diagnostics Tool

An open-source diagnostic tool designed to help identify and resolve real-time data transmission issues between traceability systems. This tool provides a neutral, transparent way to assess data structure correctness, API functionality, and identify common issues like formatting errors or protocol mismatches.

## Overview

The Diagnostics Tool addresses the challenge that even when traceability systems have passed Capability Tests, certification doesn't always guarantee seamless interoperability. When data exchange fails, it can be difficult to pinpoint whether the problem lies with the sender or the receiver. This tool serves as a neutral diagnostic solution that can be used by either solution provider to generate shareable diagnostic reports and flag potential issues.

## Technology Stack

- **Framework**: C# with .NET 8 (latest LTS version)
- **Web Server**: ASP.NET Core framework
- **License**: MIT License (open-source)
- **Source Control**: Public GitHub repository

## Deployment

### Docker Support
The software tool is designed for easy deployment via Docker. Users have two options:

1. **Docker Hub**: Use the pre-built Docker image from Docker Hub for quick deployment
2. **Source Build**: Download and compile the source code directly

## How It Works

The Diagnostics Tool operates by sending requests to traceability systems and analyzing their responses to ensure compliance with expected traceability standards or frameworks.

### Execution Flow

1. **API Request**: User calls the Diagnostics Tool API with:
   - URL of the target API
   - Request parameters for the API request
   - Optional parameters:
     - Automatic traceback execution
     - Automatic master data resolution

2. **System Verification**: The tool executes the request and verifies:
   - Response headers and content are formatted correctly and valid
   - No unexpected results (e.g., events that don't match query parameters)
   - No validity errors (e.g., duplicate Event IDs across multiple events)
   - Potential validity warnings (e.g., events recorded before EPC creation)

3. **Results Documentation**: Each verification error is captured with detailed information providing clear insights into API compliance failures

4. **Response Format**: Results are returned synchronously in the original HTTP request as:
   - **Default**: JSON format
   - **Optional**: PDF format (specify in Accept-Content header)

## Validation Approach

### JSON Schemas
The tool primarily utilizes JSON schemas to validate API request responses. When JSON schema validation is insufficient, custom C# code handles specific verification requirements.

## Multi-Commodity Support

The Diagnostics Tool is **commodity agnostic**, supporting traceability across multiple sectors through its flexible architecture:

- Supports beef, leather, seafood, and other commodity supply chains
- New commodities can be added by including additional JSON schemas
- Extensible design allows validation of traceability data from a wide range of products
- Built on shared core standards/frameworks for easy extension

## Scope and Limitations

### Completeness Checks
The Diagnostics Tool **does not** check for CTE/KDE matrix completeness. It is designed for real-world traceability data, which typically:
- May not contain all KDEs of applicable modules
- May not include all CTEs of the entire supply chain

## Getting Started

1. Clone the repository from GitHub
2. Build using .NET 8 SDK or use the Docker image
3. Configure your traceability system endpoints
4. Make API calls to begin diagnostics

## Contributing

This is an open-source project under the MIT license. Contributions are welcome to help improve traceability system interoperability across various supply chains.

