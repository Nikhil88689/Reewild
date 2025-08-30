# 🌱 Foodprint API - Carbon Footprint Estimator

A powerful .NET 9 Web API that estimates the carbon footprint of food dishes using AI-powered ingredient analysis. Built for the Reewild Backend Engineer Take Home Challenge.

## 🚀 Features

- **Text-based Analysis** - Estimate carbon footprint from dish names using GPT-4
- **Image Analysis** - Analyze food images using GPT-4 Vision to identify dishes and ingredients
- **Accurate Carbon Calculations** - Comprehensive database of ingredient carbon footprints
- **Robust Error Handling** - Comprehensive validation and error management
- **OpenAPI Documentation** - Interactive Swagger documentation
- **Structured Logging** - Detailed logging with Serilog
- **Docker Support** - Containerized deployment ready
- **Production Ready** - Scalable architecture with best practices

## 🏗️ Architecture

```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   Controllers   │ -> │     Services     │ -> │   AI Models     │
│                 │    │                  │    │                 │
│ • EstimateCtrl  │    │ • LLMService     │    │ • OpenAI GPT-4  │
│ • HealthCtrl    │    │ • VisionService  │    │ • GPT-4 Vision  │
└─────────────────┘    │ • CarbonCalc     │    └─────────────────┘
                       └──────────────────┘    
                                ↓
                       ┌──────────────────┐
                       │ Carbon Database  │
                       │ (In-Memory)      │
                       └──────────────────┘
```

## 📋 API Endpoints

### POST `/api/estimate`
Estimate carbon footprint from dish name.

**Request:**
```json
{
  "dish": "Chicken Biryani"
}
```

**Response:**
```json
{
  "dish": "Chicken Biryani",
  "estimatedCarbonKg": 4.2,
  "confidence": 0.85,
  "ingredients": [
    {
      "name": "Chicken",
      "carbonKg": 2.5,
      "estimatedQuantity": "150g",
      "category": "protein"
    },
    {
      "name": "Rice",
      "carbonKg": 1.1,
      "estimatedQuantity": "200g", 
      "category": "grain"
    }
  ],
  "metadata": {
    "analysisMethod": "text",
    "analyzedAt": "2024-01-15T10:30:00Z",
    "modelUsed": "gpt-4",
    "processingTimeMs": 1250
  }
}
```

### POST `/api/estimate/image`
Estimate carbon footprint from food image.

**Request:**
- `multipart/form-data` with image file
- Supported formats: JPEG, PNG, GIF, WebP
- Max size: 20MB

**Response:** Same format as text endpoint

### GET `/api/health`
Basic health check endpoint.

### GET `/api/health/detailed`
Detailed health check with dependency status.

## 🛠️ Getting Started

### Prerequisites

- .NET 9.0 SDK
- OpenAI API Key
- (Optional) Docker

### Local Development

1. **Clone the repository**
```bash
git clone <repository-url>
cd FoodprintApi
```

2. **Configure OpenAI API Key**

Update `appsettings.Development.json`:
```json
{
  "OpenAI": {
    "ApiKey": "your-openai-api-key-here"
  }
}
```

Or set environment variable:
```bash
export OpenAI__ApiKey="your-openai-api-key-here"
```

3. **Restore dependencies**
```bash
dotnet restore
```

4. **Run the application**
```bash
dotnet run
```

The API will be available at:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger UI: `https://localhost:5001` (root path)

### Using Docker

1. **Build the image**
```bash
docker build -t foodprint-api .
```

2. **Run the container**
```bash
docker run -p 8080:8080 -e OpenAI__ApiKey="your-api-key" foodprint-api
```

Access at: `http://localhost:8080`

## 🧪 Example Requests

### Using curl

**Text Analysis:**
```bash
curl -X POST "https://localhost:5001/api/estimate" \
  -H "Content-Type: application/json" \
  -d '{"dish": "Margherita Pizza"}'
```

**Image Analysis:**
```bash
curl -X POST "https://localhost:5001/api/estimate/image" \
  -F "image=@path/to/food-image.jpg"
```

### Using PowerShell

**Text Analysis:**
```powershell
$body = @{ dish = "Beef Tacos" } | ConvertTo-Json
Invoke-RestMethod -Uri "https://localhost:5001/api/estimate" -Method Post -Body $body -ContentType "application/json"
```

## 🏭 Production Considerations

### What I Would Add for Production

#### Security
- **API Authentication** - JWT tokens or API keys
- **Rate Limiting** - Prevent abuse with throttling
- **Input Sanitization** - Enhanced validation for malicious content
- **HTTPS Enforcement** - SSL/TLS termination
- **CORS Policies** - Restrictive cross-origin policies

#### Scalability
- **Caching Layer** - Redis for frequent dish lookups
- **Database Integration** - PostgreSQL/MongoDB for ingredient data
- **Message Queues** - Background processing for heavy analysis
- **Load Balancing** - Multiple API instances
- **CDN Integration** - Image processing and caching

