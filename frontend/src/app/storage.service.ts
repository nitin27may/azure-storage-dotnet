import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, Subject } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class StorageService {
  private readonly chunkSize = 5 * 1024 * 1024; // 5 MB per chunk
  private baseUrl = 'https://localhost:7149/api';

  constructor(private http: HttpClient) {}

  uploadFile(file: File): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post(`${this.baseUrl}/upload`, formData);
  }

  uploadLargeFile(file: File): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post(`${this.baseUrl}/upload`, formData);
  }

  getFiles(containerName: string): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/blob/list?containerName=${containerName}`);
  }

  deleteFile(fileName: string): Observable<any> {
    return this.http.delete(`${this.baseUrl}/delete/${fileName}`);
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
  async streamUpload(file: File, containerName: string, blobName: string, progress$: Subject<number>): Promise<void> {
    const fileStream = file.stream();

    const headers = new Headers({
      'Container-Name': containerName,
      'Blob-Name': blobName,
    });

    const totalSize = file.size; // Total file size in bytes
    let uploadedSize = 0; // Tracks the number of bytes uploaded

    const reader = fileStream.getReader();


    try {
      const response = await fetch(`${this.baseUrl}/blob/stream-upload`, {
        method: 'POST',
        headers,
        duplex: 'half',
        body: new ReadableStream({
          start(controller) {
            function push() {
              reader.read().then(({ done, value }) => {
                if (done) {
                  controller.close(); // End the stream
                  progress$.next(100); // Emit 100% on completion
                  return;
                }

                if (value) {
                  const chunkSize = value.length; // Size of the current chunk
                  uploadedSize += chunkSize; // Increment uploaded size
                  const progress = Math.round((uploadedSize / totalSize) * 100); // Calculate progress
                  progress$.next(progress); // Emit progress value
                  console.log(`Uploaded chunk size: ${chunkSize} bytes`);
                  console.log(`Uploaded so far: ${uploadedSize} bytes (${progress}%)`);
                }

                controller.enqueue(value); // Pass the chunk to the stream
                push(); // Continue reading the next chunk
              });
            }
            push();
          },
        }) ,
      } as RequestInit);

      if (response.ok) {
        console.log('File uploaded successfully');
      } else {
        console.error('File upload failed:', response.statusText);
        throw new Error(`Upload failed with status: ${response.status}`);
      }
    } catch (error) {
      console.error('Error during upload:', error);
      throw error;
    } finally {
      progress$.complete(); // Notify completion
    }
  }
}
