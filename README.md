# Azure Blob Storage Example with .NET API and Angular

[https://nitinksingh.com/efficient-file-management-on-azure-blob-storage-crud-operations-and-upload-strategies](https://nitinksingh.com/efficient-file-management-on-azure-blob-storage-crud-operations-and-upload-strategies)

## About the Repository
This repository showcases a comprehensive example of Azure Blob Storage integration using a modern tech stack:

- **Backend**: A .NET 9 API project.
- **Frontend**: An Angular 19 project with Material Design for a sleek and user-friendly interface.

It provides robust examples for handling Azure Blob Storage, including three types of upload APIs:

1. **File Upload**: Upload files directly to Azure Blob Storage.
2. **Chunk Upload**: Handle large files by uploading them in smaller chunks.
3. **Stream Upload**: Stream data directly to Azure Blob Storage for efficient handling of larger datasets.

This repository is an excellent starting point for developers looking to integrate Azure Blob Storage in their projects with modern web technologies.

---
## Features
A complete backend and frontend project structure to show case File operations (CRUD) for Azure Storage

### Backend (.NET 9 API)
- [x] **Azure Blob Storage Integration**: Includes complete implementation for uploading, downloading, and managing files.
- [ ] **Azure Table Storage Integration**: CRUD example for table records.
- [ ] **Azure File Share Integration**: Includes complete implementation for uploading, downloading, and managing files.

### Frontend (Angular 19 with Material Design)
- [x] **File Management UI**: A user-friendly interface for file upload and management.

### Dockerized Setup
- [ ] Docker-Compose for Multi-Container Orchestration with NGINX as Load Balancer
- [ ] Health Checks
- [ ] Docker Debug mode with hot reload for the API and UI
- [ ] Docker Production version

## Getting Started

### Prerequisites
- **Backend Requirements**:
  - .NET 9 SDK
  - Azure Storage Account with Blob Storage
  - IDE: Visual Studio or VS Code
- **Frontend Requirements**:
  - Node.js (v20 or higher)
  - Angular CLI
  - IDE: VS Code or any preferred code editor

---

### Setup Instructions

#### 1. Clone the Repository
```bash
git clone https://github.com/nitin27may/azure-storage-dotnet.git
cd azure-storage-dotnet
```

#### 2. Configure the Backend
- Navigate to the `.NET API` project folder.
- Update the `appsettings.json` file with your Azure Blob Storage connection string:

```json
{
  "ConnectionStrings": {
    "AzureStorage": "<Your_Connection_String>"
  }
}
```

- Run the backend:
```bash
dotnet run
```

#### 3. Configure the Frontend
- Navigate to the Angular project folder.
- Install dependencies:
```bash
npm install
```
- Start the Angular development server:
```bash
ng serve
```
- Open your browser and navigate to `http://localhost:4200`.

---

## API Endpoints

Please refer the API Collection folder, it has all endpoints example. You can use Bruno client for it.

---

## Contributing
We welcome contributions! Please feel free to submit issues or pull requests to improve this repository.

---

## License
This project is licensed under the MIT License.

---

## Contact
For any questions or feedback, please reach out at [nitin27may@gmail.com](mailto:nitin27may@gmail.com).