#### Observability
- **Distributed Tracing** - OpenTelemetry with Azure App Insights
- **Metrics Collection** - Performance counters and business metrics
- **Health Checks** - Comprehensive dependency monitoring
- **Alerting** - Critical error notifications

#### Data Management
- **Real Carbon Database** - Live emissions data integration
- **Regional Variations** - Location-based carbon calculations
- **User Analytics** - Usage patterns and optimization insights
- **Audit Logging** - Compliance and debugging support

#### Infrastructure
- **Kubernetes Deployment** - Container orchestration
- **CI/CD Pipeline** - Automated testing and deployment
- **Environment Management** - Dev/staging/production isolation
- **Backup Strategy** - Data persistence and recovery

## 🎯 Key Design Decisions

### 1. **Service-Oriented Architecture**
- **Separation of Concerns**: LLM, Vision, and Carbon services are isolated
- **Testability**: Easy to mock dependencies for unit testing
- **Extensibility**: Simple to add new AI providers or carbon data sources

### 2. **Strongly Typed Models**
- **Type Safety**: Comprehensive DTOs for all requests/responses
- **Validation**: FluentValidation for robust input checking
- **Documentation**: Rich OpenAPI specifications

### 3. **Resilient Error Handling**
- **Global Exception Middleware**: Consistent error responses
- **Graceful Degradation**: Fallback analysis when AI services fail
- **Request Tracking**: Unique request IDs for debugging

### 4. **Performance Optimization**
- **Async/Await**: Non-blocking I/O operations
- **Streaming**: Efficient image processing
- **Caching Ready**: Architecture supports future caching layers

### 5. **Configuration Management**
- **Environment-based**: Different configs for dev/production
- **Secrets Management**: API keys via environment variables
- **Feature Flags Ready**: Easy to add conditional features

## 📊 Carbon Footprint Data

The API includes a comprehensive database of carbon footprints for common ingredients:

- **Proteins**: Beef (27.0), Chicken (6.1), Fish (5.4) kg CO2/kg
- **Grains**: Rice (2.7), Wheat (1.4), Pasta (1.1) kg CO2/kg  
- **Vegetables**: Tomatoes (1.4), Onions (0.3), Broccoli (0.4) kg CO2/kg
- **Dairy**: Cheese (13.5), Milk (3.2), Butter (23.8) kg CO2/kg

*Sources: Various environmental studies and carbon databases*

## 🧪 Testing

### Manual Testing

1. **Start the API**
2. **Visit Swagger UI** at root URL
3. **Test both endpoints** with sample data
4. **Verify responses** match expected format

### Sample Test Cases

**Edge Cases to Test:**
- Empty dish names
- Very long dish names  
- Special characters in dish names
- Invalid image formats
- Oversized images
- Network timeouts
- Invalid API keys

### Expected Behavior
- ✅ **Graceful degradation** when AI services fail
- ✅ **Consistent error formats** across all endpoints
- ✅ **Request tracking** with unique IDs
- ✅ **Proper HTTP status codes**

## 🔧 Configuration

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `OpenAI__ApiKey` | OpenAI API Key | *Required* |
| `OpenAI__Model` | Text analysis model | `gpt-4` |
| `OpenAI__VisionModel` | Image analysis model | `gpt-4-vision-preview` |
| `ASPNETCORE_ENVIRONMENT` | Environment | `Production` |

### appsettings.json Structure

```json
{
  "OpenAI": {
    "ApiKey": "your-key",
    "Model": "gpt-4",
    "VisionModel": "gpt-4-vision-preview",
    "MaxTokens": 1000,
    "Temperature": 0.3,
    "TimeoutSeconds": 30
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information"
    }
  }
}
```

## 🚨 Limitations & Assumptions

### Current Limitations
- **OpenAI Dependency**: Requires OpenAI API access
- **In-Memory Data**: Carbon data is not persisted
- **No Authentication**: Open API endpoints
- **Rate Limits**: Subject to OpenAI rate limiting
- **Image Size**: 20MB maximum file size

### Assumptions
- **English Dish Names**: Optimized for English language
- **Standard Portions**: Typical serving sizes assumed
- **Generic Carbon Data**: Not region-specific
- **Stable AI Models**: OpenAI model availability

## 🤝 Contributing

This is a take-home challenge project, but the architecture supports:

1. **Adding new AI providers** (Anthropic, Azure Cognitive Services)
2. **Database integration** (PostgreSQL, MongoDB)
3. **Enhanced security** (JWT, API keys)
4. **Advanced features** (caching, background processing)

## 📄 License

This project is created for the Reewild Backend Engineer Take Home Challenge.

---

## 🎉 Ready to Test!

1. **Get an OpenAI API key** from [OpenAI Platform](https://platform.openai.com/)
2. **Configure the key** in appsettings.Development.json
3. **Run the project** with `dotnet run`
4. **Visit** `https://localhost:5001` for Swagger UI
5. **Test the endpoints** with your favorite dishes!

🌱 *Built with sustainability in mind - helping users make conscious food choices!*#   R e e w i l d  
 