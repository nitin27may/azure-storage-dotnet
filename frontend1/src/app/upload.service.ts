import { Injectable } from '@angular/core';
import { HttpClient, HttpErrorResponse, HttpRequest } from '@angular/common/http';
import { catchError, map, throwError } from "rxjs";

@Injectable({ providedIn: 'root' })
export class UploadService {
  constructor(private httpClient: HttpClient) { }


  getAllFiles() {
    return this.httpClient.get('/api/File/videos').pipe(
      map((data: any) => {
        return data.sort((a: any, b: any) => {
          return new Date(b.createdOn).getTime() - new Date(a.createdOn).getTime();
        })
      }),
      catchError(this.handleErrorObservable)
    );
  }

  upload(formData: FormData) {
    const url = 'api/File';
    const uploadReq = new HttpRequest('POST', url, formData, {
      reportProgress: true,
    });

    return this.httpClient.request(uploadReq).pipe(
      catchError(this.handleErrorObservable)
    );

  }


  private handleErrorObservable(error: HttpErrorResponse) {
    return throwError(error);
  }
}
