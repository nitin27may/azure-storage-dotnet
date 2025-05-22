import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { map, Observable, Subject, switchMap } from 'rxjs';
import { environment } from "../environments/environment";

@Injectable({ providedIn: 'root' })
export class StorageService {
  private readonly chunkSize = 5 * 1024 * 1024; // 5 MB per chunk
  private baseUrl = environment.apiEndpoint;

  constructor(private http: HttpClient) {}

  uploadFile(file: File, containerName: string, blobName: string): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('containerName', containerName);
    formData.append('blobName', blobName);
    return this.http.post(`${this.baseUrl}/blob/upload`, formData);
  }

  uploadLargeFile1(file: File, containerName: string, blobName: string): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('containerName', containerName);
    formData.append('blobName', blobName);
    return this.http.post(`${this.baseUrl}/blob/upload-large`, formData);
  }

  getFiles(containerName: string): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/blob/list?containerName=${containerName}`);
  }

  deleteFile(fileName: string): Observable<any> {
    return this.http.delete(`${this.baseUrl}/blob/delete/${fileName}`);
  }

  uploadChunkFile(file: File, containerName: string): Observable<any> {
    const chunkSize = this.chunkSize || 5 * 1024 * 1024; // Default chunk size: 5 MB
    const totalChunks = Math.ceil(file.size / chunkSize);
    let isCompleted = false; // Guard to ensure process runs only once

    const uploadChunk = async (chunkIndex: number): Promise<void> => {
      const start = chunkIndex * chunkSize;
      const end = Math.min(start + chunkSize, file.size);
      const chunk = file.slice(start, end);

      const formData = new FormData();
      formData.append('chunk', chunk, file.name);
      formData.append('containerName', containerName);
      formData.append('blobName', file.name);
      formData.append('chunkIndex', chunkIndex.toString());
      formData.append('totalChunks', totalChunks.toString());

      try {
        const response = await this.http.post(`${this.baseUrl}/blob/upload-chunk`, formData).toPromise();
        console.log(`Uploaded chunk ${chunkIndex + 1}/${totalChunks}`, response);

        if (chunkIndex + 1 >= totalChunks) {
          console.log('All chunks uploaded successfully.');
          isCompleted = true; // Mark completion
          return; // End recursion explicitly
        }
      } catch (error) {
        console.error(`Error uploading chunk ${chunkIndex + 1}:`, error);
        throw error; // Propagate error to calling code
      }
    };

    return new Observable((observer) => {
      if (isCompleted) {
        console.log('Upload already completed, skipping.');
        return; // Exit if already completed
      }

      let uploadedChunks = 0;
      let isCancelled = false;

      const uploadChunksSequentially = async (): Promise<void> => {
        for (let i = 0; i < totalChunks && !isCancelled; i++) {
          try {
            await uploadChunk(i);
            uploadedChunks++;
            observer.next({
              progress: Math.round((uploadedChunks / totalChunks) * 100),
            });
          } catch (error) {
            observer.error(error);
            return; // Stop further processing on error
          }
        }
      };

      uploadChunksSequentially()
        .then(() => {
          if (!isCancelled && !isCompleted) {
            //observer.next({ message: 'File uploaded successfully.' });
            observer.complete();
          }
        })
        .catch((error) => {
          observer.error(error);
        });

      // Cleanup function to handle cancellation
      return () => {
        isCancelled = true;
        console.log('Upload cancelled by the user.');
      };
    });
  }
  streamUpload(file: File, containerName: string, blobName: string): Observable<void> {
    const fileStream = file.stream();
    const headers = new Headers({
      'Container-Name': containerName,
      'Blob-Name': blobName,
    });

    const reader = fileStream.getReader();

    return new Observable<void>((observer) => {
      fetch(`${this.baseUrl}/blob/stream-upload`, {
        method: 'POST',
        headers,
        duplex: 'half',
        body: new ReadableStream({
          start(controller) {
            function push() {
              reader.read().then(({ done, value }) => {
                if (done) {
                  controller.close();
                  observer.next(); // Notify completion
                  observer.complete();
                  return;
                }
                controller.enqueue(value); // Pass the chunk to the stream
                push(); // Continue reading the next chunk
              }).catch((error) => {
                observer.error(error); // Notify error
              });
            }
            push();
          },
        }),
      } as RequestInit)
        .then((response) => {
          if (response.ok) {
            observer.next(); // Notify success
            observer.complete(); // Complete the observable
          } else {
            observer.error(new Error(`Upload failed with status: ${response.status}`));
          }
        })
        .catch((error) => {
          observer.error(error); // Notify error
        });
    });
  }

  uploadLargeFile(
    file: File,
    containerName: string,
    onProgress?: (progress: UploadProgress) => void
  ): Observable<any> {
    // Step 1: Get upload URL
    const request: LargeFileUploadRequest = {
      containerName,
      fileName: file.name,
      contentType: file.type,
      fileSize: file.size
    };

    return this.http.post<LargeFileUploadResponse>(
      `${this.baseUrl}/blob/get-upload-url`,
      request
    ).pipe(
      switchMap(response => this.UploadFileToSAS(file, response.sasUri, onProgress)),
    );
  }

  UploadFileToSAS(file: File, sasUrl: string, onProgress?: (progress: UploadProgress) => void): Observable<any> {
    return new Observable(observer => {
      const xhr = new XMLHttpRequest();

      xhr.upload.onprogress = (event) => {
        if (onProgress && event.lengthComputable) {
          onProgress({
            loaded: event.loaded,
            total: event.total,
            percentage: Math.round((event.loaded / event.total) * 100)
          });
        }
      };

      xhr.onload = () => {
        if (xhr.status >= 200 && xhr.status < 300) {
          observer.next(xhr.response);
          observer.complete();
        } else {
          console.error('Upload failed with status:', xhr.status);
          console.error('Response:', xhr.responseText);
          observer.error(new Error(`Upload failed: ${xhr.status} ${xhr.statusText} - ${xhr.responseText || 'No response details'}`));
        }
      };

      xhr.onerror = (error) => {
        console.error('Upload error:', error);
        observer.error(new Error(`Network error during upload: ${error instanceof Error ? error.message : 'Unknown error'}`));
      };

      xhr.onabort = () => {
        observer.error(new Error('Upload aborted by user'));
      };
      
      xhr.open('PUT', sasUrl, true);
      
      // These headers are required for Azure Blob Storage
      xhr.setRequestHeader('x-ms-blob-type', 'BlockBlob');
      
      // Content type header
      const contentType = file.type || 'application/octet-stream';
      xhr.setRequestHeader('Content-Type', contentType);

      // Send the file
      xhr.send(file);

      return () => {
        xhr.abort();
      };
    });
  }
  

  downloadBytes(file: any): Observable<Blob> {
    return this.http.get(
      `${this.baseUrl}/blob/download-bytes?containerName=${file.containerName}&blobName=${file.name}`, 
      { responseType: 'blob' }
    );
  }
  
  downloadStream(file: any): Observable<Blob> {
    return this.http.get(
      `${this.baseUrl}/blob/download-stream?containerName=${file.containerName}&blobName=${file.name}`, 
      { responseType: 'blob' }
    ).pipe(
      map(blob => {
        // Create a blob URL for the file
        return new Blob([blob], { type: file.contentType || 'application/octet-stream' });
      })
    );
  }
}



export interface UploadProgress {
  loaded: number;
  total: number;
  percentage: number;
}

export interface LargeFileUploadRequest {
  containerName: string;
  fileName: string;
  contentType: string;
  fileSize: number;
}

export interface LargeFileUploadResponse {
  sasUri: string;
  blobName: string;
  expiresOn: string;
  containerName: string;
}
