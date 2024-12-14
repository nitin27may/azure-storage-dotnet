import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class StorageService {
  private baseUrl = 'https://your-api-endpoint';

  constructor(private http: HttpClient) {}

  uploadFile(file: File): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post(`${this.baseUrl}/upload`, formData);
  }

  getFiles(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/files`);
  }

  deleteFile(fileName: string): Observable<any> {
    return this.http.delete(`${this.baseUrl}/delete/${fileName}`);
  }
}
