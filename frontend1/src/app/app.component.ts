import { Component } from '@angular/core';
import { HttpClient, HttpEventType, HttpRequest } from '@angular/common/http';
import { UploadService } from "./upload.service";

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css'],
})
export class AppComponent {
  loader: boolean;
  imageDetails: any;
  formData: FormData;

  files: any;
  constructor(private httpClient: HttpClient, private uploadService: UploadService) {
    this.loader = false;
    this.getAllFiles();
  }
  working = false;
  uploadFile: File | null;
  uploadFileLabel: string | undefined = 'Choose an image to upload';
  uploadProgress: number;
  uploadUrl: string;

  handleFileInput(files: FileList) {
    if (files.length > 0) {
      this.uploadFile = files.item(0);
      this.uploadFileLabel = this.uploadFile?.name;
    }
  }

  upload() {
    if (!this.uploadFile) {
      alert('Choose a file to upload first');
      return;
    }

    const formData = new FormData();
    formData.append(this.uploadFile.name, this.uploadFile);

    this.uploadUrl = '';
    this.uploadProgress = 0;
    this.working = true;

    this.uploadService.upload(formData).subscribe((event: any) => {
      if (event.type === HttpEventType.UploadProgress) {
        this.uploadProgress = Math.round((100 * event.loaded) / event.total);
      } else if (event.type === HttpEventType.Response) {
        this.uploadUrl = event.body.fileURL;
        this.uploaded();
      }
    }, (error: any) => {
      console.error(error);
    }).add(() => {
      this.working = false;
    });
  }

  uploaded(){
    this.getAllFiles();
    this.uploadFileLabel = "";
    setTimeout(() => {
      this.uploadUrl = "";
    }, 2500);
  }

  getAllFiles(){
    this.loader = true;
    this.uploadService.getAllFiles().subscribe(
      (data: any) => {
        this.files = data;
        this.loader = false;
      },
      (error) => {
        this.loader = false;
      }
    );
  }
}
