# FusionOS API Documentation

## Overview

FusionOS provides a RESTful API built on **.NET 8** with **Clean Architecture**, **CQRS (MediatR)**, and strict **Tenant Isolation**.

## OpenAPI / Swagger Specification

When running FusionOS locally in development mode or via Docker Compose, interactive Swagger API documentation is served at:

```
http://localhost:5000/swagger
```

## Standard API Conventions

- **Base URL**: `/api/v1`
- **Content Type**: `application/json`
- **Error Response Format**: [RFC 7807 ProblemDetails](https://tools.ietf.org/html/rfc7807)
- **Authentication**: HTTP Authorization header with `Bearer <JWT_TOKEN>`

## Response Schemas

### Standard Success Response
```json
{
  "data": { ... },
  "message": "Operation completed successfully",
  "statusCode": 200
}
```

### Standard Error Response (RFC 7807 ProblemDetails)
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "detail": "The SKU provided already exists in the system.",
  "errors": {
    "Sku": ["SKU 'PROD-001' must be unique."]
  }
}
```
