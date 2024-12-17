# Azure Blob Storage Example with .NET 9 API and Angular 19

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

### Backend (.NET 9 API)
- **Azure Blob Storage Integration**: Includes complete implementation for uploading, downloading, and managing files.
- **API Endpoints**: Demonstrates three upload strategies—file upload, chunk upload, and stream upload.
- **Scalable Architecture**: Designed with scalability and performance in mind.

### Frontend (Angular 19 with Material Design)
- **File Management UI**: A user-friendly interface for file upload and management.
- **Responsive Design**: Built with Angular Material Design for a polished and responsive user experience.
- **Real-Time Feedback**: Progress bars and notifications for upload operations.

---

## Getting Started

### Prerequisites
- **Backend Requirements**:
  - .NET 9 SDK
  - Azure Storage Account with Blob Storage
  - IDE: Visual Studio or VS Code
- **Frontend Requirements**:
  - Node.js (v18 or higher)
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
  "AzureBlobStorage": {
    "ConnectionString": "<Your_Connection_String>",
    "ContainerName": "<Your_Container_Name>"
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

### File Upload API
- **Endpoint**: `POST /api/upload/file`
- **Description**: Uploads a single file directly to Azure Blob Storage.

### Chunk Upload API
- **Endpoint**: `POST /api/upload/chunk`
- **Description**: Uploads large files in smaller chunks.

### Stream Upload API
- **Endpoint**: `POST /api/upload/stream`
- **Description**: Streams file data directly to Azure Blob Storage.

---

## Contributing
We welcome contributions! Please feel free to submit issues or pull requests to improve this repository.

---

## License
This project is licensed under the MIT License.

---

## Contact
For any questions or feedback, please reach out at [nitin27may@gmail.com](mailto:nitin27may@gmail.com).
